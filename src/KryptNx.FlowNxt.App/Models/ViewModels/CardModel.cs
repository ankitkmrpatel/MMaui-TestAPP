// StackedCards.cs
// MauiReactor components: CardFront, CardBack, CardStack + example MainPage usage.
// Put in your project and adapt namespaces as needed.

using Microsoft.Maui.Graphics;

namespace KryptNx.FlowNxt.App.Models.ViewModels
{
    public enum BackgroundType
    {
        Solid,
        Gradient,
        Icon,
        Image
    }

    public class CardModel
    {
        public string Id { get; set; }
        public string Title { get; set; }        // shown top-right above desc area
        public string Description { get; set; }  // big body text
        public BackgroundType BackgroundType { get; set; } = BackgroundType.Solid;

        // Solid
        public Color SolidColor { get; set; } = Colors.White;

        // Gradient
        public Color GradientStart { get; set; } = Colors.LightGray;
        public Color GradientEnd { get; set; } = Colors.DarkGray;

        // Icon (glyph or image name) - used when BackgroundType.Icon
        public string IconGlyph { get; set; } = "★";

        // Image
        public string ImageUrl { get; set; }
    }
}
