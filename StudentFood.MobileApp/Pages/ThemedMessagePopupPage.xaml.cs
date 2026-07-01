namespace StudentFood.MobileApp.Pages;

public partial class ThemedMessagePopupPage : ContentPage
{
    private readonly TaskCompletionSource<bool> _closed = new();

    public string TitleText { get; }
    public string MessageText { get; }
    public string ButtonText { get; }

    public ThemedMessagePopupPage(string titleText, string messageText, string buttonText = "Đã hiểu")
    {
        InitializeComponent();
        TitleText = titleText;
        MessageText = messageText;
        ButtonText = buttonText;
        BindingContext = this;
    }

    public static async Task ShowAsync(string titleText, string messageText, string buttonText = "Đã hiểu")
    {
        var popup = new ThemedMessagePopupPage(titleText, messageText, buttonText);
        await Shell.Current.Navigation.PushModalAsync(popup);
        await popup._closed.Task;
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        _closed.TrySetResult(true);
        await Shell.Current.Navigation.PopModalAsync();
    }
}