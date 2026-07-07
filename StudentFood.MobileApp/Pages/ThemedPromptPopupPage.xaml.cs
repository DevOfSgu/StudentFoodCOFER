namespace StudentFood.MobileApp.Pages;

public partial class ThemedPromptPopupPage : ContentPage
{
    private readonly TaskCompletionSource<string?> _result = new();

    public string TitleText { get; }
    public string MessageText { get; }
    public string PlaceholderText { get; }
    public string CancelText { get; }
    public string ConfirmText { get; }

    public ThemedPromptPopupPage(
        string titleText,
        string messageText,
        string placeholderText = "Nhập lý do...",
        string cancelText = "Hủy",
        string confirmText = "Xác nhận")
    {
        InitializeComponent();
        TitleText = titleText;
        MessageText = messageText;
        PlaceholderText = placeholderText;
        CancelText = cancelText;
        ConfirmText = confirmText;
        BindingContext = this;
    }

    /// <summary>
    /// Shows the prompt dialog and returns the entered text if the user tapped Confirm, or null if they tapped Cancel.
    /// </summary>
    public static async Task<string?> ShowAsync(
        string titleText,
        string messageText,
        string placeholderText = "Nhập lý do...",
        string cancelText = "Hủy",
        string confirmText = "Xác nhận")
    {
        var popup = new ThemedPromptPopupPage(titleText, messageText, placeholderText, cancelText, confirmText);
        await Shell.Current.Navigation.PushModalAsync(popup, animated: false);
        var result = await popup._result.Task;
        await Shell.Current.Navigation.PopModalAsync(animated: false);
        return result;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        _result.TrySetResult(null);
    }

    private void OnConfirmClicked(object sender, EventArgs e)
    {
        _result.TrySetResult(ReasonEntry.Text?.Trim() ?? string.Empty);
    }
}
