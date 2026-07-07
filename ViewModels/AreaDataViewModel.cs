

using CommunityToolkit.Mvvm.ComponentModel;
using Deer_o_matic.Models;
using NetTopologySuite.Geometries;

namespace Deer_o_matic.ViewModels;

public partial class AreaDataViewModel : ViewModelBase, IDisplayableOnMap
{
    [ObservableProperty]
    private string _name;

    public Geometry[] geometry;

    public AreaDataViewModel(AreaData areaData)
    {
        _name = areaData.name;
        geometry = areaData.geometries;
    }

    public AreaData Get()
    {
        return new AreaData(Name, geometry);
    }
}