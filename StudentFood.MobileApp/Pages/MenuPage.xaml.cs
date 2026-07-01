using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Windows.Input;
using StudentFood.MobileApp.Models;
using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Pages;

public partial class MenuPage : ContentPage
{
    private readonly FoodService _foodService = new();
    private readonly List<FoodItem> _allFoods = [];
    private string _searchQuery = string.Empty;
    private string _selectedCategory = "all";
    private int _currentPage = 1;
    private const int PageSize = 10;

    public ObservableCollection<FoodItem> Foods { get; } = [];
    public ICommand RefreshCommand { get; }
    public ICommand FoodTappedCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand SelectRiceCommand { get; }
    public ICommand SelectNoodleCommand { get; }
    public ICommand SelectDrinkCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }

    private bool _isLoaded;
    private bool _isRefreshing;

    public bool HasPagination => TotalPages > 1;
    public int TotalPages { get; private set; } = 1;
    public string PageInfoDisplay => HasPagination ? $"Trang {_currentPage}/{TotalPages}" : string.Empty;

    public string AllChipBackgroundColor => _selectedCategory == "all" ? "#FF8A00" : "White";
    public string AllChipTextColor => _selectedCategory == "all" ? "White" : "#5B5147";
    public string RiceChipBackgroundColor => _selectedCategory == "rice" ? "#FF8A00" : "White";
    public string RiceChipTextColor => _selectedCategory == "rice" ? "White" : "#5B5147";
    public string NoodleChipBackgroundColor => _selectedCategory == "noodle" ? "#FF8A00" : "White";
    public string NoodleChipTextColor => _selectedCategory == "noodle" ? "White" : "#5B5147";
    public string DrinkChipBackgroundColor => _selectedCategory == "drink" ? "#FF8A00" : "White";
    public string DrinkChipTextColor => _selectedCategory == "drink" ? "White" : "#5B5147";

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public MenuPage()
    {
        InitializeComponent();
        BindingContext = this;

        RefreshCommand = new Command(async () => await LoadDataAsync());
        FoodTappedCommand = new Command<FoodItem>(async (food) => await OnFoodTappedAsync(food));
        SelectAllCommand = new Command(() => ApplyCategory("all"));
        SelectRiceCommand = new Command(() => ApplyCategory("rice"));
        SelectNoodleCommand = new Command(() => ApplyCategory("noodle"));
        SelectDrinkCommand = new Command(() => ApplyCategory("drink"));
        PreviousPageCommand = new Command(PreviousPage, () => _currentPage > 1);
        NextPageCommand = new Command(NextPage, () => _currentPage < TotalPages);
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

            var foods = await _foodService.GetAllFoodsAsync();

            _allFoods.Clear();
            _allFoods.AddRange(foods);
            _currentPage = 1;
            ApplyFilters();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchQuery = e.NewTextValue?.Trim() ?? string.Empty;
        _currentPage = 1;
        ApplyFilters();
    }

    private void ApplyCategory(string category)
    {
        _selectedCategory = category;
        _currentPage = 1;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var query = Normalize(_searchQuery);
        var filteredFoods = _allFoods.Where(food => MatchesCategory(food, _selectedCategory) && MatchesQuery(food, query)).ToList();

        TotalPages = filteredFoods.Count == 0 ? 1 : (int)Math.Ceiling(filteredFoods.Count / (double)PageSize);
        _currentPage = Math.Clamp(_currentPage, 1, TotalPages);

        var pageFoods = filteredFoods
            .Skip((_currentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        Foods.Clear();
        foreach (var food in pageFoods)
        {
            Foods.Add(food);
        }

        OnPropertyChanged(nameof(HasPagination));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(PageInfoDisplay));
        OnPropertyChanged(nameof(AllChipBackgroundColor));
        OnPropertyChanged(nameof(AllChipTextColor));
        OnPropertyChanged(nameof(RiceChipBackgroundColor));
        OnPropertyChanged(nameof(RiceChipTextColor));
        OnPropertyChanged(nameof(NoodleChipBackgroundColor));
        OnPropertyChanged(nameof(NoodleChipTextColor));
        OnPropertyChanged(nameof(DrinkChipBackgroundColor));
        OnPropertyChanged(nameof(DrinkChipTextColor));
        (PreviousPageCommand as Command)?.ChangeCanExecute();
        (NextPageCommand as Command)?.ChangeCanExecute();
    }

    private void PreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            ApplyFilters();
        }
    }

    private void NextPage()
    {
        if (_currentPage < TotalPages)
        {
            _currentPage++;
            ApplyFilters();
        }
    }

    private static bool MatchesQuery(FoodItem food, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        return Normalize(food.Name).Contains(query, StringComparison.OrdinalIgnoreCase) ||
               Normalize(food.Description).Contains(query, StringComparison.OrdinalIgnoreCase) ||
               Normalize(food.CanteenName).Contains(query, StringComparison.OrdinalIgnoreCase) ||
               Normalize(food.CategoryName).Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchesCategory(FoodItem food, string category)
    {
        if (category == "all")
        {
            return true;
        }

        var combinedText = $"{Normalize(food.CategoryName)} {Normalize(food.Name)} {Normalize(food.Description)}";

        return category switch
        {
            "rice" => combinedText.Contains("com", StringComparison.OrdinalIgnoreCase) || combinedText.Contains("rice", StringComparison.OrdinalIgnoreCase),
            "noodle" => combinedText.Contains("mi", StringComparison.OrdinalIgnoreCase) || combinedText.Contains("pho", StringComparison.OrdinalIgnoreCase) || combinedText.Contains("noodle", StringComparison.OrdinalIgnoreCase),
            "drink" => combinedText.Contains("do uong", StringComparison.OrdinalIgnoreCase) || combinedText.Contains("drink", StringComparison.OrdinalIgnoreCase) || combinedText.Contains("juice", StringComparison.OrdinalIgnoreCase) || combinedText.Contains("nuoc", StringComparison.OrdinalIgnoreCase),
            _ => true
        };
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : RemoveDiacritics(value.Trim().ToLowerInvariant().Replace('đ', 'd'));
    }

    private static string RemoveDiacritics(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new System.Text.StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
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