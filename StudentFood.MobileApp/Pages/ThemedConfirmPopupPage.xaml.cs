namespace StudentFood.MobileApp.Pages;

public partial class ThemedConfirmPopupPage : ContentPage
{
    private readonly TaskCompletionSource<bool> _result = new();

    public string TitleText { get; }
    public string MessageText { get; }
    public string CancelText { get; }
    public string ConfirmText { get; }

    public ThemedConfirmPopupPage(
        string titleText,
        string messageText,
        string cancelText = "Hủy",
        string confirmText = "Đồng ý")
    {
        InitializeComponent();
        TitleText = titleText;
        MessageText = messageText;
        CancelText = cancelText;
        ConfirmText = confirmText;
        BindingContext = this;
    }

    /// <summary>
    /// Shows the confirmation dialog and returns true if the user tapped Confirm, false if they tapped Cancel.
    /// </summary>
    public static async Task<bool> ShowAsync(
        string titleText,
        string messageText,
        string cancelText = "Hủy",
        string confirmText = "Đồng ý")
    {
        var popup = new ThemedConfirmPopupPage(titleText, messageText, cancelText, confirmText);
        await Shell.Current.Navigation.PushModalAsync(popup, animated: false);
        var result = await popup._result.Task;
        // Tắt modal sau khi task hoàn tất, tránh xung đột navigation
        await Shell.Current.Navigation.PopModalAsync(animated: false);
        return result;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        _result.TrySetResult(false);
    }

    private void OnConfirmClicked(object sender, EventArgs e)
    {
        _result.TrySetResult(true);
    }
}
