using StudentFood.MobileApp.Models;
using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Pages;

public partial class EditProfilePage : ContentPage
{
    private readonly UserService _userService = new();
    private readonly AuthService _authService = new();

    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _phoneNumber = string.Empty;

    public string FullName
    {
        get => _fullName;
        set { _fullName = value; OnPropertyChanged(); OnPropertyChanged(nameof(AvatarInitial)); }
    }

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set { _phoneNumber = value; OnPropertyChanged(); }
    }

    public string AvatarInitial => string.IsNullOrWhiteSpace(FullName)
        ? "?"
        : FullName.Trim()[0].ToString().ToUpper();

    public EditProfilePage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        var userId = Preferences.Default.Get("user_id", 0);
        if (userId <= 0) return;

        var result = await _userService.GetProfileAsync(userId);
        if (result.Success && result.User != null)
        {
            FullName = result.User.FullName;
            Email = result.User.Email;
            PhoneNumber = result.User.PhoneNumber;
        }
        else
        {
            // Fallback to cached preferences
            FullName = _authService.GetCurrentUserName();
            PhoneNumber = _authService.GetCurrentPhoneNumber();
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            await ThemedMessagePopupPage.ShowAsync("Thiếu thông tin", "Vui lòng nhập họ và tên.");
            return;
        }

        var userId = Preferences.Default.Get("user_id", 0);
        if (userId <= 0)
        {
            await ThemedMessagePopupPage.ShowAsync("Lỗi", "Không thể xác định người dùng. Vui lòng đăng nhập lại.");
            return;
        }

        var request = new UpdateProfileRequest
        {
            FullName = FullName.Trim(),
            Email = Email.Trim(),
            PhoneNumber = PhoneNumber.Trim()
        };

        var result = await _userService.UpdateProfileAsync(userId, request);

        if (result.Success)
        {
            // Update local cache
            Preferences.Default.Set("user_full_name", request.FullName);
            Preferences.Default.Set("user_phone_number", request.PhoneNumber);

            await ThemedMessagePopupPage.ShowAsync("Đã lưu", "Thông tin cá nhân đã được cập nhật thành công.");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await ThemedMessagePopupPage.ShowAsync("Không thể lưu", result.Message ?? "Đã xảy ra lỗi khi lưu thông tin.");
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
