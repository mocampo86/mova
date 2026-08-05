using System.Text.Json.Serialization;

namespace Mova.Contracts.Common;

public sealed class PagedResult<T>
{
    [JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; init; } = [];

    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; init; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; init; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; init; }

    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalItems)
    {
        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 0
        };
    }
}
