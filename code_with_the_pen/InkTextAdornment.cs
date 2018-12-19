using System;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace code_with_the_pen
{
    internal sealed class InkTextAdornment
    {
        private readonly IAdornmentLayer layer;
        private readonly IWpfTextView view;

        private readonly Brush brush;
        private readonly Pen pen;

        public InkTextAdornment(IWpfTextView view)
        {
            this.layer = view.GetAdornmentLayer("InkTextAdornment");
            this.view = view;
            this.view.LayoutChanged += this.OnLayoutChanged;

            // Create the pen and brush to color the box behind the a's
            this.brush = new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x00, 0xff));
            this.brush.Freeze();

            var penBrush = new SolidColorBrush(Colors.Red);
            penBrush.Freeze();

            this.pen = new Pen(penBrush, 0.5);
            this.pen.Freeze();
        }

        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach (ITextViewLine line in e.NewOrReformattedLines)
            {
                this.CreateVisuals(line);
            }
        }

        private void CreateVisuals(ITextViewLine line)
        {
            IWpfTextViewLineCollection textViewLines = this.view.TextViewLines;

            // loop through each character, and place a box around any 'a'
            for (int charIndex = line.Start; charIndex < line.End; charIndex++)
            {
                if (this.view.TextSnapshot[charIndex] == 'a')
                {
                    SnapshotSpan span = new SnapshotSpan(this.view.TextSnapshot,
                        Span.FromBounds(charIndex, charIndex + 1));
                    Geometry geometry = textViewLines.GetMarkerGeometry(span);
                    if (geometry != null)
                    {
                        var drawing = new GeometryDrawing(this.brush, this.pen, geometry);
                        drawing.Freeze();

                        var drawingImage = new DrawingImage(drawing);
                        drawingImage.Freeze();

                        var image = new Image
                        {
                            Source = drawingImage,
                        };

                        // Align the image with the top of the bounds of the text geometry
                        Canvas.SetLeft(image, geometry.Bounds.Left);
                        Canvas.SetTop(image, geometry.Bounds.Top);

                        this.layer.AddAdornment(AdornmentPositioningBehavior.TextRelative,
                            span, null, image, null);
                    }
                }
            }
        }
    }
}
