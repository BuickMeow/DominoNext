using System.Collections.Generic;
using Avalonia;
using DominoNext.Models.Music;

namespace DominoNext.ViewModels.Editor.State
{
    /// <summary>
    /// 音符拖拽状态管理
    /// </summary>
    public class DragState
    {
        public bool IsDragging { get; set; }
        public NoteViewModel? DraggingNote { get; set; }
        public List<NoteViewModel> DraggingNotes { get; set; } = new();
        public Point DragStartPosition { get; set; }
        
        // 记录原始位置用于实时预览和约束
        public Dictionary<NoteViewModel, (MusicalFraction OriginalStartPosition, int OriginalPitch)> 
            OriginalDragPositions { get; set; } = new();

        public void StartDrag(NoteViewModel note, Point startPosition)
        {
            IsDragging = true;
            DraggingNote = note;
            DragStartPosition = startPosition;
            
            if (!note.IsSelected)
            {
                note.IsSelected = true;
            }
        }

        public void EndDrag()
        {
            IsDragging = false;
            DraggingNote = null;
            DraggingNotes.Clear();
            OriginalDragPositions.Clear();
        }

        public void Reset()
        {
            EndDrag();
        }
    }
}