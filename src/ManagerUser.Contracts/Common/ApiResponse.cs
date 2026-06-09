namespace ManagerUser.Contracts.Common;
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
    public T? Data { get; init; }
    public IReadOnlyList<ApiError> Errors { get; init; } = Array.Empty<ApiError>();
    public string TraceId { get; init; } = "";

    public static ApiResponse<T> Ok(T data, string message, string traceId)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Code = "SUCCESS",
            Message = message,
            Data = data,
            TraceId = traceId
        };
    }

    public static ApiResponse<T> Fail(
        string code,
        string message,
        IReadOnlyList<ApiError> errors,
        string traceId)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Code = code,
            Message = message,
            Errors = errors,
            TraceId = traceId
        };
    }
}
