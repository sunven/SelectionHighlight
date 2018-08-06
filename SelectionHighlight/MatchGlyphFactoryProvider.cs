using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace SelectionHighlight
{
    [TagType(typeof(MatchTag)), ContentType("any"), Name("MatchGlyph"), Order(After = "VsTextMarker"), Export(typeof(IGlyphFactoryProvider))]
	internal sealed class MatchGlyphFactoryProvider : IGlyphFactoryProvider
	{
		public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
		{
			return new MatchGlyphFactory(view);
		}
	}
}
