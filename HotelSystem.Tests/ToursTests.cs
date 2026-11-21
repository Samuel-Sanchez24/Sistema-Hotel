using HotelSystem.Application.CierreCuenta;
using HotelSystem.Application.Tours;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;
using HotelSystem.Tests.Soporte;

namespace HotelSystem.Tests;

/// <summary>Tours: reserva (HU-09) y coordinacion de cupos y recursos (HU-10, RN-03, RN-05, RN-11).</summary>
public class ToursTests : IDisposable
{
    private readonly EntornoPruebas _entorno = new();

    public void Dispose() => _entorno.Dispose();

    private TourResumenDto Tour(string nombre) =>
        _entorno.En<ITourService, IReadOnlyList<TourResumenDto>>(t => t.Listar()).Single(t => t.Nombre == nombre);

    private SolicitudReservaTour Solicitud(string tour, int personas) =>
        new(_entorno.IdPersona("valentina.lagos"), Tour(tour).Id, personas, true, TipoPago.Tarjeta, "Visa **** 4242");

    [Fact]
    public void ReservarTour_BloqueaCuposYSeCargaALaCuenta()
    {
        var antes = Tour("City Tour Costero").CuposReservados;

        var reserva = _entorno.En<ITourService, ReservaTourDto>(t => t.ReservarTour(Solicitud("City Tour Costero", 2)));

        Assert.Equal(110000m, reserva.ValorTotalTour);
        Assert.Equal(antes + 2, Tour("City Tour Costero").CuposReservados);

        // RN-07: el tour aparece en la cuenta unica de la estancia (hab. 408).
        var cuenta = _entorno.En<ICuentaService, PrefacturaDto?>(c => c.ConsultarPorNumero("408"))!;
        Assert.Equal(260000m, cuenta.TotalTours);
    }

    [Fact]
    public void RN11_TourLleno_NoAceptaReservas()
    {
        var lancha = Tour("Paseo en Lancha");
        Assert.True(lancha.EstaLleno);

        var error = Assert.Throws<ReglaNegocioException>(() =>
            _entorno.En<ITourService, ReservaTourDto>(t => t.ReservarTour(Solicitud("Paseo en Lancha", 1))));
        Assert.Equal("RN-11", error.Regla);
    }

    [Fact]
    public void RN11_NoSePuedenBloquearMasCuposQueElMaximo()
    {
        var playa = Tour("Playa Volcanica");

        var error = Assert.Throws<ReglaNegocioException>(() =>
            _entorno.En<ITourService, TourResumenDto>(t => t.BloquearCupos(playa.Id, playa.CuposDisponibles + 1, _entorno.IdPersona("ltorres"))));
        Assert.Equal("RN-11", error.Regla);
    }

    [Fact]
    public void RN05_NoSeLiberanCuposDeReservasDeHuespedes()
    {
        // La lancha tiene 10 cupos de huespedes y 2 bloqueados a mano por el coordinador.
        var lancha = Tour("Paseo en Lancha");
        var coordinador = _entorno.IdPersona("ltorres");

        var error = Assert.Throws<ReglaNegocioException>(() =>
            _entorno.En<ITourService, TourResumenDto>(t => t.LiberarCupos(lancha.Id, 3, coordinador)));
        Assert.Equal("RN-05", error.Regla);

        var liberado = _entorno.En<ITourService, TourResumenDto>(t => t.LiberarCupos(lancha.Id, 2, coordinador));
        Assert.Equal(2, liberado.CuposDisponibles);
    }

    [Fact]
    public void RN03_GuiaOVehiculoEnOtroTourDelMismoHorario_SeRechaza()
    {
        // "Reserva Natural" (7-11 h) se cruza con "Playa Volcanica" (8-12 h), que ya tiene a Alberto Cruz y la Van #001.
        var natural = Tour("Reserva Natural");
        var recursos = _entorno.En<ITourService, RecursosDto>(t => t.ListarRecursos(natural.Id));
        var alberto = recursos.Guias.Single(g => g.Nombre == "Alberto Cruz");
        var carmen = recursos.Guias.Single(g => g.Nombre == "Carmen Vega");
        var van1 = recursos.Vehiculos.Single(v => v.Nombre == "Van #001");
        var bus = recursos.Vehiculos.Single(v => v.Nombre == "Bus #003");
        var coordinador = _entorno.IdPersona("ltorres");

        Assert.True(alberto.Ocupado);
        Assert.False(carmen.Ocupado);

        var porGuia = Assert.Throws<ReglaNegocioException>(() =>
            _entorno.En<ITourService, TourResumenDto>(t => t.AsignarRecursos(natural.Id, alberto.Id, bus.Id, coordinador)));
        Assert.Equal("RN-03", porGuia.Regla);

        var porVehiculo = Assert.Throws<ReglaNegocioException>(() =>
            _entorno.En<ITourService, TourResumenDto>(t => t.AsignarRecursos(natural.Id, carmen.Id, van1.Id, coordinador)));
        Assert.Equal("RN-03", porVehiculo.Regla);

        var asignado = _entorno.En<ITourService, TourResumenDto>(t => t.AsignarRecursos(natural.Id, carmen.Id, bus.Id, coordinador));
        Assert.Equal("Carmen Vega", asignado.NombreGuia);
        Assert.Equal("Bus #003", asignado.NombreVehiculo);
    }

    [Fact]
    public void LiberarRecursos_DejaElTourSinGuiaNiVehiculo()
    {
        var playa = Tour("Playa Volcanica");

        var liberados = _entorno.En<ITourService, RecursosLiberadosDto>(t => t.LiberarRecursos(playa.Id, _entorno.IdPersona("ltorres")));

        Assert.Equal("Alberto Cruz", liberados.NombreGuia);
        Assert.Null(Tour("Playa Volcanica").GuiaId);
    }
}
