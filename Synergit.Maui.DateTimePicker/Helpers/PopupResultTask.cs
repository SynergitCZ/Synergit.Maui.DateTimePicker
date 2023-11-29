namespace Synegit.Maui.DateTimePicker;

internal class PopupResultTask<T>
{
    internal Task<T> Result { get; }
    private readonly TaskCompletionSource<T> tcs = new();

    internal PopupResultTask()
    {
        Result = tcs.Task;
    }

    internal void SetResult(T result)
    {
        tcs.SetResult(result);
    }

    internal void SetException(Exception exception)
    {
        tcs.SetException(exception);
    }
}
