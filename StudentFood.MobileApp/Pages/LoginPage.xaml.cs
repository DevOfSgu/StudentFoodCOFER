using StudentFood.MobileApp.Models;
using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Pages;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService = new();
    private bool _isPasswordVisible;

    public LoginPage()
    {
        InitializeComponent();
    }

    private void OnTogglePasswordClicked(object? sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        TogglePasswordIcon.Source = _isPasswordVisible ? "visibility_off.png" : "visibility.png";
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlertAsync("Thiếu thông tin", "Vui lòng nhập email và mật khẩu.", "OK");
            return;
        }

        LoginButton.IsEnabled = false;
        LoginButton.Text = "Đang đăng nhập...";

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = EmailEntry.Text.Trim(),
            Password = PasswordEntry.Text
        });

        LoginButton.IsEnabled = true;
        LoginButton.Text = "Đăng nhập";

        if (!result.Success)
        {
            await DisplayAlertAsync("Đăng nhập thất bại", result.Message, "OK");
            return;
        }

        Application.Current!.MainPage = new AppShell();
    }

    private async void OnRegisterTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }
}
