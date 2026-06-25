using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Deer_o_matic.Models;
using Deer_o_matic.Services;

namespace Deer_o_matic.ViewModels;

public partial class HunterDeclarationViewModel : ViewModelBase
{
    private readonly ISettingsService _Settings;
    private readonly INotificationService _Notifications;

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

    public AsyncRelayCommand SaveDefaultsCommand { get; }


    #region Hunt Info    
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
    #endregion

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
    public HunterDeclarationViewModel(ISettingsService settings, INotificationService notifications)
    {
        _Settings = settings;
        _Notifications = notifications;

        SaveDefaultsCommand = new AsyncRelayCommand(SaveDefaults);
 
        _hunterName = "";
        _otherHunterNames = "";
        _rmpIdentifier = "";
        _dateOfArrivalAtProcessor = "";
        _helicopterRegistrationNumber = "";
        _hunterIdentificationNumber = "";

        var _ = LoadDefaults();
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

    private async Task LoadDefaults()
    {
        HunterDeclarationSettings? settings = await _Settings.LoadHunterDeclarationSettingsAsync();
    
        if (settings == null)
        {
            return;
        }

        HunterName = settings.HunterName;
        HunterIdentificationNumber = settings.HunterID;
        OtherHunterNames = settings.OtherHunterNames;
        RmpIdentifier = settings.RmpIdentifier;
        HelicopterRegistrationNumber = settings.HelicopterRegistrationNumber;
        
        DateOfArrivalAtProcessor = String.Empty;


        QuestionA = settings.QuestionResponses[0];
        QuestionB = settings.QuestionResponses[1];
        QuestionC = settings.QuestionResponses[2];
        QuestionD = settings.QuestionResponses[3];
        QuestionE = settings.QuestionResponses[4];
        QuestionF = settings.QuestionResponses[5];
        QuestionG = settings.QuestionResponses[6];
    }

    private async Task SaveDefaults()
    {
        HunterDeclarationSettings settings = new HunterDeclarationSettings()
        {
            HunterName = HunterName,
            HunterID = HunterIdentificationNumber,
            OtherHunterNames = OtherHunterNames,
            RmpIdentifier = RmpIdentifier,
            HelicopterRegistrationNumber = HelicopterRegistrationNumber,
            QuestionResponses = new bool[]
            {
                QuestionA,
                QuestionB,
                QuestionC,
                QuestionD,
                QuestionE,
                QuestionF,
                QuestionG
            }
        };
    
        await _Settings.SaveHunterDeclarationSettingsAsync(settings);
 
        await _Notifications.ShowSuccessAsync("✓ Settings Saved");
    }
}