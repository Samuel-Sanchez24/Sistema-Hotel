namespace HotelSystem.Web.DTOs.Account;

public record LoginResultDto(bool Success, string? Error, string DisplayName = "");
