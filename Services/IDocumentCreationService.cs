using System;
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
            otherHunters = hunterDeclaration.OtherHunterNames,
            rmpIdentifier = hunterDeclaration.RmpIdentifier,
            numAndTypeOfAnimals = "Not Yet Implemented",
            dateOfArrivalAtProcessor = CreateDateTime(hunterDeclaration.DateOfArrivalAtProcessor),
            helicopterRegistration = hunterDeclaration.HelicopterRegistrationNumber,
            questionTicks = questionTicks
        };

        foreach (var f in fileUpload.FlightData)
        {
            doc.flightDatas.Add(f.Get());
        }

        return doc;
    }

    private DateTime CreateDateTime(string dateTimeString)
    {
        // TODO: Implement this
        return DateTime.Now;
    }
}