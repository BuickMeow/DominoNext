using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using System;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// ѡ�����Ⱦ��
    /// </summary>
    public class SelectionBoxRenderer
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
        /// ��Ⱦѡ���
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel)
        {
            // ����Ƿ����ڽ���ѡ���Լ���ʼ�ͽ������Ƿ񶼴���
            if (!viewModel.SelectionState.IsSelecting || 
                viewModel.SelectionStart == null || 
                viewModel.SelectionEnd == null) 
                return;

            var start = viewModel.SelectionStart.Value;
            var end = viewModel.SelectionEnd.Value;

            var x = Math.Min(start.X, end.X);
            var y = Math.Min(start.Y, end.Y);
            var width = Math.Abs(end.X - start.X);
            var height = Math.Abs(end.Y - start.Y);

            // ֻ�е�ѡ�����һ����Сʱ����Ⱦ�����ⵥ��ʱ��ʾ��С�Ŀ�
            if (width > 2 || height > 2)
            {
                var selectionRect = new Rect(x, y, width, height);
                
                // ʹ����Դ�е�ѡ�����ɫ
                var selectionBrush = GetResourceBrush("SelectionBrush", "#800099FF");
                var selectionPen = GetResourcePen("SelectionBrush", "#FF0099FF", 2);
                
                context.DrawRectangle(selectionBrush, selectionPen, selectionRect);
                
                // ��ӵ������
                System.Diagnostics.Debug.WriteLine($"��Ⱦѡ���: {selectionRect}, IsSelecting: {viewModel.SelectionState.IsSelecting}");
            }
        }
    }
}