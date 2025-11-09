using KryptNx.FlowNxt.App.Components;
using KryptNx.FlowNxt.App.Components2;
using MauiReactor;

namespace KryptNx.FlowNxt.App.Pages;

public class MainPage : Component
{
    public override VisualNode Render()
    {
        return new ContentPage
        {
            new ScrollView
            {
                new VerticalStackLayout
                {
                    // These should be view components (not ContentPage)
                    new DemoPage(),           // if DemoPage renders visual nodes (views)
                    new StackedCardDemoPage() // same here
                }
                .Padding(new Thickness(12))
                .Spacing(12)
            }
        };
    }
}