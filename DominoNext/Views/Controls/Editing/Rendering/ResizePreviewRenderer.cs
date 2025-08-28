using System;
using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// ������СԤ����Ⱦ��
    /// </summary>
    public class ResizePreviewRenderer
    {
        private readonly Color _resizePreviewColor = Color.Parse("#E91E63");
        private readonly IPen _resizePreviewPen = new Pen(new SolidColorBrush(Color.Parse("#C2185B")), 2);

        /// <summary>
        /// ��Ⱦ������СԤ��Ч��
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel.ResizeState.ResizingNotes == null || viewModel.ResizeState.ResizingNotes.Count == 0) return;

            // Ϊÿ�����ڵ�����С����������Ԥ��
            foreach (var note in viewModel.ResizeState.ResizingNotes)
            {
                var noteRect = calculateNoteRect(note);
                if (noteRect.Width > 0 && noteRect.Height > 0)
                {
                    // ʹ�÷�ɫ������СԤ����ɫ������͸������ͻ����ʾ
                    var brush = new SolidColorBrush(_resizePreviewColor, 0.8);
                    context.DrawRectangle(brush, _resizePreviewPen, noteRect);

                    // ��ʾ��ǰ������Ϣ�����������Ա�鿴
                    if (noteRect.Width > 25 && noteRect.Height > 8)
                    {
                        var durationText = note.Duration.ToString();
                        DrawNoteText(context, durationText, noteRect, 10);
                    }
                }
            }
        }

        /// <summary>
        /// �������ϻ����ı���Ϣ
        /// </summary>
        private void DrawNoteText(DrawingContext context, string text, Rect noteRect, double fontSize)
        {
            var typeface = new Typeface(FontFamily.Default);
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black);

            var textPosition = new Point(
                noteRect.X + (noteRect.Width - formattedText.Width) / 2,
                noteRect.Y + (noteRect.Height - formattedText.Height) / 2);

            // �����ı���������߿ɶ���
            var textBounds = new Rect(
                textPosition.X - 2,
                textPosition.Y - 1,
                formattedText.Width + 4,
                formattedText.Height + 2);
            context.DrawRectangle(new SolidColorBrush(Colors.White, 0.8), null, textBounds);

            context.DrawText(formattedText, textPosition);
        }
    }
}