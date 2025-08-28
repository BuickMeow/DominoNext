using System;
using System.Linq;
using Avalonia;
using DominoNext.ViewModels.Editor.State;
using DominoNext.Services.Interfaces;
using DominoNext.Models.Music;
using System.Diagnostics;

namespace DominoNext.ViewModels.Editor.Modules
{
    /// <summary>
    /// ������ק����ģ��
    /// </summary>
    public class NoteDragModule
    {
        private readonly DragState _dragState;
        private readonly ICoordinateService _coordinateService;
        private PianoRollViewModelV2? _pianoRollViewModel;

        public NoteDragModule(DragState dragState, ICoordinateService coordinateService)
        {
            _dragState = dragState;
            _coordinateService = coordinateService;
        }

        public void SetPianoRollViewModel(PianoRollViewModelV2 viewModel)
        {
            _pianoRollViewModel = viewModel;
        }

        /// <summary>
        /// ��ʼ��ק����
        /// </summary>
        public void StartDrag(NoteViewModel note, Point startPosition)
        {
            if (_pianoRollViewModel == null) return;

            _dragState.StartDrag(note, startPosition);
            
            // ��ȡ����ѡ�е�����������ק
            _dragState.DraggingNotes = _pianoRollViewModel.Notes.Where(n => n.IsSelected).ToList();

            // ��¼���б���ק������ԭʼλ��
            _dragState.OriginalDragPositions.Clear();
            foreach (var dragNote in _dragState.DraggingNotes)
            {
                _dragState.OriginalDragPositions[dragNote] = (dragNote.StartPosition, dragNote.Pitch);
                dragNote.PropertyChanged += OnDraggingNotePropertyChanged;
            }

            Debug.WriteLine($"��ʼ��ק {_dragState.DraggingNotes.Count} ������");
        }

        /// <summary>
        /// ������ק
        /// </summary>
        public void UpdateDrag(Point currentPosition)
        {
            if (!_dragState.IsDragging || _pianoRollViewModel == null) return;

            try
            {
                var deltaX = currentPosition.X - _dragState.DragStartPosition.X;
                var deltaY = currentPosition.Y - _dragState.DragStartPosition.Y;

                // ����ʱ�������ƫ��
                var timeDeltaInTicks = deltaX / (_pianoRollViewModel.PixelsPerTick * _pianoRollViewModel.Zoom);
                var pitchDelta = -(int)(deltaY / _pianoRollViewModel.KeyHeight);

                bool anyNoteChanged = false;

                // �������б���ק������
                foreach (var note in _dragState.DraggingNotes)
                {
                    if (_dragState.OriginalDragPositions.TryGetValue(note, out var originalPos))
                    {
                        var originalTimeInTicks = originalPos.OriginalStartPosition.ToTicks(_pianoRollViewModel.TicksPerBeat);
                        var newTimeInTicks = Math.Max(0, originalTimeInTicks + timeDeltaInTicks);
                        var newPitch = Math.Max(0, Math.Min(127, originalPos.OriginalPitch + pitchDelta));

                        // ������λ��
                        var quantizedTimeInTicks = _pianoRollViewModel.SnapToGridTime(newTimeInTicks);
                        var newStartPosition = MusicalFraction.FromTicks(quantizedTimeInTicks, _pianoRollViewModel.TicksPerBeat);

                        bool positionChanged = !note.StartPosition.Equals(newStartPosition);
                        bool pitchChanged = note.Pitch != newPitch;

                        if (positionChanged || pitchChanged)
                        {
                            if (positionChanged) note.StartPosition = newStartPosition;
                            if (pitchChanged) note.Pitch = newPitch;
                            
                            note.InvalidateCache();
                            anyNoteChanged = true;
                        }
                    }
                }

                if (anyNoteChanged)
                {
                    // �������Ա��֪ͨ
                    OnDragUpdated?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"��ק���´���: {ex.Message}");
                // ����ʱ�ָ�ԭʼλ��
                RestoreOriginalPositions();
            }
        }

        /// <summary>
        /// ������ק
        /// </summary>
        public void EndDrag()
        {
            if (_dragState.IsDragging)
            {
                Debug.WriteLine($"�����ק {_dragState.DraggingNotes.Count} ������");
                
                // ȡ�����Ա仯����
                foreach (var note in _dragState.DraggingNotes)
                {
                    note.PropertyChanged -= OnDraggingNotePropertyChanged;
                }
            }

            _dragState.EndDrag();
            OnDragEnded?.Invoke();
        }

        /// <summary>
        /// ȡ����ק���ָ�ԭʼλ��
        /// </summary>
        public void CancelDrag()
        {
            if (_dragState.IsDragging && _dragState.DraggingNotes.Count > 0)
            {
                RestoreOriginalPositions();
                Debug.WriteLine($"ȡ����ק���ָ� {_dragState.DraggingNotes.Count} ��������ԭʼλ��");
            }

            EndDrag();
        }

        private void RestoreOriginalPositions()
        {
            foreach (var note in _dragState.DraggingNotes)
            {
                if (_dragState.OriginalDragPositions.TryGetValue(note, out var originalPos))
                {
                    note.StartPosition = originalPos.OriginalStartPosition;
                    note.Pitch = originalPos.OriginalPitch;
                    note.InvalidateCache();
                }
                
                note.PropertyChanged -= OnDraggingNotePropertyChanged;
            }
        }

        private void OnDraggingNotePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NoteViewModel.StartPosition) || e.PropertyName == nameof(NoteViewModel.Pitch))
            {
                OnDragUpdated?.Invoke();
            }
        }

        // �¼�
        public event Action? OnDragUpdated;
        public event Action? OnDragEnded;

        // ֻ������
        public bool IsDragging => _dragState.IsDragging;
        public NoteViewModel? DraggingNote => _dragState.DraggingNote;
        public System.Collections.Generic.List<NoteViewModel> DraggingNotes => _dragState.DraggingNotes;
    }
}