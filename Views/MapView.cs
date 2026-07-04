using System;
using Avalonia.Controls;
using Deer_o_matic.ViewModels;

namespace Deer_o_matic.Views;

public partial class MapView : UserControl
{
    public MapView()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    
    private async void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Console.WriteLine($"Loaded! Context: {DataContext}");
        
        if (DataContext is MapViewModel vm)
        {
            await vm.TestAddAreaAsync();
        }
    }

}