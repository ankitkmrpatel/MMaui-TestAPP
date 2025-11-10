// StackedCardRefactor.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MauiReactor;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Devices;

namespace KryptNx.FlowNxt.App.Components3
{
    public enum BackgroundType
    {
        Solid,
        Gradient,
        Icon,
        Image
    }

    // Single merged model for each stacked card
    public class CardStackedModel
    {
        public string Id { get; set; }

        // Front content
        public string FrontTitle { get; set; }
        public string FrontDescription { get; set; }

        // Visual behavior
        public BackgroundType BackgroundType { get; set; } = BackgroundType.Solid;

        // Solid color
        public Color SolidColor { get; set; } = Colors.White;

        // Gradient
        public Color GradientStart { get; set; } = Colors.LightGray;
        public Color GradientEnd { get; set; } = Colors.DarkGray;

        // Icon (for Icon background)
        public string IconGlyph { get; set; } = "★";

        // Image
        public string ImageUrl { get; set; }

        // Back card: only background color (grey/black)
        public Color BackBackgroundColor { get; set; } = Colors.Black.WithAlpha(0.06f);

        // Action icons for back (heart, user, etc.)
        public VisualNode[] BackActionIcons { get; set; } = Array.Empty<VisualNode>();
    }

    static class Responsive
    {
        public static double GetDeviceWidthDp()
        {
            var info = DeviceDisplay.MainDisplayInfo;
            return info.Width / info.Density;
        }

        public static double GetDeviceHeightDp()
        {
            var info = DeviceDisplay.MainDisplayInfo;
            return info.Height / info.Density;
        }

        // Card height as a ratio of screen height with minimum
        public static double ComputeCardHeight(double ratio = 0.32)
        {
            var h = GetDeviceHeightDp();
            var height = Math.Max(140, Math.Round(h * ratio)); // minimum 140dp
            return height;
        }

        // Scale fonts based on width
        public static double FontScale(double baseSize)
        {
            var w = GetDeviceWidthDp();
            if (w < 360) return baseSize * 0.88;     // small phones
            if (w < 420) return baseSize * 0.95;     // typical phones
            if (w < 900) return baseSize * 1.0;      // tablets / desktop small
            return baseSize * 1.05;                 // large screens
        }
    }

    static class TextUtil
    {
        public static string TruncateWithEllipsis(string text, int maxChars)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.Length <= maxChars) return text;
            return text.Substring(0, Math.Max(0, maxChars - 3)).TrimEnd() + "...";
        }
    }



    // Back card — darker, sits behind and offset
    public class CardBack : Component
    {

        public CardBack(CardStackedModel m)
        {
            _m = m;
        }
        private readonly CardStackedModel _m;
        private readonly double _height = Responsive.ComputeCardHeight(0.32);
        private readonly Thickness _actionBoxOverlap = new Thickness(-20, 0, 0, 12); // negative left to overlap half-outside

        public override VisualNode Render()
        {
            // Back only shows background and action elements. No title/subtitle/desc.
            return new Frame
            {
                new Grid
                {
                    // background
                    new BoxView().Color(_m.BackBackgroundColor),

                    // overlay grid for menu & action icons
                    new Grid
                    {
                        // bottom-right menu (⋯)
                        new Button("⋯")
                            .FontSize(18)
                            .WidthRequest(36)
                            .HeightRequest(36)
                            .CornerRadius(18)
                            .HorizontalOptions(MauiControls.LayoutOptions.End)
                            .VerticalOptions(MauiControls.LayoutOptions.End)
                            .Margin(new Thickness(0, 0, 12, 12))
                            .OnClicked(() => {
                                // TODO: show options view/edit/add
                            }),

                        // bottom-left action icons box that slightly overflows left (half-outside)
                        new ContentView
                        {
                            new Frame
                            {
                                new HorizontalStackLayout
                                {
                                    _m.BackActionIcons
                                }
                                .Spacing(8)
                                .Padding(new Thickness(8, 6))
                            }
                            .CornerRadius(8)
                            .HasShadow(false)
                            .BackgroundColor(Colors.White) // icon box color
                        }
                        .HorizontalOptions(MauiControls.LayoutOptions.Start)
                        .VerticalOptions(MauiControls.LayoutOptions.End)
                        .Margin(_actionBoxOverlap) // negative left margin makes the box overflow
                    }
                }
            }
            .CornerRadius(12)
            .HasShadow(false)
            .HeightRequest(_height)
            .Margin(new Thickness(12, 16, 0, 0))
            .BackgroundColor(_m.BackBackgroundColor);
        }
    }

    // Front card — white (or slightly translucent) with large image centered
    // FRONT card (Title + Description + optional tilted icon for Icon variant)
    public class CardFront : Component
    {
        private readonly CardStackedModel _m;
        private readonly double _height;
        private readonly double _titleSize;
        private readonly double _descSize;

        public CardFront(CardStackedModel m)
        {
            _m = m;
            _height = Responsive.ComputeCardHeight(0.32);
            _titleSize = Responsive.FontScale(16);
            _descSize = Responsive.FontScale(13);
        }

        private VisualNode BuildBackground()
        {
            switch (_m.BackgroundType)
            {
                case BackgroundType.Gradient:
                    {
                        var brush = new MauiControls.LinearGradientBrush(
                            [
                                new MauiControls.GradientStop(_m.GradientStart, 0f),
                                new MauiControls.GradientStop(_m.GradientEnd, 1f)
                            ],
                            new Point(0, 0),
                            new Point(1, 1)
                        );
                        return new Grid { }.Background(brush);
                    }

                case BackgroundType.Image:
                    if (!string.IsNullOrWhiteSpace(_m.ImageUrl))
                        return new Image(_m.ImageUrl).Aspect(Aspect.AspectFill);
                    return new BoxView().Color(_m.SolidColor);

                case BackgroundType.Icon:
                case BackgroundType.Solid:
                default:
                    return new BoxView().Color(_m.SolidColor);
            }
        }

        public override VisualNode Render()
        {
            // conservative char limit for trimming
            int charLimit = 140; // safe default (will be cut with Label MaxLines)
            var desc = TextUtil.TruncateWithEllipsis(_m.FrontDescription ?? string.Empty, charLimit);

            var background = BuildBackground();

            // Icon overlay when BackgroundType == Icon: rotated, placed bottom-right and tilted left
            var iconOverlay = (_m.BackgroundType == BackgroundType.Icon)
                ? (VisualNode)new Label(_m.IconGlyph ?? "★")
                    .FontSize(64)
                    .Opacity(0.14)
                    .Rotation(-16) // tilted left
                    .HorizontalOptions(MauiControls.LayoutOptions.End)
                    .VerticalOptions(MauiControls.LayoutOptions.End)
                    .Margin(new Thickness(0, 0, 8, 6))
                : new BoxView().Color(Colors.Transparent);

            return new Frame
            {
                // stacking background and overlay content
                new Grid
                {
                    background,

                    // content column
                    new VerticalStackLayout
                    {
                        // Title row top-right style: push to right
                        new HorizontalStackLayout
                        {
                            new Label() // spacer left to push title right
                                .HorizontalOptions(MauiControls.LayoutOptions.StartAndExpand),

                            new Label(_m.FrontTitle ?? string.Empty)
                                .FontAttributes(MauiControls.FontAttributes.Bold)
                                .FontSize(_titleSize)
                                .HorizontalOptions(MauiControls.LayoutOptions.End)
                                .VerticalOptions(MauiControls.LayoutOptions.Start)
                                .Padding(new Thickness(0, 8, 8, 0))
                        }
                        .Padding(new Thickness(12, 8, 12, 0)),

                        // description - trimmed and limited to lines
                        new Label(desc)
                            .FontSize(_descSize)
                            .LineBreakMode(LineBreakMode.TailTruncation)
                            .MaxLines(3)
                            .Padding(new Thickness(12, 6, 12, 8))
                            .HorizontalOptions(MauiControls.LayoutOptions.Fill)
                            .VerticalOptions(MauiControls.LayoutOptions.Start)
                    },

                    // Icon overlay (z-order above content)
                    iconOverlay
                }
            }
            .CornerRadius(12)
            .HasShadow(true)
            .BackgroundColor(Colors.Transparent)
            .HeightRequest(_height);
        }
    }

    // Composite that stacks back first and front second (so front overlays back)
    // Composite card that stacks back (z=0) then front (z=1)
    public class CardStack : Component
    {
        private readonly CardStackedModel _m;

        public CardStack(CardStackedModel m)
        {
            _m = m;
        }

        public override VisualNode Render()
        {
            // wrapper so we can apply layout options
            return new ContentView
            {
                new Grid
                {
                    // back first
                    new CardBack(_m),
                    // front inset so the back peeks on right & bottom
                    new ContentView
                    {
                        new CardFront(_m)
                    }
                    .Margin(new Thickness(0, 0, 22, 22)) // front inset: controls peek
                }
            }
            .HorizontalOptions(MauiControls.LayoutOptions.Fill)
            .Margin(new Thickness(12, 8));
        }
    }

    // Demo page: generate random variants and show cards responsive
    public class StackedCardDemoPage : Component
    {
        private readonly List<CardStackedModel> _items;

        public StackedCardDemoPage(int count = 6)
        {
            var rand = new Random();
            _items = [.. Enumerable.Range(1, count).Select(i =>
            {
                var variant = (BackgroundType)rand.Next(Enum.GetValues(typeof(BackgroundType)).Length);

                return new CardStackedModel
                {
                    Id = i.ToString(),
                    FrontTitle = $"My Pending Actions",
                    FrontDescription = (i % 2 == 0)
                        ? "This is short desc of this card....."
                        : "This is a longer description that should be trimmed and show ellipsis when it overflows the visible area of the card. It demonstrates trimming behavior across devices.",
                    BackgroundType = variant,
                    SolidColor = variant == BackgroundType.Solid ? Colors.White : Colors.White,
                    GradientStart = Color.FromUint(0xFFB3E5FC), // light blue
                    GradientEnd = Color.FromUint(0xFF81D4FA), // medium blue
                    ImageUrl = "https://images.unsplash.com/photo-1503023345310-bd7c1de61c7d?w=800&q=80",
                    IconGlyph = "★",
                    BackBackgroundColor = Colors.Black.WithAlpha(0.08f),
                    BackActionIcons =
                    [
                        new Button("❤").WidthRequest(36).HeightRequest(34).CornerRadius(8),
                        new Button("👤").WidthRequest(36).HeightRequest(34).CornerRadius(8)
                    ]
                };
            })];
        }

        public override VisualNode Render()
        {
            var nodes = new List<VisualNode>();

            foreach (var item in _items)
            {
                nodes.Add(
                        //new ContentView
                        //{
                        //    new CardStack(item)
                        new CardStack(item)
                //}
                //.HorizontalOptions(MauiControls.LayoutOptions.FillAndExpand)
                );
            }

            return new VerticalStackLayout
            {
                nodes.ToArray()
            }
            .Spacing(14)
            .Padding(new Thickness(16));
        }
    }
}
