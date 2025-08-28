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
        private readonly IPen _selectionBoxPen = new Pen(new SolidColorBrush(Color.Parse("#2196F3")), 2);
        private readonly IBrush _selectionBoxBrush = new SolidColorBrush(Color.Parse("#2196F3"), 0.2);

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

            // ֻ�е�ѡ�����һ����Сʱ����Ⱦ�����ⵥ���ʱ���ֺ�С�Ŀ�
            if (width > 2 || height > 2)
            {
                var selectionRect = new Rect(x, y, width, height);
                context.DrawRectangle(_selectionBoxBrush, _selectionBoxPen, selectionRect);
                
                // ��ӵ������
                System.Diagnostics.Debug.WriteLine($"��Ⱦѡ���: {selectionRect}, IsSelecting: {viewModel.SelectionState.IsSelecting}");
            }
        }
    }
}