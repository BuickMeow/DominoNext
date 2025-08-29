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
        // 资源画刷获取助手方法
        private IBrush GetResourceBrush(string key, string fallbackHex)
        {
            try
            {
                if (Application.Current?.Resources.TryGetResource(key, null, out var obj) == true && obj is IBrush brush)
                    return brush;
            }
            catch { }

            try
            {
                return new SolidColorBrush(Color.Parse(fallbackHex));
            }
            catch
            {
                return Brushes.Transparent;
            }
        }

        private IPen GetResourcePen(string brushKey, string fallbackHex, double thickness = 1)
        {
            var brush = GetResourceBrush(brushKey, fallbackHex);
            return new Pen(brush, thickness);
        }

        /// <summary>
        /// 渲染调整大小预览效果
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel.ResizeState.ResizingNotes == null || viewModel.ResizeState.ResizingNotes.Count == 0) return;

            // 获取调整大小预览颜色 - 使用选中音符颜色的变体来表示调整大小状态
            var resizeBrush = CreateBrushWithOpacity(GetResourceBrush("NoteSelectedBrush", "#FFFF9800"), 0.8);
            var resizePen = GetResourcePen("NoteSelectedPenBrush", "#FFF57C00", 2);

            // 为每个正在调整大小的音符绘制预览
            foreach (var note in viewModel.ResizeState.ResizingNotes)
            {
                var noteRect = calculateNoteRect(note);
                if (noteRect.Width > 0 && noteRect.Height > 0)
                {
                    // 使用亮色标识调整大小预览，高透明度突出显示
                    context.DrawRectangle(resizeBrush, resizePen, noteRect);

                    // 显示当前时值信息，便于编辑者查看
                    if (noteRect.Width > 25 && noteRect.Height > 8)
                    {
                        var durationText = note.Duration.ToString();
                        DrawNoteText(context, durationText, noteRect, 10);
                    }
                }
            }
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
        /// 在音符上绘制文本信息
        /// </summary>
        private void DrawNoteText(DrawingContext context, string text, Rect noteRect, double fontSize)
        {
            // 使用微软雅黑字体系列（更适合中文界面）
            var typeface = new Typeface(new FontFamily("Microsoft YaHei"));
            var textBrush = GetResourceBrush("MeasureTextBrush", "#FF000000");
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                textBrush);

            var textPosition = new Point(
                noteRect.X + (noteRect.Width - formattedText.Width) / 2,
                noteRect.Y + (noteRect.Height - formattedText.Height) / 2);

            // 给文本添加背景提高可读性
            var textBounds = new Rect(
                textPosition.X - 2,
                textPosition.Y - 1,
                formattedText.Width + 4,
                formattedText.Height + 2);
            
            var textBackgroundBrush = CreateBrushWithOpacity(GetResourceBrush("AppBackgroundBrush", "#FFFFFFFF"), 0.8);
            context.DrawRectangle(textBackgroundBrush, null, textBounds);

            context.DrawText(formattedText, textPosition);
        }
    }
}