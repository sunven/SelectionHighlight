using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;

namespace SelectionHighlight
{
	public class MatchTagger : ITagger<MatchTag>
	{
		private IClassifier _aggregator;

		private ITextView _view;

		private ITextSelection _selection;

		private ITextBuffer _sourceBuffer;

		private string selectedText = "";

		private VirtualSnapshotSpan selectedWord;

		private ITextSearchService _textSearchService;

		private List<SnapshotSpan> glyphsToPlace;

		private List<ITagSpan<MatchTag>> glyphsTagged;

		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

		internal MatchTagger(ITextView view, ITextBuffer SourceBuffer, IClassifier aggregator, ITextSearchService TextSearchService)
		{
			this._view = view;
			this._selection = view.Selection;
			this._aggregator = aggregator;
			this._sourceBuffer = SourceBuffer;
			this._textSearchService = TextSearchService;
			this.glyphsToPlace = new List<SnapshotSpan>();
			this.glyphsTagged = new List<ITagSpan<MatchTag>>();
			this._selection.SelectionChanged += new EventHandler(this.OnSelectionChanged);
			this._view.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(this.OnLayoutChanged);
		}

		private void OnSelectionChanged(object sender, object e)
		{
			lock (SelectionHighlight.updateLock)
			{
				EventHandler<SnapshotSpanEventArgs> tagsChanged = this.TagsChanged;
				this.selectedText = this._view.Selection.StreamSelectionSpan.GetText();
				this.selectedWord = this._view.Selection.StreamSelectionSpan;
				this.glyphsToPlace.Clear();
				if (tagsChanged != null)
				{
					tagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(this._sourceBuffer.CurrentSnapshot, 0, this._sourceBuffer.CurrentSnapshot.Length)));
				}
				if (!string.IsNullOrEmpty(this.selectedText) && !string.IsNullOrWhiteSpace(this.selectedText))
				{
					int length = this.selectedText.Length;
					int position = this._view.Selection.StreamSelectionSpan.Start.Position.Position;
					int num = position + length;
					if (position - 1 < 0 || !char.IsLetterOrDigit(this._view.TextSnapshot[position - 1]))
					{
						if (num >= this._view.TextSnapshot.GetText().Length || !char.IsLetterOrDigit(this._view.TextSnapshot[num]))
						{
							string text = this.selectedText;
							for (int i = 0; i < text.Length; i++)
							{
								char c = text[i];
								if (!char.IsLetterOrDigit(c) && c != '_')
								{
									return;
								}
							}
							this.FindWordsInDocument();
							if (tagsChanged != null)
							{
								tagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(this._sourceBuffer.CurrentSnapshot, 0, this._sourceBuffer.CurrentSnapshot.Length)));
							}
						}
					}
				}
			}
		}

		private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
		{
			EventHandler<SnapshotSpanEventArgs> tagsChanged = this.TagsChanged;
			if (tagsChanged != null)
			{
				tagsChanged(this, new SnapshotSpanEventArgs(new SnapshotSpan(this._sourceBuffer.CurrentSnapshot, 0, this._sourceBuffer.CurrentSnapshot.Length)));
			}
		}

		private void FindWordsInDocument()
		{
			lock (SelectionHighlight.updateLock)
			{
				FindData findData = new FindData(this.selectedWord.GetText(), this.selectedWord.Snapshot);
				findData.FindOptions = FindOptions.WholeWord;
				this.glyphsToPlace.AddRange(this._textSearchService.FindAll(findData));
			}
		}

		IEnumerable<ITagSpan<MatchTag>> ITagger<MatchTag>.GetTags(NormalizedSnapshotSpanCollection spans)
		{
			this.glyphsTagged.Clear();
			foreach (SnapshotSpan current in this.glyphsToPlace)
			{
				this.glyphsTagged.Add(new TagSpan<MatchTag>(current, new MatchTag()));
			}
			return this.glyphsTagged;
		}
	}
}
