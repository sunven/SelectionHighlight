using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace SelectionHighlight
{
    internal class MatchGlyphFactory : IGlyphFactory
    {
        public IAdornmentLayer Layer { get; }

        public ITextSelection Selection { get; }

        public MatchGlyphFactory(IWpfTextView view)
        {
            Layer = view.GetAdornmentLayer("SelectionHighlight");
            Selection = view.Selection;
        }

        public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
        {
            if (!(tag is MatchTag)) return null;
            return new Rectangle
            {
                Fill = new SolidColorBrush(Colors.GreenYellow),
                StrokeThickness = 1.0,
                Stroke = Brushes.Black,
                Height = 10.0,
                Width = 10.0
            };
        }
    }
}