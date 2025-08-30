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

            // 只有当选择框有一定大小时才渲染，避免单点时显示很小的框
            if (width > 2 || height > 2)
            {
                var selectionRect = new Rect(x, y, width, height);
                
                // 使用资源中的选择框颜色
                var selectionBrush = GetResourceBrush("SelectionBrush", "#800099FF");
                var selectionPen = GetResourcePen("SelectionBrush", "#FF0099FF", 2);
                
                context.DrawRectangle(selectionBrush, selectionPen, selectionRect);
                
                // 添加调试输出
                System.Diagnostics.Debug.WriteLine($"渲染选择框: {selectionRect}, IsSelecting: {viewModel.SelectionState.IsSelecting}");
            }
        }
    }
}