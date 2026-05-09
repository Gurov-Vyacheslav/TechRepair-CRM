namespace TechRepair_CRM.DTOs.Clients;

public record ClientListItemResponse(
    int ClientId,
    string FirstName,
    string LastName,
    string Phone,
    string? Email,
    DateTime RegistrationDate
);