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
                    new Components3.CardDemoPage(), // same here
                    //new Components.DemoPage(),
                    //new Components2.StackedCardDemoPage(), // same here
                    new Components4.CardDemoPage() // same here
                }
                .Padding(new Thickness(4))
                .Spacing(12)
            }
        };
    }
}