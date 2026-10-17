namespace HotelSystem.Web.DTOs.Huesped;

public class BookTourDto
{
    public int TourId { get; set; }
    public int Persons { get; set; } = 1;
    public string? PayMethod { get; set; }
    public bool AgeConfirmed { get; set; }
}
