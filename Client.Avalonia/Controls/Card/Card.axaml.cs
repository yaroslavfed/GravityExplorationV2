using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Client.Avalonia.Controls.Card;

internal partial class Card : UserControl
{
    public Card()
    {
        InitializeComponent();
    }
    
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<Card, string>(
        "Title");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}