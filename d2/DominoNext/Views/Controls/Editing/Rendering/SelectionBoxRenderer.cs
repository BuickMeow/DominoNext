using System;
using Avalonia;
using Avalonia.Media;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// Ñ¡Ôñ¿òäÖÈ¾Æ÷
    /// </summary>
    public class SelectionBoxRenderer
    {
        private readonly IPen _selectionBoxPen = new Pen(new SolidColorBrush(Color.Parse("#2196F3")), 2);
        private readonly IBrush _selectionBoxBrush = new SolidColorBrush(Color.Parse("#2196F3"), 0.3);

        /// <summary>
        /// äÖÈ¾Ñ¡Ôñ¿ò
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel)
        {
            if (viewModel?.SelectionStart == null || viewModel?.SelectionEnd == null) return;

            var start = viewModel.SelectionStart.Value;
            var end = viewModel.SelectionEnd.Value;

            var x = Math.Min(start.X, end.X);
            var y = Math.Min(start.Y, end.Y);
            var width = Math.Abs(end.X - start.X);
            var height = Math.Abs(end.Y - start.Y);

            var selectionRect = new Rect(x, y, width, height);
            context.DrawRectangle(_selectionBoxBrush, _selectionBoxPen, selectionRect);
        }
    }
}