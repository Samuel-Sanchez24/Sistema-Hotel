namespace HotelSystem.Web.DTOs.Common;

/// <summary>Tour cargado a la cuenta de una estancia (RN-07): aparece en la prefactura.</summary>
public record BillTourDto(string Name, string Date, int Persons, decimal Total);
