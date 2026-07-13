using System.Collections.Generic;
using Deer_o_matic.ViewModels;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Deer_o_matic.Models;

/// <summary>
/// Helper class for generating layers with Mapsui.
/// </summary>
public static class MapLayerTypes
{

    // Dictionaries mapping colours to who is using them
    private static readonly Dictionary<Color, IColourable?> AnimalMarkColourDictionary = new(){
        {Color.IndianRed, null},
        {Color.Azure, null},
        {Color.Yellow, null},
        {Color.ForestGreen, null},
        {Color.White, null},
        {Color.Aquamarine, null},
        {Color.Beige, null},
        {Color.BurlyWood, null},
        {Color.Crimson, null}
    };

    private static readonly Dictionary<Color, IColourable?> AreaColorDictionary = new() {
        {Color.Red, null},
        {Color.Green, null},
        {Color.Blue, null},
        {Color.Yellow, null},
        {Color.Black, null},
        {Color.BlanchedAlmond, null},
        {Color.Purple, null},
    };

    public const int BACKGROUND_LAYER_INT = -1;
    public const int AREAS_LAYER_INT = 0;
    public const int PLACEMARKS_LAYER_INT = 1;


    public static MemoryLayer CreatePointLayer(string name, List<IFeature> features, Color color)
    {
        BaseStyle style = new SymbolStyle
        {
            SymbolScale = 0.75,
            Fill = new Brush(color)
        };

        return new MemoryLayer
        {
            Name = name,
            Features = features,
            Style = style
        };
    }

    public static MemoryLayer CreateTextLayer(string name, List<IFeature> features)
    {
        BaseStyle style = new LabelStyle
        {
            LabelColumn = "Name",

            VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
            
            Font = new Mapsui.Styles.Font { FontFamily = "Arial", Size = 8 },
            ForeColor = Color.Black,
            BackColor = new Brush(Color.White),
            
        };

        return new MemoryLayer
        {
            Name = name,
            Features = features,
            Style = style
        };

    }
   
    public static Layer CreateZoneLayer(string name, IEnumerable<IFeature> features, Color color)
    {
        // --- Styling:
        Brush brush = new Brush
        {
            Color = color,
            FillStyle = FillStyle.Solid
        };

        VectorStyle style = new()
        {
            Fill = brush,
            Outline = new Pen(Color.Black, width:1)
        };

        // --- Data Providers:
        MemoryProvider memoryProvider = new(features)
        {
            CRS = "EPSG:4326"
        };

        ProjectingProvider dataSource = new (memoryProvider)
        {
            CRS = "EPSG:3857"
        };
        
        // --- Layer:
        Layer layer = new ()
        {
            Name = name,
            DataSource = dataSource,
            Style = style,
            Opacity = 0.5,
            MaxVisible = 35,
            Enabled = true
        };

        return layer;
    }
    

    #region Colour Methods
    public static Color RequestAnimalMarkColor(IColourable requester)
    {
        return RequestLayerColour(AnimalMarkColourDictionary, requester);
    }

    public static void ReleaseAnimalMarkColor(IColourable releaser)
    {
        ReleaseLayerColour(AnimalMarkColourDictionary, releaser);
    }

    public static Color RequestAreaColor(IColourable requester)
    {
        return RequestLayerColour(AreaColorDictionary, requester);
    }

    public static void ReleaseAreaColor(IColourable releaser)
    {
        ReleaseLayerColour(AreaColorDictionary, releaser);
    }

    private static Color RequestLayerColour(Dictionary<Color, IColourable?> dict, IColourable requester)
    {
        foreach (Color color in dict.Keys)
        {
            if (dict[color] == null)
            {
                // Not in use
                dict[color] = requester;
                return color;
            }
        }

        return Color.Magenta;
    }

    private static void ReleaseLayerColour(Dictionary<Color, IColourable?> dict, IColourable releaser)
    {
        foreach (Color color in dict.Keys)
        {
            if (dict[color] == releaser)
            {
                dict[color] = null;
            }
        }
    }
    #endregion
}

public interface IColourable
{
    
}