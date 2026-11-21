using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Domain.Entidades;

/// <summary>
/// Excursión con cupo limitado (HU-09, HU-10). Controla sus cupos (RN-05, RN-11) y sabe
/// si se cruza de horario con otro tour, que es la base de la RN-03.
/// </summary>
public class Tour
{
    private Tour()
    {
        Nombre = string.Empty;
        Destino = string.Empty;
    }

    public Tour(string nombre, string destino, DateOnly fecha, TimeOnly horarioInicio, TimeOnly horarioFin, int cupoMaximo, decimal precioPorPersona)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaNegocioException("El nombre del tour es obligatorio.");
        if (horarioFin <= horarioInicio) throw new ReglaNegocioException("El horario de fin debe ser posterior al de inicio.");
        if (cupoMaximo < 1) throw new ReglaNegocioException("El cupo maximo debe ser de al menos una persona.");
        if (precioPorPersona <= 0) throw new ReglaNegocioException("El precio por persona debe ser mayor que cero.");

        Nombre = nombre.Trim();
        Destino = destino.Trim();
        Fecha = fecha;
        HorarioInicio = horarioInicio;
        HorarioFin = horarioFin;
        CupoMaximo = cupoMaximo;
        PrecioPorPersona = precioPorPersona;
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; }

    public string Destino { get; private set; }

    public DateOnly Fecha { get; private set; }

    public TimeOnly HorarioInicio { get; private set; }

    public TimeOnly HorarioFin { get; private set; }

    public int CupoMaximo { get; private set; }

    /// <summary>Cupos bloqueados: por reservas de huéspedes y por ajustes del coordinador.</summary>
    public int CuposReservados { get; private set; }

    public decimal PrecioPorPersona { get; private set; }

    public int? GuiaId { get; private set; }

    public Guia? Guia { get; private set; }

    public int? VehiculoId { get; private set; }

    public Vehiculo? Vehiculo { get; private set; }

    /* ─── Valores derivados ─── */

    public int CuposDisponibles => CupoMaximo - CuposReservados;

    public bool EstaLleno => CuposDisponibles <= 0;

    public DateTime Inicio => Fecha.ToDateTime(HorarioInicio);

    /// <summary>RN-03: dos tours se cruzan si son el mismo día y sus horarios se solapan.</summary>
    public bool SeCruzaCon(Tour otro) =>
        Fecha == otro.Fecha && HorarioInicio < otro.HorarioFin && otro.HorarioInicio < HorarioFin;

    /* ─── Cupos (RN-05, RN-11) ─── */

    /// <summary>Bloquea cupos al confirmar una reserva. Nunca supera el cupo máximo.</summary>
    public void BloquearCupos(int cantidad)
    {
        if (cantidad < 1) throw new ReglaNegocioException("La cantidad de cupos debe ser de al menos uno.");
        if (cantidad > CuposDisponibles)
            throw new ReglaNegocioException("No hay cupos suficientes. Seleccione otra fecha o tour (RN-11 - HU-09 Esc.2).", "RN-11");
        CuposReservados += cantidad;
    }

    public void LiberarCupos(int cantidad)
    {
        if (cantidad < 1) throw new ReglaNegocioException("La cantidad de cupos debe ser de al menos uno.");
        if (cantidad > CuposReservados)
            throw new ReglaNegocioException("No se pueden liberar mas cupos de los que estan reservados.");
        CuposReservados -= cantidad;
    }

    /* ─── Recursos (HU-10) ─── */

    /// <summary>
    /// Asigna guía y vehículo. El cruce de horarios con otros tours (RN-03) lo valida
    /// el servicio de aplicación, que es quien conoce los demás tours.
    /// </summary>
    public void AsignarRecursos(Guia guia, Vehiculo vehiculo)
    {
        if (!vehiculo.EsAptoParaGrupo(CuposReservados))
            throw new ReglaNegocioException($"{vehiculo.Nombre} tiene capacidad para {vehiculo.CapacidadPasajeros} pasajeros y el tour ya tiene {CuposReservados} personas.");

        Guia = guia;
        GuiaId = guia.Id;
        Vehiculo = vehiculo;
        VehiculoId = vehiculo.Id;
    }

    public void LiberarRecursos()
    {
        Guia = null;
        GuiaId = null;
        Vehiculo = null;
        VehiculoId = null;
    }
}
