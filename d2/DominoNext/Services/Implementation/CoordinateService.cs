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
            // 修复：确保x=0时返回的时间值也是0，避免任何浮点精度问题
            if (Math.Abs(x) < 1e-10)
            {
                // System.Diagnostics.Debug.WriteLine($"GetTimeFromX: x={x} -> 返回0 (特殊处理)"); // 关闭调试输出
                return 0;
            }
            
            var result = Math.Max(0, x / (pixelsPerTick * zoom));
            
            // 关闭调试信息以减少输出
            /*
            if (x < 50) // 只记录前50像素内的转换，避免日志过多
            {
                System.Diagnostics.Debug.WriteLine($"GetTimeFromX: x={x}, zoom={zoom}, pixelsPerTick={pixelsPerTick} -> time={result}");
            }
            */            
            return result;
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