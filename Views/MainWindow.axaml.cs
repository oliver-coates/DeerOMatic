using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace Deer_o_matic.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Celsius.Text != null)
        {
            CalculateTemperature(Celsius.Text);        
        }
    }

    private void Celsius_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (Celsius.Text != null)
        {
            CalculateTemperature(Celsius.Text);        
        }
    }

    private void CalculateTemperature(string celsiusInput)
    {
        if (string.IsNullOrEmpty(celsiusInput) || celsiusInput == "-")
        {
            Fahrenheit.Text = "";
        }
        else
        {
            if (double.TryParse(celsiusInput, out double celsius))
            {
                double fahrenheit = celsius * (9d / 5d) + 32;

                Fahrenheit.Text = fahrenheit.ToString("0.0");
            }
            else
            {
                Celsius.Text = "0";
                Fahrenheit.Text = "0";
            }    
        }
        
    }
}