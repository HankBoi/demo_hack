namespace TenderEvidenceChecker.Api.Services;

public sealed class AppException : Exception
{
    public AppException(string code, string userMessage, bool retryable, int statusCode = 400)
        : base(userMessage)
    {
        Code = code;
        UserMessage = userMessage;
        Retryable = retryable;
        StatusCode = statusCode;
    }

    public string Code { get; }
    public string UserMessage { get; }
    public bool Retryable { get; }
    public int StatusCode { get; }
}
