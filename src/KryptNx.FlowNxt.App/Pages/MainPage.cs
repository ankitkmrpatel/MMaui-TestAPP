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
                    //new Components3.CardDemoPage(), // same here
                    //new Components.DemoPage(),
                    //new Components2.StackedCardDemoPage(), // same here
                    new Components4.CardDemoPage(), // same here
                    //new MauiReactorCards.CardClaudeStackedMainPage()
                }
                .Padding(new Thickness(4))
                .Spacing(12)
            }
        };

        //return new ContentPage
        //{
        //    new ScrollView
        //    {
        //        new VerticalStackLayout()
        //        {
        //            new VStack(spacing: 30)
        //            {
        //                // Card without icons
        //                new MauiReactorCards.CardClaudeStackedCard(new CardData
        //                {
        //                    Title = "Project Alpha",
        //                    Description = "A revolutionary approach to mobile development with cross-platform capabilities.",
        //                    BackgroundColor = Color.FromArgb("#6366F1")
        //                }),

        //                // Card with icons
        //                new MauiReactorCards.CardClaudeStackedCard(new CardData
        //                {
        //                    Title = "Design Sprint",
        //                    Description = "Collaborative design session for the new user interface redesign project.",
        //                    BackgroundColor = Color.FromArgb("#EC4899"),
        //                    Icons = new List<CardIcon>
        //                    {
        //                        new CardIcon { IconText = "👥", Label = "Team", OnTap = () => { } },
        //                        new CardIcon { IconText = "📅", Label = "Schedule", OnTap = () => { } },
        //                        new CardIcon { IconText = "📎", Label = "Attach", OnTap = () => { } },
        //                        new CardIcon { IconText = "💬", Label = "Chat", OnTap = () => { } }
        //                    }
        //                }),

        //                // Another card with different color
        //                new MauiReactorCards.CardClaudeStackedCard(new CardData
        //                {
        //                    Title = "Marketing Campaign",
        //                    Description = "Q4 marketing strategy and social media content planning.",
        //                    BackgroundColor = Color.FromArgb("#10B981"),
        //                    Icons = new List<CardIcon>
        //                    {
        //                        new CardIcon { IconText = "📊", Label = "Stats", OnTap = () => { } },
        //                        new CardIcon { IconText = "🎯", Label = "Goals", OnTap = () => { } }
        //                    }
        //                })
        //            }
        //            .Padding(20)
        //        }
        //    }
        //}
        //.BackgroundColor(Color.FromArgb("#F3F4F6"));
    }
}