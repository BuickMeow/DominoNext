using System;
using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// ����������Ⱦ��
    /// </summary>
    public class CreatingNoteRenderer
    {
        // ��Դ��ˢ��ȡ���ַ���
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
        /// ��Ⱦ���ڴ���������
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel.CreatingNote == null || !viewModel.CreationModule.IsCreatingNote) return;

            var creatingRect = calculateNoteRect(viewModel.CreatingNote);
            if (creatingRect.Width > 0 && creatingRect.Height > 0)
            {
                // ʹ��Ԥ��������ɫ����͸������΢��һЩ���������ڴ�����״̬
                var brush = CreateBrushWithOpacity(GetResourceBrush("NotePreviewBrush", "#804CAF50"), 0.85);
                var pen = GetResourcePen("NotePreviewPenBrush", "#FF689F38", 2);
                
                context.DrawRectangle(brush, pen, creatingRect);

                // ��ʾ��ǰ������Ϣ
                if (creatingRect.Width > 30 && creatingRect.Height > 10)
                {
                    var durationText = viewModel.CreatingNote.Duration.ToString();
                    DrawNoteText(context, durationText, creatingRect, 11);
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
        /// �������ϻ����ı���Ϣ
        /// </summary>
        private void DrawNoteText(DrawingContext context, string text, Rect noteRect, double fontSize)
        {
            var typeface = new Typeface(FontFamily.Default);
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

            // ���ı���ӱ�����߿ɶ���
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