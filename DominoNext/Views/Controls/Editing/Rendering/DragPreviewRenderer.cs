using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// 拖拽预览渲染器 - 重构简化版本
    /// </summary>
    public class DragPreviewRenderer
    {
        // 文本渲染缓存
        private readonly Dictionary<string, FormattedText> _textCache = new();
        private readonly Typeface _cachedTypeface;

        // 缓存优化的预置计算缓存
        private readonly string[] _precomputedNoteNames = new string[128]; // 预置所有音符名称

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

        public DragPreviewRenderer()
        {
            // 预建缓存内容
            _cachedTypeface = new Typeface(FontFamily.Default);

            // 预计算所有可能的音符名称，避免运行时重复计算开销
            PrecomputeNoteNames();
        }

        /// <summary>
        /// 预计算所有MIDI音符名称，避免运行时重复计算开销
        /// </summary>
        private void PrecomputeNoteNames()
        {
            var noteNames = new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            
            for (int pitch = 0; pitch < 128; pitch++)
            {
                var octave = pitch / 12 - 1;
                var noteIndex = pitch % 12;
                _precomputedNoteNames[pitch] = $"{noteNames[noteIndex]}{octave}";
            }
        }

        /// <summary>
        /// 渲染拖拽预览效果 - 重构简化版本
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel.DragState.DraggingNotes == null || viewModel.DragState.DraggingNotes.Count == 0) return;

            var draggingNotes = viewModel.DragState.DraggingNotes;
            
            // 获取拖拽颜色资源
            var dragBrush = CreateBrushWithOpacity(GetResourceBrush("NoteDraggingBrush", "#FF2196F3"), 0.9);
            var dragPen = GetResourcePen("NoteDraggingPenBrush", "#FF1976D2", 2);
            
            // 直接渲染，避免复杂的分层处理
            foreach (var note in draggingNotes)
            {
                var noteRect = calculateNoteRect(note);
                if (noteRect.Width > 0 && noteRect.Height > 0)
                {
                    // 渲染音符体
                    context.DrawRectangle(dragBrush, dragPen, noteRect);
                    
                    // 只为足够大的音符显示文本
                    if (noteRect.Width > 25 && noteRect.Height > 8)
                    {
                        DrawNoteTextUltraFast(context, note.Pitch, noteRect);
                    }
                }
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

        /// <summary>
        /// 极速文本渲染 - 使用预置缓存
        /// </summary>
        private void DrawNoteTextUltraFast(DrawingContext context, int pitch, Rect noteRect)
        {
            // 直接使用预置的音符名称，避免运行时计算
            var text = _precomputedNoteNames[pitch];
            
            // 使用文本缓存
            if (!_textCache.TryGetValue(text, out var formattedText))
            {
                var textBrush = GetResourceBrush("KeyTextWhiteBrush", "#FF000000");
                formattedText = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    _cachedTypeface,
                    9,
                    textBrush);
                
                // 限制缓存大小
                if (_textCache.Count < 50)
                {
                    _textCache[text] = formattedText;
                }
            }

            var textPosition = new Point(
                noteRect.X + (noteRect.Width - formattedText.Width) * 0.5,
                noteRect.Y + (noteRect.Height - formattedText.Height) * 0.5);

            // 简化背景处理
            var textBounds = new Rect(
                textPosition.X - 1,
                textPosition.Y,
                formattedText.Width + 2,
                formattedText.Height);
            
            var textBackgroundBrush = CreateBrushWithOpacity(GetResourceBrush("AppBackgroundBrush", "#FFFFFFFF"), 0.85);
            context.DrawRectangle(textBackgroundBrush, null, textBounds);
            context.DrawText(formattedText, textPosition);
        }
    }
}