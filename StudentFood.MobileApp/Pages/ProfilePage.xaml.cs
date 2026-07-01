using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly AuthService _authService = new();

    public string DisplayName => _authService.GetCurrentUserName();
    public string DisplayPhone => _authService.GetCurrentPhoneNumber() is { Length: > 0 } p ? p : "Chưa cập nhật SĐT";
    public string AvatarInitial => string.IsNullOrWhiteSpace(DisplayName) ? "?" : DisplayName.Trim()[0].ToString().ToUpper();

    public ProfilePage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh displayed info in case it was just edited
        OnPropertyChanged(nameof(DisplayName));
        OnPropertyChanged(nameof(DisplayPhone));
        OnPropertyChanged(nameof(AvatarInitial));
    }

    private async void OnPersonalInfoClicked(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(EditProfilePage));
    }

    private async void OnOrderHistoryClicked(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(OrderHistoryPage));
    }

    private async void OnLogoutClicked(object? sender, TappedEventArgs e)
    {
        bool confirmed = await ThemedConfirmPopupPage.ShowAsync(
            "Đăng xuất",
            "Bạn có chắc chắn muốn đăng xuất khỏi tài khoản này không?",
            "Hủy",
            "Đăng xuất");

        if (!confirmed) return;

        _authService.Logout();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}