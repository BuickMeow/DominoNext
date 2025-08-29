using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using DominoNext.Services.Implementation;
using System;
using System.Collections.Generic;

namespace DominoNext.Renderers
{
    public class NoteRenderer
    {
        private readonly ThemeService _themeService;

        public NoteRenderer()
        {
            _themeService = ThemeService.Instance;
            
            // 监听主题变更
            _themeService.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            // 主题变更时可以执行一些清理或重新初始化操作
        }

        public void DrawNote(DrawingContext context, NoteViewModel note, double zoom, double pixelsPerTick, double keyHeight, NoteRenderType renderType = NoteRenderType.Normal)
        {
            var x = note.GetX(zoom, pixelsPerTick);
            var y = note.GetY(keyHeight);
            var width = note.GetWidth(zoom, pixelsPerTick);
            var height = note.GetHeight(keyHeight);

            var rect = new Rect(x, y, width, height);

            var (brush, pen) = GetNoteStyle(renderType, note.Velocity);

            context.DrawRectangle(brush, pen, rect);

            // 选中音符显示力度指示器
            if (renderType == NoteRenderType.Selected && width > 20)
            {
                DrawVelocityIndicator(context, rect, note.Velocity);
            }
        }

        private (IBrush brush, IPen pen) GetNoteStyle(NoteRenderType renderType, int velocity)
        {
            var opacity = CalculateOpacity(velocity, renderType);

            return renderType switch
            {
                NoteRenderType.Selected => (
                    CreateBrushWithOpacity(_themeService.SelectedNoteBrush, opacity),
                    _themeService.SelectedNotePen
                ),
                NoteRenderType.Dragging => (
                    CreateBrushWithOpacity(_themeService.DraggingNoteBrush, opacity),
                    _themeService.DraggingNotePen
                ),
                NoteRenderType.Preview => (
                    CreateBrushWithOpacity(_themeService.PreviewNoteBrush, opacity * 0.6),
                    _themeService.PreviewNotePen
                ),
                _ => (
                    CreateBrushWithOpacity(_themeService.NormalNoteBrush, opacity),
                    _themeService.NormalNotePen
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

            context.DrawRectangle(_themeService.VelocityIndicatorBrush, null, indicatorRect);
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