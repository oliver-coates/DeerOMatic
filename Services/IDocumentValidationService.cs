using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Deer_o_matic.Models;
using Deer_o_matic.ViewModels;

namespace Deer_o_matic.Services;

public interface IDocumentValidationService
{
    public void ValidateDocument(HunterDeclarationDocumentData doc, Collection<AreaDataViewModel> areaData, bool ensureDocPoisonAreasAreaPresent);
}

public class DocumentValidatorService : IDocumentValidationService
{
    public const int MAXIMUM_REFRIDGERATION_HOURS = 10;
    public const double MINIMUM_DISTANCE_ALLOWED_FROM_POISON_ZONE_METERS = 2000;


    private readonly IPointProximityService ProximityChecker;
    private readonly IDocPoisonAreaRetrievalService PoisonAreaRetrieval;

    public DocumentValidatorService(IPointProximityService proximityService, IDocPoisonAreaRetrievalService poisonAreaRetrieval)
    {
        ProximityChecker = proximityService;
        PoisonAreaRetrieval = poisonAreaRetrieval;
    }

    /// <summary>
    /// Performs a series of checks against the provided hunter declaration data to try to ensure it is valid.
    /// </summary>
    public void ValidateDocument(HunterDeclarationDocumentData doc, Collection<AreaDataViewModel> poisonAreas, bool ensureDocPoisonAreasAreaPresent)
    {
        ValidateAnimalsExist(doc);

        ValidateRefridgerationTime(doc.flightDatas);

        ValidatePoisonAreas(doc.flightDatas, poisonAreas, ensureDocPoisonAreasAreaPresent);
    }

    /// <summary>
    /// Checks that there is flight data and that at least one provided flight data has animals in it, or it throws an error.
    /// </summary>
    private void ValidateAnimalsExist(HunterDeclarationDocumentData doc)
    {
        if (doc.flightDatas.Count == 0)
        {
            throw new Exception("No flight data has been uploaded!");
        }
        else if (doc.numAnimals == 0)
        {
            throw new Exception("No animal placemarks were found within any uploaded flight data");
        }
    }

    /// <summary>
    /// Checks that all animal mark inside provided flight datas are no closer than <see cref="MINIMUM_DISTANCE_ALLOWED_FROM_POISON_ZONE_METERS"/> from provided geometry. 
    /// </summary>
    private void ValidatePoisonAreas(List<FlightData> flightData, Collection<AreaDataViewModel> poisonAreas, bool ensureDocPoisonAreasAreaPresent)
    {
        if (ensureDocPoisonAreasAreaPresent)
        {
            if (PoisonAreaRetrieval.CurrentState != IDocPoisonAreaRetrievalService.State.Success)
            {
                // Data has not been retrieved
                switch (PoisonAreaRetrieval.CurrentState)
                {
                    case IDocPoisonAreaRetrievalService.State.Waiting:
                        throw new Exception("Doc Poison Areas have not yet been requested the server.");
                    case IDocPoisonAreaRetrievalService.State.RequestInProgress:
                        throw new Exception("Doc Poison Areas are still being downloaded from the server. Wait until the poison areas appear on the map.");
                    case IDocPoisonAreaRetrievalService.State.Error:
                        throw new Exception("Doc Posion Areas encountered an error while attempting to download from the server.");
                }

            }
        }
        
        List<PointIntersection> intersections = new();
        foreach (FlightData flight in flightData)
        {
            intersections.AddRange(ProximityChecker.FindMarksWithinDistanceOfGeometry(flight.animalMarks, poisonAreas, MINIMUM_DISTANCE_ALLOWED_FROM_POISON_ZONE_METERS));   
        }

        foreach (PointIntersection intersection in intersections)
        {
            throw new Exception (intersection.ToString());
        }
    }

    /// <summary>
    /// For each provided flight data, check the time between the refridgeration and the first kill and throws an error if it is greater than <see cref="MAXIMUM_REFRIDGERATION_HOURS"/>.
    /// Only call this method if <see cref="startTime"/> and <see cref="refrigerationTime"/> is not null.
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="RefridgerationTimeException"></exception>
    public void ValidateRefridgerationTime(List<FlightData> flightData)
    {
        foreach (FlightData data in flightData)
        {
            if (data.startTime == null)
            {
                throw new NullReferenceException("No start time could be parsed.");
            }

            if (data.refrigerationTime == null)
            {
                throw new NullReferenceException("No Refridgeration Time found.");
            }

            DateTime start = (DateTime) data.startTime;
            DateTime fridge = (DateTime) data.refrigerationTime;

            // Ensure that the refrigeration time is later than the start time
            if (DateTime.Compare(start, fridge) > 0)
            {
                throw new RefridgerationTimeException("Refridgeration time cannot be earlier than the start time.");        
            }

            TimeSpan difference = (fridge - start);
            double hoursUntilFridge = difference.TotalHours;

            if (hoursUntilFridge > MAXIMUM_REFRIDGERATION_HOURS)
            {
                throw new RefridgerationTimeException($"Time between first kill and refridgeration cannot exceed {MAXIMUM_REFRIDGERATION_HOURS} hours (It is {difference.Hours}:{difference.Minutes})");
            }
        }
    }


    [System.Serializable]
    public class RefridgerationTimeException : System.Exception
    {
        public RefridgerationTimeException() { }
        public RefridgerationTimeException(string message) : base(message) { }
        public RefridgerationTimeException(string message, System.Exception inner) : base(message, inner) { }
    }
}