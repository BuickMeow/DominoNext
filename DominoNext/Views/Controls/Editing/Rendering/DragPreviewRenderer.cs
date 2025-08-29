using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using DominoNext.Services.Implementation;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// 拖拽预览渲染器 - 空间优化版本
    /// </summary>
    public class DragPreviewRenderer
    {
        private readonly ThemeService _themeService;
        
        // 文本渲染缓存
        private readonly Dictionary<string, FormattedText> _textCache = new();
        private readonly Typeface _cachedTypeface;

        // 性能优化：预览计算缓存
        private readonly string[] _precomputedNoteNames = new string[128]; // 预计算所有音符名称

        public DragPreviewRenderer()
        {
            _themeService = ThemeService.Instance;
            _cachedTypeface = new Typeface(FontFamily.Default);

            // 预计算所有可能的音符名称，避免运行时计算
            PrecomputeNoteNames();
        }

        /// <summary>
        /// 预计算所有MIDI音符名称，避免运行时计算开销
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
        /// 渲染拖拽预览效果 - 空间优化版本
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel.DragState.DraggingNotes == null || viewModel.DragState.DraggingNotes.Count == 0) return;

            var draggingNotes = viewModel.DragState.DraggingNotes;
            
            // 直接渲染，避免多余的分层处理
            foreach (var note in draggingNotes)
            {
                var noteRect = calculateNoteRect(note);
                if (noteRect.Width > 0 && noteRect.Height > 0)
                {
                    // 渲染拖拽音符
                    context.DrawRectangle(_themeService.DraggingNoteBrush, _themeService.DraggingNotePen, noteRect);
                    
                    // 只为足够大的音符显示文本
                    if (noteRect.Width > 25 && noteRect.Height > 8)
                    {
                        DrawNoteTextUltraFast(context, note.Pitch, noteRect);
                    }
                }
            }
        }

        /// <summary>
        /// 超快速文本绘制 - 使用预计算缓存
        /// </summary>
        private void DrawNoteTextUltraFast(DrawingContext context, int pitch, Rect noteRect)
        {
            // 直接使用预计算的音符名称，避免运行时计算
            var text = _precomputedNoteNames[pitch];
            
            // 使用文本缓存
            if (!_textCache.TryGetValue(text, out var formattedText))
            {
                formattedText = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    _cachedTypeface,
                    9,
                    Brushes.Black);
                
                // 限制缓存大小
                if (_textCache.Count < 50)
                {
                    _textCache[text] = formattedText;
                }
            }

            var textPosition = new Point(
                noteRect.X + (noteRect.Width - formattedText.Width) * 0.5,
                noteRect.Y + (noteRect.Height - formattedText.Height) * 0.5);

            // 简化背景绘制
            var textBounds = new Rect(
                textPosition.X - 1,
                textPosition.Y,
                formattedText.Width + 2,
                formattedText.Height);
            
            context.DrawRectangle(new SolidColorBrush(Colors.White, 0.85), null, textBounds);
            context.DrawText(formattedText, textPosition);
        }
    }
}