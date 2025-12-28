public enum ToastType
{
    Info,
    Success,
    Error
}

public class ToastData
{
    public string Message { get; }
    public ToastType Type { get; }
    public System.Action OnDismiss { get; }

    public ToastData(string message, ToastType type = ToastType.Info, System.Action onDismiss = null)
    {
        Message = message;
        Type = type;
        OnDismiss = onDismiss;
    }
}