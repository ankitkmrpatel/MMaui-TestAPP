using MauiReactor;
using MauiReactor.Compatibility;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        // Public properties (same as before)
        public string Title { get; set; } = "Title";
        public string Description { get; set; } = "Description";
        public CardVariant Variant { get; set; } = CardVariant.Outline;
        public string IconGlyph { get; set; } = "\uf0f3";
        public string IconFontFamily { get; set; } = "FA";
        public string BackgroundImage { get; set; } = "";
        public Color SolidColor { get; set; } = Colors.White;
        public (Color from, Color to)? GradientColors { get; set; } = null;
        
        //public IList<string> BackIconGlyphs { get; set; } = new List<string> { "\uf004", "\uf0f3" };
        public IList<string> BackIconGlyphs { get; set; } = [];

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


            //backInnerGrid.AddChildren(backMenuBtn);
            //backCard.AddChildren(backMenuBtn);

            // FRONT CARD
            var front = BuildFront(cardWidth);

            //// BADGES container: horizontal stack positioned so first badge is half-overlapping front bottom
            //// compute top so first badge is half overlapping the front's bottom edge
            //var badgeTop = CardHeight - (badgeSize / 2.0);

            //// Create Horizontal stack and add individual badge frames
            //var badgesContainer = new HorizontalStackLayout()
            //    .Spacing(badgeSpacing)
            //    // place container so its left edge is half outside the card (negative X) and top aligned to badgeTop
            //    .Margin(-badgeSize / 2.0 + 15, badgeTop, 0, 0);

            //// add each badge into the horizontal container
            //if (BackIconGlyphs != null)
            //{
            //    foreach (var glyph in BackIconGlyphs)
            //    {
            //        var badge = new Frame()
            //        {
            //            new Image().Source(() => CreateFontImageSource(glyph, IconFontFamily, (int)(badgeSize - 14), Colors.Black))
            //                .HorizontalOptions(MauiControls.LayoutOptions.Center)
            //                .VerticalOptions(MauiControls.LayoutOptions.Center)
            //        }
            //        //.CornerRadius((float)(badgeSize / 2.0))
            //        .CornerRadius(5)
            //        .Padding(6)
            //        .HasShadow(false)
            //        .BackgroundColor(Colors.White)
            //        .BorderColor(Colors.Transparent)
            //        .WidthRequest(badgeSize)
            //        .HeightRequest(badgeSize)
            //        .OnTapped(() => System.Diagnostics.Debug.WriteLine($"Badge {glyph} tapped"));

            //        badgesContainer.AddChildren(badge);
            //    }
            //}

            var badgeSize = 28.0;         // slightly smaller for footer
            var badgeSpacing = 6.0;
            var backIconCount = Math.Max(0, BackIconGlyphs?.Count ?? 0);

            // extra footer height only if we have icons
            var footerHeight = backIconCount > 0 ? badgeSize + 8.0 : 0.0;

            // back card is front height + footer area (if any)
            var backCardHeight = CardHeight + badgeSize;

            // footer goes only if we have icons OR we always want menu at bottom
            if (backIconCount > 0)
            {
                // --- MENU BUTTON (bottom-right inside back card) ---
                var backMenuBtn = new Button()
                    .Text("⋯")
                    .FontSize(18)
                    .BackgroundColor(Colors.Transparent)
                    .HorizontalOptions(MauiControls.LayoutOptions.End)
                    .VerticalOptions(MauiControls.LayoutOptions.Center)
                    .Margin(0, 0, 4, 0)
                    .Padding(0, 0, 8, 0)
                    .OnClicked(() => RaisePopup());

                // --- HORIZONTAL BADGES (bottom-left inside back card) ---
                var badgesRow = new HorizontalStackLayout()
                    .Spacing(badgeSpacing)
                    .Padding(0, 4, 4, 4)
                    .HorizontalOptions(MauiControls.LayoutOptions.Start)
                    .VerticalOptions(MauiControls.LayoutOptions.Center);

                // add each badge (as Label inside Frame)
                int i = 0;
                foreach (var glyph in BackIconGlyphs!)
                {
                    var badge = new Frame()
                    {
                        new Label()
                            .Text(glyph)                         // <-- Label with glyph
                            .FontFamily(IconFontFamily)          // reuse same alias as other icons
                            .FontSize(badgeSize - 8)
                            .HorizontalOptions(MauiControls.LayoutOptions.Center)
                            .VerticalOptions(MauiControls.LayoutOptions.Center)
                    }
                    .CornerRadius(5)
                    .Padding(4)
                    .HasShadow(false)
                    .BackgroundColor(Colors.White)
                    .BorderColor(Colors.Transparent)
                    .WidthRequest(badgeSize)
                    .HeightRequest(badgeSize)
                    .Margin(i++ == 0 ? -badgeSize / 2.0 : 0, 0, 0, 0)          // <-- half-outside horizontally
                    .OnTapped(() => System.Diagnostics.Debug.WriteLine($"Badge {glyph} tapped"));

                    badgesRow.AddChildren(badge);
                }

                // --- FOOTER GRID INSIDE BACK CARD ---
                // row 0 = everything else in back card (capsule, etc.)
                // row 1 = footer row (badges left + spacer + menu right)
                var backCardGrid = new Grid(
                    [
                        new(GridLength.Star),
                        new(footerHeight > 0 ? GridLength.Auto : GridLength.Auto)
                    ],
                    [
                        new(GridLength.Auto),
                        new(GridLength.Star),
                        new(GridLength.Auto)
                    ]);

                // top-left capsule in row 0
                backCardGrid.AddChildren(
                    new BoxView()
                        .WidthRequest(36)
                        .HeightRequest(24)
                        .CornerRadius(12)
                        .BackgroundColor(Colors.DarkGray)
                        .Margin(8, 6, 0, 0)
                        .GridRow(0)
                        .GridColumn(0)
                );

                backCardGrid.AddChildren(
                    badgesRow
                        .GridRow(1)
                        .GridColumn(0),

                    new BoxView()
                        .BackgroundColor(Colors.Transparent)
                        .GridRow(1)
                        .GridColumn(1),

                    backMenuBtn
                        .GridRow(1)
                        .GridColumn(2)
                );

                // finally, make backCard's content be the grid
                backCard.HeightRequest(backCardHeight);
                backCard.AddChildren(backCardGrid);
            }
            else
            {
                //menu button placed inside back card
                var backMenuBtn = new Button()
                    .Text("⋯")
                    .FontSize(18)
                    .BackgroundColor(Colors.Transparent)
                    .HorizontalOptions(MauiControls.LayoutOptions.End)
                    .VerticalOptions(MauiControls.LayoutOptions.End)
                    .Margin(0, 0, 4, 4)
                    .Padding(0, 0, 8, 0)
                    .OnClicked(() => RaisePopup());

                // finally, make backCard's content be the grid
                backCard.AddChildren(backMenuBtn);
            }

            // ROOT layout: use Grid and layer back, front, badges
            var root = new Grid([ new (GridLength.Auto), new (GridLength.Auto) ], [ new(GridLength.Star)])
            {
                backCard,
                front,
            }
            .HeightRequest(backIconCount > 0 ? backCardHeight : CardHeight)
            .WidthRequest(cardWidth);

            return new ContentView() 
            {
                root
            }.HeightRequest(backIconCount > 0 ? 210 : 180);
        }

        private void RaisePopup()
        {
            System.Diagnostics.Debug.WriteLine($"Popup Button tapped");

            // Build PopupSpec with actions that will both invoke the CardView's callbacks (if any)
            // and can be used by the parent to perform additional logic.
            var spec = new PopupSpec
            {
                OnEdit = () => OnEdit?.Invoke(),
                OnView = () => OnView?.Invoke(),
                OnDelete = () => OnDelete?.Invoke()
            };

            RequestShowOverlay?.Invoke(spec);
        }

        VisualNode BuildFront(double cardWidth)
        {
            // overlay content: title, desc, footer
            var overlay = new Grid(new[] { new Microsoft.Maui.Controls.RowDefinition(GridLength.Auto), new Microsoft.Maui.Controls.RowDefinition(GridLength.Auto), new Microsoft.Maui.Controls.RowDefinition(GridLength.Auto) }, new[] { new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star) })
            {
                // title
                new Label()
                    .Text(() => "A" + Title ?? "No Title")
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
                        new Grid([new(GridLength.Star)], [new (GridLength.Star)])
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
                                        new(Color.FromRgba(0,0,0,0.28f), 0f),
                                        new(Color.FromRgba(0,0,0,0.08f), 1f)
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

        // animation-driven state (read by VisualNode)
        double _backdropOpacity = 0.0;
        double _popupOpacity = 0.0;
        double _popupOffset = 24.0; // vertical offset (px) — animate toward 0 for slide-up
        bool _isAnimating = false;

        // cancellation for in-flight animation
        CancellationTokenSource? _animCts;

        public override VisualNode Render()
        {
            List<CardView> listCard = [
                new CardView
                {
                    Title = "Pending Actions",
                    Description = "Short description here",
                    Variant = CardVariant.Outline,
                    IconGlyph = "\uf0f3",
                    IconFontFamily = "FA",
                    BackIconGlyphs = ["\uf004", "\uf0f3"],
                    RequestShowOverlay = (spec) =>
                    {
                        _overlaySpec = spec;
                        _overlayVisible = true;

                        // reset animation state to start values
                        _backdropOpacity = 0.0;
                        _popupOpacity = 0.0;
                        _popupOffset = 24.0;

                        Invalidate();

                        // start open animation on UI thread (no ElementRef required)
                        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            // small delay to ensure UI mounted
                            await Task.Delay(10);
                            await AnimateShowAsync();
                        });
                    }
                },
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

                        // reset animation state to start values
                        _backdropOpacity = 0.0;
                        _popupOpacity = 0.0;
                        _popupOffset = 24.0;

                        Invalidate();

                        // start open animation on UI thread (no ElementRef required)
                        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            // small delay to ensure UI mounted
                            await Task.Delay(10);
                            await AnimateShowAsync();
                        });
                    }
                },
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

                        // reset animation state to start values
                        _backdropOpacity = 0.0;
                        _popupOpacity = 0.0;
                        _popupOffset = 24.0;

                        Invalidate();

                        // start open animation on UI thread (no ElementRef required)
                        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            // small delay to ensure UI mounted
                            await Task.Delay(10);
                            await AnimateShowAsync();
                        });
                    }
                },
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

                        // reset animation state to start values
                        _backdropOpacity = 0.0;
                        _popupOpacity = 0.0;
                        _popupOffset = 24.0;

                        Invalidate();

                        // start open animation on UI thread (no ElementRef required)
                        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            // small delay to ensure UI mounted
                            await Task.Delay(10);
                            await AnimateShowAsync();
                        });
                    }
                }
            ];

            var list = new VerticalStackLayout
            {
                listCard /*.Select(x => new ContentView() { x }.HeightRequest(220))*/
            }
            .Spacing(12).Padding(new Thickness(6));

            var pageRoot = new Grid(new[] { new Microsoft.Maui.Controls.RowDefinition(GridLength.Star) }, new[] { new Microsoft.Maui.Controls.ColumnDefinition(GridLength.Star) })
            {
                new ScrollView
                {
                    list
                }
            };

            if (_overlayVisible && _overlaySpec != null)
            {
                // Backdrop (opacity driven by _backdropOpacity)
                var backdrop = new BoxView()
                    .BackgroundColor(() => Color.FromRgba(0, 0, 0, (float)_backdropOpacity))
                    .OnTapped(async () =>
                    {
                        // hide with animation
                        await AnimateHideAsync();
                    })
                    .GridRow(0)
                    .GridColumn(0);

                // Popup frame: opacity + top offset driven by state
                // We'll center horizontally, but use vertical offset (slide-up) for the open animation
                var deviceWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                var popupWidth = Math.Min(360, Math.Max(260, deviceWidth * 0.8));

                // Center container that positions the popup; we'll apply offset to the popup itself.
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
                // opacity & margin animated from state
                .Opacity(() => _popupOpacity)
                .Margin(0, _popupOffset, 0, 0);

                // Center popup container
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

        // show animation: fade backdrop from 0->0.15 and popup opacity 0->1 + popupOffset 24->0
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
                const int frameDelay = 16; // ~60fps -> 16ms per frame; 12 frames ≈ 200ms
                for (int i = 0; i <= frames; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var t = i / (double)frames; // 0..1
                    // ease out curve (quadratic)
                    var ease = 1 - Math.Pow(1 - t, 2);

                    _backdropOpacity = 0.15 * ease;
                    _popupOpacity = ease; // 0->1
                    _popupOffset = 24 * (1 - ease); // 24 -> 0

                    Invalidate();

                    await Task.Delay(frameDelay, token);
                }

                // ensure final values
                _backdropOpacity = 0.15;
                _popupOpacity = 1.0;
                _popupOffset = 0.0;
                Invalidate();
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AnimateShowAsync error: {ex}");
            }
            finally
            {
                _isAnimating = false;
            }
        }

        // hide animation: reverse of show, then clear overlay
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
                    var t = i / (double)frames; // 0..1
                    var ease = Math.Pow(1 - t, 2); // ease in reverse

                    _backdropOpacity = 0.15 * ease;
                    _popupOpacity = ease;
                    _popupOffset = 24 * (1 - ease);

                    Invalidate();

                    await Task.Delay(frameDelay, token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AnimateHideAsync error: {ex}");
            }
            finally
            {
                // finalize and clear
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