namespace TechRepair_CRM.DTOs;

public record ListResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount);