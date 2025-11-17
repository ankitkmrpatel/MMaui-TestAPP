// IconRenderer.cs
using System;
using MauiReactor;
using Controls = Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace KryptNx.FlowNxt.App.UI;

    public enum IconFamily { FontAwesome, Fluent, Svg, Emoji, Unknown }

    public class IconDescriptor
    {
        public IconFamily Family { get; set; } = IconFamily.Unknown;
        public string Key { get; set; }           // e.g. "fa-heart", "heart.svg", or glyph string
        public string Glyph { get; set; }         // optional codepoint like "\uF004"
        public string FontFamily { get; set; }    // e.g. "FA6Solid" or "FluentUI"
        public int Size { get; set; } = 18;
        public Color? Color { get; set; } = null;
        public string SemanticText { get; set; } = null;
    }

public static class IconRenderer
{
    public const string FA_FAMILY = "FA6Solid";    // change to your registered font family
    public const string FLUENT_FAMILY = "FluentUI";

    public static VisualNode Render(IconDescriptor desc)
    {
        if (desc == null) return new Label("?").FontSize(12);

        var color = desc.Color ?? Colors.Black;
        var size = Math.Max(10, desc.Size);

        switch (desc.Family)
        {
            case IconFamily.FontAwesome:
            case IconFamily.Fluent:
                {
                    var family = desc.FontFamily ?? (desc.Family == IconFamily.FontAwesome ? FA_FAMILY : FLUENT_FAMILY);
                    var glyph = !string.IsNullOrEmpty(desc.Glyph) ? desc.Glyph : desc.Key ?? "?";
                    return new Label(glyph)
                        .FontFamily(family)
                        .FontSize(size)
                        .TextColor(color)
                        .HorizontalOptions(Controls.LayoutOptions.Center)
                        .VerticalOptions(Controls.LayoutOptions.Center)
                        .AutomationId(desc.SemanticText ?? desc.Key);
                }

            case IconFamily.Svg:
                {
                    // 'Key' should be URI or resource path to SVG; ensure runtime supports SVG rendering
                    return new Image(desc.Key)
                        .WidthRequest(size + 8)
                        .HeightRequest(size + 8)
                        .Aspect(Aspect.AspectFit)
                        .AutomationId(desc.SemanticText ?? desc.Key);
                }

            case IconFamily.Emoji:
                return new Label(desc.Key ?? "?")
                    .FontSize(size)
                    .AutomationId(desc.SemanticText ?? desc.Key);

            default:
                return new Label(desc.Key ?? "?")
                    .FontSize(size)
                    .TextColor(color)
                    .AutomationId(desc.SemanticText ?? desc.Key);
        }
    }

    public static IconDescriptor FromDb(string family, string key, string glyph = null, string fontFamily = null, int size = 18, string colorHex = null, string semantic = null)
    {
        IconFamily f = IconFamily.Unknown;
        if (Enum.TryParse<IconFamily>(family, true, out var parsed)) f = parsed;

        Color? c = null;
        if (!string.IsNullOrWhiteSpace(colorHex))
        {
            try
            {
                var h = colorHex.TrimStart('#');
                if (h.Length == 6) h = "FF" + h;
                var u = Convert.ToUInt32(h, 16);
                c = Color.FromUint(u);
            }
            catch { c = null; }
        }

        return new IconDescriptor
        {
            Family = f,
            Key = key,
            Glyph = glyph,
            FontFamily = fontFamily,
            Size = size,
            Color = c,
            SemanticText = semantic
        };
    }
}

//// Models/CardEntity.cs
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//public class CardEntity
//{
//    [Key]
//    public int Id { get; set; }

//    public string Title { get; set; }
//    public string Description { get; set; }

//    // Icon storage fields
//    public string IconFamily { get; set; }        // e.g. "FontAwesome", "Fluent", "Svg"
//    public string IconKey { get; set; }           // e.g. "fa-heart", "heart.svg", or glyph string
//    public string IconGlyph { get; set; }         // optional explicit glyph like "\uF004"
//    public string IconFontFamily { get; set; }    // e.g. "FA6Solid"
//    public string IconColorHex { get; set; }      // e.g. "#FF0000"
//    public int IconSize { get; set; } = 18;

//    // background data (as JSON or columns)
//    public string BackgroundType { get; set; }
//    public string BackgroundJson { get; set; }    // optional: store gradient stops, image url etc.
//}
