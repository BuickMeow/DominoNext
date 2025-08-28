using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// 音符渲染器
    /// </summary>
    public class NoteRenderer
    {
        private readonly Color _noteColor = Color.Parse("#4CAF50");
        private readonly IPen _noteBorderPen = new Pen(new SolidColorBrush(Color.Parse("#2E7D32")), 2);
        private readonly Color _selectedNoteColor = Color.Parse("#FF9800");
        private readonly IPen _selectedNoteBorderPen = new Pen(new SolidColorBrush(Color.Parse("#F57C00")), 2);
        private readonly Color _previewNoteColor = Color.Parse("#81C784");
        private readonly IPen _previewNoteBorderPen = new Pen(new SolidColorBrush(Color.Parse("#66BB6A")), 2);

        /// <summary>
        /// 渲染所有音符
        /// </summary>
        public void RenderNotes(DrawingContext context, PianoRollViewModel viewModel, Dictionary<NoteViewModel, Rect> visibleNoteCache)
        {
            foreach (var kvp in visibleNoteCache)
            {
                var note = kvp.Key;
                var rect = kvp.Value;

                if (rect.Width > 0 && rect.Height > 0)
                {
                    // 检查音符是否正在被操作
                    bool isBeingDragged = viewModel.IsDragging && viewModel.DraggingNotes?.Contains(note) == true;
                    bool isBeingResized = viewModel.IsResizing && viewModel.ResizingNotes?.Contains(note) == true;
                    bool isBeingManipulated = isBeingDragged || isBeingResized;
                    
                    DrawNote(context, note, rect, isBeingManipulated);
                }
            }
        }

        /// <summary>
        /// 渲染预览音符
        /// </summary>
        public void RenderPreviewNote(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel.PreviewNote == null) return;

            var previewRect = calculateNoteRect(viewModel.PreviewNote);
            if (previewRect.Width > 0 && previewRect.Height > 0)
            {
                var brush = new SolidColorBrush(_previewNoteColor, 0.6);
                context.DrawRectangle(brush, _previewNoteBorderPen, previewRect);

                // 显示时值信息
                if (previewRect.Width > 30 && previewRect.Height > 10)
                {
                    var durationText = viewModel.PreviewNote.Duration.ToString();
                    DrawNoteText(context, durationText, previewRect, 9);
                }
            }
        }

        /// <summary>
        /// 绘制单个音符
        /// </summary>
        private void DrawNote(DrawingContext context, NoteViewModel note, Rect rect, bool isBeingManipulated = false)
        {
            var opacity = Math.Max(0.7, note.Velocity / 127.0);
            
            if (isBeingManipulated)
            {
                opacity *= 0.4; // 使原始位置的音符更淡
            }

            IBrush brush;
            IPen pen;

            if (note.IsSelected)
            {
                brush = new SolidColorBrush(_selectedNoteColor, opacity);
                pen = _selectedNoteBorderPen;
            }
            else
            {
                brush = new SolidColorBrush(_noteColor, opacity);
                pen = _noteBorderPen;
            }

            context.DrawRectangle(brush, pen, rect);
        }

        /// <summary>
        /// 在音符上绘制文本
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

            // 绘制文本背景
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