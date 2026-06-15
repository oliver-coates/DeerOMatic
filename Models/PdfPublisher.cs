using System;
using System.Collections.Generic;
using Spire.Pdf;
using Spire.Pdf.Fields;
using Spire.Pdf.Widget;

namespace Deer_o_matic.Models;

public static class PdfPublisher
{
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

