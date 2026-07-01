using System.Collections.ObjectModel;
using System.Windows.Input;
using StudentFood.MobileApp.Models;
using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Pages;

public partial class HomePage : ContentPage
{
    private readonly AuthService _authService = new();
    private readonly FoodService _foodService = new();

    public ObservableCollection<FoodItem> TrendingFoods { get; } = [];
    public ObservableCollection<FoodItem> RecommendedFoods { get; } = [];

    // Properties for trending foods to support custom layout
    private FoodItem _trendingFood1;
    public FoodItem TrendingFood1
    {
        get => _trendingFood1;
        set { _trendingFood1 = value; OnPropertyChanged(); }
    }

    private FoodItem _trendingFood2;
    public FoodItem TrendingFood2
    {
        get => _trendingFood2;
        set { _trendingFood2 = value; OnPropertyChanged(); }
    }

    private FoodItem _trendingFood3;
    public FoodItem TrendingFood3
    {
        get => _trendingFood3;
        set { _trendingFood3 = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }
    public ICommand FoodTappedCommand { get; }

    private bool _isLoaded;
    private bool _isRefreshing;
    private string _greetingText = "Chào bạn!";

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public string GreetingText
    {
        get => _greetingText;
        set
        {
            _greetingText = value;
            OnPropertyChanged();
        }
    }

    public HomePage()
    {
        InitializeComponent();
        BindingContext = this;
        RefreshCommand = new Command(async () => await LoadDataAsync());
        FoodTappedCommand = new Command<FoodItem>(async (food) => await OnFoodTappedAsync(food));
        GreetingText = BuildGreeting();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoaded)
        {
            return;
        }

        _isLoaded = true;
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsRefreshing = true;
            GreetingText = BuildGreeting();

            var trendingFoods = (await _foodService.GetTrendingFoodsAsync()).ToList();
            var recommendedFoods = (await _foodService.GetRecommendedFoodsAsync()).ToList();

            TrendingFoods.Clear();
            var trendingTake = trendingFoods.Take(3).ToList();
            foreach (var food in trendingTake)
            {
                if (food.AverageRating <= 0)
                {
                    food.AverageRating = 4.5;
                }
                TrendingFoods.Add(food);
            }

            // Assign individual properties for trending foods layout
            TrendingFood1 = trendingTake.Count > 0 ? trendingTake[0] : null;
            TrendingFood2 = trendingTake.Count > 1 ? trendingTake[1] : null;
            TrendingFood3 = trendingTake.Count > 2 ? trendingTake[2] : null;

            RecommendedFoods.Clear();
            foreach (var food in recommendedFoods.Take(3))
            {
                if (food.AverageRating <= 0)
                {
                    food.AverageRating = 4.8;
                }
                RecommendedFoods.Add(food);
            }
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private string BuildGreeting()
    {
        var hour = DateTime.Now.Hour;
        var prefix = hour < 12 ? "Chào buổi sáng" : hour < 18 ? "Chào buổi chiều" : "Chào buổi tối";
        var userName = _authService.GetCurrentUserName();

        return $"{prefix}, {userName}!";
    }

    private async Task OnFoodTappedAsync(FoodItem food)
    {
        if (food == null) return;

        await Shell.Current.GoToAsync(nameof(FoodDetailPage), new Dictionary<string, object>
        {
            { "Food", food }
        });
    }
}
