using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SelectionHighlight
{
	internal class MatchGlyphFactory : IGlyphFactory
	{
		private const double _width = 10.0;

		private const double _height = 10.0;

		private IAdornmentLayer _layer;

		private ITextSelection _selection;

		public MatchGlyphFactory(IWpfTextView view)
		{
			this._layer = view.GetAdornmentLayer("SelectionHighlight");
			this._selection = view.Selection;
		}

		public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
		{
			if (tag == null || !(tag is MatchTag))
			{
				return null;
			}
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
