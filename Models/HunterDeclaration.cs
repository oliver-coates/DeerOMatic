

using System.Collections.Generic;

namespace Deer_o_matic.Models;

/// <summary>
/// Represents a hunter declaration, a collection of flight data logs and all the animals harvested in these flight logs.
/// </summary>
public class HunterDeclaration
{
    public string hunterName;

    
    public List<FlightData> flightDatas;

    public HunterDeclaration()
    {
        hunterName = "";
        flightDatas = new ();
    } 
}