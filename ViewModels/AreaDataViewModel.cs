

using CommunityToolkit.Mvvm.ComponentModel;
using Deer_o_matic.Models;
using NetTopologySuite.Geometries;

namespace Deer_o_matic.ViewModels;

public partial class AreaDataViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _name;

    public Geometry[] _geometry;

    public AreaDataViewModel(AreaData areaData)
    {
        _name = areaData.name;
        _geometry = areaData.geometries;
    }

    public AreaData Get()
    {
        return new AreaData(Name, _geometry);
    }
}