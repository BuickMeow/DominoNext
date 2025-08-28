using Avalonia;
using DominoNext.ViewModels.Editor;

namespace DominoNext.Services.Interfaces
{
    public interface INoteEditingService
    {
        void CreateNoteAtPosition(Point position);
        void StartNoteDrag(NoteViewModel note, Point startPosition);
        void UpdateNoteDrag(Point currentPosition);
        void EndNoteDrag();
        void SelectNotesInArea(Rect area);
        void ClearSelection();
        void DeleteSelectedNotes();
        void DuplicateSelectedNotes();
        void QuantizeSelectedNotes();
        bool IsValidNotePosition(int pitch, double startTime);
    }
}