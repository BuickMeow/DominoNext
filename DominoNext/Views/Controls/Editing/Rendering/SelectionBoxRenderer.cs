using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using DominoNext.Services.Implementation;
using System;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// ѡ�����Ⱦ��
    /// </summary>
    public class SelectionBoxRenderer
    {
        private readonly ThemeService _themeService;

        public SelectionBoxRenderer()
        {
            _themeService = ThemeService.Instance;
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

            // ֻ�е�ѡ�����һ����Сʱ����Ⱦ�����ⵥ��ʱ���ֺ�С�Ŀ�
            if (width > 2 || height > 2)
            {
                var selectionRect = new Rect(x, y, width, height);
                
                // ʹ��ѡ����������ɫ��Ϊѡ�����ɫ����͸���ȸ���
                var selectionBrush = CreateBrushWithOpacity(_themeService.SelectedNoteBrush, 0.2);
                context.DrawRectangle(selectionBrush, _themeService.SelectedNotePen, selectionRect);
                
                // �������
                System.Diagnostics.Debug.WriteLine($"��Ⱦѡ���: {selectionRect}, IsSelecting: {viewModel.SelectionState.IsSelecting}");
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
    }
}