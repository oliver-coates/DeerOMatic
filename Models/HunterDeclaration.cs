

using System;
using System.Collections.Generic;

namespace Deer_o_matic.Models;

/// <summary>
/// Represents a hunter declaration, a collection of flight data logs and all the animals harvested in these flight logs.
/// </summary>
public class HunterDeclarationDocumentData
{
    public required string hunterName;
    public required string otherHunters;
    public required string rmpIdentifier;
    public required DateTime dateOfArrivalAtProcessor;
    public required string numAndTypeOfAnimals;
    public required string helicopterRegistration;
    
    public List<FlightData> flightDatas;

    public HunterDeclarationDocumentData()
    {
        flightDatas = new ();
    } 
}