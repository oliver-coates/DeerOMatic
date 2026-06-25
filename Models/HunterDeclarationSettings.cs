using System;

namespace Deer_o_matic.Models;

public class HunterDeclarationSettings
{
    public string HunterName { get; set; }= String.Empty;
    public string HunterID { get; set; } = String.Empty;
    public string OtherHunterNames { get; set; } = String.Empty;
    public string RmpIdentifier { get; set; } = String.Empty;
    public string HelicopterRegistrationNumber { get; set; } = String.Empty;
    public bool[] QuestionResponses = new bool [7];
}