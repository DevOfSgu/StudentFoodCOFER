using System.ComponentModel;
using System.Runtime.CompilerServices;
using StudentFood.MobileApp.Services;

namespace StudentFood.MobileApp.Models;

public class CartItem : INotifyPropertyChanged
{
    private int _quantity;
    private string _note = string.Empty;

    public FoodItem Food { get; set; } = null!;

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(DisplayTotalPrice));
            }
        }
    }

    public string Note
    {
        get => _note;
        set
        {
            if (_note != value)
            {
                _note = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasNote));
            }
        }
    }

    public bool HasNote => !string.IsNullOrWhiteSpace(Note);

    public decimal TotalPrice => Food.Price * Quantity;
    public string DisplayTotalPrice => CartService.FormatCurrency(TotalPrice);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
