using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using DominoNext.ViewModels.Editor.State;
using DominoNext.Services.Interfaces;

namespace DominoNext.ViewModels.Editor.Modules
{
    /// <summary>
    /// ����ѡ����ģ��
    /// </summary>
    public class NoteSelectionModule
    {
        private readonly SelectionState _selectionState;
        private readonly ICoordinateService _coordinateService;
        private PianoRollViewModel? _pianoRollViewModel;

        public NoteSelectionModule(SelectionState selectionState, ICoordinateService coordinateService)
        {
            _selectionState = selectionState;
            _coordinateService = coordinateService;
        }

        public void SetPianoRollViewModel(PianoRollViewModel viewModel)
        {
            _pianoRollViewModel = viewModel;
        }

        /// <summary>
        /// ��ȡָ��λ�õ�����
        /// </summary>
        public NoteViewModel? GetNoteAtPosition(Point position, IEnumerable<NoteViewModel> notes, double zoom, double pixelsPerTick, double keyHeight)
        {
            foreach (var note in notes)
            {
                var noteRect = _coordinateService.GetNoteRect(note, zoom, pixelsPerTick, keyHeight);
                if (noteRect.Contains(position))
                {
                    return note;
                }
            }
            return null;
        }

        /// <summary>
        /// ��ʼѡ���
        /// </summary>
        public void StartSelection(Point startPoint)
        {
            _selectionState.StartSelection(startPoint);
            OnSelectionUpdated?.Invoke();
        }

        /// <summary>
        /// ����ѡ���
        /// </summary>
        public void UpdateSelection(Point currentPoint)
        {
            _selectionState.UpdateSelection(currentPoint);
            OnSelectionUpdated?.Invoke();
        }

        /// <summary>
        /// ����ѡ���
        /// </summary>
        public void EndSelection(IEnumerable<NoteViewModel> notes)
        {
            if (_selectionState.SelectionStart.HasValue && _selectionState.SelectionEnd.HasValue && _pianoRollViewModel != null)
            {
                var start = _selectionState.SelectionStart.Value;
                var end = _selectionState.SelectionEnd.Value;

                var x = System.Math.Min(start.X, end.X);
                var y = System.Math.Min(start.Y, end.Y);
                var width = System.Math.Abs(end.X - start.X);
                var height = System.Math.Abs(end.Y - start.Y);

                var selectionRect = new Rect(x, y, width, height);
                SelectNotesInArea(selectionRect, notes);
            }

            _selectionState.EndSelection();
            OnSelectionUpdated?.Invoke();
        }

        /// <summary>
        /// ѡ��������������
        /// </summary>
        public void SelectNotesInArea(Rect area, IEnumerable<NoteViewModel> notes)
        {
            if (_pianoRollViewModel == null) return;

            foreach (var note in notes)
            {
                var noteRect = _coordinateService.GetNoteRect(note, 
                    _pianoRollViewModel.Zoom, 
                    _pianoRollViewModel.PixelsPerTick, 
                    _pianoRollViewModel.KeyHeight);
                
                if (area.Intersects(noteRect))
                {
                    note.IsSelected = true;
                }
            }
        }

        /// <summary>
        /// ���ѡ��
        /// </summary>
        public void ClearSelection(IEnumerable<NoteViewModel> notes)
        {
            foreach (var note in notes)
            {
                note.IsSelected = false;
            }
        }

        /// <summary>
        /// ѡ����������
        /// </summary>
        public void SelectAll(IEnumerable<NoteViewModel> notes)
        {
            foreach (var note in notes)
            {
                note.IsSelected = true;
            }
        }

        // �¼�
        public event Action? OnSelectionUpdated;

        // ֻ������
        public bool IsSelecting => _selectionState.IsSelecting;
        public Point? SelectionStart => _selectionState.SelectionStart;
        public Point? SelectionEnd => _selectionState.SelectionEnd;
    }
}