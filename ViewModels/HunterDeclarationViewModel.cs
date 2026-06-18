using CommunityToolkit.Mvvm.ComponentModel;
using Deer_o_matic.Models;

namespace Deer_o_matic.ViewModels;

public partial class HunterDeclarationViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _hunterName;
    
    [ObservableProperty]
    private string _otherHunterNames;

    [ObservableProperty]
    private string _rmpIdentifier;

    [ObservableProperty]
    private string _dateOfArrivalAtProcessor;

    [ObservableProperty]
    private string _helicopterRegistrationNumber;
    

    /// <summary>
    /// Creates a new, blank, HunterDeclaration ViewModel
    /// </summary>
    public HunterDeclarationViewModel()
    {
        _hunterName = "";
        _otherHunterNames = "";
        _rmpIdentifier = "";
        _dateOfArrivalAtProcessor = "";
        _helicopterRegistrationNumber = "";
    }
}