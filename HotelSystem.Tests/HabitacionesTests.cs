using HotelSystem.Application.Housekeeping;
using HotelSystem.Application.Reservas;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;
using HotelSystem.Tests.Soporte;

namespace HotelSystem.Tests;

/// <summary>Housekeeping (HU-11): patron State a traves del servicio y patron Observer con los paneles.</summary>
public class HabitacionesTests : IDisposable
{
    private readonly EntornoPruebas _entorno = new();

    public void Dispose() => _entorno.Dispose();

    private HabitacionDto CambiarEstado(string numero, EstadoHabitacion estado, string? nota = null) =>
        _entorno.En<IHabitacionService, HabitacionDto>(h =>
            h.CambiarEstado(_entorno.IdHabitacion(numero), estado, nota, _entorno.IdPersona("amartinez")));

    [Fact]
    public void LaSemillaCreaLas60Habitaciones()
    {
        var habitaciones = _entorno.En<IHabitacionService, IReadOnlyList<HabitacionDto>>(h => h.Listar());

        Assert.Equal(60, habitaciones.Count);
        Assert.Equal(13, habitaciones.Count(h => h.Estado == EstadoHabitacion.Ocupada));
        Assert.All(habitaciones.Where(h => h.Estado == EstadoHabitacion.Ocupada), h => Assert.NotNull(h.HuespedActual));
    }

    [Fact]
    public void Limpieza_TerminadaDejaLaHabitacionDisponible()
    {
        var habitacion = CambiarEstado("105", EstadoHabitacion.Disponible);

        Assert.Equal(EstadoHabitacion.Disponible, habitacion.Estado);
    }

    [Fact]
    public void RN02_HousekeepingNoPuedeMarcarOcupada()
    {
        var error = Assert.Throws<ReglaNegocioException>(() => CambiarEstado("101", EstadoHabitacion.Ocupada));
        Assert.Equal("RN-02", error.Regla);
    }

    [Fact]
    public void RN02_UnaHabitacionOcupadaNoPasaADisponible()
    {
        var error = Assert.Throws<ReglaNegocioException>(() => CambiarEstado("102", EstadoHabitacion.Disponible));
        Assert.Equal("RN-02", error.Regla);
    }

    [Fact]
    public void Incidencia_SinDescripcion_SeRechaza()
    {
        Assert.Throws<ReglaNegocioException>(() => CambiarEstado("101", EstadoHabitacion.Mantenimiento, "  "));

        var conNota = CambiarEstado("101", EstadoHabitacion.Mantenimiento, "Aire acondicionado no enfria");
        Assert.Equal(EstadoHabitacion.Mantenimiento, conNota.Estado);
        Assert.Equal("Aire acondicionado no enfria", conNota.NotaMantenimiento);
    }

    [Fact]
    public void Observer_LosPanelesSeEnteranDeCadaCambio()
    {
        var recepcion = _entorno.Unico<PanelRecepcion>();
        var housekeeping = _entorno.Unico<PanelHousekeeping>();
        var versionRecepcion = recepcion.Version;

        CambiarEstado("101", EstadoHabitacion.EnLimpieza);

        Assert.True(recepcion.Version > versionRecepcion);
        var novedad = recepcion.UltimasNovedades.First();
        Assert.Equal("101", novedad.Numero);
        Assert.Equal(EstadoHabitacion.Disponible, novedad.EstadoAnterior);
        Assert.Equal(EstadoHabitacion.EnLimpieza, novedad.EstadoNuevo);
        Assert.Contains(housekeeping.ColaLimpieza, t => t.Numero == "101");

        CambiarEstado("101", EstadoHabitacion.Disponible);
        Assert.DoesNotContain(housekeeping.ColaLimpieza, t => t.Numero == "101");
    }

    [Fact]
    public void PlanLimpieza_PrimeroLasHabitacionesConLlegadaHoy()
    {
        var plan = _entorno.En<IHabitacionService, IReadOnlyList<TareaLimpiezaDto>>(h => h.PlanLimpieza());

        Assert.NotEmpty(plan);
        Assert.All(plan, t => Assert.Equal(EstadoHabitacion.EnLimpieza, t.Habitacion.Estado));
        Assert.Equal("203", plan[0].Habitacion.Numero);
        Assert.True(plan[0].LlegadaHoy);
        Assert.All(plan.Skip(1), t => Assert.False(t.LlegadaHoy));
    }
}
