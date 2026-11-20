using HotelSystem.Application.CierreCuenta;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Application.ServiciosAdicionales;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;
using HotelSystem.Tests.Soporte;

namespace HotelSystem.Tests;

/// <summary>Patron Facade (CuentaFacade): prefactura y cierre de cuenta (HU-06, HU-12, RN-06, RN-07).</summary>
public class CuentaTests : IDisposable
{
    private readonly EntornoPruebas _entorno = new();

    public void Dispose() => _entorno.Dispose();

    private PrefacturaDto Prefactura(string habitacion) =>
        _entorno.En<ICuentaService, PrefacturaDto?>(c => c.ConsultarPorNumero(habitacion))
        ?? throw new InvalidOperationException($"La {habitacion} no tiene cuenta abierta");

    [Fact]
    public void Prefactura_ConsolidaAlojamientoServiciosYTours()
    {
        // Valentina, hab. 408 (doble, $150.000): 4 noches, bar + transporte, y el tour de senderismo para 2.
        var cuenta = Prefactura("408");

        Assert.Equal(4, cuenta.Noches);
        Assert.Equal(600000m, cuenta.ValorTotalAlojamiento);
        Assert.Equal(95000m, cuenta.TotalServicios);
        Assert.Equal(150000m, cuenta.TotalTours);
        Assert.Equal(845000m, cuenta.MontoConsolidado);
        Assert.False(cuenta.CuentaCerrada);
    }

    [Fact]
    public void Prefactura_DelHuesped_EsLaDeSuEstanciaEnCurso()
    {
        var cuenta = _entorno.En<ICuentaService, PrefacturaDto?>(c => c.ConsultarDeHuesped(_entorno.IdPersona("valentina.lagos")));

        Assert.NotNull(cuenta);
        Assert.Equal("408", cuenta.NumeroHabitacion);
    }

    [Fact]
    public void GenerarPrefactura_QuedaNumerada()
    {
        var generada = _entorno.En<ICuentaService, PrefacturaDto>(c =>
            c.GenerarPrefactura(_entorno.IdHabitacion("408"), _entorno.IdPersona("mlopez")));

        Assert.NotNull(generada.IdPrefactura);
        Assert.Equal(845000m, generada.MontoConsolidado);
    }

    [Fact]
    public void CerrarCuenta_ConMontoDiferente_SeRechazaYNoCambiaNada()
    {
        var solicitud = new SolicitudCierreCuenta(_entorno.IdHabitacion("102"), 500000m, TipoPago.Efectivo, null, _entorno.IdPersona("mlopez"));

        Assert.Throws<ReglaNegocioException>(() => _entorno.En<ICuentaService, PrefacturaDto>(c => c.CerrarCuenta(solicitud)));

        Assert.False(Prefactura("102").CuentaCerrada);
        Assert.Equal(EstadoHabitacion.Ocupada, EstadoDe("102"));
    }

    [Fact]
    public void CerrarCuenta_HaceElCheckOutCompletoEnUnaOperacion()
    {
        // Carlos Mejia, hab. 102 (simple, $120.000): 4 noches + restaurante + lavanderia.
        var total = Prefactura("102").MontoConsolidado;
        Assert.Equal(567000m, total);

        var cierre = _entorno.En<ICuentaService, PrefacturaDto>(c => c.CerrarCuenta(
            new SolicitudCierreCuenta(_entorno.IdHabitacion("102"), total, TipoPago.Efectivo, "Efectivo", _entorno.IdPersona("mlopez"))));

        Assert.True(cierre.CuentaCerrada);
        Assert.NotNull(cierre.IdPrefactura);
        Assert.Equal(EstadoHabitacion.EnLimpieza, EstadoDe("102"));

        // RN-07: con la cuenta cerrada ya no se pueden cargar consumos a esa habitacion.
        var error = Assert.Throws<ReglaNegocioException>(() => RegistrarConsumo("102", "Bar", 20000m, _entorno.Hoy));
        Assert.Equal("RN-07", error.Regla);
    }

    [Fact]
    public void RN06_ConsumoDeOtroDia_SeRechaza()
    {
        var error = Assert.Throws<ReglaNegocioException>(() => RegistrarConsumo("408", "Spa", 80000m, _entorno.Hoy.AddDays(-1)));
        Assert.Equal("RN-06", error.Regla);
    }

    [Fact]
    public void ConsumoDeHoy_SeCargaALaCuentaDeLaEstancia()
    {
        var consumo = RegistrarConsumo("408", "Spa", 80000m, _entorno.Hoy);

        Assert.Equal(80000m, consumo.Subtotal);
        Assert.Equal(175000m, Prefactura("408").TotalServicios);
    }

    [Fact]
    public void RN07_HabitacionSinEstancia_NoAceptaConsumos()
    {
        var error = Assert.Throws<ReglaNegocioException>(() => RegistrarConsumo("101", "Bar", 20000m, _entorno.Hoy));
        Assert.Equal("RN-07", error.Regla);
    }

    private ConsumoDto RegistrarConsumo(string habitacion, string servicio, decimal precio, DateOnly fecha) =>
        _entorno.En<IServicioAdicionalService, ConsumoDto>(s => s.RegistrarConsumo(
            new RegistroConsumo(habitacion, null, servicio, 1, precio, fecha, _entorno.IdPersona("pgomez"))));

    private EstadoHabitacion EstadoDe(string numero) =>
        _entorno.En<IHabitacionService, HabitacionDto?>(h => h.Obtener(_entorno.IdHabitacion(numero)))!.Estado;
}
