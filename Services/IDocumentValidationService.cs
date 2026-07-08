using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Deer_o_matic.Models;
using Deer_o_matic.ViewModels;

namespace Deer_o_matic.Services;

public interface IDocumentValidationService
{
    public void ValidateDocument(Collection<FlightDataViewModel> flightData, Collection<AreaDataViewModel> areaData);
}

public class DocumentValidatorService : IDocumentValidationService
{
    private readonly IPointProximityService ProximityChecker;

    public DocumentValidatorService(IPointProximityService proximityService)
    {
        ProximityChecker = proximityService;
    }

    public void ValidateDocument(Collection<FlightDataViewModel> flightData, Collection<AreaDataViewModel> areaData)
    {
        List<PointIntersection> intersections = new();
        foreach (FlightDataViewModel flight in flightData)
        {
            intersections.AddRange(ProximityChecker.FindMarksWithinDistanceOfGeometry(flight.marks, areaData));   
        }

        Console.WriteLine($"Found {intersections.Count} intersections!");
    }
}