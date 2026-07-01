namespace StudentFood.MobileApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var navigationPage = new NavigationPage(new Pages.LoginPage())
		{
			BarBackgroundColor = Color.FromArgb("#FFFFFF"),
			BarTextColor = Color.FromArgb("#2A241F")
		};

		return new Window(navigationPage);
	}
}
