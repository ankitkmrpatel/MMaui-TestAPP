using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS.UI.Components;

class StatCard : Component
{
    private string _label = string.Empty;
    private int _value;
    private string _icon = "📊";
    private Color _color = AppColors.Accent;

    public StatCard SetLabel(string label) { _label = label; return this; }
    public StatCard SetValue(int value) { _value = value; return this; }
    public StatCard HasIcon(string icon) { _icon = icon; return this; }
    public StatCard WithColor(Color color) { _color = color; return this; }

    public override VisualNode Render() =>
        Border(
            VStack(
                Label(_icon).FontSize(24).HCenter(),
                Label(_value.ToString())
                    .FontSize(28)
                    .FontAttributes(FontAttributes.Bold)
                    .TextColor(_color)
                    .HCenter(),
                Label(_label)
                    .FontSize(12)
                    .TextColor(AppColors.TextSecondary)
                    .HCenter()
            )
            .Spacing(4)
            .Padding(12)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(14))
        .BackgroundColor(AppColors.Card)
        .Stroke(AppColors.CardBorder)
        .StrokeThickness(1)
        .HFill();
}
