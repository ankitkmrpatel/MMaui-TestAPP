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
        public string IconGlyph { get; set; } = null!;        // e.g. "\uf0f3"
        public string IconFontFamily { get; set; } = "FA";   // font alias registered in MauiProgram
        public string BackgroundImage { get; set; } = null!;  // file name or url
        public Color SolidColor { get; set; } = Colors.White;
        public (Color from, Color to)? GradientColors { get; set; } = null;
        public double ImageOpacity { get; set; } = 0.6;

        public override VisualNode Render()
        {
            // single-cell overlay grid (back card peeking + front card + optional overlapping badge)
            IEnumerable<MauiControls.RowDefinition> rows = [new MauiControls.RowDefinition(GridLength.Star)];
            IEnumerable<MauiControls.ColumnDefinition> cols = [new MauiControls.ColumnDefinition(GridLength.Star)];

            // BACK card: shifted down-right so it peeks from behind the front card
            var backCard = new Frame
            {
                new Grid
                {
                    // back badge placeholder (this is inside back card)
                    new BoxView()
                        .WidthRequest(36)
                        .HeightRequest(24)
                        .CornerRadius(12)
                        .BackgroundColor(Colors.DarkGray)
                        .Margin(8, 6, 0, 0)
                }
            }
            .HasShadow(false)
            .CornerRadius(10)
            .Padding(0)
            .BackgroundColor(Colors.DarkGray)
            .Margin(18, 18, 0, 0)
            .GridRow(0)
            .GridColumn(0);

            // FRONT card
            var front = BuildFront().GridRow(0).GridColumn(0);

            // Badge/capsule that should be half-in half-out (placed above front, after front so z-order is top)
            // We compute margin so the badge sits roughly near the left-top of back card and overlaps front slightly.
            var overlapBadge = new Frame
            {
                new Label()
                    .Text(() => "\uf058" ) // example glyph (replace with desired)
                    //.Text(() => IconGlyph ?? string.Empty)
                    .FontFamily(() => IconFontFamily)
                    .FontSize(12)
                    .HorizontalOptions(MauiControls.LayoutOptions.Center)
                    .VerticalOptions(MauiControls.LayoutOptions.Center)
            }
            .CornerRadius(10)
            .Padding(6, 4)
            .HasShadow(false)
            .BackgroundColor(Colors.DarkGray)
            // Negative left/top margin would require absolute layout; instead we place with a small offset:
            // set margin so it's visually between back and front; adjust numbers to taste
            .Margin(10, 120, 0, 0)
            .GridRow(0)
            .GridColumn(0)
            .IsVisible(() => true); // toggle if needed

            // Tilted glyph (watermark) for Outline variant: move inside the front card overlay rather than top-level grid
            // For clear z-ordering and positioning the glyph is rendered inside the front content (see BuildFront).

            return new Grid(rows, cols)
            {
                backCard,
                front,
                // add the overlap badge AFTER front so it appears above front (z-order)
                //overlapBadge
            }
            //.RowDefinitions(new[] { GridLength.Star })
            //.ColumnDefinitions(new[] { GridLength.Star })
            .HeightRequest(160)
            .WidthRequest(320);
        }

        Frame BuildFront()
        {
            // overlay content: title, description, footer
            IEnumerable<MauiControls.RowDefinition> rows = [new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto)];
            IEnumerable<MauiControls.ColumnDefinition> cols = [new(GridLength.Star)];

            IEnumerable<MauiControls.ColumnDefinition> footerCols = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)];

            // Build the content overlay grid (title / desc / footer)
            var overlay = new Grid(rows, [new(GridLength.Star)])
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
                    .Opacity(0.95)
                    .Margin(12, 0, 12, 6)
                    .GridRow(1)
                    .GridColumn(0),

                // Footer row (row 2): left icon, spacer, three-dots button
                new Grid([], footerCols)
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

            // Create a container grid that contains the overlay + the tilted icon (so the tilted icon is guaranteed to be part of front)
            var frontContentGrid = new Grid([new(GridLength.Star)], [new(GridLength.Star)])
            {
                // overlay fills grid
                overlay.GridRow(0).GridColumn(0)
            };

            // Add tilted glyph inside the frontContentGrid, bottom-right aligned, only for Outline variant
            frontContentGrid.AddChildren(
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
                    .GridColumn(0)
            );

            // front card appearance based on variant
            switch (Variant)
            {
                case CardVariant.Outline:
                    return new Frame()
                    {
                        frontContentGrid
                    }
                    .CornerRadius(10)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(Colors.White)
                    .BorderColor(Colors.LightGray)
                    // bring front slightly up-left so back peeks down-right
                    .Margin(0, 0, 18, 18);

                case CardVariant.Solid:
                    return new Frame()
                    {
                        frontContentGrid
                    }
                    .CornerRadius(10)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(SolidColor)
                    .BorderColor(Colors.Transparent)
                    .Margin(0, 0, 18, 18);

                case CardVariant.Gradient:
                    var (from, to) = GradientColors ?? (Colors.MediumPurple, Colors.LightBlue);
                    var mid = Blend(from, to, 0.5f);
                    return new Frame()
                    {
                        frontContentGrid
                    }
                    .CornerRadius(10)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(mid)
                    .BorderColor(Colors.Transparent)
                    .Margin(0, 0, 18, 18);

                case CardVariant.ImageBackground:
                    // image as background with overlay content; apply ImageOpacity to the image so overlay text is readable
                    var imageGrid = new Grid([new(GridLength.Star)], [new(GridLength.Star)])
                    {
                        // background image (first child)
                        new Image()
                            .Source(() =>
                            {
                                if (string.IsNullOrEmpty(BackgroundImage)) return null!;
                                if (BackgroundImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                    return MauiControls.ImageSource.FromUri(new Uri(BackgroundImage));
                                return MauiControls.ImageSource.FromFile(BackgroundImage);
                            })
                            .Aspect(Aspect.AspectFill)
                            .Opacity(ImageOpacity)
                            .GridRow(0)
                            .GridColumn(0),

                        // overlay above image
                        overlay.GridRow(0).GridColumn(0)
                    };

                    // add tilted glyph (if outline-like behaviour required don't show it here; keep hidden)
                    // (we won't show tilted glyph for image background by default)

                    return new Frame()
                    {
                        imageGrid
                    }
                    .CornerRadius(10)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(Colors.White)
                    .Margin(0, 0, 18, 18);

                default:
                    return new Frame()
                    {
                        frontContentGrid
                    }
                    .CornerRadius(10)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(Colors.White)
                    .Margin(0, 0, 18, 18);
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
    public partial class CardDemoPage : Component
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
                    }.HeightRequest(180),

                    new ContentView()
                    {
                        new CardView
                        {
                            Title = "Solid Card",
                            Description = "No icons on this one",
                            Variant = CardVariant.Solid,
                            SolidColor = Colors.LightGreen
                        }
                    }.HeightRequest(180),

                    new ContentView()
                    {
                        new CardView
                        {
                            Title = "Gradient Card",
                            Description = "Front card with gradient-like background",
                            Variant = CardVariant.Gradient,
                            GradientColors = (Colors.Orange, Colors.Purple)
                        }
                    }.HeightRequest(180),

                    new ContentView()
                    {
                        new CardView
                        {
                            Title = "Image Background",
                            Description = "Using an image as card background",
                            Variant = CardVariant.ImageBackground,
                            BackgroundImage = "https://images.unsplash.com/photo-1503023345310-bd7c1de61c7d?w=800&q=80"
                        }
                    }.HeightRequest(180),
                }
                .Spacing(12)
                .Padding(new Thickness(4)),
            };
        }
    }
}