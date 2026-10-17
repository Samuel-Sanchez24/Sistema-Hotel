namespace HotelSystem.Web.DTOs.Common;

public class TourDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Destination { get; set; } = "";
    public string Date { get; set; } = "";
    public string Time { get; set; } = "";
    public int Capacity { get; set; }
    public int Booked { get; set; }
    public string Guide { get; set; } = "";
    public string Vehicle { get; set; } = "";
    public decimal Price { get; set; }

    public bool IsFull => Booked >= Capacity;
    public int FreeSpots => Capacity - Booked;
    public int Percent => (int)Math.Round(Booked * 100.0 / Capacity);
}
