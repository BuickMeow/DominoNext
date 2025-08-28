using System;
using Avalonia;
using DominoNext.Services.Interfaces;
using DominoNext.ViewModels.Editor;

namespace DominoNext.Services.Implementation
{
    public class CoordinateService : ICoordinateService
    {
        public int GetPitchFromY(double y, double keyHeight)
        {
            var keyIndex = (int)(y / keyHeight);
            return Math.Max(0, Math.Min(127, 127 - keyIndex));
        }

        public double GetTimeFromX(double x, double zoom, double pixelsPerTick)
        {
            return Math.Max(0, x / (pixelsPerTick * zoom));
        }

        public Point GetPositionFromNote(NoteViewModel note, double zoom, double pixelsPerTick, double keyHeight)
        {
            return new Point(
                note.GetX(zoom, pixelsPerTick),
                note.GetY(keyHeight)
            );
        }

        public Rect GetNoteRect(NoteViewModel note, double zoom, double pixelsPerTick, double keyHeight)
        {
            var x = note.GetX(zoom, pixelsPerTick);
            var y = note.GetY(keyHeight);
            var width = note.GetWidth(zoom, pixelsPerTick);
            var height = note.GetHeight(keyHeight);

            return new Rect(x, y, width, height);
        }
    }
}