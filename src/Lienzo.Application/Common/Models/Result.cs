namespace Lienzo.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(T value, bool isSuccess, string? error, string? errorCode)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result<T> Success(T value) => new(value, true, null, null);
    public static Result<T> Failure(string error, string? errorCode = null) => new(default!, false, error, errorCode);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(string error) => Failure(error);
}

public class PaginatedResult<T> : Result<List<T>>
{
    public int TotalCount { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalPages { get; }

    private PaginatedResult(List<T> items, int totalCount, int page, int pageSize, int totalPages, bool isSuccess, string? error, string? errorCode)
        : base(items, isSuccess, error, errorCode)
    {
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
        TotalPages = totalPages;
    }

    public static PaginatedResult<T> Success(List<T> items, int totalCount, int page, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResult<T>(items, totalCount, page, pageSize, totalPages, true, null, null);
    }

    public static new PaginatedResult<T> Failure(string error, string? errorCode = null)
        => new([], 0, 0, 0, 0, false, error, errorCode);
}
