namespace CMS.Main.Services;

public class ConfirmationRequest
{
    public string Title { get; set; } = "Confirm Action";
    public string Message { get; set; } = "Are you sure you want to proceed?";
    public string ConfirmText { get; set; } = "Confirm";
    public string CancelText { get; set; } = "Cancel";
}

public class ConfirmationService
{
    private TaskCompletionSource<bool>? tcs;

    public ConfirmationRequest? CurrentRequest { get; private set; }
    public bool IsOpen { get; private set; }
    public event Func<Task>? OnStateChanged;

    public async Task<bool> ShowAsync(string title, string message, string confirmText = "Confirm",
        string cancelText = "Cancel")
    {
        if (IsOpen)
            throw new InvalidOperationException("A confirmation is already in progress.");

        tcs = new TaskCompletionSource<bool>();
        CurrentRequest = new ConfirmationRequest
        {
            Title = title,
            Message = message,
            ConfirmText = confirmText,
            CancelText = cancelText
        };
        IsOpen = true;
        if (OnStateChanged is not null)
            await OnStateChanged.Invoke();

        var result = await tcs.Task;

        IsOpen = false;
        CurrentRequest = null;
        if (OnStateChanged is not null)
            await OnStateChanged.Invoke();

        return result;
    }

    public void Confirm()
    {
        tcs?.TrySetResult(true);
    }

    public void Cancel()
    {
        tcs?.TrySetResult(false);
    }
}