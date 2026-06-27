using System;
using System.Collections.Generic;
using Spire.Pdf;
using Spire.Pdf.Fields;
using Spire.Pdf.Widget;
using System.Threading.Tasks;
using Deer_o_matic.Models;
using Avalonia.Platform.Storage;
using System.Linq;
using System.IO;

namespace Deer_o_matic.Services;

public interface IPdfExportService
{
    public Task ExportDocumentAsync(HunterDeclarationDocumentData data, IStorageFolder folder, bool fillable);
}


public class PdfExportService : IPdfExportService
{
    private static readonly string[] formQuestionsTrue =
    {
        "bool-procured-accordance-operations-manual-yes",
            "bool-recovery-none-poisoned-yes",
            "bool-animals-no-disease-yes",
            "bool-below-mrl-mpl-yes",
            "bool-no-chemicals-medicine-yes",
            "bool-carcasses-good-conditions-yes",
            "bool-deer-tb-free-yes"
    };

    private static readonly string[] formQuestionsFalse =
    {
        "bool-procured-accordance-operations-manual-no",
        "bool-recovery-none-poisoned-no",
        "bool-animals-no-disease-no",
        "bool-below-mrl-mpl-no",
        "bool-no-chemicals-medicine-no",
        "bool-carcasses-good-conditions-no",
        "bool-deer-tb-free-no"
    };

    public async Task ExportDocumentAsync(HunterDeclarationDocumentData data, IStorageFolder folder, bool fillable)
    {
        PdfDocument doc = new PdfDocument();

        doc.LoadFromStream(GetFormTemplate());

        FormArgument[] arguments = GetFormArguments(data);
        
        FillForm(doc, arguments);
        if (fillable == false)
        {
            Flatten(doc);        
        }

        string fullPath = $"{folder.Path.LocalPath} LHDF ({DateTime.Now.ToString("d-M-y HH-mm")}).pdf";

        doc.SaveToFile(fullPath);
    }

    private Stream GetFormTemplate()
    {
        var assembly = typeof(IPdfExportService).Assembly;
        var resourcePath = "Deer-o-matic.Assets.Forms.LHSD.pdf";

        var stream = assembly.GetManifestResourceStream(resourcePath)
            ?? throw new FileNotFoundException($"LHSD Form not found! at {resourcePath}");
    
        return stream;
    }

    private FormArgument[] GetFormArguments(HunterDeclarationDocumentData data)
    {
        FormArgument[] primaryArguments =
        [
            new FormArgument("text-hunter-name", $"{data.hunterName} {data.hunterId}"),
            new FormArgument("text-other-hunters", data.otherHunters),
            new FormArgument("text-animal-material-depot", data.rmpIdentifier),
            new FormArgument("date-date-of-arrival", data.dateOfArrivalAtProcessor),
            new FormArgument("text-number-and-species", data.numAndTypeOfAnimals),
            new FormArgument("text-helicopter-registration", data.helicopterRegistration),
            new FormArgument("text-date-signed", data.dateSigned.ToShortDateString()),
            new FormArgument("listed-hunter-signature", data.hunterName)
        ];

        FormArgument[] questionArguments = new FormArgument[7];
        for (int questionIndex = 0; questionIndex < 7; questionIndex++)
        {
            string argument;
            if (data.questionTicks[questionIndex])
            {
                argument = formQuestionsTrue[questionIndex];
            }
            else
            {
                argument = formQuestionsFalse[questionIndex];    
            }
            questionArguments[questionIndex] = new FormArgument(argument, "Yes");
        }

        FormArgument[] consignmentArguments = GetConsignmentArguments(data);

        return [.. primaryArguments, .. questionArguments, .. consignmentArguments];
    }

    private FormArgument[] GetConsignmentArguments(HunterDeclarationDocumentData data)
    {
        List<FormArgument> arguments = new();
        int absoluteIndex = 1;
        int row = 0;

        foreach (FlightData flightData in data.flightDatas)
        {
            // How many animals were killed this flight?
            int numAnimalsThisFlight = (flightData.animalMarks.Length+1);

            // The start and end index for our marks (i.e "Mark 7 to 24")
            int startIndex = absoluteIndex;
            int endIndex = startIndex + numAnimalsThisFlight - 1;

            if (flightData.refrigerationTime == null)
            {
                throw new NullReferenceException("No refridgeration time has been set.");
            }
            else if (flightData.startTime == null)
            {
                throw new NullReferenceException("Unable to find a start time.");
            }

            string carcassIdentifier = $"{startIndex} to {endIndex}";
            string killLocation = $"Mark 1 to {numAnimalsThisFlight}";
            string dateAndTime = $"{flightData.startTime?.ToString("dd/MM/yy HH:mm")} NZST";
            string timeRefrigerated = $"{flightData.refrigerationTime?.ToString("dd/MM/yy HH:mm")} NZST";

            arguments.AddRange(
                new FormArgument[] {
                    new FormArgument($"text-carcass-id-{row}", carcassIdentifier),
                    new FormArgument($"text-kill-location-{row}", killLocation),
                    new FormArgument($"date-date-time-killed-{row}", dateAndTime),
                    new FormArgument($"date-date-and-time-fridge-{row}", timeRefrigerated)
                }
            );

            // Increment the absolute index forward for the next flightdata
            // If this flight data went from animals 7-24,
            // The next flight data might go for animals 25-32 for example
            absoluteIndex = endIndex+1;
            row+=1;
        }

        return arguments.ToArray();
    }


    /// <summary>
    /// Publishes a dummy PDF to the user's desktop. Used for debugging and testing the PDF publisher.
    /// </summary>
    /// <param name="formLocation"></param>
    public void PublishDummyPdf(string formLocation)
    {
        PdfDocument doc = new PdfDocument();
        doc.LoadFromFile(formLocation);

        // Dummy info:
        FormArgument[] arguments = new FormArgument[]
        {
            new FormArgument("text-hunter-name", "Oliver Coates"),
            new FormArgument("text-other-hunters", "Test"),
            new FormArgument("bool-procured-accordance-operations-manual-yes", "Yes")
        };

        FillForm(doc, arguments);
        Flatten(doc);

        doc.SaveToFile("C:/Users/Oliver/Desktop/dummy.pdf");
    }

    /// <summary>
    /// Fills out a PDF document object with the provided arguments.
    /// </summary>
    private void FillForm(PdfDocument doc, FormArgument[] arguments)
    {
        PdfFormWidget widgets = (PdfFormWidget)doc.Form;

        // Assemble argument dictionary
        Dictionary<string, FormArgument> argumentDictionary = new();
        foreach (FormArgument argument in arguments)
        {
            argumentDictionary.Add(argument.name, argument);
        }

        // Iterate across each
        for (int widgetIndex = 0; widgetIndex < widgets.FieldsWidget.Count; widgetIndex++)
        {
            // Get the field and ensure it is not null
            PdfField? field = widgets.FieldsWidget.List[widgetIndex] as PdfField; 
            if (field == null)
            {
                throw new NullReferenceException();
            }

            // Retrieve the argumenet from the dictionary which matches this field's name
            if (argumentDictionary.ContainsKey(field.Name))
            {
                FormArgument argument = argumentDictionary[field.Name];
                
                FillField(field, argument);
            }
            else
            {
                continue;
            }
        }
    }

    /// <summary>
    /// Fills a provided pdfField with a provided argument.
    /// </summary>
    /// <param name="field"></param>
    /// <param name="argument"></param>
    private static void FillField(PdfField field, FormArgument argument)
    {
        if (field is PdfTextBoxFieldWidget textField)
        {
            textField.Text = argument.value;
        }
        else if (field is PdfCheckBoxWidgetFieldWidget checkField)
        {
            checkField.Checked = (argument.value == "Yes");            
        }
    }

    /// <summary>
    /// Flattens this PDF so that all fields are read-only and cannot be edited.
    /// </summary>
    /// <param name="doc">The document to edit</param>
    public static void Flatten(PdfDocument doc)
    {
        PdfFormWidget widgets = (PdfFormWidget) doc.Form; 
        
        foreach (PdfField field in widgets.FieldsWidget.List)
        {
            field.Flatten = true;
            field.ReadOnly = true;
        }
    }

    /// <summary>
    /// Helper object to couple a field name with a bit of information for the field.
    /// </summary>
    private class FormArgument
    {
        public readonly string name;
        public readonly string value;
        public FormArgument(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
    
}