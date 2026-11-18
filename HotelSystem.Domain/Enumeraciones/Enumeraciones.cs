namespace HotelSystem.Domain.Enumeraciones;

// Enumeraciones del diagrama de clases. En la base de datos se guardan como texto
// (ver HotelSystem.Persistence/Configuraciones) para que las tablas se puedan leer a simple vista.

/// <summary>Tipos de habitación del hotel. Pisos 1-2 simples, 3-4 dobles, 5-6 suites.</summary>
public enum TipoHabitacion
{
    Simple,
    Doble,
    Suite,
}

/// <summary>
/// Estados de una habitación (RN-02). Es el valor que se persiste; el comportamiento
/// de cada estado vive en las clases del patrón State (carpeta Estados).
/// </summary>
public enum EstadoHabitacion
{
    Disponible,
    Ocupada,
    EnLimpieza,
    Mantenimiento,
}

/// <summary>Ciclo de vida de una reserva de habitación o de tour.</summary>
public enum EstadoReserva
{
    Solicitada,
    Confirmada,
    EnCurso,
    Finalizada,
    Cancelada,
}

/// <summary>Estados de la cuenta única de una estancia (RN-07).</summary>
public enum EstadoCuenta
{
    Abierta,
    Cerrada,
    Anulada,
}

/// <summary>Estados de una solicitud de servicio adicional hecha por el huésped (HU-07).</summary>
public enum EstadoSolicitud
{
    Pendiente,
    EnProceso,
    Completada,
}

/// <summary>Rol del personal. Cada empleado tiene exactamente uno (RN-08).</summary>
public enum RolEmpleado
{
    Recepcion,
    Housekeeping,
    CoordinadorTours,
    Servicios,
    Administrador,
}

/// <summary>Medio con el que se respalda una reserva (RN-04) o se paga la cuenta (HU-06).</summary>
public enum TipoPago
{
    Efectivo,
    Tarjeta,
    Transferencia,
    Deposito,
}

/// <summary>Tipo de operación registrada en la bitácora. Alimenta el reporte de personal (HU-13).</summary>
public enum TipoOperacion
{
    CheckIn,
    CheckOut,
    Prefactura,
    CambioEstadoHabitacion,
    Incidencia,
    RegistroConsumo,
    AtencionSolicitud,
    AjusteCupos,
    AsignacionRecursos,
    GestionUsuarios,
    ExportacionReporte,
}
