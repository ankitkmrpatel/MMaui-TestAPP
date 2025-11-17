using MauiReactor;
using MauiReactor.Compatibility;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace KryptNx.FlowNxt.App.Components4
{
    public enum CardVariant
    {
        Outline,
        Solid,
        Gradient,
        ImageBackground
    }

    public class PopupSpec
    {
        public Action? OnEdit { get; set; }
        public Action? OnView { get; set; }
        public Action? OnDelete { get; set; }
    }

    public class CardView : Component
    {
        // Public properties (same as before)
        public string Title { get; set; } = "Title";
        public string Description { get; set; } = "Description";
        public CardVariant Variant { get; set; } = CardVariant.Outline;
        public string IconGlyph { get; set; } = "\uf0f3";
        public string IconFontFamily { get; set; } = "FA";
        public string BackgroundImage { get; set; } = string.Empty;
        public Color SolidColor { get; set; } = Colors.White;
        public (Color from, Color to)? GradientColors { get; set; } = null;
        public double ImageOpacity { get; set; } = 0.36;
        public double CardHeight { get; set; } = 160;

        // Callbacks
        public Action? OnEdit { get; set; }
        public Action? OnView { get; set; }
        public Action? OnDelete { get; set; }

        // Request for parent to show overlay popup; parent should render the popup content and handle closing.
        // PopupSpec contains the actions parent will invoke when popup buttons are tapped.
        public Action<PopupSpec>? RequestShowOverlay { get; set; }

        const float Corner = 10f;

        public override VisualNode Render()
        {
            // responsive width
            var deviceWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            var cardWidth = Math.Max(280, deviceWidth - 24);

            // BACK CARD (peeking)
            var backCard = new Frame()
                .HasShadow(false)
                .CornerRadius(Corner)
                .Padding(0)
                .BackgroundColor(Colors.LightGray)
                .Margin(22, 30, 0, 0)
                .WidthRequest(cardWidth)
                .HeightRequest(CardHeight)
                .GridRow(0)
                .GridColumn(0);

            // small capsule in back card top-left (peeking)
            var capsule = new BoxView()
                .WidthRequest(36)
                .HeightRequest(24)
                .CornerRadius(12)
                .BackgroundColor(Colors.DarkGray)
                .Margin(8, 6, 0, 0)
                .GridRow(0)
                .GridColumn(0);

            // make a menu request handler that packages the popup spec and asks parent to show it
            Action raisePopup = () =>
            {
                // Build PopupSpec with actions that will both invoke the CardView's callbacks (if any)
                // and can be used by the parent to perform additional logic.
                var spec = new PopupSpec
                {
                    OnEdit = () => OnEdit?.Invoke(),
                    OnView = () => OnView?.Invoke(),
                    OnDelete = () => OnDelete?.Invoke()
                };

                RequestShowOverlay?.Invoke(spec);
            };

            // menu button placed inside back card
            var backMenuBtn = new Button()
                .Text("⋯")
                .FontSize(18)
                .BackgroundColor(Colors.Transparent)
                .HorizontalOptions(MauiControls.LayoutOptions.End)
                .VerticalOptions(MauiControls.LayoutOptions.End)
                .Margin(0, 0, 4, 4)
                .Padding(0, 0, 8, 0)
                .OnClicked(() => raisePopup());

            //backInnerGrid.AddChildren(backMenuBtn);
            backCard.AddChildren(backMenuBtn);

            // FRONT CARD
            var front = BuildFront(cardWidth);

            // BADGES (half-in / half-out using negative left margin)
            var heartBadge = new Frame()
                .CornerRadius(18)
                .Padding(6)
                .HasShadow(false)
                .BackgroundColor(Colors.White)
                .BorderColor(Colors.Transparent)
                .WidthRequest(36)
                .HeightRequest(36)
                .Margin(-18, 14, 0, 0); // negative left -> half out

             heartBadge.AddChildren(
                    new Image().Source(() => CreateFontImageSource("\uf004", IconFontFamily, 20, Colors.Red))
                        .HorizontalOptions(MauiControls.LayoutOptions.Center)
                        .VerticalOptions(MauiControls.LayoutOptions.Center)
                );

            heartBadge.OnTapped(() => System.Diagnostics.Debug.WriteLine("Heart tapped"));

            // bell badge (gray on white)
            var bellBadge = new Frame()
                .CornerRadius(18)
                .Padding(6)
                .HasShadow(false)
                .BackgroundColor(Colors.White)
                .BorderColor(Colors.Transparent)
                .WidthRequest(36)
                .HeightRequest(36)
                .Margin(-18, 60, 0, 0);

            bellBadge.AddChildren(new Image()
                        .Source(() => CreateFontImageSource("\uf0f3", IconFontFamily, 20, Colors.Gray))
                        .HorizontalOptions(MauiControls.LayoutOptions.Center)
                        .VerticalOptions(MauiControls.LayoutOptions.Center)
                );

            bellBadge.OnTapped(() => System.Diagnostics.Debug.WriteLine("Bell tapped"));

            // ROOT layout: use Grid and layer back, front, badges
            var root = new Grid(new[] { new Microsoft.Maui.Controls.RowDefinition(GridLength.Star) }, new[] { new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star) })
            {
                backCard,
                front,
                heartBadge,
                bellBadge,
            }
            .HeightRequest(CardHeight)
            .WidthRequest(cardWidth);

            return root;
        }

        VisualNode BuildFront(double cardWidth)
        {
            // overlay content: title, desc, footer
            var overlay = new Grid(new[] { new Microsoft.Maui.Controls.RowDefinition(GridLength.Auto), new Microsoft.Maui.Controls.RowDefinition(GridLength.Auto), new Microsoft.Maui.Controls.RowDefinition(GridLength.Auto) }, new[] { new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star) })
            {
                // title
                new Label()
                    .Text(() => Title ?? "No Title")
                    .FontSize(20)
                    .FontAttributes(MauiControls.FontAttributes.Bold)
                    .Margin(12, 8, 12, 2)
                    .GridRow(0)
                    .GridColumn(0),

                // description
                new Label()
                    .Text(() => Description ?? "No Description")
                    .FontSize(13)
                    .Opacity(0.95)
                    .Margin(12, 0, 12, 6)
                    .GridRow(1)
                    .GridColumn(0),

                // footer: left icon + spacer; three-dots moved to back card
                new Grid([], new Microsoft.Maui.Controls.ColumnDefinition[] { new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Auto), new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star), new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Auto) })
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
                        .GridColumn(1)
                }
                .GridRow(2)
                .GridColumn(0)
            };

            // front content grid holds overlay + watermark glyph
            var frontContentGrid = new Grid(new[] { new Microsoft.Maui.Controls.RowDefinition(GridLength.Star) }, new[] { new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star) })
            {
                overlay.GridRow(0).GridColumn(0)
            };

            // watermark using FontImageSource -> Image.Source lambda
            frontContentGrid.AddChildren(
                new Image()
                    .Source(() => CreateFontImageSource(IconGlyph, IconFontFamily, 40, Colors.Black))
                    .Rotation(20)
                    .HorizontalOptions(MauiControls.LayoutOptions.End)
                    .VerticalOptions(MauiControls.LayoutOptions.End)
                    .Margin(0, 0, 8, 8)
                    .Opacity(0.12)
                    .IsVisible(() => Variant == CardVariant.Outline && !string.IsNullOrEmpty(IconGlyph))
                    .GridRow(0)
                    .GridColumn(0)
            );

            // Choose frame appearance based on Variant
            switch (Variant)
            {
                case CardVariant.Outline:
                    return new Frame()
                        {
                            frontContentGrid
                        }
                        .CornerRadius(Corner)
                        .HasShadow(true)
                        .Padding(0)
                        .BackgroundColor(Colors.White)
                        .BorderColor(Colors.LightGray)
                        .Margin(0, 0, 18, 18)
                        .WidthRequest(cardWidth)
                        .HeightRequest(CardHeight);

                case CardVariant.Solid:
                    return new Frame()
                        {
                            frontContentGrid
                        }
                        .CornerRadius(Corner)
                        .HasShadow(true)
                        .Padding(0)
                        .BackgroundColor(SolidColor)
                        .BorderColor(Blend(SolidColor, Colors.Black, 0.08f))
                        .Margin(0, 0, 18, 18)
                        .WidthRequest(cardWidth)
                        .HeightRequest(CardHeight);

                case CardVariant.Gradient:
                    var (from, to) = GradientColors ?? (Colors.MediumPurple, Colors.LightBlue);
                    // BoxView with gradient as background - overlay placed above it
                    var gradBox = new BoxView()
                        .Background(new MauiControls.LinearGradientBrush(
                            new MauiControls.GradientStopCollection
                            {
                                new MauiControls.GradientStop(from, 0f),
                                new MauiControls.GradientStop(to, 1f)
                            },
                            new Point(0, 0),
                            new Point(1, 1)
                        ))
                        .GridRow(0)
                        .GridColumn(0);

                    var gradientGrid = new Grid(new[] { new Microsoft.Maui.Controls.RowDefinition(GridLength.Star) }, new[] { new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star) })
                    {
                        gradBox,
                        overlay.GridRow(0).GridColumn(0)
                    };

                    return new Frame()
                        {
                            gradientGrid
                        }
                        .CornerRadius(Corner)
                        .HasShadow(true)
                        .Padding(0)
                        .BackgroundColor(Colors.Transparent)
                        .BorderColor(Colors.Transparent)
                        .Margin(0, 0, 18, 18)
                        .WidthRequest(cardWidth)
                        .HeightRequest(CardHeight);

                case CardVariant.ImageBackground:
                    return new Frame()
                    {
                        new Grid(new[] { new Microsoft.Maui.Controls.RowDefinition(GridLength.Star) }, new[] { new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star) })
                        {
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

                            new BoxView()
                                .Background(new MauiControls.LinearGradientBrush(
                                    new MauiControls.GradientStopCollection
                                    {
                                        new MauiControls.GradientStop(Color.FromRgba(0,0,0,0.28f), 0f),
                                        new MauiControls.GradientStop(Color.FromRgba(0,0,0,0.08f), 1f)
                                    },
                                    new Point(0,0),
                                    new Point(0,1)
                                ))
                                .GridRow(0)
                                .GridColumn(0),

                            overlay.GridRow(0).GridColumn(0)
                        }
                    }
                    .CornerRadius(Corner)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(Colors.Transparent)
                    .Margin(0, 0, 18, 18)
                    .WidthRequest(cardWidth)
                    .HeightRequest(CardHeight);

                default:
                    return new Frame()
                        {
                            frontContentGrid
                        }
                        .CornerRadius(Corner)
                        .HasShadow(true)
                        .Padding(0)
                        .BackgroundColor(Colors.White)
                        .BorderColor(Colors.LightGray)
                        .Margin(0, 0, 18, 18)
                        .WidthRequest(cardWidth)
                        .HeightRequest(CardHeight);
            }
        }

        // Helper: create FontImageSource for Image.Source lambdas
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

        // Blend helper
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

    public class CardDemoPage : Component
    {
        // overlay state
        bool _overlayVisible = false;
        VisualNode? _overlayContent = null;
        PopupSpec? _overlaySpec = null;


        public override VisualNode Render()
        {
            var list =
                new VerticalStackLayout
                {
                    new ContentView()
                    {
                        new CardView
                        {
                            Title = "Pending Actions",
                            Description = "Short description here",
                            Variant = CardVariant.Outline,
                            IconGlyph = "\uf0f3",
                            IconFontFamily = "FA",
                            RequestShowOverlay = (spec) =>
                            {
                                _overlaySpec = spec;
                                _overlayVisible = true;
                                Invalidate();
                            }
                        }
                    }.HeightRequest(180),

                    new ContentView
                    {
                        new CardView
                        {
                            Title = "Solid Card",
                            Description = "More vibrant solid color",
                            Variant = CardVariant.Solid,
                            SolidColor = Color.FromArgb("#4CAF50"),
                            RequestShowOverlay = (spec) =>
                            {
                                _overlaySpec = spec;
                                _overlayVisible = true;
                                Invalidate();
                            }
                        }
                    }.HeightRequest(180),

                    new ContentView
                    {
                        new CardView
                        {
                            Title = "Gradient Card",
                            Description = "Smooth linear gradient",
                            Variant = CardVariant.Gradient,
                            GradientColors = (Color.FromArgb("#FF8A00"), Color.FromArgb("#E52E71")),
                            RequestShowOverlay = (spec) =>
                            {
                                _overlaySpec = spec;
                                _overlayVisible = true;
                                Invalidate();
                            }
                        }
                    }.HeightRequest(180),

                    new ContentView
                    {
                        new CardView
                        {
                            Title = "Image Background",
                            Description = "Photo background with overlay",
                            Variant = CardVariant.ImageBackground,
                            BackgroundImage = "https://images.unsplash.com/photo-1503023345310-bd7c1de61c7d?w=800&q=80",
                            ImageOpacity = 0.32,
                            RequestShowOverlay = (spec) =>
                            {
                                _overlaySpec = spec;
                                _overlayVisible = true;
                                Invalidate();
                            }
                        }
                    }.HeightRequest(180)
                }
                .Spacing(12)
                .Padding(new Thickness(6));

            // Compose page root and overlay if needed
            var pageRoot = new Grid(new[] { new Microsoft.Maui.Controls.RowDefinition(GridLength.Star) }, new[] { new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star) })
            {
                new ScrollView
                {
                    list
                }
            };

            // If overlay requested, parent renders full-screen overlay with popup built from the stored PopupSpec.
            if (_overlayVisible && _overlaySpec != null)
            {
                // Full-screen transparent backdrop that closes the overlay when tapped
                var backdrop = new BoxView()
                    .BackgroundColor(Color.FromRgba(0, 0, 0, 0.15))
                    .OnTapped(() =>
                    {
                        _overlayVisible = false;
                        _overlaySpec = null;
                        Invalidate();
                    })
                    .GridRow(0)
                    .GridColumn(0);

                // Build popup content here using _overlaySpec actions; buttons call spec action then close overlay
                var popupFrame = new Frame()
                    .CornerRadius(8)
                    .HasShadow(true)
                    .BackgroundColor(Colors.White)
                    .BorderColor(Color.FromRgba(0, 0, 0, 0.06f))
                    .WidthRequest(260)
                    .Padding(0);

                popupFrame.AddChildren(
                        new VerticalStackLayout()
                        {
                            new Button().Text("Edit").TextColor(Colors.Black).BackgroundColor(Colors.Transparent).OnClicked(() =>
                            {
                                _overlaySpec?.OnEdit?.Invoke();
                                _overlayVisible = false;
                                _overlaySpec = null;
                                Invalidate();
                            }),
                            new BoxView().HeightRequest(1).BackgroundColor(Color.FromRgba(0,0,0,0.06f)),
                            new Button().Text("View").TextColor(Colors.Black).BackgroundColor(Colors.Transparent).OnClicked(() =>
                            {
                                _overlaySpec?.OnView?.Invoke();
                                _overlayVisible = false;
                                _overlaySpec = null;
                                Invalidate();
                            }),
                            new BoxView().HeightRequest(1).BackgroundColor(Color.FromRgba(0,0,0,0.06f)),
                            new Button().Text("Delete").TextColor(Colors.Black).BackgroundColor(Colors.Transparent).OnClicked(() =>
                            {
                                _overlaySpec?.OnDelete?.Invoke();
                                _overlayVisible = false;
                                _overlaySpec = null;
                                Invalidate();
                            })
                        }
                    );

                // center the popup in the screen
                var overlayContainer = new Grid(new[] { new Microsoft.Maui.Controls.RowDefinition(GridLength.Star) }, new[] { new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star) })
                {
                    backdrop,
                    popupFrame.Margin(new Thickness(0,0,0,0)) // will center via layout below
                };

                // Instead of complex absolute positioning, place popup centered using a simple stack:
                // We'll add the overlay as a child and then another centered container
                pageRoot.AddChildren(
                    backdrop,
                    new Grid()
                    {
                        new ContentView()
                            {
                                popupFrame
                            }
                            .HorizontalOptions(MauiControls.LayoutOptions.Center)
                            .VerticalOptions(MauiControls.LayoutOptions.Center)
                    }
                );
            }

            return pageRoot;
        }
    }
}