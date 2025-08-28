using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using System;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// 选择框渲染器
    /// </summary>
    public class SelectionBoxRenderer
    {
        private readonly IPen _selectionBoxPen = new Pen(new SolidColorBrush(Color.Parse("#2196F3")), 2);
        private readonly IBrush _selectionBoxBrush = new SolidColorBrush(Color.Parse("#2196F3"), 0.2);

        /// <summary>
        /// 渲染选择框
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel)
        {
            // 检查是否正在进行选择，以及起始和结束点是否都存在
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

            // 只有当选择框有一定大小时才渲染（避免单点击时出现很小的框）
            if (width > 2 || height > 2)
            {
                var selectionRect = new Rect(x, y, width, height);
                context.DrawRectangle(_selectionBoxBrush, _selectionBoxPen, selectionRect);
                
                // 添加调试输出
                System.Diagnostics.Debug.WriteLine($"渲染选择框: {selectionRect}, IsSelecting: {viewModel.SelectionState.IsSelecting}");
            }
        }
    }
}