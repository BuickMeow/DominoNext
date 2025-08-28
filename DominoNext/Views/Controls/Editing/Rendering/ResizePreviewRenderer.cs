using System;
using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// 调整大小预览渲染器
    /// </summary>
    public class ResizePreviewRenderer
    {
        private readonly Color _resizePreviewColor = Color.Parse("#E91E63");
        private readonly IPen _resizePreviewPen = new Pen(new SolidColorBrush(Color.Parse("#C2185B")), 2);

        /// <summary>
        /// 渲染调整大小预览效果
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel.ResizeState.ResizingNotes == null || viewModel.ResizeState.ResizingNotes.Count == 0) return;

            // 为每个正在调整大小的音符绘制预览
            foreach (var note in viewModel.ResizeState.ResizingNotes)
            {
                var noteRect = calculateNoteRect(note);
                if (noteRect.Width > 0 && noteRect.Height > 0)
                {
                    // 使用粉色调整大小预览颜色，增加透明度以突出显示
                    var brush = new SolidColorBrush(_resizePreviewColor, 0.8);
                    context.DrawRectangle(brush, _resizePreviewPen, noteRect);

                    // 显示当前长度信息，增大字体以便查看
                    if (noteRect.Width > 25 && noteRect.Height > 8)
                    {
                        var durationText = note.Duration.ToString();
                        DrawNoteText(context, durationText, noteRect, 10);
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
    }
}