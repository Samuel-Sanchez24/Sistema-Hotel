namespace HotelSystem.Web.DTOs.Recepcion;

/// <summary>Formulario de registro de pago y cierre de cuenta.</summary>
public class BillingPaymentDto
{
    public int RoomId { get; set; }
    public string? PayMethod { get; set; }
    public string? PaidAmount { get; set; }
}
