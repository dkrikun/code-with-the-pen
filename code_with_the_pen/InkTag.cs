
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace code_with_the_pen
{
    internal sealed class InkTag : TextMarkerTag
    {
#if false
        InkTag(UIElement adornment, AdornmentRemovedCallback removalCallback)
            : base(adornment, removalCallback)
        {
        }
#endif

        internal InkTag()
            : base("MarkerFormatDefinition/InkFormatDefinition")
        {
        }
    }


    [Export(typeof(EditorFormatDefinition))]
    [Name("MarkerFormatDefinition/InkFormatDefinition")]
    [UserVisible(true)]
    internal class InkFormatDefinition : MarkerFormatDefinition
    {
        public InkFormatDefinition()
        {
            this.BackgroundColor = Colors.LightBlue;
            this.ForegroundColor = Colors.DarkBlue;
            this.DisplayName = "Highlight Word";
            this.ZOrder = 5;
        }
    }

    internal class InkTagger : ITagger<InkTag>
    {
        ITextView View { get; set; }
        ITextBuffer SourceBuffer { get; set; }
        ITextSearchService TextSearchService { get; set; }
        ITextStructureNavigator TextStructureNavigator { get; set; }

        NormalizedSnapshotSpanCollection WordSpans { get; set; }
        SnapshotSpan? CurrentWord { get; set; }
        SnapshotPoint RequestedPoint { get; set; }

        readonly object updateLock = new object();

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<InkTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (CurrentWord == null || spans.Count == 0)
                yield break;

            var currentWord = CurrentWord.Value;
            var snapshot = spans.First().Snapshot;
            if(snapshot != currentWord.Snapshot)
                currentWord = currentWord.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);

            if (spans.OverlapsWith(new NormalizedSnapshotSpanCollection(currentWord)))
                yield return new TagSpan<InkTag>(currentWord, new InkTag());

        }

        public InkTagger(ITextView view, ITextBuffer sourceBuffer,
            ITextSearchService textSearchService,
            ITextStructureNavigator textStructureNavigator)
        {
            this.View = view;
            this.SourceBuffer = sourceBuffer;
            this.TextSearchService = textSearchService;
            this.TextStructureNavigator = textStructureNavigator;
            this.WordSpans = new NormalizedSnapshotSpanCollection();
            this.CurrentWord = null;

            this.View.Caret.PositionChanged += CaretPositionChanged;
            this.View.LayoutChanged += ViewLayoutChanged;
        }

        void ViewLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            if (e.NewSnapshot != e.OldSnapshot)
            {
                Debug.WriteLine("VIEW LAYOUT CHANGED");
                UpdateAtCaretPosition(View.Caret.Position);
            }
        }

        void CaretPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            Debug.WriteLine("CARET POSITION CHANGED");
            UpdateAtCaretPosition(e.NewPosition);
        }

        void UpdateAtCaretPosition(CaretPosition caretPosition)
        {
            SnapshotPoint? point = caretPosition.Point.GetPoint(SourceBuffer, caretPosition.Affinity);

            if (!point.HasValue)
                return;

            if (CurrentWord?.Snapshot == View.TextSnapshot)
            {
                if (point.Value >= CurrentWord?.Start &&
                    point.Value <= CurrentWord?.End)
                    return;
            }

            lock (updateLock)
            {
                CurrentWord = GetWordExtent(point.Value)?.Span;

#if false
                var span = new SnapshotSpan(SourceBuffer.CurrentSnapshot,
                   start: 0,
                   length: SourceBuffer.CurrentSnapshot.Length);
#endif
                var span = new SnapshotSpan(SourceBuffer.CurrentSnapshot, CurrentWord ?? new Span(0,0));
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
            }
        }
         
        TextExtent? GetWordExtent(SnapshotPoint currentRequest)
        {
            // Find all words in the buffer like the one the caret is on  
            TextExtent word = TextStructureNavigator.GetExtentOfWord(currentRequest);

            // If we've selected something not worth highlighting,
            // we might have missed a "word" by a little bit  
            if (WordExtentIsValid(currentRequest, word))
                return word;

            //Before we retry, make sure it is worthwhile   
            if (word.Span.Start != currentRequest
                    || currentRequest == currentRequest.GetContainingLine().Start
                    || char.IsWhiteSpace((currentRequest - 1).GetChar()))
            {
                return null;
            }

            // Try again, one character previous.    
            // If the caret is at the end of a word, pick up the word.  
            word = TextStructureNavigator.GetExtentOfWord(currentRequest - 1);

            //If the word still isn't valid, we're done   
            if (!WordExtentIsValid(currentRequest, word))
                return null;

            return word;
        }

        static bool WordExtentIsValid(SnapshotPoint currentRequest, TextExtent word)
        {
            return word.IsSignificant
                && currentRequest.Snapshot.GetText(word.Span).Any(c => char.IsLetter(c));
        }

        void SynchronousUpdate(SnapshotPoint currentRequest,
            NormalizedSnapshotSpanCollection newSpans, SnapshotSpan? newCurrentWord)
        {
            lock (updateLock)
            {
                if (currentRequest != RequestedPoint)
                    return;

                WordSpans = newSpans;
                CurrentWord = newCurrentWord;

                var span = new SnapshotSpan(SourceBuffer.CurrentSnapshot,
                    start: 0,
                    length: SourceBuffer.CurrentSnapshot.Length);

                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
            }
        }
    }

    [Export(typeof(IViewTaggerProvider))]
    [ContentType("text")]
    [TagType(typeof(TextMarkerTag))]
    public class InkTaggerProvider : IViewTaggerProvider
    {
        [Import]
        internal ITextSearchService TextSearchService { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelector { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            // provide highlighting only on the top buffer   
            if (textView.TextBuffer != buffer)
                return null;

            var textStructureNavigator =
                TextStructureNavigatorSelector.GetTextStructureNavigator(buffer);

            return new InkTagger(textView, buffer,
                TextSearchService, textStructureNavigator) as ITagger<T>;
        }
    };
}