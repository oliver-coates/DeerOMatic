using System.Collections.Generic;
using System.Linq;
using Deer_o_matic.Models;

namespace Deer_o_matic.Services;

public interface IDocumentDataSplitter
{
    public HunterDeclarationDocumentData[] SplitDataByAnimalTypes(HunterDeclarationDocumentData data);
}

public class DocumentDataSplitter : IDocumentDataSplitter
{
    public HunterDeclarationDocumentData[] SplitDataByAnimalTypes(HunterDeclarationDocumentData baseData)
    {
        List<HunterDeclarationDocumentData> outData = new();

        // Organise all the flight data in a stack of lists, grouping together connected like flight datas,
        // and segregating them by animal type.
        Stack<List<FlightData>> flightDataByAnimalTypes = new();
        FlightData.AnimalType currentAnimalType = FlightData.AnimalType.NotSet;

        foreach (FlightData thisFlightData in baseData.flightDatas)
        {
            if (thisFlightData.animalType == currentAnimalType)
            {
                // Type matches, add onto the existing list.
                flightDataByAnimalTypes.Peek().Add(thisFlightData);
            }
            else
            {
                // Type does not match, push a new list
                flightDataByAnimalTypes.Push(new List<FlightData>() {thisFlightData});
                currentAnimalType = thisFlightData.animalType;
            }
        }

        // Iterate back over the stack but in reverse, so we go from the first flight data first (like a queue)
        foreach (List<FlightData> dataGroup in flightDataByAnimalTypes.ToArray().Reverse())
        {
            int animalCount = CountTotalAnimals(dataGroup);
            string animalTypeName = FlightData.AnimalTypeAsReadableString(dataGroup[0].animalType);

            HunterDeclarationDocumentData splitDocument = new HunterDeclarationDocumentData()
            {
                hunterName = baseData.hunterName,
                hunterId = baseData.hunterId,
                otherHunters = baseData.otherHunters,
                rmpIdentifier = baseData.rmpIdentifier,
                dateOfArrivalAtProcessor = baseData.dateOfArrivalAtProcessor,
                helicopterRegistration = baseData.helicopterRegistration,
                questionTicks = baseData.questionTicks,

                numAnimals = animalCount,
                numAndTypeOfAnimals = $"{animalCount} {animalTypeName}",

                flightDatas = dataGroup,
            };

            outData.Add(splitDocument);
        }

        return outData.ToArray();
    }

    private int CountTotalAnimals(IEnumerable<FlightData> flightDatas)
    {
        int total = 0;
        foreach (FlightData flightData in flightDatas)
        {
            total += flightData.animalMarks.Length;
        }

        return total;
    }
}