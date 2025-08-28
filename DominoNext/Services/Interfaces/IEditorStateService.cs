using Avalonia;
using DominoNext.ViewModels.Editor;

namespace DominoNext.Services.Interfaces
{
    /// <summary>
    /// 编辑器状态管理服务
    /// </summary>
    public interface IEditorStateService
    {
        bool IsDragging { get; }
        bool IsSelecting { get; }
        bool IsResizing { get; }

        void StartDrag(NoteViewModel note, Point startPosition);
        void UpdateDrag(Point currentPosition);
        void EndDrag();

        void StartSelection(Point startPosition);
        void UpdateSelection(Point currentPosition);
        void EndSelection();
    }
}