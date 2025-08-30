using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using System;
using System.Collections.Generic;

namespace DominoNext.Renderers
{
    public class NoteRenderer
    {
        // 预创建样式缓存
        private readonly Dictionary<NoteRenderType, (IBrush brush, IPen pen)> _styleCache = new();

        public NoteRenderer()
        {
            InitializeStyles();
        }

        private void InitializeStyles()
        {
            // 普通音符
            _styleCache[NoteRenderType.Normal] = (
                new SolidColorBrush(Color.Parse("#4CAF50")),
                new Pen(new SolidColorBrush(Color.Parse("#2E7D32")), 1)
            );

            // 选中音符
            _styleCache[NoteRenderType.Selected] = (
                new SolidColorBrush(Color.Parse("#FF9800")),
                new Pen(new SolidColorBrush(Color.Parse("#F57C00")), 2)
            );

            // 拖拽中音符
            _styleCache[NoteRenderType.Dragging] = (
                new SolidColorBrush(Color.Parse("#2196F3")),
                new Pen(new SolidColorBrush(Color.Parse("#1976D2")), 2)
            );

            // 预览音符
            _styleCache[NoteRenderType.Preview] = (
                new SolidColorBrush(Color.Parse("#4CAF50"), 0.5),
                new Pen(new SolidColorBrush(Color.Parse("#2E7D32")), 1)
                {
                    DashStyle = new DashStyle(new double[] { 3, 3 }, 0)
                }
            );
        }

        public void DrawNote(DrawingContext context, NoteViewModel note, double zoom, double pixelsPerTick, double keyHeight, int ticksPerBeat, NoteRenderType renderType = NoteRenderType.Normal)
        {
            var x = note.GetX(pixelsPerTick, ticksPerBeat);
            var y = note.GetY(keyHeight);
            var width = note.GetWidth(pixelsPerTick, ticksPerBeat);
            var height = note.GetHeight(keyHeight);

            var rect = new Rect(x, y, width, height);

            if (_styleCache.TryGetValue(renderType, out var style))
            {
                var opacity = CalculateOpacity(note.Velocity, renderType);
                var brush = CreateBrushWithOpacity(style.brush, opacity);

                context.DrawRectangle(brush, style.pen, rect);

                // 选中音符显示力度指示器
                if (renderType == NoteRenderType.Selected && width > 20)
                {
                    DrawVelocityIndicator(context, rect, note.Velocity);
                }
            }
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

            var velocityBrush = new SolidColorBrush(Color.Parse("#FFC107"));
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