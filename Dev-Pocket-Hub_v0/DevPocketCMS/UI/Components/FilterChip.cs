using DevPocketCMS.UI.Theme;
using MauiReactor;
using MauiReactor.Shapes;

namespace DevPocketCMS.UI.Components;

class FilterChip : Component
{
    private string _label = string.Empty;
    private bool _isSelected;
    private Color _color = AppColors.Accent;
    private Action? _onTap;

    public FilterChip WithLabel(string label) { _label = label; return this; }
    public FilterChip SetIsSelected(bool sel) { _isSelected = sel; return this; }
    public FilterChip WithColor(Color color) { _color = color; return this; }
    public FilterChip SetOnTap(Action action) { _onTap = action; return this; }

    public override VisualNode Render() =>
        Border(
            Label(_label)
                .FontSize(13)
                .FontAttributes(_isSelected ? FontAttributes.Bold : FontAttributes.None)
                .TextColor(_isSelected ? AppColors.Background : AppColors.TextSecondary)
                .Padding(12, 7)
        )
        .StrokeShape(new RoundRectangle().CornerRadius(20))
        .BackgroundColor(_isSelected ? _color : AppColors.Card)
        .Stroke(_isSelected ? _color : AppColors.CardBorder)
        .StrokeThickness(1)
        .OnTapped(_onTap);
}
