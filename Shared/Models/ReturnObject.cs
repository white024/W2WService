namespace Shared.Models;

public class ReturnObject<T>
{
    public int Result { get; set; }
    public string? ResultMessage { get; set; }
    public T? ResultObject { get; set; }
    public object? Exception { get; set; }
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

    public static ReturnObject<T> Fail(string message, List<string>? errors = null, List<object>? errorObjects = null, object? exception = null)
        => new()
        {
            Result        = 0,
            ResultMessage = message,
            Errors        = errors,
            ErrorObjects = errorObjects,
            Exception = exception
        };

    public static ReturnObject<T> Error(string message, Exception ex, List<string>? errors = null, List<object>? errorObjects = null)
        => new()
        {
            Result        = -1,
            ResultMessage = message,
            Errors        = [ex.Message],
            ErrorObjects = errorObjects,
            Exception = ex
        };
}
