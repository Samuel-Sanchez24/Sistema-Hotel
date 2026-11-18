using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;
using HotelSystem.Domain.Interfaces;

namespace HotelSystem.Tests;

/// <summary>Pruebas del dominio puro, sin base de datos: patron State, Observer e ICalculable.</summary>
public class DominioTests
{
    private static readonly DateTime Ahora = new(2026, 9, 9, 12, 0, 0);

    private static Habitacion NuevaHabitacion() => new("999", TipoHabitacion.Doble, 9, 3, 150000m, Ahora);

    private static Huesped NuevoHuesped() =>
        new("Huesped Prueba", "prueba@email.com", "prueba", "hash", new DateOnly(1990, 1, 1), Ahora);

    /* ─── Patron State (RN-02) ─── */

    [Fact]
    public void Habitacion_RecorreElCicloPermitido()
    {
        var habitacion = NuevaHabitacion();
        Assert.Equal(EstadoHabitacion.Disponible, habitacion.Estado);

        habitacion.Ocupar(Ahora);
        Assert.Equal(EstadoHabitacion.Ocupada, habitacion.Estado);

        habitacion.Liberar(Ahora);
        Assert.Equal(EstadoHabitacion.EnLimpieza, habitacion.Estado);

        habitacion.MarcarDisponible(Ahora);
        Assert.Equal(EstadoHabitacion.Disponible, habitacion.Estado);
    }

    [Fact]
    public void Habitacion_Ocupada_NoPuedePasarDirectoADisponible()
    {
        var habitacion = NuevaHabitacion();
        habitacion.Ocupar(Ahora);

        var error = Assert.Throws<ReglaNegocioException>(() => habitacion.MarcarDisponible(Ahora));
        Assert.Equal("RN-02", error.Regla);
        Assert.Equal(EstadoHabitacion.Ocupada, habitacion.Estado);
    }

    [Fact]
    public void Habitacion_EnMantenimiento_NoSePuedeOcupar()
    {
        var habitacion = NuevaHabitacion();
        habitacion.EnviarAMantenimiento("Fuga en el lavamanos", Ahora);

        var error = Assert.Throws<ReglaNegocioException>(() => habitacion.Ocupar(Ahora));
        Assert.Equal("RN-02", error.Regla);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Habitacion_MantenimientoSinNota_SeRechaza(string? nota)
    {
        var habitacion = NuevaHabitacion();

        Assert.Throws<ReglaNegocioException>(() => habitacion.EnviarAMantenimiento(nota, Ahora));
        Assert.Equal(EstadoHabitacion.Disponible, habitacion.Estado);
    }

    /* ─── Patron Observer ─── */

    private sealed class ObservadorDePrueba : IObservadorHabitacion
    {
        public List<(string Numero, EstadoHabitacion Antes, EstadoHabitacion Despues)> Avisos { get; } = [];

        public void Actualizar(Habitacion habitacion, EstadoHabitacion estadoAnterior) =>
            Avisos.Add((habitacion.Numero, estadoAnterior, habitacion.Estado));
    }

    [Fact]
    public void Habitacion_NotificaCadaCambioASusObservadores()
    {
        var habitacion = NuevaHabitacion();
        var primero = new ObservadorDePrueba();
        var segundo = new ObservadorDePrueba();
        habitacion.Suscribir(primero);
        habitacion.Suscribir(segundo);

        habitacion.Ocupar(Ahora);
        habitacion.Liberar(Ahora);

        Assert.Equal(2, primero.Avisos.Count);
        Assert.Equal(("999", EstadoHabitacion.Disponible, EstadoHabitacion.Ocupada), primero.Avisos[0]);
        Assert.Equal(("999", EstadoHabitacion.Ocupada, EstadoHabitacion.EnLimpieza), primero.Avisos[1]);
        Assert.Equal(primero.Avisos, segundo.Avisos);
    }

    [Fact]
    public void Habitacion_TransicionRechazada_NoNotifica()
    {
        var habitacion = NuevaHabitacion();
        var observador = new ObservadorDePrueba();
        habitacion.Suscribir(observador);

        Assert.Throws<ReglaNegocioException>(() => habitacion.Liberar(Ahora));
        Assert.Empty(observador.Avisos);
    }

    /* ─── ICalculable / IPagable (RN-07) ─── */

    [Fact]
    public void Cuenta_SumaAlojamientoServiciosYTours()
    {
        var habitacion = NuevaHabitacion();
        var huesped = NuevoHuesped();
        var reserva = new ReservaHabitacion(huesped, habitacion, new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 13), 2, Ahora);
        var cuenta = new Cuenta(reserva, Ahora);

        var spa = new ServicioAdicional("Spa", "Bienestar", "Masajes", 80000m);
        cuenta.RegistrarConsumo(spa, 2, 80000m, Ahora, null);

        var tour = new Tour("Tour", "Destino", new DateOnly(2026, 9, 11), new TimeOnly(8, 0), new TimeOnly(12, 0), 10, 50000m);
        tour.BloquearCupos(2);
        var reservaTour = new ReservaTour(tour, huesped, cuenta, 2, TipoPago.Tarjeta, null, Ahora);

        // Cada parte se calcula con el mismo contrato ICalculable (sustitucion de Liskov).
        ICalculable[] partes = [reservaTour];
        Assert.Equal(100000m, partes.Sum(p => p.CalcularTotal()));

        Assert.Equal(3, reserva.Noches);
        Assert.Equal(450000m, cuenta.TotalAlojamiento);
        Assert.Equal(160000m, cuenta.TotalServicios);
        Assert.Equal(100000m, cuenta.TotalTours);
        Assert.Equal(710000m, ((ICalculable)cuenta).CalcularTotal());
    }

    [Fact]
    public void Cuenta_SoloSeCierraConElMontoExacto()
    {
        var reserva = new ReservaHabitacion(NuevoHuesped(), NuevaHabitacion(), new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 11), 1, Ahora);
        IPagable cuenta = new Cuenta(reserva, Ahora);

        Assert.Throws<ReglaNegocioException>(() => cuenta.RegistrarPago(1000m, TipoPago.Efectivo, null, Ahora));
        Assert.False(cuenta.EstaPagada);

        cuenta.RegistrarPago(150000m, TipoPago.Efectivo, null, Ahora);
        Assert.True(cuenta.EstaPagada);
    }

    [Fact]
    public void Reserva_CapacidadExcedida_SeRechaza()
    {
        var error = Assert.Throws<ReglaNegocioException>(() =>
            new ReservaHabitacion(NuevoHuesped(), NuevaHabitacion(), new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 11), 4, Ahora));
        Assert.Equal("RN-11", error.Regla);
    }

    [Fact]
    public void Reserva_LasFechasSeCruzanSoloSiCompartenNoches()
    {
        var reserva = new ReservaHabitacion(NuevoHuesped(), NuevaHabitacion(), new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 13), 1, Ahora);

        Assert.True(reserva.SeCruzaCon(new DateOnly(2026, 9, 12), new DateOnly(2026, 9, 15)));
        // El dia de salida de una reserva puede ser el dia de entrada de la siguiente (RN-01).
        Assert.False(reserva.SeCruzaCon(new DateOnly(2026, 9, 13), new DateOnly(2026, 9, 15)));
        Assert.False(reserva.SeCruzaCon(new DateOnly(2026, 9, 8), new DateOnly(2026, 9, 10)));
    }
}
