using MauiReactor;
using MauiReactor.Compatibility;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace KryptNx.FlowNxt.App.Components3
{
    public enum CardVariant
    {
        Outline,
        Solid,
        Gradient,
        ImageBackground
    }

    // Reactor component (non-generic) with public properties to set from caller
    public partial class CardView : Component
    {
        public string Title { get; set; } = "Title";
        public string Description { get; set; } = "Description";
        public CardVariant Variant { get; set; } = CardVariant.Outline;
        public string IconGlyph { get; set; } = null;        // e.g. "\uf0f3"
        public string IconFontFamily { get; set; } = "FA";   // font alias registered in MauiProgram
        public string BackgroundImage { get; set; } = null;  // file name or url
        public Color SolidColor { get; set; } = Colors.White;
        public (Color from, Color to)? GradientColors { get; set; } = null;

        public override VisualNode Render()
        {
            // single-cell overlay grid (back card peeking + front card + tilted glyph)
            IEnumerable<MauiControls.RowDefinition> rows = [new MauiControls.RowDefinition(GridLength.Star)];
            IEnumerable<MauiControls.ColumnDefinition> cols = [new MauiControls.ColumnDefinition(GridLength.Star)];
            return new Grid(rows, cols)
            {
                // BACK card (peeking)
                new Frame()
                {
                    new Grid
                    {
                        // little badge that visually peeks on top-left of back card
                        new BoxView()
                            .WidthRequest(36)
                            .HeightRequest(24)
                            .CornerRadius(12)
                            .BackgroundColor(Colors.DarkGray)
                            .Margin(8, 6, 0, 0)
                    }
                }
                .HasShadow(false)
                .CornerRadius(12)
                .Padding(0)
                .BackgroundColor(Colors.LightGray)
                .GridRow(0)
                .GridColumn(0),

                // FRONT card (main)
                BuildFront().GridRow(0).GridColumn(0),

                // Tilted glyph watermark on bottom-right (Outline variant)
                new Label()
                    .Text(() => IconGlyph ?? string.Empty)
                    .FontFamily(() => IconFontFamily ?? "FA")
                    .FontSize(40)
                    .Rotation(20)
                    .HorizontalOptions(MauiControls.LayoutOptions.End)
                    .VerticalOptions(MauiControls.LayoutOptions.End)
                    .Margin(0, 0, 8, 8)
                    .Opacity(0.12)
                    .IsVisible(() => Variant == CardVariant.Outline && !string.IsNullOrEmpty(IconGlyph))
                    .GridRow(0)
                    .GridColumn(0),
            }
            //.RowDefinitions(new[] { GridLength.Star })   // single row full
            //.ColumnDefinitions(new[] { GridLength.Star }) // single column full
            .HeightRequest(160) // default sizing - change as needed
            .WidthRequest(320);
        }

        Frame BuildFront()
        {
            // overlay content: title, description, footer
            IEnumerable<MauiControls.RowDefinition> rows = [new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto)];
            IEnumerable<MauiControls.ColumnDefinition> cols = [new(GridLength.Star)];

            IEnumerable<MauiControls.RowDefinition> footerrows = [];
            IEnumerable<MauiControls.ColumnDefinition> footercols = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)];
            var overlay = new Grid(rows, cols)
            {
                // Title (row 0)
                new Label()
                    .Text(() => Title ?? "No Title")
                    .FontSize(20)
                    .FontAttributes(MauiControls.FontAttributes.Bold)
                    .Margin(12, 8, 12, 2)
                    .GridRow(0)
                    .GridColumn(0),

                // Description (row 1)
                new Label()
                    .Text(() => Description ?? "No Description")
                    .FontSize(13)
                    .Opacity(0.85)
                    .Margin(12, 0, 12, 6)
                    .GridRow(1)
                    .GridColumn(0),


                // Footer row (row 2): left icon, spacer, three-dots button
                new Grid(footerrows, footercols)
                {
                    new Label()
                        .Text(() => IconGlyph ?? string.Empty)
                        .FontFamily(() => IconFontFamily ?? "FA")
                        .FontSize(16)
                        .VerticalOptions(MauiControls.LayoutOptions.Center)
                        .IsVisible(() => !string.IsNullOrEmpty(IconGlyph))
                        .GridColumn(0),

                    // spacer: empty BoxView expands due to Star column
                    new BoxView()
                        .BackgroundColor(Colors.Transparent)
                        .GridColumn(1),

                    new Button()
                        .Text("⋯")
                        .BackgroundColor(Colors.Transparent)
                        .OnClicked(() => OnMenuClicked())
                        .GridColumn(2)
                }
                .GridRow(2)
                .GridColumn(0)
            };

            // front card appearance based on variant
            switch (Variant)
            {
                case CardVariant.Outline:
                    return new Frame()
                    {
                        overlay
                    }
                    .CornerRadius(12)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(Colors.White)
                    .BorderColor(Colors.LightGray);

                case CardVariant.Solid:
                    return new Frame()
                    {
                        overlay
                    }
                    .CornerRadius(10)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(SolidColor)
                    .BorderColor(Colors.Transparent);

                case CardVariant.Gradient:
                    // Reactor v3 may not provide cross-platform LinearGradientBrush in all targets,
                    // so we fallback to a midpoint solid color that compiles everywhere.
                    var (from, to) = GradientColors ?? (Colors.MediumPurple, Colors.LightBlue);
                    var mid = Blend(from, to, 0.5f);
                    return new Frame()
                    {
                        overlay
                    }
                    .CornerRadius(10)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(mid)
                    .BorderColor(Colors.Transparent);

                case CardVariant.ImageBackground:
                    // image as background with overlay content
                    return new Frame()
                    {
                        new Grid
                        {
                            // background image
                            new Image()
                                .Source(() => {
                                    if (string.IsNullOrEmpty(BackgroundImage)) return null!;
                                    if (BackgroundImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                        return MauiControls.ImageSource.FromUri(new Uri(BackgroundImage));
                                    return MauiControls.ImageSource.FromFile(BackgroundImage);
                                })
                                .Aspect(Aspect.AspectFill)
                                .GridRow(0)
                                .GridColumn(0),

                            // overlay content above the image
                            overlay.GridRow(0).GridColumn(0)
                        }
                    }
                    .CornerRadius(10)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(Colors.Transparent);

                default:
                    return new Frame()
                    {
                        overlay
                    }
                    .CornerRadius(10)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(Colors.White);
            }
        }

        void OnMenuClicked()
        {
            // stub (user can replace with a Popup/ContextMenu)
            System.Diagnostics.Debug.WriteLine("Card: menu clicked");

        }

        static Color Blend(Color a, Color b, float t)
        {
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            return Color.FromRgba(
                a.Red + (b.Red - a.Red) * t,
                a.Green + (b.Green - a.Green) * t,
                a.Blue + (b.Blue - a.Blue) * t,
                a.Alpha + (b.Alpha - a.Alpha) * t
            );
        }
    }

    // Demo page showing how to use CardView within Reactor render
    public class CardDemoPage : Component
    {
        public override VisualNode Render()
        {
            return new ScrollView
            {
                new VerticalStackLayout
                {
                    
                    // Wrap CardView inside ContentView (so Reactor treats it as IView and margins work)
                    new ContentView()
                    {
                        new CardView
                        {
                            Title = "My Pending Actions",
                            Description = "This is short desc of this card.....",
                            Variant = CardVariant.Outline,
                            IconGlyph = "\uf0f3",
                            IconFontFamily = "FA"
                        }
                    }.HeightRequest(160),

                    new ContentView()
                    {
                        new CardView
                        {
                            Title = "Solid Card",
                            Description = "No icons on this one",
                            Variant = CardVariant.Solid,
                            SolidColor = Colors.LightGreen
                        }
                    }.HeightRequest(160),

                    new ContentView()
                    {
                        new CardView
                        {
                            Title = "Gradient Card",
                            Description = "Front card with gradient-like background",
                            Variant = CardVariant.Gradient,
                            GradientColors = (Colors.Orange, Colors.Purple)
                        }
                    }.HeightRequest(160),

                    new ContentView()
                    {
                        new CardView
                        {
                            Title = "Image Background",
                            Description = "Using an image as card background",
                            Variant = CardVariant.ImageBackground,
                            BackgroundImage = "https://images.unsplash.com/photo-1503023345310-bd7c1de61c7d?w=800&q=80"
                        }
                    }.HeightRequest(160),
                }
                .Spacing(12)
                .Padding(new Thickness(12)),
            };
        }
    }
}