using TechRepair_CRM.DTOs.Devices;
using TechRepair_CRM.DTOs.Orders;

namespace TechRepair_CRM.DTOs.Clients;

public record ClientDetailsResponse(
    int ClientId,
    string FirstName,
    string LastName,
    string Phone,
    string? Email,
    string? Address,
    DateTime RegistrationDate,
    string? Notes,
    IReadOnlyList<ClientDeviceResponse> Devices,
    IReadOnlyList<OrderListItemResponse> Orders
);