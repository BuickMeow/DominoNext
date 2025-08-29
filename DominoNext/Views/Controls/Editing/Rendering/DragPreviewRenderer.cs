using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// ��קԤ����Ⱦ�� - �ع��򻯰汾
    /// </summary>
    public class DragPreviewRenderer
    {
        // �ı���Ⱦ����
        private readonly Dictionary<string, FormattedText> _textCache = new();
        private readonly Typeface _cachedTypeface;

        // �����Ż���Ԥ�ü��㻺��
        private readonly string[] _precomputedNoteNames = new string[128]; // Ԥ��������������

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

        public DragPreviewRenderer()
        {
            // Ԥ����������
            _cachedTypeface = new Typeface(FontFamily.Default);

            // Ԥ�������п��ܵ��������ƣ���������ʱ�ظ����㿪��
            PrecomputeNoteNames();
        }

        /// <summary>
        /// Ԥ��������MIDI�������ƣ���������ʱ�ظ����㿪��
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
        /// ��Ⱦ��קԤ��Ч�� - �ع��򻯰汾
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel.DragState.DraggingNotes == null || viewModel.DragState.DraggingNotes.Count == 0) return;

            var draggingNotes = viewModel.DragState.DraggingNotes;
            
            // ��ȡ��ק��ɫ��Դ
            var dragBrush = CreateBrushWithOpacity(GetResourceBrush("NoteDraggingBrush", "#FF2196F3"), 0.9);
            var dragPen = GetResourcePen("NoteDraggingPenBrush", "#FF1976D2", 2);
            
            // ֱ����Ⱦ�����⸴�ӵķֲ㴦��
            foreach (var note in draggingNotes)
            {
                var noteRect = calculateNoteRect(note);
                if (noteRect.Width > 0 && noteRect.Height > 0)
                {
                    // ��Ⱦ������
                    context.DrawRectangle(dragBrush, dragPen, noteRect);
                    
                    // ֻΪ�㹻���������ʾ�ı�
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
        /// �����ı���Ⱦ - ʹ��Ԥ�û���
        /// </summary>
        private void DrawNoteTextUltraFast(DrawingContext context, int pitch, Rect noteRect)
        {
            // ֱ��ʹ��Ԥ�õ��������ƣ���������ʱ����
            var text = _precomputedNoteNames[pitch];
            
            // ʹ���ı�����
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
                
                // ���ƻ����С
                if (_textCache.Count < 50)
                {
                    _textCache[text] = formattedText;
                }
            }

            var textPosition = new Point(
                noteRect.X + (noteRect.Width - formattedText.Width) * 0.5,
                noteRect.Y + (noteRect.Height - formattedText.Height) * 0.5);

            // �򻯱�������
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