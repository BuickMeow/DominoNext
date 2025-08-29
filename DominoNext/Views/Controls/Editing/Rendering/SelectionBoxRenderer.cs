using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using DominoNext.Services.Implementation;
using System;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// 选择框渲染器
    /// </summary>
    public class SelectionBoxRenderer
    {
        private readonly ThemeService _themeService;

        public SelectionBoxRenderer()
        {
            _themeService = ThemeService.Instance;
        }

        /// <summary>
        /// 渲染选择框
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel)
        {
            // 检查是否正在进行选择以及开始和结束点是否都存在
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

            // 只有当选择框有一定大小时才渲染，避免单击时出现很小的框
            if (width > 2 || height > 2)
            {
                var selectionRect = new Rect(x, y, width, height);
                
                // 使用选中音符的颜色作为选择框颜色，但透明度更低
                var selectionBrush = CreateBrushWithOpacity(_themeService.SelectedNoteBrush, 0.2);
                context.DrawRectangle(selectionBrush, _themeService.SelectedNotePen, selectionRect);
                
                // 调试输出
                System.Diagnostics.Debug.WriteLine($"渲染选择框: {selectionRect}, IsSelecting: {viewModel.SelectionState.IsSelecting}");
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