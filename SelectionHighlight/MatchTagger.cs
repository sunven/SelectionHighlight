using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;

namespace SelectionHighlight
{
    public class MatchTagger : ITagger<MatchTag>
    {
        public IClassifier Aggregator { get; }

        private readonly ITextBuffer _sourceBuffer;

        private readonly ITextSearchService _textSearchService;

        private readonly ITextView _view;

        private readonly List<ITagSpan<MatchTag>> _glyphsTagged;

        private readonly List<SnapshotSpan> _glyphsToPlace;

        private string _selectedText = "";

        private VirtualSnapshotSpan _selectedWord;

        internal MatchTagger(ITextView view, ITextBuffer sourceBuffer, IClassifier aggregator,
            ITextSearchService textSearchService)
        {
            _view = view;
            var selection = view.Selection;
            Aggregator = aggregator;
            _sourceBuffer = sourceBuffer;
            _textSearchService = textSearchService;
            _glyphsToPlace = new List<SnapshotSpan>();
            _glyphsTagged = new List<ITagSpan<MatchTag>>();
            selection.SelectionChanged += OnSelectionChanged;
            _view.LayoutChanged += OnLayoutChanged;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        IEnumerable<ITagSpan<MatchTag>> ITagger<MatchTag>.GetTags(NormalizedSnapshotSpanCollection spans)
        {
            _glyphsTagged.Clear();
            foreach (var current in _glyphsToPlace) _glyphsTagged.Add(new TagSpan<MatchTag>(current, new MatchTag()));
            return _glyphsTagged;
        }

        private void OnSelectionChanged(object sender, object e)
        {
            lock (SelectionHighlight.UpdateLock)
            {
                var tagsChanged = TagsChanged;
                _selectedText = _view.Selection.StreamSelectionSpan.GetText();
                _selectedWord = _view.Selection.StreamSelectionSpan;
                _glyphsToPlace.Clear();
                tagsChanged?.Invoke(this,
    new SnapshotSpanEventArgs(new SnapshotSpan(_sourceBuffer.CurrentSnapshot, 0,
        _sourceBuffer.CurrentSnapshot.Length)));
                if (!string.IsNullOrEmpty(_selectedText) && !string.IsNullOrWhiteSpace(_selectedText))
                {
                    var length = _selectedText.Length;
                    var position = _view.Selection.StreamSelectionSpan.Start.Position.Position;
                    var num = position + length;
                    if (position - 1 >= 0 && char.IsLetterOrDigit(_view.TextSnapshot[position - 1])) return;
                    if (num < _view.TextSnapshot.GetText().Length &&
                        char.IsLetterOrDigit(_view.TextSnapshot[num])) return;
                    var text = _selectedText;
                    foreach (var c in text)
                    {
                        if (!char.IsLetterOrDigit(c) && c != '_') return;
                    }

                    FindWordsInDocument();
                    tagsChanged?.Invoke(this,
                        new SnapshotSpanEventArgs(new SnapshotSpan(_sourceBuffer.CurrentSnapshot, 0,
                            _sourceBuffer.CurrentSnapshot.Length)));
                }
            }
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            TagsChanged?.Invoke(this,
                    new SnapshotSpanEventArgs(new SnapshotSpan(_sourceBuffer.CurrentSnapshot, 0,
                        _sourceBuffer.CurrentSnapshot.Length)));
        }

        private void FindWordsInDocument()
        {
            lock (SelectionHighlight.UpdateLock)
            {
                Task.Factory.StartNew(() =>
                {
                    var findData =
                                        new FindData(_selectedWord.GetText(), _selectedWord.Snapshot) { FindOptions = FindOptions.WholeWord };
                    _glyphsToPlace.AddRange(_textSearchService.FindAll(findData));
                });
            }
        }
    }
}