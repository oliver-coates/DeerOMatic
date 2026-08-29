namespace Deer_o_matic.Models;

public static class DocumentExportMetaData
{


    // Global counter for documente export.
    // Starts at 0 for the first animal and increments with each flight data's animal marks.
    public static int animalCounter; 

    static DocumentExportMetaData()
    {
        Flush();
    }

    public static void Flush()
    {
        animalCounter = 0;       
    }
}
