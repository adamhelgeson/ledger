using System.Diagnostics.CodeAnalysis;

namespace Ledger.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string[] Errors { get; init; } = [];

    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
        Justification = "Factory method pattern is intentional for ApiResponse<T>.")]
    public static ApiResponse<T> Ok(T data) =>
        new() { Success = true, Data = data };

    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
        Justification = "Factory method pattern is intentional for ApiResponse<T>.")]
    public static ApiResponse<T> Fail(params string[] errors) =>
        new() { Success = false, Errors = errors };
}

public class ApiResponse
{
    public bool Success { get; init; }
    public string[] Errors { get; init; } = [];

    public static ApiResponse Ok() => new() { Success = true };
    public static ApiResponse Fail(params string[] errors) => new() { Success = false, Errors = errors };
}
