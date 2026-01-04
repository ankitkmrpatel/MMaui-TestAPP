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
            if (t < 0) t = 0;
            if (t > 1) t = 1;

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
            var deviceWidth = CardViewHelpers.GetDeviceWidth();
            var cardWidth = Math.Min(deviceWidth * 0.9, 360);

            var badgeSize = 24.0;
            var badgeSpacing = 6.0;
            var hasBadges = BackIconGlyphs != null && BackIconGlyphs.Count > 0;

            var backCard = new Frame()
                .HasShadow(false)
                .CornerRadius(Corner)
                .Padding(0)
                .BackgroundColor(Colors.LightGray)
                .Margin(18, 18, 0, 0)
                .WidthRequest(cardWidth)
                .HeightRequest(CardHeight + (hasBadges ? badgeSize + 6 : 0));

            var front = BuildFront(cardWidth).GridRow(0).GridColumn(0);

            var backMenuBtn = new Button()
                .Text("⋯")
                .FontSize(18)
                .BackgroundColor(Colors.Transparent)
                .HorizontalOptions(MauiControls.LayoutOptions.End)
                .VerticalOptions(MauiControls.LayoutOptions.Start)
                .Margin(0, CardHeight - 16, 0, 0)
                .Padding(0, 0, 8, 0)
                .OnClicked(() => RaisePopup())
                .GridRow(0)
                .GridColumn(0);

            HorizontalStackLayout? badgesRow = null;
            if (hasBadges)
            {
                var badgeTop = CardHeight + 6;

                badgesRow = new HorizontalStackLayout()
                    .Spacing(badgeSpacing)
                    .Margin(-badgeSize / 2.0 + 12, badgeTop, 0, 0)
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
                    .OnTapped(() => System.Diagnostics.Debug.WriteLine($"Badge {glyph} tapped"));

                    badgesRow.AddChildren(badge);
                    index++;
                }
            }

            var root = new Grid([new MauiControls.RowDefinition(GridLength.Star)],
                [new MauiControls.ColumnDefinition(GridLength.Star)])
            {
                backCard,
                front
            }
            .HeightRequest(CardHeight)
            .WidthRequest(cardWidth)
            .Margin(0, 0, 0, hasBadges ? badgeSize : 16);

            if (badgesRow != null)
            {
                root.AddChildren(badgesRow);
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
            // overlay content: title, desc, footer
            var overlay = new Grid([new MauiControls.RowDefinition(GridLength.Auto), new MauiControls.RowDefinition(GridLength.Auto),
                    new MauiControls.RowDefinition(GridLength.Auto)],
                [new MauiControls.ColumnDefinition(GridLength.Star)])
            {
                new Label()
                    .Text(() => Title ?? "No Title")
                    .FontSize(20)
                    .FontAttributes(MauiControls.FontAttributes.Bold)
                    .Margin(12, 8, 12, 2)
                    .GridRow(0)
                    .GridColumn(0),

                new Label()
                    .Text(() => Description ?? "No Description")
                    .FontSize(13)
                    .Opacity(0.95)
                    .Margin(12, 0, 12, 6)
                    .GridRow(1)
                    .GridColumn(0),
            };

            var frontContentGrid = new Grid([new MauiControls.RowDefinition(GridLength.Star)],
                [new MauiControls.ColumnDefinition(GridLength.Star)])
            {
                overlay.GridRow(0).GridColumn(0)
            };

            if (Variant == CardVariant.Outline && !string.IsNullOrEmpty(IconGlyph))
            {
                frontContentGrid.AddChildren(
                    new Image()
                        .Source(() => CardViewHelpers.CreateFontImageSource(IconGlyph, IconFontFamily, 40, Colors.Black))
                        .Rotation(20)
                        .HorizontalOptions(MauiControls.LayoutOptions.End)
                        .VerticalOptions(MauiControls.LayoutOptions.End)
                        .Margin(0, 0, 8, 8)
                        .Opacity(0.12)
                        .GridRow(0)
                        .GridColumn(0)
                );
            }

            Frame BaseFrame(VisualNode content, Color bg, Color? border = null)
            {
                var baseFrame = new Frame { content }
                    .CornerRadius(Corner)
                    .HasShadow(true)
                    .Padding(0)
                    .BackgroundColor(bg)
                    .Margin(0, 0, 18, 18)
                    .WidthRequest(cardWidth)
                    .HeightRequest(CardHeight);

                if (border != null)
                {
                    baseFrame = baseFrame.BorderColor(border);
                }

                return baseFrame;
            }

            switch (Variant)
            {
                case CardVariant.Outline:
                    return BaseFrame(frontContentGrid, Colors.White, Colors.LightGray);

                case CardVariant.Solid:
                    return BaseFrame(frontContentGrid, SolidColor, CardViewHelpers.Blend(SolidColor, Colors.Black, 0.08f));

                case CardVariant.Gradient:
                    var (from, to) = GradientColors ?? (Colors.MediumPurple, Colors.LightBlue);
                    var gradBox = new BoxView()
                        .Background(new MauiControls.LinearGradientBrush([new MauiControls.GradientStop(from, 0f),
                            new MauiControls.GradientStop(to, 1f)],
                            new Point(0, 0),
                            new Point(1, 1)
                        ))
                        .GridRow(0)
                        .GridColumn(0);

                    var gradientGrid = new Grid([new MauiControls.RowDefinition(GridLength.Star)],
                        [new MauiControls.ColumnDefinition(GridLength.Star)])
                    {
                        gradBox,
                        overlay.GridRow(0).GridColumn(0)
                    };

                    return BaseFrame(gradientGrid, Colors.Transparent, Colors.Transparent);

                case CardVariant.ImageBackground:
                    Image? imageBackground = string.IsNullOrEmpty(BackgroundImage) ? null : new Image()
                            .Source(() =>
                            {
                                if (BackgroundImage.StartsWith("http", StringComparison.OrdinalIgnoreCase) || BackgroundImage.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                                    return MauiControls.ImageSource.FromUri(new Uri(BackgroundImage));

                                return MauiControls.ImageSource.FromFile(BackgroundImage);
                            })
                            .Aspect(Aspect.AspectFill)
                            .Opacity(ImageOpacity)
                            .GridRow(0)
                            .GridColumn(0);

                    return BaseFrame(new Grid([new MauiControls.RowDefinition(GridLength.Star)],
                            [new MauiControls.ColumnDefinition(GridLength.Star)])
                        {
                            imageBackground,

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
                        }, Colors.White);

                default:
                    return BaseFrame(frontContentGrid, Colors.White, Colors.LightGray);
            }
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