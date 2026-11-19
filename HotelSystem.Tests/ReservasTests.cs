using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Application.Reservas;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;
using HotelSystem.Tests.Soporte;

namespace HotelSystem.Tests;

/// <summary>Reservas de habitacion (HU-03), correo (HU-04), check-in (HU-06) y RN-01, RN-04, RN-09, RN-10, RN-11.</summary>
public class ReservasTests : IDisposable
{
    private readonly EntornoPruebas _entorno = new();

    public void Dispose() => _entorno.Dispose();

    private SolicitudReservaHabitacion Solicitud(string usuario, string habitacion, int desdeHoy, int hastaHoy, int personas = 1, TipoPago? pago = TipoPago.Tarjeta, bool mayor = true) =>
        new(_entorno.IdPersona(usuario), _entorno.IdHabitacion(habitacion), _entorno.Hoy.AddDays(desdeHoy), _entorno.Hoy.AddDays(hastaHoy), personas, mayor, pago, "Visa **** 4242");

    private ReservaConfirmadaDto Reservar(SolicitudReservaHabitacion solicitud) =>
        _entorno.En<IReservaService, ReservaConfirmadaDto>(r => r.ReservarHabitacion(solicitud));

    [Fact]
    public void Reservar_GuardaLaReservaConNochesYTotalReales()
    {
        var confirmada = Reservar(Solicitud("valentina.lagos", "204", 10, 13));

        Assert.StartsWith("RES-", confirmada.Codigo);
        Assert.Equal(3, confirmada.Noches);
        Assert.Equal(360000m, confirmada.ValorTotalAlojamiento);

        var mias = _entorno.En<IReservaService, IReadOnlyList<ReservaHuespedDto>>(r => r.ReservasDeHuesped(_entorno.IdPersona("valentina.lagos")));
        Assert.Contains(mias, r => r.Codigo == confirmada.Codigo && r.Estado == EstadoReserva.Confirmada);
    }

    [Fact]
    public void Reservar_EnviaElCorreoDeConfirmacion()
    {
        var confirmada = Reservar(Solicitud("valentina.lagos", "204", 10, 13));

        Assert.True(confirmada.CorreoEnviado);
        var correo = Assert.Single(_entorno.Correo.Enviados);
        Assert.Equal("valentina.lagos@email.com", correo.Para);
        Assert.Contains(confirmada.Codigo, correo.Asunto);
    }

    [Fact]
    public void RN01_NoSePuedeReservarLaMismaHabitacionEnFechasCruzadas()
    {
        Reservar(Solicitud("valentina.lagos", "204", 10, 13));

        var error = Assert.Throws<ReglaNegocioException>(() => Reservar(Solicitud("camila.duarte", "204", 12, 14)));
        Assert.Equal("RN-01", error.Regla);

        var disponibles = _entorno.En<IReservaService, IReadOnlyList<HabitacionDto>>(r =>
            r.BuscarDisponibles(TipoHabitacion.Simple, _entorno.Hoy.AddDays(11), _entorno.Hoy.AddDays(12), 1));
        Assert.DoesNotContain(disponibles, h => h.Numero == "204");
    }

    [Fact]
    public void RN01_ElDiaDeSalidaSePuedeReservarComoEntrada()
    {
        Reservar(Solicitud("valentina.lagos", "204", 10, 13));

        var siguiente = Reservar(Solicitud("camila.duarte", "204", 13, 15));
        Assert.Equal("204", siguiente.NumeroHabitacion);
    }

    [Fact]
    public async Task RNF07_DosReservasSimultaneasDeLaMismaHabitacion_SoloUnaSeGuarda()
    {
        var primera = Solicitud("valentina.lagos", "204", 20, 22);
        var segunda = Solicitud("camila.duarte", "204", 20, 22);
        using var salida = new Barrier(2);

        // Las dos peticiones arrancan al mismo tiempo. La transaccion serializable hace que
        // una espere a la otra: la segunda ve el cruce de fechas y se rechaza.
        var tareas = new[] { primera, segunda }
            .Select(solicitud => Task.Run(() =>
            {
                salida.SignalAndWait();
                try
                {
                    Reservar(solicitud);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }))
            .ToList();
        var resultados = await Task.WhenAll(tareas);

        Assert.Equal(1, resultados.Count(exito => exito));
        var habitacion = _entorno.IdHabitacion("204");
        var guardadas = _entorno.En<IReservaHabitacionRepository, int>(r =>
            r.ListarActivasQueSeCruzan(_entorno.Hoy.AddDays(20), _entorno.Hoy.AddDays(22)).Count(x => x.HabitacionId == habitacion));
        Assert.Equal(1, guardadas);
    }

    [Fact]
    public void RN04_SinMedioDePago_NoSeConfirma()
    {
        var error = Assert.Throws<ReglaNegocioException>(() => Reservar(Solicitud("valentina.lagos", "204", 10, 13, pago: null)));
        Assert.Equal("RN-04", error.Regla);
    }

    [Fact]
    public void RN10_TitularMenorDeEdad_SeRechazaAunqueMarqueLaCasilla()
    {
        var error = Assert.Throws<ReglaNegocioException>(() => Reservar(Solicitud("tomas.lagos", "204", 10, 13)));
        Assert.Equal("RN-10", error.Regla);
    }

    [Fact]
    public void RN10_SinDeclararMayoriaDeEdad_SeRechaza()
    {
        var error = Assert.Throws<ReglaNegocioException>(() => Reservar(Solicitud("valentina.lagos", "204", 10, 13, mayor: false)));
        Assert.Equal("RN-10", error.Regla);
    }

    [Fact]
    public void RN11_MasHuespedesQueLaCapacidad_SeRechaza()
    {
        var error = Assert.Throws<ReglaNegocioException>(() => Reservar(Solicitud("valentina.lagos", "204", 10, 13, personas: 3)));
        Assert.Equal("RN-11", error.Regla);
    }

    [Fact]
    public void RN09_CancelarAMenosDe48Horas_CobraUnaNoche()
    {
        // Valentina tiene la 210 para manana: faltan menos de 48 horas para el check-in.
        var resultado = Cancelar("valentina.lagos", "210");

        Assert.True(resultado.Tardia);
        Assert.Equal(120000m, resultado.Cargo);
    }

    [Fact]
    public void RN09_CancelarConAnticipacion_NoTieneCargo()
    {
        // La 602 es para dentro de 35 dias.
        var resultado = Cancelar("valentina.lagos", "602");

        Assert.False(resultado.Tardia);
        Assert.Equal(0m, resultado.Cargo);
    }

    private CancelacionDto Cancelar(string usuario, string habitacion)
    {
        var huesped = _entorno.IdPersona(usuario);
        var codigo = _entorno.En<IReservaService, IReadOnlyList<ReservaHuespedDto>>(r => r.ReservasDeHuesped(huesped))
            .Single(r => !r.EsTour && r.NumeroHabitacion == habitacion && r.Estado == EstadoReserva.Confirmada)
            .Codigo;
        Assert.True(ReservaHabitacion.TryIdDesdeCodigo(codigo, out var id));

        return _entorno.En<IReservaService, CancelacionDto>(r => r.CancelarReserva(id, huesped));
    }

    [Fact]
    public void CheckIn_DejaLaHabitacionOcupada()
    {
        var llegada = _entorno.En<IReservaService, IReadOnlyList<MovimientoDto>>(r => r.LlegadasDeHoy())
            .Single(m => m.NumeroHabitacion == "305");

        var movimiento = _entorno.En<IReservaService, MovimientoDto>(r => r.RegistrarCheckIn(llegada.ReservaId, _entorno.IdPersona("mlopez")));

        Assert.Equal(EstadoReserva.EnCurso, movimiento.Estado);
        Assert.Equal(EstadoHabitacion.Ocupada, movimiento.EstadoHabitacion);
    }

    [Fact]
    public void CheckIn_HabitacionTodaviaEnLimpieza_SeRechaza()
    {
        // La 203 recibe huesped hoy pero housekeeping aun no la ha liberado (RN-02).
        var llegada = _entorno.En<IReservaService, IReadOnlyList<MovimientoDto>>(r => r.LlegadasDeHoy())
            .Single(m => m.NumeroHabitacion == "203");

        var error = Assert.Throws<ReglaNegocioException>(() =>
            _entorno.En<IReservaService, MovimientoDto>(r => r.RegistrarCheckIn(llegada.ReservaId, _entorno.IdPersona("mlopez"))));
        Assert.Equal("RN-02", error.Regla);
    }
}
