namespace Shared.Domain.DTOs;

public class Filter
{
    public uint PageNumber { get; set; } = 0;
    public uint PageSize { get; set; } = 20;
    public List<FilterParam> Params { get; set; } = [];
    public List<SortParam> Sort { get; set; } = [];
}

public class FilterParam(string key, string value, FilterOption option)
{
    public string Key { get; set; } = key;
    public string Value { get; set; } = value;
    public FilterOption Option { get; set; } = option;
}

public enum FilterOption
{
    StartsWith = 1,
    EndsWith,
    Contains,
    DoesNotContain,
    IsEmpty,
    IsNotEmpty,
    IsGreaterThan,
    IsGreaterThanOrEqualTo,
    IsLessThan,
    IsLessThanOrEqualTo,
    IsEqualTo,
    IsNotEqualTo
}

public enum SortOrder
{
    Asc,
    Desc,
}

public class SortParam(string field, SortOrder order = SortOrder.Asc)
{
    public string Field { get; private set; } = field;
    public SortOrder SortOrder { get; private set; } = order;
}
