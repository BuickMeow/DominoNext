using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using System;
using System.Collections.Generic;

namespace DominoNext.Renderers
{
    public class NoteRenderer
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

        public void DrawNote(DrawingContext context, NoteViewModel note, double zoom, double pixelsPerTick, double keyHeight, NoteRenderType renderType = NoteRenderType.Normal)
        {
            var x = note.GetX(zoom, pixelsPerTick);
            var y = note.GetY(keyHeight);
            var width = note.GetWidth(zoom, pixelsPerTick);
            var height = note.GetHeight(keyHeight);

            var rect = new Rect(x, y, width, height);

            // 根据渲染类型获取对应的画刷和画笔
            var (brush, pen) = GetStyleForRenderType(renderType, note.Velocity);

            context.DrawRectangle(brush, pen, rect);

            // 选中音符显示力度指示器
            if (renderType == NoteRenderType.Selected && width > 20)
            {
                DrawVelocityIndicator(context, rect, note.Velocity);
            }
        }

        private (IBrush brush, IPen pen) GetStyleForRenderType(NoteRenderType renderType, int velocity)
        {
            var opacity = CalculateOpacity(velocity, renderType);

            return renderType switch
            {
                NoteRenderType.Selected => (
                    CreateBrushWithOpacity(GetResourceBrush("NoteSelectedBrush", "#FFFF9800"), opacity),
                    GetResourcePen("NoteSelectedPenBrush", "#FFF57C00", 2)
                ),
                NoteRenderType.Dragging => (
                    CreateBrushWithOpacity(GetResourceBrush("NoteDraggingBrush", "#FF2196F3"), opacity),
                    GetResourcePen("NoteDraggingPenBrush", "#FF1976D2", 2)
                ),
                NoteRenderType.Preview => (
                    CreateBrushWithOpacity(GetResourceBrush("NotePreviewBrush", "#804CAF50"), opacity * 0.6),
                    CreateDashedPen(GetResourceBrush("NotePreviewPenBrush", "#FF2E7D32"), 1)
                ),
                _ => ( // Normal
                    CreateBrushWithOpacity(GetResourceBrush("NoteBrush", "#FF4CAF50"), opacity),
                    GetResourcePen("NotePenBrush", "#FF2E7D32", 1)
                )
            };
        }

        private double CalculateOpacity(int velocity, NoteRenderType renderType)
        {
            var baseOpacity = Math.Max(0.3, velocity / 127.0);

            return renderType switch
            {
                NoteRenderType.Preview => baseOpacity * 0.6,
                NoteRenderType.Dragging => Math.Min(0.8, baseOpacity * 1.2),
                _ => baseOpacity
            };
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

        private IPen CreateDashedPen(IBrush brush, double thickness)
        {
            return new Pen(brush, thickness)
            {
                DashStyle = new DashStyle(new double[] { 3, 3 }, 0)
            };
        }

        private void DrawVelocityIndicator(DrawingContext context, Rect noteRect, int velocity)
        {
            var indicatorHeight = 3;
            var indicatorWidth = (velocity / 127.0) * noteRect.Width;
            var indicatorRect = new Rect(
                noteRect.X,
                noteRect.Bottom - indicatorHeight,
                indicatorWidth,
                indicatorHeight
            );

            var velocityBrush = GetResourceBrush("VelocityIndicatorBrush", "#FFFFC107");
            context.DrawRectangle(velocityBrush, null, indicatorRect);
        }
    }

    public enum NoteRenderType
    {
        Normal,
        Selected,
        Dragging,
        Preview
    }
}