namespace StudentFood.MobileApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        
        Routing.RegisterRoute(nameof(Pages.FoodDetailPage), typeof(Pages.FoodDetailPage));
		Routing.RegisterRoute(nameof(Pages.CheckoutPage), typeof(Pages.CheckoutPage));
		Routing.RegisterRoute(nameof(Pages.OrderTrackingPage), typeof(Pages.OrderTrackingPage));
		Routing.RegisterRoute(nameof(Pages.ReviewOrderPage), typeof(Pages.ReviewOrderPage));
        Routing.RegisterRoute(nameof(Pages.OrderHistoryPage), typeof(Pages.OrderHistoryPage));
        Routing.RegisterRoute(nameof(Pages.EditProfilePage), typeof(Pages.EditProfilePage));
	}
}
