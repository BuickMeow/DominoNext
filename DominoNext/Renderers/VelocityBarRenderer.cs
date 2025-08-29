using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using DominoNext.ViewModels.Editor.Modules;
using System;
using System.Linq;

namespace DominoNext.Renderers
{
    /// <summary>
    /// ��������Ⱦ�� - �����������������
    /// </summary>
    public class VelocityBarRenderer
    {
        private const double BAR_MARGIN = 1.0;
        private const double MIN_BAR_WIDTH = 2.0;

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

        public void DrawVelocityBar(DrawingContext context, NoteViewModel note, Rect canvasBounds,
            double zoom, double pixelsPerTick, VelocityRenderType renderType = VelocityRenderType.Normal)
        {
            // ����������ʱ�����ϵ�λ�úͿ��
            var noteX = note.GetX(zoom, pixelsPerTick);
            var noteWidth = note.GetWidth(zoom, pixelsPerTick);
            
            // ȷ���������ڻ�����Χ��
            if (noteX + noteWidth < 0 || noteX > canvasBounds.Width) return;
            
            // �����������ĳߴ�
            var barWidth = Math.Max(MIN_BAR_WIDTH, noteWidth - BAR_MARGIN * 2);
            var barHeight = CalculateBarHeight(note.Velocity, canvasBounds.Height);
            
            var barX = noteX + BAR_MARGIN;
            var barY = canvasBounds.Height - barHeight;
            
            var barRect = new Rect(barX, barY, barWidth, barHeight);

            // ������Ⱦ���ͻ�ȡ��ʽ
            var (brush, pen) = GetStyleForRenderType(renderType, note.Velocity);

            // ����������
            context.DrawRectangle(brush, pen, barRect);

            // ����������㹻����������ֵ
            if (barWidth > 30 && renderType == VelocityRenderType.Selected)
            {
                DrawVelocityValue(context, barRect, note.Velocity);
            }
        }

        public void DrawEditingPreview(DrawingContext context, Rect canvasBounds, 
            VelocityEditingModule editingModule, double zoom, double pixelsPerTick)
        {
            if (editingModule.EditingPath?.Any() != true) return;

            var previewBrush = GetResourceBrush("VelocityEditingPreviewBrush", "#80FF5722");
            var previewPen = new Pen(previewBrush, 2, new DashStyle(new double[] { 3, 3 }, 0));

            // ���Ʊ༭·��
            var points = editingModule.EditingPath.Select(p => new Point(p.X, p.Y)).ToArray();
            if (points.Length > 1)
            {
                for (int i = 0; i < points.Length - 1; i++)
                {
                    context.DrawLine(previewPen, points[i], points[i + 1]);
                }
            }

            // ���Ƶ�ǰ�༭λ�õ�������Ԥ��
            if (editingModule.CurrentEditPosition.HasValue)
            {
                var pos = editingModule.CurrentEditPosition.Value;
                var velocity = CalculateVelocityFromY(pos.Y, canvasBounds.Height);
                
                var previewHeight = CalculateBarHeight(velocity, canvasBounds.Height);
                var previewRect = new Rect(pos.X - 5, canvasBounds.Height - previewHeight, 10, previewHeight);
                
                context.DrawRectangle(previewBrush, previewPen, previewRect);
                
                // ��ʾ��ǰ����ֵ
                DrawVelocityValue(context, previewRect, velocity, true);
            }
        }

        private (IBrush brush, IPen pen) GetStyleForRenderType(VelocityRenderType renderType, int velocity)
        {
            var opacity = CalculateOpacity(velocity);

            return renderType switch
            {
                VelocityRenderType.Selected => (
                    CreateBrushWithOpacity(GetResourceBrush("VelocitySelectedBrush", "#FFFF9800"), opacity),
                    GetResourcePen("VelocitySelectedPenBrush", "#FFF57C00", 1)
                ),
                VelocityRenderType.Editing => (
                    CreateBrushWithOpacity(GetResourceBrush("VelocityEditingBrush", "#FFFF5722"), opacity),
                    GetResourcePen("VelocityEditingPenBrush", "#FFD84315", 2)
                ),
                VelocityRenderType.Dragging => (
                    CreateBrushWithOpacity(GetResourceBrush("VelocityDraggingBrush", "#FF2196F3"), opacity),
                    GetResourcePen("VelocityDraggingPenBrush", "#FF1976D2", 1)
                ),
                _ => ( // Normal
                    CreateBrushWithOpacity(GetResourceBrush("VelocityBrush", "#FF4CAF50"), opacity),
                    GetResourcePen("VelocityPenBrush", "#FF2E7D32", 1)
                )
            };
        }

        private double CalculateOpacity(int velocity)
        {
            // ��������ֵ����͸���ȣ�ȷ���ɼ���
            return Math.Max(0.4, velocity / 127.0);
        }

        private double CalculateBarHeight(int velocity, double maxHeight)
        {
            // ��MIDI����ֵ(0-127)ӳ�䵽���θ߶�
            var normalizedVelocity = Math.Max(0, Math.Min(127, velocity)) / 127.0;
            return normalizedVelocity * maxHeight;
        }

        public static int CalculateVelocityFromY(double y, double maxHeight)
        {
            // ��Y���귴������ֵ - �����˷�����������ʹ��
            var normalizedY = Math.Max(0, Math.Min(1, (maxHeight - y) / maxHeight));
            var velocity = Math.Max(1, Math.Min(127, (int)Math.Round(normalizedY * 127)));
            
            System.Diagnostics.Debug.WriteLine($"CalculateVelocityFromY: y={y}, maxHeight={maxHeight}, normalizedY={normalizedY}, velocity={velocity}");
            
            return velocity;
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

        private void DrawVelocityValue(DrawingContext context, Rect barRect, int velocity, bool isPreview = false)
        {
            var textBrush = GetResourceBrush("VelocityTextBrush", "#FFFFFFFF");
            var typeface = new Typeface("Segoe UI", FontStyle.Normal, FontWeight.Normal);
            
            var formattedText = new FormattedText(
                velocity.ToString(),
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                isPreview ? 12 : 10, // Ԥ��ʱ�Դ�һЩ
                textBrush);

            var textX = barRect.X + (barRect.Width - formattedText.Width) / 2;
            var textY = isPreview ? barRect.Y - 15 : barRect.Y + 2; // Ԥ��ʱ��ʾ�������Ϸ�

            context.DrawText(formattedText, new Point(textX, textY));
        }
    }

    /// <summary>
    /// ��������Ⱦ����
    /// </summary>
    public enum VelocityRenderType
    {
        Normal,
        Selected,
        Editing,
        Dragging
    }
}