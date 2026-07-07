namespace StudentFood.MobileApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Auto-login nếu đã có session trong Preferences
		var userId = Preferences.Default.Get("user_id", 0);
		if (userId > 0)
		{
			return new Window(new AppShell());
		}

		var navigationPage = new NavigationPage(new Pages.LoginPage())
		{
			BarBackgroundColor = Color.FromArgb("#FFFFFF"),
			BarTextColor = Color.FromArgb("#2A241F")
		};

		return new Window(navigationPage);
	}
}
