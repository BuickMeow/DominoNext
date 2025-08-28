using System.Collections.Generic;
using Avalonia;
using DominoNext.Models.Music;

namespace DominoNext.ViewModels.Editor.State
{
    /// <summary>
    /// 音符调整大小状态管理
    /// </summary>
    public class ResizeState
    {
        public bool IsResizing { get; set; }
        public ResizeHandle CurrentResizeHandle { get; set; } = ResizeHandle.None;
        public NoteViewModel? ResizingNote { get; set; }
        public List<NoteViewModel> ResizingNotes { get; set; } = new();
        
        // 记录原始长度用于约束
        public Dictionary<NoteViewModel, MusicalFraction> OriginalDurations { get; set; } = new();

        public void StartResize(NoteViewModel note, ResizeHandle handle)
        {
            IsResizing = true;
            ResizingNote = note;
            CurrentResizeHandle = handle;
        }

        public void EndResize()
        {
            IsResizing = false;
            CurrentResizeHandle = ResizeHandle.None;
            ResizingNote = null;
            ResizingNotes.Clear();
            OriginalDurations.Clear();
        }

        public void Reset()
        {
            EndResize();
        }
    }

    /// <summary>
    /// 调整大小手柄类型
    /// </summary>
    public enum ResizeHandle
    {
        None,
        StartEdge,
        EndEdge
    }
}