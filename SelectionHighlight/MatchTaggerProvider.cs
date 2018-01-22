using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace SelectionHighlight
{
	[TagType(typeof(MatchTag)), ContentType("any"), Export(typeof(IViewTaggerProvider))]
	public class MatchTaggerProvider : IViewTaggerProvider
	{
		[Import]
		internal IClassifierAggregatorService AggregatorFactory;

		[Import]
		internal ITextSearchService TextSearchService
		{
			get;
			set;
		}

		public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}
			if (textView.TextBuffer != buffer)
			{
				return null;
			}
			return new MatchTagger(textView, buffer, AggregatorFactory.GetClassifier(buffer), TextSearchService) as ITagger<T>;
		}
	}
}
