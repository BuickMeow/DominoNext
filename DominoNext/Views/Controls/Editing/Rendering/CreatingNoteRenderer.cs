using System;
using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using DominoNext.Services.Implementation;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// 创建音符渲染器
    /// </summary>
    public class CreatingNoteRenderer
    {
        private readonly ThemeService _themeService;

        public CreatingNoteRenderer()
        {
            _themeService = ThemeService.Instance;
        }

        /// <summary>
        /// 渲染正在创建的音符
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel.CreatingNote == null || !viewModel.CreationModule.IsCreatingNote) return;

            var creatingRect = calculateNoteRect(viewModel.CreatingNote);
            if (creatingRect.Width > 0 && creatingRect.Height > 0)
            {
                // 使用专用的创建音符样式
                context.DrawRectangle(_themeService.CreatingNoteBrush, _themeService.CreatingNotePen, creatingRect);

                // 显示当前时长信息
                if (creatingRect.Width > 30 && creatingRect.Height > 10)
                {
                    var durationText = viewModel.CreatingNote.Duration.ToString();
                    DrawNoteText(context, durationText, creatingRect, 11);
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

            // 绘制文本背景，提高可读性
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