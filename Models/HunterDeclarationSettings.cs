using System;
using System.Collections.Generic;

namespace Deer_o_matic.Models;

public class HunterDeclarationSettings
{
    public string HunterName { get; set; }= String.Empty;
    public string HunterID { get; set; } = String.Empty;
    public string OtherHunterNames { get; set; } = String.Empty;
    public string RmpIdentifier { get; set; } = String.Empty;
    public string HelicopterRegistrationNumber { get; set; } = String.Empty;

    // The following is a SUPER quick and dirty solution to the fact that the C# native JSON
    // Serializer doesn't like to serialize arrays of booleans.
    public bool[] QuestionResponses
    {
        set
        {
            questionA = value[0];
            questionB = value[1];
            questionC = value[2];
            questionD = value[3];
            questionE = value[4];
            questionF = value[5];
            questionG = value[6];
        }
        get
        {
            return
            [
                questionA,
                questionB,
                questionC,
                questionD,
                questionE,
                questionF,
                questionG
            ];
        }
    }

    public bool questionA;
    public bool questionB;
    public bool questionC;
    public bool questionD;
    public bool questionE;
    public bool questionF;
    public bool questionG;
}