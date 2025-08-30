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
        /// ��Ⱦ������СԤ��Ч��
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel.ResizeState.ResizingNotes == null || viewModel.ResizeState.ResizingNotes.Count == 0) return;

            // ��ȡ������СԤ����ɫ - ʹ��ѡ��������ɫ�ı�������ʾ������С״̬
            var resizeBrush = CreateBrushWithOpacity(GetResourceBrush("NoteSelectedBrush", "#FFFF9800"), 0.8);
            var resizePen = GetResourcePen("NoteSelectedPenBrush", "#FFF57C00", 2);

            // Ϊÿ�����ڵ�����С����������Ԥ��
            foreach (var note in viewModel.ResizeState.ResizingNotes)
            {
                var noteRect = calculateNoteRect(note);
                if (noteRect.Width > 0 && noteRect.Height > 0)
                {
                    // ʹ����ɫ��ʶ������СԤ������͸����ͻ����ʾ
                    context.DrawRectangle(resizeBrush, resizePen, noteRect);

                    // ��ʾ��ǰʱֵ��Ϣ�����ڱ༭�߲鿴
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
        /// �������ϻ����ı���Ϣ
        /// </summary>
        private void DrawNoteText(DrawingContext context, string text, Rect noteRect, double fontSize)
        {
            // ʹ��΢���ź�����ϵ�У����ʺ����Ľ��棩
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