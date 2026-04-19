namespace Shared.Models;

public class ReturnObject<T>
{
    public int Result { get; set; }
    public string? ResultMessage { get; set; }
    public T? ResultObject { get; set; }
    public object? ResultObject2 { get; set; }
    public ExceptionInfo? Exception { get; set; }
    public List<string>? Errors { get; set; }
    public List<object>? ErrorObjects { get; set; }

    public bool IsSuccess => Result == 1;

    public static ReturnObject<T> Success(T obj, string message = "OK")
        => new()
        {
            Result        = 1,
            ResultMessage = message,
            ResultObject  = obj
        };

    public static ReturnObject<T> SendResponse(string message = "WaitResponse", object obj = null!)
        => new()
        {
            Result        = 2,
            ResultMessage = message,
            ResultObject2 = obj
        };

    public static ReturnObject<T> Fail(string message, object resultObject2 = null!, List<string>? errors = null, List<object>? errorObjects = null, Exception? exception = null)
        => new()
        {
            Result        = 0,
            ResultMessage = message,
            Errors        = errors,
            ErrorObjects  = errorObjects,
            Exception     = exception != null ? ExceptionInfo.From(exception) : null
        };

    public static ReturnObject<T> Error(string message, Exception ex, List<string>? errors = null, List<object>? errorObjects = null)
        => new()
        {
            Result        = -1,
            ResultMessage = message,
            Errors        = [ex.Message],
            ErrorObjects  = errorObjects,
            Exception     = ExceptionInfo.From(ex)
        };
}

public class ExceptionInfo
{
    public string? Message { get; set; }
    public string? Type { get; set; }
    public string? StackTrace { get; set; }
    public ExceptionInfo? InnerException { get; set; }

    public static ExceptionInfo From(Exception ex) => new()
    {
        Message        = ex.Message,
        Type           = ex.GetType().Name,
        StackTrace     = ex.StackTrace,
        InnerException = ex.InnerException != null ? From(ex.InnerException) : null
    };
}