using HotelSystem.Web.DTOs.Account;
using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.Services;

/// <summary>
/// Fuente de datos de la capa de presentacion. Hoy es una implementacion en memoria
/// con los datos del prototipo; mas adelante debe delegar en HotelSystem.Application.
/// </summary>
public interface IHotelDataService
{
    LoginResultDto ValidateLogin(LoginDto login);

    IReadOnlyList<RoomDto> GetRooms();
    RoomDto? GetRoom(int id);
    void SetRoomStatus(int roomId, RoomStatus status, string? maintenanceNote = null);
    IReadOnlyList<CheckInOutItemDto> GetTodayCheckIns();
    IReadOnlyList<CheckInOutItemDto> GetTodayCheckOuts();
    CheckInOutItemDto? RegisterCheckIn(string reservationId);
    void CloseAccount(int roomId);

    IReadOnlyList<TourDto> GetTours();
    TourDto? GetTour(int id);
    void AdjustTourQuota(int tourId, int delta);
    OperationResultDto AssignTourResources(int tourId, string guide, string vehicle);
    TourDto? ReleaseTourResources(int tourId);
    IReadOnlyList<string> GetGuides();
    IReadOnlyList<string> GetVehicles();
    OperationResultDto BookGuestTour(int tourId, int persons);
    IReadOnlyCollection<int> GetGuestBookedTourIds();

    IReadOnlyList<ConsumptionDto> GetConsumptions();
    ConsumptionDto AddConsumption(string roomNumber, string service, decimal amount);

    IReadOnlyList<ServiceCatalogItemDto> GetCatalog();
    IReadOnlyList<ServiceRequestDto> GetServiceRequests();
    ServiceRequestDto? AddServiceRequest(int serviceId, string roomNumber);

    IReadOnlyList<GuestReservationDto> GetGuestReservations();
    GuestReservationDto? CancelGuestReservation(string id);

    IReadOnlyList<StaffUserDto> GetStaff();
    StaffUserDto CreateStaff(string name, string email, Role role);
    StaffUserDto? ToggleStaffActive(int id);
    void UpdateStaffRole(int id, Role role);
}
