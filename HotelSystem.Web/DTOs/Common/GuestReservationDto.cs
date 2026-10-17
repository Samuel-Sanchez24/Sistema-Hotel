namespace HotelSystem.Web.DTOs.Common;

public class GuestReservationDto
{
    public string Id { get; set; } = "";
    public ReservationKind Kind { get; set; }
    public ReservationStatus Status { get; set; }

    // Habitacion
    public RoomType? RoomType { get; set; }
    public string? RoomNumber { get; set; }
    public string? CheckIn { get; set; }
    public string? CheckOut { get; set; }
    public int? Nights { get; set; }
    public decimal? PricePerNight { get; set; }

    // Tour
    public string? TourName { get; set; }
    public string? TourDate { get; set; }
    public int? Persons { get; set; }
    public decimal? TourPrice { get; set; }

    public bool Cancellable { get; set; }
    public bool LateCancel { get; set; }
    public string? CancelReason { get; set; }
}
