using Avalonia;
using Avalonia.Media;
using System;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// ��קԤ����Ⱦ��
    /// </summary>
    public class DragPreviewRenderer
    {
        private readonly Color _dragPreviewColor = Color.Parse("#FFC107");
        private readonly IPen _dragPreviewPen = new Pen(new SolidColorBrush(Color.Parse("#FF8F00")), 2);

        /// <summary>
        /// ��Ⱦ��קԤ��Ч��
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel?.DraggingNotes == null || viewModel.DraggingNotes.Count == 0) return;

            // Ϊÿ��������ק����������Ԥ��
            foreach (var note in viewModel.DraggingNotes)
            {
                var noteRect = calculateNoteRect(note);
                if (noteRect.Width > 0 && noteRect.Height > 0)
                {
                    // ʹ�ó�ɫ��קԤ����ɫ������͸������ͻ����ʾ
                    var brush = new SolidColorBrush(_dragPreviewColor, 0.85);
                    context.DrawRectangle(brush, _dragPreviewPen, noteRect);

                    // ��ʾ������Ϣ
                    if (noteRect.Width > 35 && noteRect.Height > 10)
                    {
                        var noteInfo = $"{GetNoteName(note.Pitch)}";
                        DrawNoteText(context, noteInfo, noteRect, 10);
                    }
                    
                    // ������קָʾ��
                    DrawDragIndicator(context, noteRect, note, viewModel);
                }
            }
        }

        /// <summary>
        /// ������קָʾ��
        /// </summary>
        private void DrawDragIndicator(DrawingContext context, Rect noteRect, NoteViewModel note, PianoRollViewModel viewModel)
        {
            if (viewModel?.DraggingNotes == null || viewModel.DraggingNotes.Count <= 1) return;

            // ���ڶ������϶�����ÿ����������ʾͬ���ƶ�ָʾ��
            const double indicatorSize = 6.0;
            var indicatorBrush = new SolidColorBrush(Colors.White, 0.9);
            var indicatorPen = new Pen(new SolidColorBrush(Colors.DarkOrange), 1.5);

            // ���������Ļ���СԲ��ָʾ��
            var centerX = noteRect.X + noteRect.Width / 2;
            var centerY = noteRect.Y + noteRect.Height / 2;
            
            var indicatorRect = new Rect(
                centerX - indicatorSize / 2,
                centerY - indicatorSize / 2,
                indicatorSize,
                indicatorSize);
            
            context.DrawEllipse(indicatorBrush, indicatorPen, indicatorRect);

            // ����Ƕ����������ʾͬ���ƶ���������
            if (viewModel.DraggingNotes.Count > 1)
            {
                var connectionPen = new Pen(new SolidColorBrush(Colors.Orange, 0.6), 1);
                
                // ���Ƶ�������ק�����������ߣ��򻯰汾��
                foreach (var otherNote in viewModel.DraggingNotes)
                {
                    if (otherNote != note)
                    {
                        var otherRect = calculateNoteRect(otherNote);
                        var otherCenterX = otherRect.X + otherRect.Width / 2;
                        var otherCenterY = otherRect.Y + otherRect.Height / 2;
                        
                        // ֻ���ƾ���Ͻ��������ߣ������Ӿ�����
                        var distance = Math.Sqrt(Math.Pow(centerX - otherCenterX, 2) + Math.Pow(centerY - otherCenterY, 2));
                        if (distance < 200) // 200�����ڵ���������ʾ������
                        {
                            context.DrawLine(connectionPen, new Point(centerX, centerY), new Point(otherCenterX, otherCenterY));
                        }
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

        /// <summary>
        /// ��ȡ��������
        /// </summary>
        private string GetNoteName(int pitch)
        {
            var noteNames = new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            var octave = pitch / 12 - 1;
            var noteIndex = pitch % 12;
            return $"{noteNames[noteIndex]}{octave}";
        }
    }
}