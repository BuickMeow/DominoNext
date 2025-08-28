using System;
using Avalonia;
using Avalonia.Media;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// ���ڴ���������Ⱦ��
    /// </summary>
    public class CreatingNoteRenderer
    {
        private readonly Color _creatingNoteColor = Color.Parse("#8BC34A");
        private readonly IPen _creatingNoteBorderPen = new Pen(new SolidColorBrush(Color.Parse("#689F38")), 2);

        /// <summary>
        /// ��Ⱦ���ڴ���������
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel?.CreatingNote == null || !viewModel.IsCreatingNote) return;

            var creatingRect = calculateNoteRect(viewModel.CreatingNote);
            if (creatingRect.Width > 0 && creatingRect.Height > 0)
            {
                // ʹ��ר�ŵĴ���������ʽ
                var brush = new SolidColorBrush(_creatingNoteColor, 0.85);
                context.DrawRectangle(brush, _creatingNoteBorderPen, creatingRect);

                // ��ʾ��ǰ������Ϣ
                if (creatingRect.Width > 30 && creatingRect.Height > 10)
                {
                    var durationText = viewModel.CreatingNote.Duration.ToString();
                    DrawNoteText(context, durationText, creatingRect, 11);
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