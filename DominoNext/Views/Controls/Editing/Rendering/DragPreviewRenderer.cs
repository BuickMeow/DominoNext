using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// ��קԤ����Ⱦ�� - �ռ��򻯰汾
    /// </summary>
    public class DragPreviewRenderer
    {
        private readonly Color _dragPreviewColor = Color.Parse("#FFC107");
        private readonly IPen _dragPreviewPen = new Pen(new SolidColorBrush(Color.Parse("#FF8F00")), 2);
        
        // ���滭ˢ�ͻ��ʣ������ظ�����
        private readonly IBrush _cachedDragBrush;
        
        // �ı���Ⱦ����
        private readonly Dictionary<string, FormattedText> _textCache = new();
        private readonly Typeface _cachedTypeface;

        // �����Ż���Ԥ���㻺��
        private readonly string[] _precomputedNoteNames = new string[128]; // Ԥ����������������

        public DragPreviewRenderer()
        {
            // Ԥ�����������
            _cachedDragBrush = new SolidColorBrush(_dragPreviewColor, 0.9);
            _cachedTypeface = new Typeface(FontFamily.Default);

            // Ԥ�������п��ܵ��������ƣ���������ʱ����
            PrecomputeNoteNames();
        }

        /// <summary>
        /// Ԥ��������MIDI�������ƣ�������������ʱ���㿪��
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
        /// ��Ⱦ��קԤ��Ч�� - �ռ��򻯰汾
        /// </summary>
        public void Render(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel.DragState.DraggingNotes == null || viewModel.DragState.DraggingNotes.Count == 0) return;

            var draggingNotes = viewModel.DragState.DraggingNotes;
            
            // ֱ����Ⱦ�����踴�ӵķֲ㴦��
            foreach (var note in draggingNotes)
            {
                var noteRect = calculateNoteRect(note);
                if (noteRect.Width > 0 && noteRect.Height > 0)
                {
                    // ��Ⱦ��������
                    context.DrawRectangle(_cachedDragBrush, _dragPreviewPen, noteRect);
                    
                    // ֻΪ�㹻���������ʾ�ı�
                    if (noteRect.Width > 25 && noteRect.Height > 8)
                    {
                        DrawNoteTextUltraFast(context, note.Pitch, noteRect);
                    }
                }
            }
        }

        /// <summary>
        /// �������ı����� - ʹ��Ԥ������
        /// </summary>
        private void DrawNoteTextUltraFast(DrawingContext context, int pitch, Rect noteRect)
        {
            // ֱ��ʹ��Ԥ������������ƣ�������ʱ����
            var text = _precomputedNoteNames[pitch];
            
            // ʹ���ı�����
            if (!_textCache.TryGetValue(text, out var formattedText))
            {
                formattedText = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    _cachedTypeface,
                    9,
                    Brushes.Black);
                
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
            
            context.DrawRectangle(new SolidColorBrush(Colors.White, 0.85), null, textBounds);
            context.DrawText(formattedText, textPosition);
        }
    }
}