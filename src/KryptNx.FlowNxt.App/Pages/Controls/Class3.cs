using System.Collections.Generic;
using System.Linq;
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

    public record PopupSpec(Action? OnEdit, Action? OnView, Action? OnDelete);

    public static class CardViewHelpers
    {
        private static double? _cachedDeviceWidth;
        private static readonly Dictionary<string, MauiControls.FontImageSource> _fontCache = [];

        public static double GetDeviceWidth()
        {
            if (_cachedDeviceWidth == null)
            {
                var info = DeviceDisplay.MainDisplayInfo;
                _cachedDeviceWidth = info.Width / info.Density;
            }

            return _cachedDeviceWidth.Value;
        }

        public static MauiControls.FontImageSource CreateFontImageSource(string glyph, string fontFamily, int size, Color color)
        {
            var key = $"{glyph}:{fontFamily}:{size}:{color}";
            if (_fontCache.TryGetValue(key, out var src))
                return src;

            return _fontCache[key] = new MauiControls.FontImageSource
            {
                Glyph = glyph ?? string.Empty,
                FontFamily = fontFamily,
                Size = size,
                Color = color
            };
        }

        public static Color Blend(Color a, Color b, float t)
        {
            t = Math.Clamp(t, 0, 1);

            return Color.FromRgba(a.Red + (b.Red - a.Red) * t, a.Green + (b.Green - a.Green) * t,
                a.Blue + (b.Blue - a.Blue) * t, a.Alpha + (b.Alpha - a.Alpha) * t);
        }
    }

    public partial class CardView : Component
    {
        // Public props
        public string Title { get; set; } = "Title";
        public string Description { get; set; } = "Description";
        public CardVariant Variant { get; set; } = CardVariant.Outline;
        
        // Icon on front card and watermark
        public string IconGlyph { get; set; } = "\uf0f3";   // FontAwesome bell
        public string IconFontFamily { get; set; } = "FA";  // alias defined in MauiProgram

        // Back icon badges (bottom-left)
        public IList<string> BackIconGlyphs { get; set; } = []; // e.g. { "\uf004", "\uf0f3" }

        // Appearance
        public string BackgroundImage { get; set; } = "";
        public Color SolidColor { get; set; } = Colors.White;
        public (Color from, Color to)? GradientColors { get; set; }
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
            var deviceWidth = CardViewHelpers.GetDeviceWidth();
            var rootWidth = deviceWidth * 0.92;
            var cardWidth = Math.Min(rootWidth * 0.96, 360);

            bool hasBadges = BackIconGlyphs != null && BackIconGlyphs.Count > 0;
            double badgeSize = 24;
            double badgeHeight = hasBadges ? 36 : 0;

            var cardLayer = new Grid([new MauiControls.RowDefinition(GridLength.Star)],
                [new MauiControls.ColumnDefinition(GridLength.Star)]
            )
            .BackgroundColor(Colors.Blue)
            .HeightRequest(CardHeight + 20);

            var backCard = new Frame()
                .CornerRadius(Corner)
                .HasShadow(false)
                .BackgroundColor(Colors.LightGray)
                .WidthRequest(cardWidth)
                .HeightRequest(CardHeight + (hasBadges ? (badgeSize - 6) : 0))
                .HorizontalOptions(MauiControls.LayoutOptions.End)
                .VerticalOptions(MauiControls.LayoutOptions.Start)
                .Margin(0, 20, 0, 0);

            var frontCard = BuildFront(cardWidth)
                .WidthRequest(cardWidth)
                .HeightRequest(CardHeight)
                .HorizontalOptions(MauiControls.LayoutOptions.Start)
                .VerticalOptions(MauiControls.LayoutOptions.Start);

            var backMenuBtn = new Button()
                .Text("⋯")
                .FontSize(18)
                .BackgroundColor(Colors.Transparent)
                .HorizontalOptions(MauiControls.LayoutOptions.End)
                .VerticalOptions(MauiControls.LayoutOptions.Start)
                .TranslationX(10)
                .TranslationY(CardHeight - 10 + (hasBadges ? badgeSize - 6 : 0))
                .OnClicked(RaisePopup);

            cardLayer.AddChildren(backCard, frontCard);

            VisualNode? badgesScroller = null;

            if (hasBadges)
            {
                badgesScroller = new ScrollView()
                    {
                        new HorizontalStackLayout()
                            {
                                BackIconGlyphs!.Select(glyph =>
                                        new Frame
                                        {
                                            new Label()
                                                .Text(glyph)
                                                .FontFamily(IconFontFamily)
                                                .FontSize(12)
                                                .HorizontalOptions(MauiControls.LayoutOptions.Center)
                                                .VerticalOptions(MauiControls.LayoutOptions.Center)
                                        }
                                        .WidthRequest(badgeSize)
                                        .HeightRequest(badgeSize)
                                        .CornerRadius(6)
                                        .HasShadow(false)
                                        .BackgroundColor(Colors.White)
                                        .BorderColor(Colors.LightGray)
                                    ).ToArray()
                            }
                            .Spacing(8)
                            .Padding(8, 8)
                    }
                    .Orientation(ScrollOrientation.Horizontal)
                    .WidthRequest(cardWidth * 0.95)
                    .HeightRequest(badgeHeight)
                    .GridRow(1);
            }

            var root = new Grid([new MauiControls.RowDefinition(GridLength.Auto), // CardLayer
                    new MauiControls.RowDefinition(GridLength.Auto)  // Badges
                ],
                [new MauiControls.ColumnDefinition(GridLength.Star)]
            )
            {
                cardLayer
            }
            .BackgroundColor(Colors.Red)
            .WidthRequest(rootWidth)
            .Margin(0, 0, 0, 16);

            if (badgesScroller != null)
            {
                root.AddChildren(badgesScroller);
            }

            root.AddChildren(backMenuBtn);

            return root;
        }

        void RaisePopup()
        {
            var spec = new PopupSpec(() => OnEdit?.Invoke(), () => OnView?.Invoke(), () => OnDelete?.Invoke());
            RequestShowOverlay?.Invoke(spec);
        }

        Frame BuildFront(double cardWidth)
        {
            var overlay = new VerticalStackLayout
            {
                new Label()
                    .Text(() => Title)
                    .FontSize(20)
                    .FontAttributes(MauiControls.FontAttributes.Bold)
                    .Margin(12, 8, 12, 2),

                new Label()
                    .Text(() => Description)
                    .FontSize(13)
                    .Opacity(0.95)
                    .Margin(12, 0, 12, 6)
            };

            static Frame BaseFrame(VisualNode content, Color bg, Color? border = null)
            {
                var frame = new Frame { content }
                    .CornerRadius(Corner)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(bg);

                if (border != null)
                    frame = frame.BorderColor(border);

                return frame;
            }

            return Variant switch
            {
                CardVariant.Solid => BaseFrame(overlay, SolidColor, CardViewHelpers.Blend(SolidColor, Colors.Black, 0.08f)),

                CardVariant.Gradient =>
                    BaseFrame(
                        new Grid
                        {
                            new BoxView().Background(
                                new MauiControls.LinearGradientBrush(
                                    [
                                        new MauiControls.GradientStop(GradientColors?.from ?? Colors.Purple, 0),
                                        new MauiControls.GradientStop(GradientColors?.to ?? Colors.Blue, 1)
                                    ],
                                    new Point(0, 0),
                                    new Point(1, 1)
                                )
                            ),
                            overlay
                        },
                        Colors.Transparent
                    ),

                CardVariant.ImageBackground =>
                    BaseFrame(
                            new Grid
                            {
                                string.IsNullOrWhiteSpace(BackgroundImage) ? null
                                : new Image()
                                    .Source(() =>
                                    {
                                        if (BackgroundImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                            return MauiControls.ImageSource.FromUri(new Uri(BackgroundImage));

                                        return MauiControls.ImageSource.FromFile(BackgroundImage);
                                    })
                                    .Aspect(Aspect.AspectFill)
                                    .GridRow(0)
                                    .GridColumn(0),

                                new BoxView()
                                    .Background(
                                        new MauiControls.LinearGradientBrush(
                                            [
                                                new MauiControls.GradientStop(Color.FromRgba(0, 0, 0, 0.45f), 0),
                                                new MauiControls.GradientStop(Color.FromRgba(0, 0, 0, 0.15f), 1)
                                            ],
                                            new Point(0, 0),
                                            new Point(0, 1)
                                        )
                                    )
                                    .GridRow(0)
                                    .GridColumn(0),
                                overlay
                            },
                            Colors.Transparent
                        ),

                _ => BaseFrame(overlay, Colors.White, Colors.LightGray)
            };
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
            .Spacing(16)
            .Padding(new Thickness(8));

            var pageRoot = new Grid([new MauiControls.RowDefinition(GridLength.Star)],
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