using TechRepair_CRM.DTOs.Orders;

namespace TechRepair_CRM.DTOs;

public record ListResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount)
{
    public ListResponse(): this(Array.Empty<T>(), 0)
    {}
}