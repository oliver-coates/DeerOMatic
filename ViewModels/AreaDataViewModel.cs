

using CommunityToolkit.Mvvm.ComponentModel;
using Deer_o_matic.Models;
using Deer_o_matic.Services;
using NetTopologySuite.Geometries;

namespace Deer_o_matic.ViewModels;

public partial class AreaDataViewModel : ViewModelBase, IDisplayableOnMap
{
    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private Avalonia.Media.SolidColorBrush _uiColor;

    public Geometry[] geometry;
    
    private Mapsui.Styles.Color _mapColor;
    public Mapsui.Styles.Color MapColour {get => _mapColor; }

    public AreaDataViewModel(AreaData areaData)
    {
        _name = areaData.name;
        geometry = areaData.geometries;

        _mapColor = areaData.color; // <-- The colour used on the map
        _uiColor = ConvertColor(_mapColor); // The colour used on the UI
    }

    public AreaData Get()
    {
        return new AreaData(Name, geometry, _mapColor);
    }

    private Avalonia.Media.SolidColorBrush ConvertColor(Mapsui.Styles.Color mapColor)
    {
        var avaloniaColor = new Avalonia.Media.Color(
            (byte) mapColor.A,
            (byte) mapColor.R,
            (byte) mapColor.G,
            (byte) mapColor.B
        );

        return new Avalonia.Media.SolidColorBrush(avaloniaColor);
    }
}