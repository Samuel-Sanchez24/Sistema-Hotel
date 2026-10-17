namespace HotelSystem.Web.DTOs.Common;

/// <summary>Resultado de una operacion de negocio: exito o mensaje de error para la vista.</summary>
public record OperationResultDto(bool Success, string Message)
{
    public static OperationResultDto Ok(string message = "") => new(true, message);
    public static OperationResultDto Fail(string message) => new(false, message);
}
