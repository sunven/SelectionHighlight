using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace SelectionHighlight
{
    public class SelectionHighlight
    {
        public static object UpdateLock = new object();

        private readonly Brush _brush;
        private readonly IAdornmentLayer _layer;

        private readonly Pen _pen;

        private readonly ITextSearchService _textSearchService;
        public ITagAggregator<MatchTag> TagAggregator { get; }

        private readonly IWpfTextView _view;

        private string _selectedText = "";

        private VirtualSnapshotSpan _selectedWord;

        public List<SnapshotSpan> SnapShotsToColor = new List<SnapshotSpan>();

        public SelectionHighlight(IWpfTextView view, ITextSearchService textSearchService,
            ITagAggregator<MatchTag> tagAggregator)
        {
            _view = view;
            _layer = view.GetAdornmentLayer("SelectionHighlight");
            var selection = view.Selection;
            _textSearchService = textSearchService;
            TagAggregator = tagAggregator;
            _view.LayoutChanged += OnLayoutChanged;
            selection.SelectionChanged += OnSelectionChanged;
            Brush brush = new SolidColorBrush(Colors.GreenYellow);
            brush.Freeze();
            Brush brush2 = new SolidColorBrush(Colors.AliceBlue);
            brush2.Freeze();
            var pen = new Pen(brush2, 0.5);
            pen.Freeze();
            _brush = brush;
            _pen = pen;
        }

        private void OnSelectionChanged(object sender, object e)
        {
            _selectedText = _view.Selection.StreamSelectionSpan.GetText();
            _selectedWord = _view.Selection.StreamSelectionSpan;
            _layer.RemoveAllAdornments();
            SnapShotsToColor.Clear();
            if (string.IsNullOrEmpty(_selectedText) || string.IsNullOrWhiteSpace(_selectedText)) return;
            var length = _selectedText.Length;
            var position = _view.Selection.StreamSelectionSpan.Start.Position.Position;
            var num = position + length;
            if (position - 1 >= 0 && char.IsLetterOrDigit(_view.TextSnapshot[position - 1])) return;
            if (num < _view.TextSnapshot.GetText().Length && char.IsLetterOrDigit(_view.TextSnapshot[num])) return;
            var text = _selectedText;
            if (text.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
            {
                return;
            }

            //FindWordsInDocument();
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(FindWordsInDocument), DispatcherPriority.ApplicationIdle, null);
        }


        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            _layer.RemoveAllAdornments();
            ColorWords();
        }

        private void FindWordsInDocument()
        {
            lock (UpdateLock)
            {
                var findData = new FindData(_selectedWord.GetText(), _selectedWord.Snapshot)
                {
                    FindOptions = FindOptions.WholeWord
                };
                SnapShotsToColor.AddRange(_textSearchService.FindAll(findData));
                ColorWords();
            }
        }

        private void ColorWords()
        {
            var textViewLines = _view.TextViewLines;
            foreach (var current in SnapShotsToColor)
                try
                {
                    var markerGeometry = textViewLines.GetMarkerGeometry(current);
                    if (markerGeometry == null)
                    {
                        continue;
                    }
                    var geometryDrawing = new GeometryDrawing(_brush, _pen, markerGeometry);
                    geometryDrawing.Freeze();
                    var drawingImage = new DrawingImage(geometryDrawing);
                    drawingImage.Freeze();
                    var image = new Image { Source = drawingImage };
                    Canvas.SetLeft(image, markerGeometry.Bounds.Left);
                    Canvas.SetTop(image, markerGeometry.Bounds.Top);
                    _layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, current, null, image, null);
                }
                catch
                {
                    // ignored
                }
        }
    }
}