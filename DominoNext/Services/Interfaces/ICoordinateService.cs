using Avalonia;
using DominoNext.ViewModels.Editor;

namespace DominoNext.Services.Interfaces
{
    public interface ICoordinateService
    {
        int GetPitchFromY(double y, double keyHeight);
        double GetTimeFromX(double x, double zoom, double pixelsPerTick);
        Point GetPositionFromNote(NoteViewModel note, double zoom, double pixelsPerTick, double keyHeight);
        Rect GetNoteRect(NoteViewModel note, double zoom, double pixelsPerTick, double keyHeight);
    }
}