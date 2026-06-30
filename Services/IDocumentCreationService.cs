using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Deer_o_matic.Models;
using Deer_o_matic.ViewModels;

namespace Deer_o_matic.Services;

/// <summary>
/// Service which creates hunter declaration document data
/// </summary>
public interface IDocumentCreationService
{
    HunterDeclarationDocumentData BuildDocument(
        FileUploadViewModel fileUpload,
        HunterDeclarationViewModel hunterDeclaration
    );
}

public class DocumentCreationService : IDocumentCreationService
{

    public HunterDeclarationDocumentData BuildDocument(FileUploadViewModel fileUpload, HunterDeclarationViewModel hunterDeclaration)
    {
        bool[] questionTicks = new bool[]
        {
            hunterDeclaration.QuestionA,
            hunterDeclaration.QuestionB,
            hunterDeclaration.QuestionC,
            hunterDeclaration.QuestionD,
            hunterDeclaration.QuestionE,
            hunterDeclaration.QuestionF,
            hunterDeclaration.QuestionG,
        };

        HunterDeclarationDocumentData doc = new()
        {
            hunterName = hunterDeclaration.HunterName,
            hunterId = hunterDeclaration.HunterIdentificationNumber,
            otherHunters = hunterDeclaration.OtherHunterNames,
            rmpIdentifier = hunterDeclaration.RmpIdentifier,
            numAndTypeOfAnimals = $"{GetTotalNumPlacemarks(fileUpload.FlightData)} Deer",
            dateOfArrivalAtProcessor = hunterDeclaration.DateOfArrivalAtProcessor,
            helicopterRegistration = hunterDeclaration.HelicopterRegistrationNumber,
            questionTicks = questionTicks
        };

        foreach (var file in fileUpload.FlightData)
        {
            FlightData flightData = file.Get();
            
            flightData.ValidateTime();                

            doc.flightDatas.Add(flightData);
        }

        return doc;
    }

    private int GetTotalNumPlacemarks(ObservableCollection<FlightDataViewModel> flightDatas)
    {
        int total = 0;

        foreach (var f in flightDatas)
        {
            total += f.Get().animalMarks.Length;
        }

        return total;
    }

}