namespace Synegit.Maui.DateTimePicker;

internal static class MainThreadHelper
{
    internal static void SafeBeginInvokeOnMainThread(Action action)
    {
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            if (Application.Current !=  null)
            {
                Application.Current.Dispatcher.Dispatch(action);
            }
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(action);
        }
    }

    internal static async Task SafeInvokeOnMainThreadAsync(Action action)
    {
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            if (Application.Current != null)
            {
                await Application.Current.Dispatcher.DispatchAsync(action);
            }
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(action);
        }
    }

    internal static async Task<T> SafeInvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask)
    {
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {

            if (Application.Current != null)
            {
                return await Application.Current.Dispatcher.DispatchAsync(funcTask);
            }
            throw new ArgumentNullException(nameof(funcTask));
        }
        else
        {
            return await MainThread.InvokeOnMainThreadAsync(funcTask);
        }
    }
}
