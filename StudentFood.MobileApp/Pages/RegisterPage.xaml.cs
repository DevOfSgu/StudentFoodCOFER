using StudentFood.MobileApp.Models;
using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _authService = new();

    private bool _isPasswordVisible;
    private bool _isConfirmPasswordVisible;

    public RegisterPage()
    {
        InitializeComponent();
    }

    private void OnTogglePasswordClicked(object? sender, EventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordEntry.IsPassword = !_isPasswordVisible;
        TogglePasswordIcon.Source = _isPasswordVisible ? "visibility_off.png" : "visibility.png";
    }

    private void OnToggleConfirmPasswordClicked(object? sender, EventArgs e)
    {
        _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
        ConfirmPasswordEntry.IsPassword = !_isConfirmPasswordVisible;
        ToggleConfirmPasswordIcon.Source = _isConfirmPasswordVisible ? "visibility_off.png" : "visibility.png";
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FullNameEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text))
        {
            await DisplayAlertAsync("Thiếu thông tin", "Vui lòng nhập đầy đủ thông tin.", "OK");
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlertAsync("Sai mật khẩu", "Mật khẩu nhập lại không khớp.", "OK");
            return;
        }

        RegisterButton.IsEnabled = false;
        RegisterButton.Text = "Đang tạo tài khoản...";

        var result = await _authService.RegisterAsync(new RegisterRequest
        {
            FullName = FullNameEntry.Text.Trim(),
            Email = EmailEntry.Text.Trim(),
            Password = PasswordEntry.Text,
            PhoneNumber = string.Empty
        });

        RegisterButton.IsEnabled = true;
        RegisterButton.Text = "Đăng ký";

        if (!result.Success)
        {
            await DisplayAlertAsync("Đăng ký thất bại", result.Message, "OK");
            return;
        }

        await DisplayAlertAsync("Thành công", result.Message, "OK");
        await Navigation.PopAsync();
    }

    private async void OnLoginTapped(object? sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}
