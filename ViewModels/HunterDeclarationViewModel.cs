using CommunityToolkit.Mvvm.ComponentModel;
using Deer_o_matic.Models;

namespace Deer_o_matic.ViewModels;

public partial class HunterDeclarationViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _hunterName;


    /// <summary>
    /// Creates a new, blank, HunterDeclaration ViewModel
    /// </summary>
    public HunterDeclarationViewModel()
    {
        _hunterName = "";        
    }

    public HunterDeclaration Get()
    {
        HunterDeclaration declaration = new()
        {
            hunterName = this.HunterName
        };

        return declaration;
    }
}