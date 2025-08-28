using Avalonia;
using System.ComponentModel;

namespace DominoNext.ViewModels.Editor.State
{
    /// <summary>
    /// Ñ¡Ôñ¿ò×´Ì¬¹ÜÀí
    /// </summary>
    public class SelectionState : INotifyPropertyChanged
    {
        private bool _isSelecting;
        private Point? _selectionStart;
        private Point? _selectionEnd;

        public bool IsSelecting
        {
            get => _isSelecting;
            set
            {
                if (_isSelecting != value)
                {
                    _isSelecting = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelecting)));
                }
            }
        }

        public Point? SelectionStart
        {
            get => _selectionStart;
            set
            {
                if (_selectionStart != value)
                {
                    _selectionStart = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectionStart)));
                }
            }
        }

        public Point? SelectionEnd
        {
            get => _selectionEnd;
            set
            {
                if (_selectionEnd != value)
                {
                    _selectionEnd = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectionEnd)));
                }
            }
        }

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

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}