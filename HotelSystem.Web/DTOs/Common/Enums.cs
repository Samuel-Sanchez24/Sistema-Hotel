namespace HotelSystem.Web.DTOs.Common;

public enum Role { Huesped, Recepcion, Housekeeping, Coordinador, Servicios, Admin }

public enum RoomStatus { Disponible, Ocupada, EnLimpieza, Mantenimiento }

public enum RoomType { Simple, Doble, Suite }

public enum ReservationStatus { Confirmada, EnCurso, Cancelada }

public enum ReservationKind { Habitacion, Tour }

public enum ServiceRequestStatus { Pendiente, EnProceso, Completado }

public enum ToastType { Success, Error }
