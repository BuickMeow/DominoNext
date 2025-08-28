using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;

namespace DominoNext.Views.Controls.Editing.Rendering
{
    /// <summary>
    /// ������Ⱦ��
    /// </summary>
    public class NoteRenderer
    {
        private readonly Color _noteColor = Color.Parse("#4CAF50");
        private readonly IPen _noteBorderPen = new Pen(new SolidColorBrush(Color.Parse("#2E7D32")), 2);
        private readonly Color _selectedNoteColor = Color.Parse("#FF9800");
        private readonly IPen _selectedNoteBorderPen = new Pen(new SolidColorBrush(Color.Parse("#F57C00")), 2);
        private readonly Color _previewNoteColor = Color.Parse("#81C784");
        private readonly IPen _previewNoteBorderPen = new Pen(new SolidColorBrush(Color.Parse("#66BB6A")), 2);

        /// <summary>
        /// ��Ⱦ��������
        /// </summary>
        public void RenderNotes(DrawingContext context, PianoRollViewModel viewModel, Dictionary<NoteViewModel, Rect> visibleNoteCache)
        {
            int drawnNotes = 0;
            foreach (var kvp in visibleNoteCache)
            {
                var note = kvp.Key;
                var rect = kvp.Value;

                if (rect.Width > 0 && rect.Height > 0)
                {
                    // ����������ڱ���ק�������С��ʹ�ýϵ�����ɫ��Ⱦԭʼλ��
                    bool isBeingDragged = (viewModel.DragState.IsDragging && viewModel.DragState.DraggingNotes.Contains(note));
                    bool isBeingResized = (viewModel.ResizeState.IsResizing && viewModel.ResizeState.ResizingNotes.Contains(note));
                    bool isBeingManipulated = isBeingDragged || isBeingResized;
                    
                    DrawNote(context, note, rect, isBeingManipulated);
                    drawnNotes++;
                }
            }

            System.Diagnostics.Debug.WriteLine($"������ {drawnNotes} ���ɼ�����");
        }

        /// <summary>
        /// ��ȾԤ������
        /// </summary>
        public void RenderPreviewNote(DrawingContext context, PianoRollViewModel viewModel, Func<NoteViewModel, Rect> calculateNoteRect)
        {
            if (viewModel.PreviewNote == null) return;

            var previewRect = calculateNoteRect(viewModel.PreviewNote);
            if (previewRect.Width > 0 && previewRect.Height > 0)
            {
                var brush = new SolidColorBrush(_previewNoteColor, 0.6);
                context.DrawRectangle(brush, _previewNoteBorderPen, previewRect);

                var durationText = viewModel.PreviewNote.Duration.ToString();
                if (previewRect.Width > 30 && previewRect.Height > 10)
                {
                    DrawNoteText(context, durationText, previewRect, 9);
                }
            }
        }

        /// <summary>
        /// ���Ƶ�������
        /// </summary>
        private void DrawNote(DrawingContext context, NoteViewModel note, Rect rect, bool isBeingManipulated = false)
        {
            var opacity = Math.Max(0.7, note.Velocity / 127.0);
            
            // ���ڲ���������ʹ�ø��ߵ�͸���ȣ����������ɼ�
            if (isBeingManipulated)
            {
                opacity = Math.Min(1.0, opacity * 1.1); // ��ߵ��ӽ���͸���������Ӿ�������
            }

            IBrush brush;
            IPen pen;

            if (note.IsSelected)
            {
                // ѡ������ʹ�ø���������ɫ
                brush = new SolidColorBrush(_selectedNoteColor, opacity);
                pen = _selectedNoteBorderPen;
            }
            else
            {
                brush = new SolidColorBrush(_noteColor, opacity);
                pen = _noteBorderPen;
            }

            // Ϊ��ק�е����������΢����ӰЧ������ǿ�Ӿ�����
            if (isBeingManipulated)
            {
                var shadowOffset = new Vector(1, 1);
                var shadowRect = rect.Translate(shadowOffset);
                var shadowBrush = new SolidColorBrush(Colors.Black, 0.2);
                context.DrawRectangle(shadowBrush, null, shadowRect);
            }

            context.DrawRectangle(brush, pen, rect);
        }

        /// <summary>
        /// �������ϻ����ı�
        /// </summary>
        private void DrawNoteText(DrawingContext context, string text, Rect noteRect, double fontSize)
        {
            var typeface = new Typeface(FontFamily.Default);
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black);

            var textPosition = new Point(
                noteRect.X + (noteRect.Width - formattedText.Width) / 2,
                noteRect.Y + (noteRect.Height - formattedText.Height) / 2);

            var textBounds = new Rect(
                textPosition.X - 2,
                textPosition.Y - 1,
                formattedText.Width + 4,
                formattedText.Height + 2);
            context.DrawRectangle(new SolidColorBrush(Colors.White, 0.8), null, textBounds);

            context.DrawText(formattedText, textPosition);
        }
    }
}