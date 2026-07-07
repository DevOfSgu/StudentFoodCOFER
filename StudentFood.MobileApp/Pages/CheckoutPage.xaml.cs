using System.Windows.Input;
using System.Collections.ObjectModel;
using StudentFood.MobileApp.Models;
using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Pages;

public partial class CheckoutPage : ContentPage
{
    private enum PaymentMethod
    {
        Cash,
        Transfer
    }

    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;
    private readonly AuthService _authService = new();
    private readonly OrderService _orderService = new();

    public ObservableCollection<CartItem> CartItems => CartService.Instance.Items;

    public string TotalDisplay => CartService.FormatCurrency(CartService.Instance.Total);
    public string SubtotalDisplay => CartService.FormatCurrency(CartService.Instance.Subtotal);
    public string DeliveryFeeDisplay => CartService.FormatCurrency(CartService.Instance.DeliveryFee);
    public string TaxDisplay => CartService.FormatCurrency(CartService.Instance.Tax);
    public string ItemsCountDisplay => $"{CartItems.Count} món";

    public bool IsCartEmpty => !CartItems.Any();

    public string CashBorderColor => _selectedPaymentMethod == PaymentMethod.Cash ? "#FF8A00" : "#E8D8C6";
    public string CashBackgroundColor => _selectedPaymentMethod == PaymentMethod.Cash ? "#FFF7F0" : "White";
    public string CashIcon => _selectedPaymentMethod == PaymentMethod.Cash ? "\ue837" : "\ue836";
    public string CashIconColor => _selectedPaymentMethod == PaymentMethod.Cash ? "#FF8A00" : "#8A847E";

    public string TransferBorderColor => _selectedPaymentMethod == PaymentMethod.Transfer ? "#FF8A00" : "#E8D8C6";
    public string TransferBackgroundColor => _selectedPaymentMethod == PaymentMethod.Transfer ? "#FFF7F0" : "White";
    public string TransferIcon => _selectedPaymentMethod == PaymentMethod.Transfer ? "\ue837" : "\ue836";
    public string TransferIconColor => _selectedPaymentMethod == PaymentMethod.Transfer ? "#FF8A00" : "#8A847E";

    public ICommand SelectCashCommand { get; }
    public ICommand SelectTransferCommand { get; }

    public CheckoutPage()
    {
        SelectCashCommand = new Command(SelectCash);
        SelectTransferCommand = new Command(SelectTransfer);

        InitializeComponent();
        BindingContext = this;
        RefreshBindings();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshCustomerInfo();
        RefreshBindings();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnConfirmOrderClicked(object? sender, EventArgs e)
    {
        if (!CartItems.Any())
        {
            await ThemedMessagePopupPage.ShowAsync("Giỏ hàng trống", "Vui lòng thêm món ăn vào giỏ hàng trước khi đặt.");
            return;
        }

        var canteenIds = CartItems
            .Select(item => item.Food.Canteen?.Id ?? 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (canteenIds.Count > 1)
        {
            await ThemedMessagePopupPage.ShowAsync("Không hỗ trợ", "Vui lòng chỉ đặt món từ một căn tin trong một lần thanh toán.");
            return;
        }

        if (CartItems.Any(item => !item.Food.IsCanteenOpen))
        {
            var closedItem = CartItems.First(item => !item.Food.IsCanteenOpen);
            await ShowStoreClosedPopupAsync(closedItem.Food.CanteenName);
            return;
        }

        if (string.IsNullOrWhiteSpace(FullNameEntry.Text) ||
            string.IsNullOrWhiteSpace(PhoneEntry.Text) ||
            string.IsNullOrWhiteSpace(AddressEntry.Text))
        {
            await ThemedMessagePopupPage.ShowAsync("Thiếu thông tin", "Vui lòng nhập đầy đủ họ tên, số điện thoại và địa chỉ giao hàng.");
            return;
        }

        var studentId = Preferences.Default.Get("user_id", 0);
        if (studentId <= 0)
        {
            await ThemedMessagePopupPage.ShowAsync("Phiên đăng nhập hết hạn", "Vui lòng đăng nhập lại để tiếp tục đặt hàng.");
            return;
        }

        var result = await _orderService.PlaceOrderAsync(new PlaceOrderRequest
        {
            StudentId = studentId,
            PaymentMethod = _selectedPaymentMethod == PaymentMethod.Transfer ? "transfer" : "cash",
            DeliveryAddress = AddressEntry.Text.Trim(),
            PhoneNumber = PhoneEntry.Text.Trim(),
            CustomerName = FullNameEntry.Text.Trim(),
            Items = CartItems.Select(item => new PlaceOrderItemRequest
            {
                FoodId = item.Food.Id,
                Quantity = item.Quantity,
                Note = string.IsNullOrWhiteSpace(item.Note) ? null : item.Note.Trim()
            }).ToList()
        });

        if (!result.Success || result.Payload?.Order == null)
        {
            await ThemedMessagePopupPage.ShowAsync("Không thể đặt hàng", result.Message ?? "Đã xảy ra lỗi khi đặt hàng.");
            return;
        }

        CartService.Instance.ClearCart();

        var title = _selectedPaymentMethod == PaymentMethod.Transfer
            ? "Thanh toán thành công"
            : "Đặt hàng thành công";

        var message = _selectedPaymentMethod == PaymentMethod.Transfer
            ? "Chuyển khoản đã được ghi nhận. Đơn hàng đang được xử lý."
            : "Đơn hàng của bạn đã được đặt thành công. Vui lòng chờ xác nhận từ căn tin.";

        await ThemedMessagePopupPage.ShowAsync(title, message);

        // Về trang chủ sau khi tắt popup
        await Shell.Current.GoToAsync("//HomePage");
    }

    private static async Task ShowStoreClosedPopupAsync(string canteenName)
    {
        var popup = new ClosedStorePopupPage(
            "Ngoài giờ hoạt động",
            $"{canteenName} hiện đã đóng cửa. Vui lòng quay lại khi quán mở lại.");

        await Shell.Current.Navigation.PushModalAsync(popup);
    }

    private void RefreshCustomerInfo()
    {
        if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
        {
            FullNameEntry.Text = _authService.GetCurrentUserName();
        }

        if (string.IsNullOrWhiteSpace(PhoneEntry.Text))
        {
            PhoneEntry.Text = _authService.GetCurrentPhoneNumber();
        }
    }

    private void SelectCash()
    {
        _selectedPaymentMethod = PaymentMethod.Cash;
        RefreshBindings();
    }

    private void SelectTransfer()
    {
        _selectedPaymentMethod = PaymentMethod.Transfer;
        RefreshBindings();
    }

    private void RefreshBindings()
    {
        OnPropertyChanged(nameof(TotalDisplay));
        OnPropertyChanged(nameof(SubtotalDisplay));
        OnPropertyChanged(nameof(DeliveryFeeDisplay));
        OnPropertyChanged(nameof(TaxDisplay));
        OnPropertyChanged(nameof(ItemsCountDisplay));
        OnPropertyChanged(nameof(CashBorderColor));
        OnPropertyChanged(nameof(CashBackgroundColor));
        OnPropertyChanged(nameof(CashIcon));
        OnPropertyChanged(nameof(CashIconColor));
        OnPropertyChanged(nameof(TransferBorderColor));
        OnPropertyChanged(nameof(TransferBackgroundColor));
        OnPropertyChanged(nameof(TransferIcon));
        OnPropertyChanged(nameof(TransferIconColor));
    }
}