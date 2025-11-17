// StackedCards.cs
// MauiReactor components: CardFront, CardBack, CardStack + example MainPage usage.
// Put in your project and adapt namespaces as needed.

using System;
using System.Collections.Generic;
using MauiReactor;
using KryptNx.FlowNxt.App.Models.ViewModels;
using Microsoft.Maui.Graphics;
using MauiReactor.Compatibility;


namespace KryptNx.FlowNxt.App.Components
{
    // Reusable front card component
    public class CardFront : Component
    {
        private readonly CardModel _model;
        private readonly bool _showIcon;
        private readonly Action<string> _onAction; // optional callback for icon clicks etc.
        public CardFront(CardModel model, bool showIcon = true, Action<string> onAction = null)
        {
            _model = model;
            _showIcon = showIcon;
            _onAction = onAction;
        }

        public override VisualNode Render()
        {
            // Build background content based on model.BackgroundType
            VisualNode backgroundContent = null;

            switch (_model.BackgroundType)
            {
                case BackgroundType.Solid:
                    backgroundContent = new Border
                    {
                        new Grid()
                    }
                    .Background(_model.SolidColor);
                    break;

                case BackgroundType.Gradient:
                    // Use a simple Stack with background brush (Maui: LinearGradientBrush)
                    var brush = new MauiControls.LinearGradientBrush(
                        new MauiControls.GradientStopCollection
                        {
                            new MauiControls.GradientStop(_model.GradientStart, 0f),
                            new MauiControls.GradientStop(_model.GradientEnd, 1f)
                        },
                        new Point(0, 0),
                        new Point(1, 1)
                    );

                    backgroundContent = new Border
                    {
                        new Grid()
                    }
                    .Background(brush);
                    break;

                case BackgroundType.Icon:
                    // Use a subtle light background and an oversized icon element below (rendered later)
                    backgroundContent = new Border { new Grid() }.Background(Colors.White);
                    break;

                case BackgroundType.Image:
                    backgroundContent = new Image(_model.ImageUrl).Aspect(Aspect.AspectFill);
                    break;
            }

            // Card frame (front)
            return new Frame
            {
                // Place background as first child so it sits under overlays (Frame content stacking)
                new Grid
                {
                    // background layer (fills)
                    backgroundContent,

                    // Content column: Top area small, main description below (we'll overlay title in top-right)
                    new VerticalStackLayout
                    {
                        // Spacer (to push content down a bit; feel free to tune)
                        new BoxView().HeightRequest(12),

                        // top overlay: title in top-right, using HorizontalStackLayout aligned end
                            new HorizontalStackLayout
                            {
                                new Label(_model.Title)
                                    .FontSize(18)
                                    .FontAttributes(MauiControls.FontAttributes.Bold)
                                    .HorizontalOptions(MauiControls.LayoutOptions.EndAndExpand)
                                    .VerticalOptions(MauiControls.LayoutOptions.Start),
                            }
                            .Padding(new Thickness(12, 10, 12, 0)),

                        new Grid
                        {
                            // bottom row placeholder (so icon can be placed bottom-left via absolute)
                        },

                        // Reactor 'Content' helper: We'll create an overlay grid with AbsoluteLayout style positioning
                        new Grid
                        {
                            // Description block (center-left)
                            new Label(_model.Description)
                                .FontSize(14)
                                .LineBreakMode(LineBreakMode.TailTruncation)
                                .MaxLines(4)
                                .HorizontalOptions(MauiControls.LayoutOptions.Start)
                                .VerticalOptions(MauiControls.LayoutOptions.Start)
                                .Padding(new Thickness(12, 8, 12, 8)),

                            // This box will act as the description area already added; keep it transparent.

                            // Icon overlay bottom-left (tilted)
                            _showIcon && _model.BackgroundType == BackgroundType.Icon
                                ? (VisualNode)new Label(_model.IconGlyph)
                                    .FontSize(56)
                                    .Opacity(0.12)
                                    .TranslationX(-6)
                                    .TranslationY(10)
                                    .Rotation(-18) // tilted
                                    .HorizontalOptions(MauiControls.LayoutOptions.End)
                                    .VerticalOptions(MauiControls.LayoutOptions.Start)
                                    .Margin(new Thickness(12, 0, 0, 12))
                                    .OnTapped(() => _onAction?.Invoke("iconTapped"))
                                : new BoxView().WidthRequest(0).HeightRequest(0) // placeholder
                        }
                    }
                }
            }
            .CornerRadius(12)
            .HasShadow(true)
            .Margin(new Thickness(4));
        }
    }

    // Reusable back card (slightly offset behind the front)
    public class CardBack : Component
    {
        private readonly CardModel _model;
        private readonly IEnumerable<VisualNode> _rightIcons; // icons/button nodes shown bottom-right of the stacked card
        public CardBack(CardModel model, IEnumerable<VisualNode> rightIcons = null)
        {
            _model = model;
            _rightIcons = rightIcons ?? Array.Empty<VisualNode>();
        }

        public override VisualNode Render()
        {
            // Simple back card (usually darker or secondary)
            return new Frame
            {
                new VerticalStackLayout
                {
                    new Grid
                    {
                        // bottom-left: options button (3 dots) - simple menu simulation
                        new Button("⋯")
                            .FontSize(18)
                            .WidthRequest(40)
                            .HeightRequest(36)
                            .CornerRadius(18)
                            .HorizontalOptions(MauiControls.LayoutOptions.Start)
                            .VerticalOptions(MauiControls.LayoutOptions.End)
                            .Margin(new Thickness(12))
                            .OnClicked(() => {
                                // Put your options handling here - e.g., show a popup or change state
                            }),

                        // bottom-right: icons passed from caller
                        new HorizontalStackLayout
                        {
                            _rightIcons
                        }
                        .Spacing(8)
                        .HorizontalOptions(MauiControls.LayoutOptions.End)
                        .VerticalOptions(MauiControls.LayoutOptions.End)
                        .Padding(new Thickness(0,0,12,12))
                    }
                }
            }
            .CornerRadius(12)
            .HasShadow(false)
            // Slightly smaller elevation/offset via translation to appear behind
            .TranslationX(-12)
            .TranslationY(14)
            .Margin(new Thickness(10))
            .Background(Microsoft.Maui.Graphics.Colors.Black.WithAlpha(0.06f));
        }
    }

    // Composite that stacks front and back together
    public class CardStack : Component
    {
        private readonly CardModel _front;
        private readonly CardModel _back;
        private readonly IEnumerable<VisualNode> _rightIcons;
        private readonly Action<string> _onAction;

        public CardStack(CardModel front, CardModel back, IEnumerable<VisualNode> rightIcons = null, Action<string> onAction = null)
        {
            _front = front;
            _back = back;
            _rightIcons = rightIcons;
            _onAction = onAction;
        }

        public override VisualNode Render()
        {
            // We use a Grid and intentionally place the back first then front so they appear stacked.
            return new ContentView
            {
                new Grid
                {
                    // Back card at z-order 0
                    new CardBack(_back, _rightIcons),

                    // Front card on top
                    new CardFront(_front, showIcon: true, onAction: _onAction)
                }
            }
            .Padding(new Thickness(8))
            .HorizontalOptions(MauiControls.LayoutOptions.Fill)
            .VerticalOptions(MauiControls.LayoutOptions.Start);
        }
    }

    // Example MainPage that demonstrates a list of stacked cards and provides icons for bottom-right
    public class DemoPage : Component
    {
        private readonly List<(CardModel front, CardModel back)> _data;

        public DemoPage()
        {
            _data = new List<(CardModel, CardModel)>();

            // sample items
            for (int i = 1; i <= 4; i++)
            {
                var front = new CardModel
                {
                    Id = $"f{i}",
                    Title = $"Front Title {i}",
                    Description = $"A short description for front card {i}. This is where the H2 content would live.",
                    BackgroundType = i % 2 == 0 ? BackgroundType.Icon : BackgroundType.Image,
                    IconGlyph = "★",
                    ImageUrl = "https://images.unsplash.com/photo-1503023345310-bd7c1de61c7d?w=800&q=80",
                    SolidColor = i % 2 == 0 ? Colors.White : Colors.LightGray
                };

                var back = new CardModel
                {
                    Id = $"b{i}",
                    Title = $"Back Title {i}",
                    Description = $"Back card details for item {i}. Contains options on bottom-left.",
                    BackgroundType = BackgroundType.Solid,
                    SolidColor = Colors.Black.WithAlpha(0.04f)  //new Color(0.12f, 0.12f, 0.12f, 1f); // r,g,b,a
                };

                _data.Add((front, back));
            }
        }

        public override VisualNode Render()
        {
            // define some action icons for bottom-right (example)
            IEnumerable<VisualNode> BuildRightIcons()
            {
                return new VisualNode[]
                {
                    new Button("👁")
                        .WidthRequest(38)
                        .HeightRequest(34)
                        .CornerRadius(8)
                        .OnClicked(()=> {/* view */}),
                    new Button("✎")
                        .WidthRequest(38)
                        .HeightRequest(34)
                        .CornerRadius(8)
                        .OnClicked(()=> {/* edit */})
                };
            }

            var rows = new List<VisualNode>();
            foreach (var (front, back) in _data)
            {
                rows.Add(
                        new CardStack(front, back, BuildRightIcons(), onAction: (action) => {
                            // handle card front actions (e.g., icon tapped)
                        })
                );
            }

            return new VerticalStackLayout
            {
                rows.ToArray()
            }
            .Spacing(18)
            .Padding(new Thickness(20));
        }
    }
}
