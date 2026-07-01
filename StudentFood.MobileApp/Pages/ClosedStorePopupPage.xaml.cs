namespace StudentFood.MobileApp.Pages;

public partial class ClosedStorePopupPage : ContentPage
{
    public string TitleText { get; }
    public string MessageText { get; }

    public ClosedStorePopupPage(string titleText, string messageText)
    {
        InitializeComponent();
        TitleText = titleText;
        MessageText = messageText;
        BindingContext = this;
    }

    private async void OnCloseClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}