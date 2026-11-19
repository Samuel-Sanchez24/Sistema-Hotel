using HotelSystem.Application.Comun;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Application.CierreCuenta;

/// <summary>Ocupación de un tipo de habitación en el periodo, medida en noches-habitación.</summary>
/// <param name="NochesDisponibles">Habitaciones del tipo por los días del periodo.</param>
/// <param name="NochesOcupadas">Noches reservadas u ocupadas dentro del periodo.</param>
public sealed record FilaOcupacion(TipoHabitacion Tipo, int Habitaciones, int NochesDisponibles, int NochesOcupadas)
{
    public int Porcentaje => NochesDisponibles == 0 ? 0 : (int)Math.Round(NochesOcupadas * 100.0 / NochesDisponibles);
}

public sealed record FilaIngreso(string Categoria, decimal Monto);

public sealed record FilaServicio(string Servicio, int Consumos, decimal Total);

public sealed record FilaPersonal(string Nombre, RolEmpleado Rol, bool Activo, int Operaciones);

/// <summary>Celda de una tabla del reporte. Si tiene <see cref="Valor"/> es numérica y se alinea a la derecha.</summary>
public sealed record CeldaReporte(string Texto, decimal? Valor = null)
{
    public bool EsNumero => Valor is not null;

    public static CeldaReporte De(string texto) => new(texto);

    public static CeldaReporte Numero(decimal valor, string texto) => new(texto, valor);

    public static CeldaReporte Entero(int valor) => new(valor.ToString(), valor);

    public static CeldaReporte Dinero(decimal valor) => new(Formato.Dinero(valor), valor);
}

/// <summary>
/// Tabla del reporte en forma genérica (título, encabezados y filas). Es lo que reciben los
/// exportadores: así ni el PDF ni el Excel necesitan saber qué significa cada reporte.
/// </summary>
public sealed record SeccionReporte(string Titulo, IReadOnlyList<string> Encabezados, IReadOnlyList<IReadOnlyList<CeldaReporte>> Filas);

/// <summary>
/// Reporte de gestión de un rango de fechas (HU-13, RF-13): ocupación, ingresos por
/// habitaciones, tours y servicios, y desempeño del personal.
/// </summary>
/// <param name="SinDatos">No hay ningún registro en el periodo (HU-13 Esc.2).</param>
public sealed record ReporteGestion(
    DateOnly Desde,
    DateOnly Hasta,
    DateTime GeneradoEn,
    bool SinDatos,
    int TotalHabitaciones,
    int HabitacionesOcupadasHoy,
    int HabitacionesDisponiblesHoy,
    IReadOnlyList<FilaOcupacion> Ocupacion,
    IReadOnlyList<FilaIngreso> Ingresos,
    IReadOnlyList<FilaServicio> Servicios,
    IReadOnlyList<FilaPersonal> Personal)
{
    public int NochesDisponibles => Ocupacion.Sum(o => o.NochesDisponibles);

    public int NochesOcupadas => Ocupacion.Sum(o => o.NochesOcupadas);

    /// <summary>Porcentaje de noches-habitación ocupadas en el periodo.</summary>
    public int TasaOcupacion => NochesDisponibles == 0 ? 0 : (int)Math.Round(NochesOcupadas * 100.0 / NochesDisponibles);

    public decimal TotalIngresos => Ingresos.Sum(i => i.Monto);

    public string Periodo => $"{Formato.FechaConAnio(Desde)} a {Formato.FechaConAnio(Hasta)}";

    /// <summary>Convierte el reporte en tablas genéricas para los exportadores (PDF, Excel).</summary>
    public IReadOnlyList<SeccionReporte> ComoSecciones()
    {
        var ocupacion = Ocupacion
            .Select(o => Fila(
                CeldaReporte.De(o.Tipo.ToString()),
                CeldaReporte.Entero(o.Habitaciones),
                CeldaReporte.Entero(o.NochesDisponibles),
                CeldaReporte.Entero(o.NochesOcupadas),
                CeldaReporte.Numero(o.Porcentaje, o.Porcentaje + "%")))
            .ToList();
        ocupacion.Add(Fila(
            CeldaReporte.De("Total"),
            CeldaReporte.Entero(TotalHabitaciones),
            CeldaReporte.Entero(NochesDisponibles),
            CeldaReporte.Entero(NochesOcupadas),
            CeldaReporte.Numero(TasaOcupacion, TasaOcupacion + "%")));

        var ingresos = Ingresos
            .Select(i => Fila(CeldaReporte.De(i.Categoria), CeldaReporte.Dinero(i.Monto)))
            .ToList();
        ingresos.Add(Fila(CeldaReporte.De("Total"), CeldaReporte.Dinero(TotalIngresos)));

        var servicios = Servicios
            .Select(s => Fila(CeldaReporte.De(s.Servicio), CeldaReporte.Entero(s.Consumos), CeldaReporte.Dinero(s.Total)))
            .ToList();

        var personal = Personal
            .Select(p => Fila(
                CeldaReporte.De(p.Nombre),
                CeldaReporte.De(p.Rol.ToString()),
                CeldaReporte.De(p.Activo ? "Activo" : "Inactivo"),
                CeldaReporte.Entero(p.Operaciones)))
            .ToList();

        return
        [
            new SeccionReporte("Ocupación", ["Tipo de habitación", "Habitaciones", "Noches disponibles", "Noches ocupadas", "Ocupación %"], ocupacion),
            new SeccionReporte("Ingresos", ["Categoría", "Monto"], ingresos),
            new SeccionReporte("Servicios adicionales", ["Servicio", "Consumos", "Ingresos"], servicios),
            new SeccionReporte("Desempeño del personal", ["Empleado", "Rol", "Estado", "Operaciones"], personal),
        ];
    }

    private static IReadOnlyList<CeldaReporte> Fila(params CeldaReporte[] celdas) => celdas;
}
