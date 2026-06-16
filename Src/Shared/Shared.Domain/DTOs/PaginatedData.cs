namespace Shared.Domain.DTOs;

public class PaginatedData<T>(List<T> rows, uint totalCount = 0, uint pageNumber = 0, uint pageSize = 20)
{
    public uint PageNumber { get; private set; } = pageNumber;
    public uint PageSize { get; private set; } = pageSize;
    public uint TotalCount { get; private set; } = totalCount;
    public List<T> Rows { get; private set; } = rows;
}
