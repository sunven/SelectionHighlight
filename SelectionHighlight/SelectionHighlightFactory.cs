using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace SelectionHighlight
{
    [TextViewRole("DOCUMENT"), ContentType("text"), Export(typeof(IWpfTextViewCreationListener))]
	internal sealed class SelectionHighlightFactory : IWpfTextViewCreationListener
	{
		[TextViewRole("DOCUMENT"), Name("SelectionHighlight"), Order(After = "SelectionAndProvisionHighlight", Before = "Text"), Export(typeof(AdornmentLayerDefinition))]
		public AdornmentLayerDefinition EditorAdornmentLayer;

		[Import]
		internal ITextSearchService TextSearchService
		{
			get;
			set;
		}

		[Import]
		internal IViewTagAggregatorFactoryService AggregatorFactory
		{
			get;
			set;
		}

		public void TextViewCreated(IWpfTextView textView)
		{
			//var tagAggregator = AggregatorFactory.CreateTagAggregator<MatchTag>(textView);
		    //var selectionHighlight = new SelectionHighlight(textView, TextSearchService, tagAggregator);
		}
	}
}
