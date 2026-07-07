using System.Collections.Generic;
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
   
    public static BaseLayer CreateZoneLayer(string name, IEnumerable<IFeature> features)
    {
        // --- Styling:
        Brush brush = new Brush
        {
            Color = Color.Green,
            // Background = Color.Red,
            FillStyle = FillStyle.Solid
        };

        VectorStyle style = new()
        {
            Fill = brush,
            // Line = new Pen(Color.DarkRed, width:3),
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
            Enabled = true
        };

        return layer;
    }

}