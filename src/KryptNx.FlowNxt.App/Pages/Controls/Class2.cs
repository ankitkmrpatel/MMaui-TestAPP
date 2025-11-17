using MauiReactor;
using MauiReactor.Compatibility;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace KryptNx.FlowNxt.App.Components3
{
    public enum CardVariant
    {
        Outline,
        Solid,
        Gradient,
        ImageBackground
    }

    // Reactor component
    public partial class CardView : Component
    {
        public string Title { get; set; } = "Title";
        public string Description { get; set; } = "Description";
        public CardVariant Variant { get; set; } = CardVariant.Outline;
        public string IconGlyph { get; set; } = null!;        // e.g. "\uf0f3" (FontAwesome)
        public string IconFontFamily { get; set; } = "FA";   // registered alias
        public string BackgroundImage { get; set; } = null!;  // file name or url
        public Color SolidColor { get; set; } = Colors.White;
        public (Color from, Color to)? GradientColors { get; set; } = null;
        public double ImageOpacity { get; set; } = 0.36; // refined default
        public double CardHeight { get; set; } = 160;

        // Menu state is stored here for internal native popup rendering
        bool _isMenuOpen;

        // Callbacks
        public Action? OnEdit { get; set; }
        public Action? OnView { get; set; }
        public Action? OnDelete { get; set; }

        public override VisualNode Render()
        {
            // We'll return a native MAUI ContentView node inside Reactor. Reactor will host it as a native control.
            // BuildNativeCard returns a Microsoft.Maui.Controls.View (native) which Reactor will accept as a child.
            var nativeCard = BuildNativeCard();
            // Wrap in Reactor ContentView so margins behave like earlier examples
            return new ContentView
            {
                // Add the native control as a child; Reactor hosts native controls when placed inside ContentView.
                // Use the native instance's properties for sizing.
                nativeCard
            }
            .HeightRequest(CardHeight + 6); // keep some vertical spacing in Reactor layout
        }

        public VisualNode BuildNativeCard()
        {
            // responsive width: device logical width minus some padding
            var deviceWidth = GetDeviceLogicalWidth();
            var cardWidth = Math.Max(280, deviceWidth - 24);

            // BACK card content grid (contains the back badge placeholder & menu button at bottom-right)
            var backCardGrid = new Grid
            {
                // small placeholder capsule near top-left of back card
                new BoxView()
                    .WidthRequest(36)
                    .HeightRequest(24)
                    .CornerRadius(12)
                    .BackgroundColor(Colors.DarkGray)
                    .Margin(8, 6, 0, 0)
                    .GridRow(0)
                    .GridColumn(0)
            };

            // three-dots moved to back card bottom-right
            var backMenuBtn = new Button()
                .Text("⋯")
                .FontSize(18)
                .BackgroundColor(Colors.Transparent)
                .HorizontalOptions(MauiControls.LayoutOptions.End)
                .VerticalOptions(MauiControls.LayoutOptions.End)
                .Margin(0, 0, 8, 8)
                .OnClicked(() =>
                {
                    _isMenuOpen = true;
                    Invalidate(); // request re-render
                });

            backCardGrid.AddChildren(backMenuBtn);

            var backCard = new Frame
            {
                backCardGrid
            }
            .HasShadow(false)
            .CornerRadius(10)
            .Padding(0)
            .BackgroundColor(Colors.LightGray)
            .Margin(18, 18, 0, 0)
            .WidthRequest(cardWidth)
            .HeightRequest(CardHeight)
            .GridRow(0)
            .GridColumn(0);

            // FRONT card
            var front = BuildFront(cardWidth).GridRow(0).GridColumn(0);

            // Overlapping badges using AbsoluteLayout for pixel-perfect positioning
            var badgeLayout = new AbsoluteLayout()
                .WidthRequest(cardWidth)
                .HeightRequest(CardHeight)
                .GridRow(0)
                .GridColumn(0);

            //// heart (red on white) and bell (gray on white)
            //var heartBadge = MakeCircularBadge("\uf004", IconFontFamily, 28, Colors.Red, Colors.White)
            //    .OnTapped(() =>
            //    {
            //        System.Diagnostics.Debug.WriteLine("Heart tapped");
            //    });

            //var bellBadge = MakeCircularBadge("\uf0f3", IconFontFamily, 28, Colors.Gray, Colors.White)
            //    .OnTapped(() =>
            //    {
            //        System.Diagnostics.Debug.WriteLine("Bell tapped");
            //    });

            //// Place badges half-outside (negative X) and positioned vertically (tweak as needed)
            //badgeLayout.AddChildren(
            //    heartBadge.Bounds(BoundsHelper.Create(-18, 14, 36, 36)),
            //    bellBadge.Bounds(BoundsHelper.Create(-18, 60, 36, 36))
            //);

            // main root: back, front, badges overlay
            var rootGrid = new Grid([new(GridLength.Star)], [new(GridLength.Star)])
            {
                backCard,
                front,
                badgeLayout
            }
            .HeightRequest(CardHeight)
            .WidthRequest(cardWidth);

            // If menu is open, render the Reactor popup overlay (top-level overlay anchored near the back card's menu button)
            // We'll render overlay as part of this component — it will draw above main content.
            if (_isMenuOpen)
            {
                // compute popup position relative to card: place slightly above bottom-right of backCard
                // crude pixel coords (you can calculate more precisely using device metrics / Layout)
                var popupLeft = cardWidth - 160 - 8; // pop width 160
                var popupTop = CardHeight - 120; // upward offset so popup sits above the button

                var menuOverlay = MakeMenuPopup(
                    onEdit: () =>
                    {
                        _isMenuOpen = false;
                        Invalidate();
                        OnEdit?.Invoke();
                    },
                    onView: () =>
                    {
                        _isMenuOpen = false;
                        Invalidate();
                        OnView?.Invoke();
                    },
                    onDelete: () =>
                    {
                        _isMenuOpen = false;
                        Invalidate();
                        OnDelete?.Invoke();
                    },
                    popupLeft: popupLeft,
                    popupTop: popupTop
                );

                // Add menu overlay to root: AbsoluteLayout overlay covering full card area so shadow/backdrop possible
                // We'll wrap rootGrid into an AbsoluteLayout to place popup precisely.
                var overlay = new AbsoluteLayout
                {
                    // the card itself
                    //rootGrid.Bounds(BoundsHelper.Create(0, 0, cardWidth, CardHeight)),

                    // popup menu (above)
                    //menuOverlay.Bounds(BoundsHelper.Create(popupLeft, popupTop, 160, 110))
                }
                .WidthRequest(cardWidth)
                .HeightRequest(CardHeight);

                // Return overlay that contains card and popup on top
                return overlay;
            }

            return rootGrid;
        }

        // Build front portion (title/desc/footer/watermark) - returns Frame as front card
        Frame BuildFront(double cardWidth)
        {
            // overlay content: title, description, footer
            IEnumerable<MauiControls.RowDefinition> rows = [new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto)];
            IEnumerable<MauiControls.ColumnDefinition> cols = [new(GridLength.Star)];

            IEnumerable<MauiControls.ColumnDefinition> footerCols = [new(GridLength.Auto), new(GridLength.Star), new(GridLength.Auto)];

            // overlay content grid
            var overlay = new Grid(rows, cols)
            {
                // Title
                new Label()
                    .Text(() => Title ?? "No Title")
                    .FontSize(20)
                    .FontAttributes(MauiControls.FontAttributes.Bold)
                    .Margin(12, 8, 12, 2)
                    .GridRow(0)
                    .GridColumn(0),

                // Description
                new Label()
                    .Text(() => Description ?? "No Description")
                    .FontSize(13)
                    .Opacity(0.95)
                    .Margin(12, 0, 12, 6)
                    .GridRow(1)
                    .GridColumn(0),

                // Footer (left icon + spacer). Three-dots removed from front as requested.
                new Grid([], footerCols)
                {
                    new Label()
                        .Text(() => IconGlyph ?? string.Empty)
                        .FontFamily(() => IconFontFamily ?? "FA")
                        .FontSize(16)
                        .VerticalOptions(MauiControls.LayoutOptions.Center)
                        .IsVisible(() => !string.IsNullOrEmpty(IconGlyph))
                        .GridColumn(0),

                    new BoxView()
                        .BackgroundColor(Colors.Transparent)
                        .GridColumn(1),
                }
                .GridRow(2)
                .GridColumn(0)
            };

            // front content grid with watermark (tilted icon using FontImageSource for higher fidelity)
            var frontContentGrid = new Grid([new(GridLength.Star)], [new(GridLength.Star)])
            {
                // overlay fills grid
                overlay.GridRow(0).GridColumn(0)
            };

            // Tilted watermark using FontImageSource inside an Image control
            var watermark = new Image()
                .Source(() => CreateFontImageSource(IconGlyph, IconFontFamily, 40, Colors.Black))
                .Rotation(20)
                .HorizontalOptions(MauiControls.LayoutOptions.End)
                .VerticalOptions(MauiControls.LayoutOptions.End)
                .Margin(0, 0, 8, 8)
                .Opacity(0.12)
                .IsVisible(() => Variant == CardVariant.Outline && !string.IsNullOrEmpty(IconGlyph));

            frontContentGrid.AddChildren(watermark);

            // Create front Frame based on variant
            switch (Variant)
            {
                case CardVariant.Outline:
                    return CreateFrame(frontContentGrid, Colors.White, Colors.LightGray, 10, cardWidth);

                case CardVariant.Solid:
                    var vibrant = SolidColor;
                    var border = Blend(vibrant, Colors.Black, 0.08f);
                    return new Frame
                    {
                        frontContentGrid
                    }
                    .CornerRadius(10)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(vibrant)
                    .BorderColor(border)
                    .Margin(0, 0, 18, 18)
                    .WidthRequest(cardWidth)
                    .HeightRequest(CardHeight);

                case CardVariant.Gradient:
                    var (from, to) = GradientColors ?? (Colors.MediumPurple, Colors.LightBlue);
                    var brush = new MauiControls.LinearGradientBrush([new(from, 0), new(to, 1)], new Point(0, 0), new Point(1, 1));

                    var gradientGrid = new Grid([new(GridLength.Star)], [new(GridLength.Star)])
                    {
                        frontContentGrid
                    }
                    .Background(brush);

                    return new Frame
                    {
                        gradientGrid
                    }
                    .CornerRadius(10)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(Colors.Transparent)
                    .BorderColor(Colors.Transparent)
                    .Margin(0, 0, 18, 18)
                    .WidthRequest(cardWidth)
                    .HeightRequest(CardHeight);

                case CardVariant.ImageBackground:
                    // build background image with refined opacity and semi-dark overlay
                    MauiControls.ImageSource? isrc = null!;
                    if (!string.IsNullOrEmpty(BackgroundImage))
                    {
                        if (BackgroundImage.StartsWith("http", StringComparison.OrdinalIgnoreCase) || BackgroundImage.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                            isrc = MauiControls.ImageSource.FromUri(new Uri(BackgroundImage));
                        else
                            isrc = MauiControls.ImageSource.FromFile(BackgroundImage);
                    }

                    var imageGrid = new Grid([new(GridLength.Star)], [new(GridLength.Star)])
                    {
                        new Image()
                            .Source(() => isrc)
                            .Aspect(Aspect.AspectFill)
                            .Opacity(ImageOpacity)
                            .GridRow(0)
                            .GridColumn(0),

                        // semi-dark overlay to boost contrast
                        new BoxView()
                            .Background(new MauiControls.LinearGradientBrush(
                                [
                                    new(Color.FromRgba(0,0,0,0.28f), 0),
                                    new(Color.FromRgba(0,0,0,0.08f), 1)
                                ],
                                new Point(0,0),
                                new Point(0,1)
                            ))
                            .GridRow(0)
                            .GridColumn(0),

                        overlay.GridRow(0).GridColumn(0)
                    };

                    return new Frame
                    {
                        imageGrid
                    }
                    .CornerRadius(10)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(Colors.Transparent)
                    .Margin(0, 0, 18, 18)
                    .WidthRequest(cardWidth)
                    .HeightRequest(CardHeight);

                default:
                    return CreateFrame(frontContentGrid, Colors.White, Colors.LightGray, 10, cardWidth);
            }
        }

        // Create a simple Frame helper
        Frame CreateFrame(VisualNode content, Color bg, Color border, float cornerRadius, double width)
        {
            return new Frame
            {
                content
            }
            .CornerRadius(cornerRadius)
            .HasShadow(true)
            .Padding(0)
            .BackgroundColor(bg)
            .BorderColor(border)
            .Margin(0, 0, 18, 18)
            .WidthRequest(width)
            .HeightRequest(CardHeight);
        }

        // Create FontImageSource for high-fidelity glyphs
        MauiControls.FontImageSource CreateFontImageSource(string glyph, string fontFamily, int size, Color color)
        {
            return new MauiControls.FontImageSource
            {
                Glyph = glyph ?? string.Empty,
                FontFamily = fontFamily,
                Size = size,
                Color = color
            };
        }

        // Build circular badge (Frame with Image inside)
        VisualNode MakeCircularBadge(string glyph, string fontFamily, double outerSize, Color glyphColor, Color background)
        {
            var img = new Image()
                .Source(() => CreateFontImageSource(glyph, fontFamily, (int)(outerSize - 10), glyphColor))
                .HorizontalOptions(MauiControls.LayoutOptions.Center)
                .VerticalOptions(MauiControls.LayoutOptions.Center);

            return new Frame
            {
                img
            }
            .CornerRadius((float)(outerSize / 2))
            .Padding(6)
            .HasShadow(false)
            .BackgroundColor(background)
            .BorderColor(Colors.Transparent)
            .WidthRequest(outerSize)
            .HeightRequest(outerSize);
        }

        // Reactor-rendered popup menu (no external libraries)
        VisualNode MakeMenuPopup(Action onEdit, Action onView, Action onDelete, double popupLeft, double popupTop)
        {
            // Popup content: small white framed menu with 3 buttons
            var popupContent = new Frame
            {
                new VerticalStackLayout
                {
                    new Button().Text("Edit").BackgroundColor(Colors.Transparent).OnClicked(() => onEdit()),
                    new BoxView().HeightRequest(1).BackgroundColor(Color.FromRgba(0,0,0,0.06f)),
                    new Button().Text("View").BackgroundColor(Colors.Transparent).OnClicked(() => onView()),
                    new BoxView().HeightRequest(1).BackgroundColor(Color.FromRgba(0,0,0,0.06f)),
                    new Button().Text("Delete").BackgroundColor(Colors.Transparent).OnClicked(() => onDelete()),
                }
                .Padding(0)
                .Spacing(0)
            }
            .CornerRadius(8)
            .HasShadow(true)
            .BackgroundColor(Colors.White)
            .BorderColor(Color.FromRgba(0, 0, 0, 0.06f))
            .WidthRequest(160)
            .HeightRequest(110);

            // Add a semi-transparent overlay as well to capture taps outside (we place it adjacent in AbsoluteLayout)
            // But since we're returning only the menu VisualNode (and positioned by parent AbsoluteLayout), capture outside taps via the parent,
            // so we'll return popupContent itself; parent will include entire AbsoluteLayout to cover card area.

            return popupContent;
        }

        // Simple helpers
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

        static double GetDeviceLogicalWidth()
        {
            var info = DeviceDisplay.MainDisplayInfo;
            return info.Width / info.Density;
        }

        // Allow tapping on VisualNode via Reactor extension
        // Note: The small extension below attaches a "Tapped" attribute to the node; map it to GestureRecognizer in your Reactor runtime as needed.
    }

    // small helper to create Bounds for AbsoluteLayout usage
    //static class BoundsHelper
    //{
    //    // left, top, width, height (device independent)
    //    public static Rectangle Create(double left, double top, double width, double height) => new Rectangle(left, top, width, height);
    //}

    // Reactor-compatible extension methods (basic)
    //static class ReactorExtensions
    //{
    //    public static VisualNode Bounds(this VisualNode node, Rectangle bounds)
    //    {
    //        node.Attrib("AbsoluteLayout.Bounds", bounds);
    //        return node;
    //    }

    //    public static VisualNode OnTapped(this VisualNode node, Action handler)
    //    {
    //        node.Attrib("Tapped", handler);
    //        return node;
    //    }
    //}

    // Demo page
    public partial class CardDemoPage : Component
    {
        public override VisualNode Render()
        {
            return new ScrollView
            {
                new VerticalStackLayout
                {
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
                            SolidColor = Color.FromArgb("#4CAF50") // vibrant
                        }
                    }.HeightRequest(180),

                    new ContentView()
                    {
                        new CardView
                        {
                            Title = "Gradient Card",
                            Description = "Front card with gradient-like background",
                            Variant = CardVariant.Gradient,
                            GradientColors = (Color.FromArgb("#FF8A00"), Color.FromArgb("#E52E71"))
                        }
                    }.HeightRequest(180),

                    new ContentView()
                    {
                        new CardView
                        {
                            Title = "Image Background",
                            Description = "Using an image as card background",
                            Variant = CardVariant.ImageBackground,
                            BackgroundImage = "https://images.unsplash.com/photo-1503023345310-bd7c1de61c7d?w=800&q=80",
                            ImageOpacity = 0.32
                        }
                    }.HeightRequest(180)
                }
                .Spacing(12)
                .Padding(new Thickness(6))
            };
        }
    }
}