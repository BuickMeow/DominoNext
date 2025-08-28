using Avalonia;
using Avalonia.Media;
using System;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// 拖拽预览渲染器
    /// </summary>
    public class DragPreviewRenderer
    {
        private readonly Color _dragPreviewColor = Color.Parse("#FFC107");
        private readonly IPen _dragPreviewPen = new Pen(new SolidColorBrush(Color.Parse("#FF8F00")), 2);

        /// <summary>
        /// 渲染拖拽预览效果
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel?.DraggingNotes == null || viewModel.DraggingNotes.Count == 0) return;

            // 为每个正在拖拽的音符绘制预览
            foreach (var note in viewModel.DraggingNotes)
            {
                var noteRect = calculateNoteRect(note);
                if (noteRect.Width > 0 && noteRect.Height > 0)
                {
                    // 使用橙色拖拽预览颜色，增加透明度以突出显示
                    var brush = new SolidColorBrush(_dragPreviewColor, 0.85);
                    context.DrawRectangle(brush, _dragPreviewPen, noteRect);

                    // 显示音符信息
                    if (noteRect.Width > 35 && noteRect.Height > 10)
                    {
                        var noteInfo = $"{GetNoteName(note.Pitch)}";
                        DrawNoteText(context, noteInfo, noteRect, 10);
                    }
                    
                    // 绘制拖拽指示器
                    DrawDragIndicator(context, noteRect, note, viewModel);
                }
            }
        }

        /// <summary>
        /// 绘制拖拽指示器
        /// </summary>
        private void DrawDragIndicator(DrawingContext context, Rect noteRect, NoteViewModel note, PianoRollViewModel viewModel)
        {
            if (viewModel?.DraggingNotes == null || viewModel.DraggingNotes.Count <= 1) return;

            // 对于多音符拖动，在每个音符上显示同步移动指示器
            const double indicatorSize = 6.0;
            var indicatorBrush = new SolidColorBrush(Colors.White, 0.9);
            var indicatorPen = new Pen(new SolidColorBrush(Colors.DarkOrange), 1.5);

            // 在音符中心绘制小圆点指示器
            var centerX = noteRect.X + noteRect.Width / 2;
            var centerY = noteRect.Y + noteRect.Height / 2;
            
            var indicatorRect = new Rect(
                centerX - indicatorSize / 2,
                centerY - indicatorSize / 2,
                indicatorSize,
                indicatorSize);
            
            context.DrawEllipse(indicatorBrush, indicatorPen, indicatorRect);

            // 如果是多个音符，显示同步移动的连接线
            if (viewModel.DraggingNotes.Count > 1)
            {
                var connectionPen = new Pen(new SolidColorBrush(Colors.Orange, 0.6), 1);
                
                // 绘制到其他拖拽音符的连接线（简化版本）
                foreach (var otherNote in viewModel.DraggingNotes)
                {
                    if (otherNote != note)
                    {
                        var otherRect = calculateNoteRect(otherNote);
                        var otherCenterX = otherRect.X + otherRect.Width / 2;
                        var otherCenterY = otherRect.Y + otherRect.Height / 2;
                        
                        // 只绘制距离较近的连接线，避免视觉混乱
                        var distance = Math.Sqrt(Math.Pow(centerX - otherCenterX, 2) + Math.Pow(centerY - otherCenterY, 2));
                        if (distance < 200) // 200像素内的音符才显示连接线
                        {
                            context.DrawLine(connectionPen, new Point(centerX, centerY), new Point(otherCenterX, otherCenterY));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 在音符上绘制文本信息
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

            // 绘制文本背景以提高可读性
            var textBounds = new Rect(
                textPosition.X - 2,
                textPosition.Y - 1,
                formattedText.Width + 4,
                formattedText.Height + 2);
            context.DrawRectangle(new SolidColorBrush(Colors.White, 0.8), null, textBounds);

            context.DrawText(formattedText, textPosition);
        }

        /// <summary>
        /// 获取音符名称
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