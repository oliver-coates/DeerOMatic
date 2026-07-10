using System.Collections.Generic;
using Deer_o_matic.Models;
using Deer_o_matic.ViewModels;
using NetTopologySuite.Geometries;

namespace Deer_o_matic.Services;

public interface IPointProximityService
{
    public List<PointIntersection> FindMarksWithinDistanceOfGeometry(IList<AnimalMark> animalMarks, IList<AreaDataViewModel> areaData, double distanceM);
}

public class PointProximityService : IPointProximityService
{

    private readonly GeometryFactory _geoFactory = new();

    public List<PointIntersection> FindMarksWithinDistanceOfGeometry(IList<AnimalMark> animalMarks, IList<AreaDataViewModel> areaData, double distanceM)
    {
        // This dictionary related each animal mark to each area data it is intersecting.
        Dictionary<AnimalMark, List<AreaDataViewModel>> intersectionDict = new();
        
        // Loop across each animal mark and find their intersections
        foreach (AnimalMark mark in animalMarks)
        {
            Point point = _geoFactory.CreatePoint(
                new Coordinate(mark.coordinates.X, mark.coordinates.Y)
            );

            // Add each intersection to the dictionary
            foreach (AreaDataViewModel area in areaData)
            {
                AddPointAreaIntersectionsToDict(intersectionDict, mark, point, area, distanceM);
            }
        }

        return CollectPointIntersections(intersectionDict);
    }


    private static void AddPointAreaIntersectionsToDict(Dictionary<AnimalMark, List<AreaDataViewModel>> intersectionDict, AnimalMark mark, Point point, AreaDataViewModel area, double distanceM)
    {
        foreach (Geometry geometry in area.geometry)
        {
            if (point.IsWithinDistance(geometry, distanceM))
            {
                RegisterIntersection(intersectionDict, mark, area);
            }
        }
    }

    private static void RegisterIntersection(Dictionary<AnimalMark, List<AreaDataViewModel>> intersectionDict, AnimalMark mark, AreaDataViewModel area)
    {
        // Check to see if the dictionary already contains an entry for this mark
        if (intersectionDict.ContainsKey(mark))
        {
            // If it does, check to see if this area has already been registered as an intersection
            if (intersectionDict[mark].Contains(area) == false)
            {
                // If it has add it, if not continue
                intersectionDict[mark].Add(area);
            }
        }
        else
        {
            // If the intersection dictionary has no record of this animal mark, add it and the area it is intersecting into it.
            intersectionDict.Add(mark, [area]);
        }
    }

    private static List<PointIntersection> CollectPointIntersections(Dictionary<AnimalMark, List<AreaDataViewModel>> intersectionDict)
    {
        List<PointIntersection> results = new();

        foreach (AnimalMark animalMark in intersectionDict.Keys)
        {
            AreaDataViewModel[] intersectingAreaViewModels = [.. intersectionDict[animalMark]];
            AreaData[] intersectingAreas = new AreaData[intersectingAreaViewModels.Length];
            
            for (int areaIndex = 0; areaIndex < intersectingAreaViewModels.Length; areaIndex++)
            {
                intersectingAreas[areaIndex] = intersectingAreaViewModels[areaIndex].Get();
            }

            results.Add(new PointIntersection(animalMark, intersectingAreas));
        }

        return results;
    }
}

public class PointIntersection
{
    public readonly AnimalMark mark;
    public readonly AreaData[] areas;

    public PointIntersection(AnimalMark mark, AreaData[] areas)
    {
        this.mark = mark;
        this.areas = areas;
    }

    public override string ToString()
    {
        string areaNames = "";
        foreach (AreaData area in areas)
        {
            areaNames += area.name + ",";
        }

        return $"Point Intersection : {mark.displayName} intersects with {areaNames}";
    }
}