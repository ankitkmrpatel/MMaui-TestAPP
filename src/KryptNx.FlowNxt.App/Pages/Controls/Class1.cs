// StackedCardsSimple.cs
using System;
using MauiReactor;
using Controls = Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiReactor.Compatibility;

namespace KryptNx.FlowNxt.App.Components2
{
    public class CardModel
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string ImageUrl { get; set; }
        public Color FrontBackground { get; set; } = Colors.White;
        public Color BackBackground { get; set; } = Colors.Black.WithAlpha(0.06f);
    }

    // Back card — darker, sits behind and offset
    public class CardBack : Component
    {
        private readonly CardModel _m;
        private readonly VisualNode[] _rightIcons;
        public CardBack(CardModel m, VisualNode[] rightIcons = null)
        {
            _m = m;
            _rightIcons = rightIcons ?? Array.Empty<VisualNode>();
        }

        public override VisualNode Render()
        {
            return new Frame
            {
                new Grid
                {
                    // background (simple BoxView)
                    new BoxView().Color(_m.BackBackground),

                    // content: small title + subtitle area
                    new VerticalStackLayout
                    {
                        new Label(_m.Title)
                            .FontAttributes(Controls.FontAttributes.Bold)
                            .FontSize(15)
                            .Padding(new Thickness(12, 12, 12, 2)),
                        new Label(_m.Subtitle)
                            .FontSize(12)
                            .Opacity(0.85)
                            .Padding(new Thickness(12, 0, 12, 12)),

                        new Grid
                {
                    // bottom-right icons holder (example)
                    new HorizontalStackLayout
                    {
                        _rightIcons
                    }
                    .Spacing(8)
                    .Padding(new Thickness(0,0,12,12))
                    .HorizontalOptions(Controls.LayoutOptions.End)
                    .VerticalOptions(Controls.LayoutOptions.End)
                }
                    }
                }
            }
            .CornerRadius(12)
            .HasShadow(false)
            // offset so it peeks from behind the front card
            .Margin(new Thickness(12, 16, 0, 0))
            .Background(_m.BackBackground);
        }
    }

    // Front card — white (or slightly translucent) with large image centered
    public class CardFront : Component
    {
        private readonly CardModel _m;
        public CardFront(CardModel m) => _m = m;

        public override VisualNode Render()
        {
            // image background in center
            var imageNode = !string.IsNullOrEmpty(_m.ImageUrl)
                ? (VisualNode)new Image(_m.ImageUrl).Aspect(Aspect.AspectFit)
                : new BoxView().Color(Colors.LightGray);

            return new Frame
            {
                new Grid
                {
                    // background below
                    new BoxView().Color(_m.FrontBackground),

                    // content layout
                    new VerticalStackLayout
                    {

                        new ContentView
                        {
                                // big image area
                            imageNode
                        }
                        .HorizontalOptions(Controls.LayoutOptions.Center)
                            .VerticalOptions(Controls.LayoutOptions.Center)
                            .Margin(new Thickness(0, 8, 0, 8)),

                        // bottom row: avatar + title
                        new HorizontalStackLayout
                        {
                            // avatar placeholder
                            new Frame
                            {
                                new Label("🙂").FontSize(18).HorizontalOptions(Controls.LayoutOptions.Center).VerticalOptions(Controls.LayoutOptions.Center)
                            }
                            .CornerRadius(18)
                            .WidthRequest(36)
                            .HeightRequest(36)
                            .Padding(0)
                            .Margin(new Thickness(8, 0, 8, 8)),

                            // title + subtitle stacked
                            new VerticalStackLayout
                            {
                                new Label(_m.Title).FontAttributes(Controls.FontAttributes.Bold).FontSize(16),
                                new Label(_m.Subtitle).FontSize(12).Opacity(0.85)
                            }
                            .VerticalOptions(Controls.LayoutOptions.Center)
                        }
                        .Padding(new Thickness(8, 0, 12, 12))
                    }
                }
            }
            .CornerRadius(12)
            .HasShadow(true)
            // Make front slightly opaque so the back is visually distinguishable where it peeks
            .BackgroundColor(_m.FrontBackground) // change .WithAlpha(...) if you want translucency
            .Padding(0)
            // Give no extra offset here — parent sets margin to reveal back
            .Margin(new Thickness(0));
        }
    }

    // Composite that stacks back first and front second (so front overlays back)
    public class CardStack : Component
    {
        private readonly CardModel _front;
        private readonly CardModel _back;
        private readonly VisualNode[] _rightIcons;

        public CardStack(CardModel front, CardModel back, VisualNode[] rightIcons = null)
        {
            _front = front;
            _back = back;
            _rightIcons = rightIcons;
        }

        public override VisualNode Render()
        {
            return new ContentView
            {
                new Grid
                {
                    // order matters: back first, front second
                    new CardBack(_back, _rightIcons),
                    // wrap front so we can apply margin to reveal the back on right/bottom
                    new ContentView
                    {
                        new CardFront(_front)
                    }
                    .Margin(new Thickness(0,0,22,22)) // <-- controls how much back peeks out (right & bottom)
                }
            }
            .HorizontalOptions(Controls.LayoutOptions.Fill)
            .Margin(new Thickness(12));
        }
    }

    // Example page to demo the stacked card similar to your screenshot
    public class StackedCardDemoPage : Component
    {
        public override VisualNode Render()
        {
            var front = new CardModel
            {
                Title = "Some card idea I saw",
                Subtitle = "Tom Hermans",
                ImageUrl = "https://images.unsplash.com/photo-1503023345310-bd7c1de61c7d?w=800&q=80",
                FrontBackground = Colors.White
            };

            var back = new CardModel
            {
                Title = "",
                Subtitle = "",
                BackBackground = Colors.Black.WithAlpha(0.14f),
            };

            var rightIcons = new VisualNode[]
            {
                new Button("⋯").WidthRequest(36).HeightRequest(36).CornerRadius(18).OnClicked(()=>{}),
                // add more icons if needed
            };

            return new VerticalStackLayout
            {
                new CardStack(front, back, rightIcons)
            }
            .Padding(new Thickness(20));
        }
    }
}
