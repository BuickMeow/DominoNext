using Avalonia;

namespace DominoNext.ViewModels.Editor.State
{
    /// <summary>
    /// Ñ¡Ôñ¿ò×´Ì¬¹ÜÀí
    /// </summary>
    public class SelectionState
    {
        public bool IsSelecting { get; set; }
        public Point? SelectionStart { get; set; }
        public Point? SelectionEnd { get; set; }

        public void StartSelection(Point startPoint)
        {
            IsSelecting = true;
            SelectionStart = startPoint;
            SelectionEnd = startPoint;
        }

        public void UpdateSelection(Point currentPoint)
        {
            if (IsSelecting)
            {
                SelectionEnd = currentPoint;
            }
        }

        public void EndSelection()
        {
            IsSelecting = false;
            SelectionStart = null;
            SelectionEnd = null;
        }

        public void Reset()
        {
            EndSelection();
        }
    }
}