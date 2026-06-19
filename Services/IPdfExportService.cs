using System;
using System.Collections.Generic;
using Spire.Pdf;
using Spire.Pdf.Fields;
using Spire.Pdf.Widget;
using System.Threading.Tasks;
using Deer_o_matic.Models;
using Avalonia.Platform.Storage;
using System.Linq;

namespace Deer_o_matic.Services;

public interface IPdfExportService
{
    public Task ExportDocumentAsync(HunterDeclarationDocumentData data, IStorageFolder folder);
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

    public async Task ExportDocumentAsync(HunterDeclarationDocumentData data, IStorageFolder folder)
    {
        PdfDocument doc = new PdfDocument();

        // TODO: This needs to be an actual reference, not just this placeholder absolute reference
        doc.LoadFromFile("C:/Projects/GCH Deer-o-matic/deer-o-matic/Assets/Forms/14743-LHSD_fillable4.pdf");

        FormArgument[] arguments = GetFormArguments(data);
        
        FillForm(doc, arguments);
        Flatten(doc);

        string fullPath = $"{folder.Path.LocalPath} LHDF ({DateTime.Now.ToString("d-M-y HH-mm")}).pdf";

        doc.SaveToFile(fullPath);
    }

    private static FormArgument[] GetFormArguments(HunterDeclarationDocumentData data)
    {
        FormArgument[] primaryArguments =
        [
            new FormArgument("text-hunter-name", $"{data.hunterName} {data.hunterId}"),
            new FormArgument("text-other-hunters", data.otherHunters),
            new FormArgument("text-animal-material-depot", data.rmpIdentifier),
            new FormArgument("date-date-of-arrival", data.dateOfArrivalAtProcessor.ToString()),
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

        return [.. primaryArguments, .. questionArguments];
    }


    /// <summary>
    /// Publishes a dummy PDF to the user's desktop. Used for debugging and testing the PDF publisher.
    /// </summary>
    /// <param name="formLocation"></param>
    public static void PublishDummyPdf(string formLocation)
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
    private static void FillForm(PdfDocument doc, FormArgument[] arguments)
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