using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using DominoNext.Services.Implementation;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// 音符渲染器
    /// </summary>
    public class NoteRenderer
    {
        private readonly ThemeService _themeService;

        public NoteRenderer()
        {
            _themeService = ThemeService.Instance;
        }

        /// <summary>
        /// 渲染可见音符
        /// </summary>
        public void RenderNotes(DrawingContext context, PianoRollViewModel viewModel, Dictionary<NoteViewModel, Rect> visibleNoteCache)
        {
            int drawnNotes = 0;
            foreach (var kvp in visibleNoteCache)
            {
                var note = kvp.Key;
                var rect = kvp.Value;

                if (rect.Width > 0 && rect.Height > 0)
                {
                    // 检查音符是否正在被拖拽或调整大小，使用较低的透明度渲染原始位置
                    bool isBeingDragged = (viewModel.DragState.IsDragging && viewModel.DragState.DraggingNotes.Contains(note));
                    bool isBeingResized = (viewModel.ResizeState.IsResizing && viewModel.ResizeState.ResizingNotes.Contains(note));
                    bool isBeingManipulated = isBeingDragged || isBeingResized;
                    
                    DrawNote(context, note, rect, isBeingManipulated);
                    drawnNotes++;
                }
            }

            System.Diagnostics.Debug.WriteLine($"绘制了 {drawnNotes} 个可见音符");
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
                context.DrawRectangle(_themeService.PreviewNoteBrush, _themeService.PreviewNotePen, previewRect);

                var durationText = viewModel.PreviewNote.Duration.ToString();
                if (previewRect.Width > 30 && previewRect.Height > 10)
                {
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
            
            // 操作中物体使用更高的透明度，让它们更加可见
            if (isBeingManipulated)
            {
                opacity = Math.Min(1.0, opacity * 1.1); // 提高操作中透明度，增强交互性
            }

            IBrush brush;
            IPen pen;

            if (note.IsSelected)
            {
                // 选中音符使用高亮的橙色
                brush = CreateBrushWithOpacity(_themeService.SelectedNoteBrush, opacity);
                pen = _themeService.SelectedNotePen;
            }
            else
            {
                brush = CreateBrushWithOpacity(_themeService.NormalNoteBrush, opacity);
                pen = _themeService.NormalNotePen;
            }

            // 为拖拽中的音符添加微妙的阴影效果，增强视觉反馈
            if (isBeingManipulated)
            {
                var shadowOffset = new Vector(1, 1);
                var shadowRect = rect.Translate(shadowOffset);
                var shadowBrush = new SolidColorBrush(Colors.Black, 0.2);
                context.DrawRectangle(shadowBrush, null, shadowRect);
            }

            context.DrawRectangle(brush, pen, rect);
        }

        private IBrush CreateBrushWithOpacity(IBrush originalBrush, double opacity)
        {
            if (originalBrush is SolidColorBrush solidBrush)
            {
                var color = solidBrush.Color;
                return new SolidColorBrush(color, opacity);
            }
            return originalBrush;
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