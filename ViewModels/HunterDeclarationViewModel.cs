using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Deer_o_matic.ViewModels;

public partial class HunterDeclarationViewModel : ViewModelBase
{
    private bool _isAllQuestionsChecked;
    public bool IsAllQuestionsChecked
    {
        get => _isAllQuestionsChecked;
        set
        {
            if (SetProperty(ref _isAllQuestionsChecked, value))
            {
                OnAllCheckboxChanged(value);
            }
        }
    }

    [ObservableProperty]
    private string _hunterName;

    [ObservableProperty]
    private string _hunterIdentificationNumber;
    
    [ObservableProperty]
    private string _otherHunterNames;

    [ObservableProperty]
    private string _rmpIdentifier;

    [ObservableProperty]
    private string _dateOfArrivalAtProcessor;

    [ObservableProperty]
    private string _helicopterRegistrationNumber;
    
    #region Questions
    [ObservableProperty]
    private bool _questionA;
    
    [ObservableProperty]
    private bool _questionB;
    
    [ObservableProperty]
    private bool _questionC;
    
    [ObservableProperty]
    private bool _questionD;
    
    [ObservableProperty]
    private bool _questionE;
    
    [ObservableProperty]
    private bool _questionF;
    
    [ObservableProperty]
    private bool _questionG;
    #endregion

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
        _hunterIdentificationNumber = "";
    }


    private void OnAllCheckboxChanged(bool value)
    {
        QuestionA = value; 
        QuestionB = value;
        QuestionC = value; 
        QuestionD = value; 
        QuestionE = value; 
        QuestionE = value; 
        QuestionF = value; 
        QuestionG = value;
    }
}