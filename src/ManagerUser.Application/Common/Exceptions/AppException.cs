using System.Net;

namespace ManagerUser.Application.Common.Exceptions;
public sealed class AppException : Exception
{
    public AppException(
        string code,
        string message,
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public string Code { get; }
    public HttpStatusCode StatusCode { get; }
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
