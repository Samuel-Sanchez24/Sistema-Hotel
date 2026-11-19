using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Comun;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Application.CierreCuenta;

/// <summary>
/// Patrón Strategy (contexto). Consulta los datos del rango de fechas y, al exportar, delega
/// el formato del archivo en la estrategia <see cref="IExportadorReporte"/> que corresponda.
/// Esta clase no sabe cómo se construye un PDF ni un Excel.
/// </summary>
public sealed class GeneradorReportes(
    IHabitacionRepository habitaciones,
    IReservaHabitacionRepository reservas,
    ITourRepository tours,
    ICuentaRepository cuentas,
    IPersonaRepository personas,
    IRegistroOperacionRepository operaciones,
    IEnumerable<IExportadorReporte> exportadores,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    Bitacora bitacora) : IReporteService
{
    public ReporteGestion Generar(DateOnly desde, DateOnly hasta)
    {
        if (hasta < desde)
            throw new ReglaNegocioException("La fecha final no puede ser anterior a la fecha inicial.");

        var dias = hasta.DayNumber - desde.DayNumber + 1;
        var inicio = desde.ToDateTime(TimeOnly.MinValue);
        var finExclusivo = hasta.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var todas = habitaciones.Listar();
        var estancias = reservas.ListarNoCanceladasEnRango(desde, hasta);
        var reservasTour = tours.ListarReservasVigentesEnRango(desde, hasta);
        var consumos = cuentas.ListarConsumosEntre(inicio, finExclusivo);
        var canceladas = reservas.ListarCanceladasEntre(inicio, finExclusivo);
        var registros = operaciones.ListarEntre(inicio, finExclusivo);

        // Ocupación por tipo, en noches-habitación dentro del periodo.
        var ocupacion = Enum.GetValues<TipoHabitacion>()
            .Select(tipo =>
            {
                var cantidad = todas.Count(h => h.Tipo == tipo);
                var noches = estancias.Where(r => r.Habitacion.Tipo == tipo).Sum(r => NochesEnRango(r, desde, hasta));
                return new FilaOcupacion(tipo, cantidad, cantidad * dias, noches);
            })
            .ToList();

        // Ingresos por categoría. El alojamiento se cuenta por noche dormida dentro del periodo.
        var ingresos = new List<FilaIngreso>
        {
            new("Alojamiento", estancias.Sum(r => NochesEnRango(r, desde, hasta) * r.PrecioNoche)),
            new("Tours", reservasTour.Sum(r => r.CalcularTotal())),
            new("Servicios adicionales", consumos.Sum(c => c.Subtotal)),
        };
        var cargosCancelacion = canceladas.Sum(r => r.CargoCancelacion);
        if (cargosCancelacion > 0)
            ingresos.Add(new FilaIngreso("Cargos por cancelacion (RN-09)", cargosCancelacion));

        var servicios = consumos
            .GroupBy(c => c.Servicio.Nombre)
            .Select(g => new FilaServicio(g.Key, g.Count(), g.Sum(c => c.Subtotal)))
            .OrderByDescending(f => f.Total)
            .ToList();

        // Desempeño del personal: operaciones registradas en la bitácora durante el periodo.
        var operacionesPorEmpleado = registros
            .GroupBy(o => o.EmpleadoId)
            .ToDictionary(g => g.Key, g => g.Count());
        var personal = personas.ListarEmpleados()
            .Where(e => e.Rol != RolEmpleado.Administrador)
            .Select(e => new FilaPersonal(e.Nombre, e.Rol, e.Activo, operacionesPorEmpleado.GetValueOrDefault(e.Id)))
            .OrderByDescending(f => f.Operaciones)
            .ThenBy(f => f.Nombre)
            .ToList();

        var sinDatos = estancias.Count == 0
            && reservasTour.Count == 0
            && consumos.Count == 0
            && canceladas.Count == 0
            && registros.Count == 0;

        return new ReporteGestion(
            desde,
            hasta,
            reloj.Ahora,
            sinDatos,
            todas.Count,
            todas.Count(h => h.Estado == EstadoHabitacion.Ocupada),
            todas.Count(h => h.Estado == EstadoHabitacion.Disponible),
            ocupacion,
            ingresos,
            servicios,
            personal);
    }

    public ArchivoExportado Exportar(DateOnly desde, DateOnly hasta, FormatoReporte formato, int empleadoId)
    {
        // Strategy: se elige la estrategia según el formato pedido y se le delega el trabajo.
        var exportador = exportadores.FirstOrDefault(e => e.FormatoSoportado == formato)
            ?? throw new ReglaNegocioException($"No hay un exportador disponible para el formato {formato}.");

        var reporte = Generar(desde, hasta);
        var archivo = exportador.Exportar(reporte);

        bitacora.Registrar(empleadoId, TipoOperacion.ExportacionReporte, $"Reporte en {formato}: {reporte.Periodo}");
        unidadDeTrabajo.GuardarCambios();

        return archivo;
    }

    /// <summary>Noches de la estancia que caen dentro del periodo [desde, hasta].</summary>
    private static int NochesEnRango(ReservaHabitacion reserva, DateOnly desde, DateOnly hasta)
    {
        var inicio = reserva.FechaEntrada > desde ? reserva.FechaEntrada : desde;
        var limite = hasta.AddDays(1);
        var finExclusivo = reserva.FechaSalida < limite ? reserva.FechaSalida : limite;

        return Math.Max(0, finExclusivo.DayNumber - inicio.DayNumber);
    }
}
