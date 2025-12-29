using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MauiReactor;
using MauiReactor.Compatibility;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

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

    public partial class CardView : Component
    {
        // Public props
        public string Title { get; set; } = "Title";
        public string Description { get; set; } = "Description";
        public CardVariant Variant { get; set; } = CardVariant.Outline;

        // Icon on front card and watermark
        public string IconGlyph { get; set; } = "\uf0f3";      // FontAwesome bell
        public string IconFontFamily { get; set; } = "FA";     // alias defined in MauiProgram

        // Back icon badges (bottom-left)
        public IList<string> BackIconGlyphs { get; set; } = []; // e.g. { "\uf004", "\uf0f3" }

        // Appearance
        public string BackgroundImage { get; set; } = "";
        public Color SolidColor { get; set; } = Colors.White;
        public (Color from, Color to)? GradientColors { get; set; } = null;
        public double ImageOpacity { get; set; } = 0.36;
        public double CardHeight { get; set; } = 150;

        // Callbacks
        public Action? OnEdit { get; set; }
        public Action? OnView { get; set; }
        public Action? OnDelete { get; set; }

        // Popup request up to parent
        public Action<PopupSpec>? RequestShowOverlay { get; set; }

        const float Corner = 10f;

        public override VisualNode Render()
        {
            BackIconGlyphs ??= [];

            // responsive width
            var deviceWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            var cardWidth = Math.Min(deviceWidth * 0.9, 360);

            // badges/ footer
            var badgeSize = 24.0;
            var badgeSpacing = 6.0;
            var hasBadges = BackIconGlyphs != null && BackIconGlyphs.Count > 0;

            // extra vertical space so badges aren't clipped
            var extraHeight = hasBadges ? badgeSize + 15 : 5.0;
            var totalHeight = CardHeight + extraHeight;

            // BACK card (peeking)
            var backCard = new Frame()
                .HasShadow(false)
                .CornerRadius(Corner)
                .Padding(0)
                .BackgroundColor(Colors.LightGray)
                .Margin(18, 18, 0, 0)          // slight peek down-right
                .WidthRequest(cardWidth)
                .HeightRequest(CardHeight + extraHeight)     // front visual height
                .GridRow(0)
                .GridColumn(0);

            // FRONT card
            var front = BuildFront(cardWidth).GridRow(0).GridColumn(0);

            // MENU BUTTON (rendered at top level so it's clickable, bottom-right)
            var backMenuBtn = new Button()
                .Text("⋯")
                .FontSize(18)
                .BackgroundColor(Colors.Transparent)
                .HorizontalOptions(MauiControls.LayoutOptions.End)
                .VerticalOptions(MauiControls.LayoutOptions.Start)
                // top = CardHeight - 32 approximately puts it near bottom edge of front card
                .Margin(0, CardHeight - 32, 10, 0)
                .Padding(0, 0, 8, 0)
                .OnClicked(() => RaisePopup())
                .GridRow(0)
                .GridColumn(0);

            // BADGES (horizontal bottom-left, first half-out horizontally)
            HorizontalStackLayout? badgesRow = null;
            if (hasBadges)
            {
                // top so first badge slightly below front-card bottom, not behind it
                var badgeTop = CardHeight - (badgeSize + 30);
                badgeTop = 168;

                badgesRow = new HorizontalStackLayout()
                    .Spacing(badgeSpacing)
                    .HorizontalOptions(MauiControls.LayoutOptions.Start)
                    .VerticalOptions(MauiControls.LayoutOptions.Start)
                    .Margin(-badgeSize / 2.0 + 12, badgeTop, 0, 0) // half out left, slightly below
                    .GridRow(0)
                    .GridColumn(0);

                int index = 0;
                foreach (var glyph in BackIconGlyphs!)
                {
                    var badge = new Frame()
                    {
                        new Label()
                            .Text(glyph)
                            .FontFamily(IconFontFamily)
                            .FontSize(badgeSize - 12)
                            .HorizontalOptions(MauiControls.LayoutOptions.Center)
                            .VerticalOptions(MauiControls.LayoutOptions.Center)
                    }
                    .CornerRadius(5)
                    .Padding(5)
                    .HasShadow(false)
                    .BackgroundColor(Colors.White)
                    .BorderColor(Colors.LightGray)
                    .WidthRequest(badgeSize)
                    .HeightRequest(badgeSize)
                    //.Margin(index == 0 ? 0 : 0, 0, 0, 0) // we already offset whole stack with negative margin
                    .OnTapped(() => System.Diagnostics.Debug.WriteLine($"Badge {glyph} tapped"));

                    badgesRow.AddChildren(badge);
                    index++;
                }
            }

            // CAPTURE: small top-left capsule on back card
            var capsule = new BoxView()
                .WidthRequest(36)
                .HeightRequest(24)
                .CornerRadius(12)
                .BackgroundColor(Colors.DarkGray)
                .Margin(8, 6, 0, 0)
                .GridRow(0)
                .GridColumn(0);

            // put capsule inside backCard using a simple grid
            var backInner = new Grid()
            {
                capsule
            };
            backCard.AddChildren(backInner);

            // ROOT layout: one cell layered grid
            var root = new Grid(
                new[] { new MauiControls.RowDefinition(GridLength.Star) },
                new[] { new MauiControls.ColumnDefinition(GridLength.Star) })
            {
                backCard
            }
            .HeightRequest(totalHeight)
            .WidthRequest(cardWidth);

            if (badgesRow != null)
            {
                root.AddChildren(badgesRow);
            }

            root.AddChildren(front);
            root.AddChildren(backMenuBtn);

            return root;
        }

        void RaisePopup()
        {
            var spec = new PopupSpec
            {
                OnEdit = () => OnEdit?.Invoke(),
                OnView = () => OnView?.Invoke(),
                OnDelete = () => OnDelete?.Invoke()
            };

            RequestShowOverlay?.Invoke(spec);
        }

        Frame BuildFront(double cardWidth)
        {
            // overlay content: title, desc, footer
            var overlay = new Grid(
                new[]
                {
                    new MauiControls.RowDefinition(GridLength.Auto),
                    new MauiControls.RowDefinition(GridLength.Auto),
                    new MauiControls.RowDefinition(GridLength.Auto)
                },
                new[] { new MauiControls.ColumnDefinition(GridLength.Star) })
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
            };

            // front content grid holds overlay + watermark glyph
            var frontContentGrid = new Grid(
                [new MauiControls.RowDefinition(GridLength.Star)],
                [new MauiControls.ColumnDefinition(GridLength.Star)])
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

                    var gradientGrid = new Grid(
                        [new MauiControls.RowDefinition(GridLength.Star)],
                        [new MauiControls.ColumnDefinition(GridLength.Star)])
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
                        new Grid(
                            [new MauiControls.RowDefinition(GridLength.Star)],
                            [new MauiControls.ColumnDefinition(GridLength.Star)])
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
                                    [
                                        new MauiControls.GradientStop(Color.FromRgba(0,0,0,0.28f), 0f),
                                        new MauiControls.GradientStop(Color.FromRgba(0,0,0,0.08f), 1f)
                                    ],
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
                    .BackgroundColor(Colors.White)
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
        PopupSpec? _overlaySpec = null;

        // animation-driven state
        double _backdropOpacity = 0.0;
        double _popupOpacity = 0.0;
        double _popupOffset = 24.0;
        bool _isAnimating = false;
        CancellationTokenSource? _animCts;

        public override VisualNode Render()
        {
            VisualNode MakeCard(string title, string description, CardVariant variant,
                Color? solid = null, (Color from, Color to)? gradient = null,
                string? image = null, IList<string>? backGlyphs = null)
            {
                return new CardView
                {
                    Title = title,
                    Description = description,
                    Variant = variant,
                    SolidColor = solid ?? Colors.White,
                    GradientColors = gradient,
                    BackgroundImage = image ?? "",
                    BackIconGlyphs = backGlyphs ?? [],
                    RequestShowOverlay = (spec) =>
                    {
                        _overlaySpec = spec;
                        _overlayVisible = true;
                        _backdropOpacity = 0.0;
                        _popupOpacity = 0.0;
                        _popupOffset = 24.0;
                        Invalidate();

                        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            await Task.Delay(10);
                            await AnimateShowAsync();
                        });
                    }
                };
            }

            var list = new VerticalStackLayout
            {
                MakeCard(
                    "Pending Actions",
                    "Short description here",
                    CardVariant.Outline,
                    backGlyphs: ["\uf004", "\uf0f3"] // heart + bell
                ),
                MakeCard(
                    "Solid Card",
                    "More vibrant solid color",
                    CardVariant.Solid,
                    solid: Color.FromArgb("#4CAF50")
                ),
                MakeCard(
                    "Gradient Card",
                    "Smooth linear gradient",
                    CardVariant.Gradient,
                    gradient: (Color.FromArgb("#FF8A00"), Color.FromArgb("#E52E71"))
                ),
                MakeCard(
                    "Image Background",
                    "Photo background with overlay",
                    CardVariant.ImageBackground,
                    image: "https://images.unsplash.com/photo-1503023345310-bd7c1de61c7d?w=800&q=80"
                )
            }
            .Spacing(24)
            .Padding(new Thickness(8));

            var pageRoot = new Grid(
                [new MauiControls.RowDefinition(GridLength.Star)],
                [new MauiControls.ColumnDefinition(GridLength.Star)])
            {
                new ScrollView
                {
                    list
                }
            };

            if (_overlayVisible && _overlaySpec != null)
            {
                var backdrop = new BoxView()
                    .BackgroundColor(() => Color.FromRgba(0, 0, 0, (float)_backdropOpacity))
                    .OnTapped(async () => await AnimateHideAsync())
                    .GridRow(0)
                    .GridColumn(0);

                var deviceWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                var popupWidth = Math.Min(360, Math.Max(260, deviceWidth * 0.8));

                var popupFrame = new Frame()
                {
                    new VerticalStackLayout()
                    {
                        new Button().Text("Edit").TextColor(Colors.Black).BackgroundColor(Colors.Transparent).OnClicked(async () =>
                        {
                            _overlaySpec?.OnEdit?.Invoke();
                            await AnimateHideAsync();
                        }),
                        new BoxView().HeightRequest(1).BackgroundColor(Color.FromRgba(0,0,0,0.06f)),
                        new Button().Text("View").TextColor(Colors.Black).BackgroundColor(Colors.Transparent).OnClicked(async () =>
                        {
                            _overlaySpec?.OnView?.Invoke();
                            await AnimateHideAsync();
                        }),
                        new BoxView().HeightRequest(1).BackgroundColor(Color.FromRgba(0,0,0,0.06f)),
                        new Button().Text("Delete").TextColor(Colors.Black).BackgroundColor(Colors.Transparent).OnClicked(async () =>
                        {
                            _overlaySpec?.OnDelete?.Invoke();
                            await AnimateHideAsync();
                        })
                    }
                }
                .CornerRadius(8)
                .HasShadow(true)
                .BackgroundColor(Colors.White)
                .BorderColor(Color.FromRgba(0, 0, 0, 0.06f))
                .WidthRequest(popupWidth)
                .Padding(0)
                .Opacity(() => _popupOpacity)
                .Margin(() => new Thickness(0, _popupOffset, 0, 0));

                var centered = new Grid()
                {
                    new ContentView
                    {
                        popupFrame
                    }
                    .HorizontalOptions(MauiControls.LayoutOptions.Center)
                    .VerticalOptions(MauiControls.LayoutOptions.Center)
                };

                pageRoot.AddChildren(backdrop, centered);
            }

            return pageRoot;
        }

        async Task AnimateShowAsync()
        {
            if (_isAnimating) return;
            _isAnimating = true;
            _animCts?.Cancel();
            _animCts = new CancellationTokenSource();
            var token = _animCts.Token;

            try
            {
                const int frames = 12;
                const int frameDelay = 16;
                for (int i = 0; i <= frames; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var t = i / (double)frames;
                    var ease = 1 - Math.Pow(1 - t, 2);

                    _backdropOpacity = 0.15 * ease;
                    _popupOpacity = ease;
                    _popupOffset = 24 * (1 - ease);

                    Invalidate();
                    await Task.Delay(frameDelay, token);
                }

                _backdropOpacity = 0.15;
                _popupOpacity = 1.0;
                _popupOffset = 0.0;
                Invalidate();
            }
            catch (OperationCanceledException) { }
            finally
            {
                _isAnimating = false;
            }
        }

        async Task AnimateHideAsync()
        {
            if (_isAnimating) return;
            _isAnimating = true;
            _animCts?.Cancel();
            _animCts = new CancellationTokenSource();
            var token = _animCts.Token;

            try
            {
                const int frames = 10;
                const int frameDelay = 16;
                for (int i = 0; i <= frames; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var t = i / (double)frames;
                    var ease = Math.Pow(1 - t, 2);

                    _backdropOpacity = 0.15 * ease;
                    _popupOpacity = ease;
                    _popupOffset = 24 * (1 - ease);

                    Invalidate();
                    await Task.Delay(frameDelay, token);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _backdropOpacity = 0.0;
                _popupOpacity = 0.0;
                _popupOffset = 24.0;
                _overlayVisible = false;
                _overlaySpec = null;
                Invalidate();

                _isAnimating = false;
            }
        }
    }
}