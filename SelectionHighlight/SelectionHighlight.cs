using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace SelectionHighlight
{
	public class SelectionHighlight
	{
		private IAdornmentLayer _layer;

		private IWpfTextView _view;

		private ITextSelection _selection;

		private Brush _brush;

		private Pen _pen;

		private string selectedText = "";

		private VirtualSnapshotSpan selectedWord;

		private ITextSearchService _textSearchService;

		private ITagAggregator<MatchTag> _tagAggregator;

		public static object updateLock = new object();

		public List<SnapshotSpan> SnapShotsToColor = new List<SnapshotSpan>();

		public SelectionHighlight(IWpfTextView view, ITextSearchService TextSearchService, ITagAggregator<MatchTag> tagAggregator)
		{
			this._view = view;
			this._layer = view.GetAdornmentLayer("SelectionHighlight");
			this._selection = view.Selection;
			this._textSearchService = TextSearchService;
			this._tagAggregator = tagAggregator;
			this._view.LayoutChanged += new EventHandler<TextViewLayoutChangedEventArgs>(this.OnLayoutChanged);
			this._selection.SelectionChanged += new EventHandler(this.OnSelectionChanged);
			Brush brush = new SolidColorBrush(Colors.GreenYellow);
			brush.Freeze();
			Brush brush2 = new SolidColorBrush(Colors.AliceBlue);
			brush2.Freeze();
			Pen pen = new Pen(brush2, 0.5);
			pen.Freeze();
			this._brush = brush;
			this._pen = pen;
		}

		private void OnSelectionChanged(object sender, object e)
		{
			this.selectedText = this._view.Selection.StreamSelectionSpan.GetText();
			this.selectedWord = this._view.Selection.StreamSelectionSpan;
			this._layer.RemoveAllAdornments();
			this.SnapShotsToColor.Clear();
			if (!string.IsNullOrEmpty(this.selectedText) && !string.IsNullOrWhiteSpace(this.selectedText))
			{
				int length = this.selectedText.Length;
				int position = this._view.Selection.StreamSelectionSpan.Start.Position.Position;
				int num = position + length;
				if (position - 1 >= 0 && char.IsLetterOrDigit(this._view.TextSnapshot[position - 1]))
				{
					return;
				}
				if (num < this._view.TextSnapshot.GetText().Length && char.IsLetterOrDigit(this._view.TextSnapshot[num]))
				{
					return;
				}
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
				this.ColorWords();
			}
		}

		private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
		{
			this._layer.RemoveAllAdornments();
			this.ColorWords();
		}

		private void FindWordsInDocument()
		{
			lock (SelectionHighlight.updateLock)
			{
				FindData findData = new FindData(this.selectedWord.GetText(), this.selectedWord.Snapshot);
				findData.FindOptions = FindOptions.WholeWord;
				this.SnapShotsToColor.AddRange(this._textSearchService.FindAll(findData));
			}
		}

		private void ColorWords()
		{
			IWpfTextViewLineCollection textViewLines = this._view.TextViewLines;
			foreach (SnapshotSpan current in this.SnapShotsToColor)
			{
				try
				{
					Geometry markerGeometry = textViewLines.GetMarkerGeometry(current);
					if (markerGeometry != null)
					{
						GeometryDrawing geometryDrawing = new GeometryDrawing(this._brush, this._pen, markerGeometry);
						geometryDrawing.Freeze();
						DrawingImage drawingImage = new DrawingImage(geometryDrawing);
						drawingImage.Freeze();
						Image image = new Image();
						image.Source = drawingImage;
						Canvas.SetLeft(image, markerGeometry.Bounds.Left);
						Canvas.SetTop(image, markerGeometry.Bounds.Top);
						this._layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, new SnapshotSpan?(current), null, image, null);
					}
				}
				catch
				{
				}
			}
		}
	}
}
