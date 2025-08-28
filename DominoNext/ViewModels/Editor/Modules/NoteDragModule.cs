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
    /// ������ק����ģ�� - ����汾
    /// </summary>
    public class NoteDragModule
    {
        private readonly DragState _dragState;
        private readonly ICoordinateService _coordinateService;
        private PianoRollViewModel? _pianoRollViewModel;

        // ������ֶ���ֻ������΢С���ƶ��ź���
        // �����Ҫ�޸ķ��ֶ����жȣ����޸��������
        private const double ANTI_SHAKE_PIXEL_THRESHOLD = 1.0;

        public NoteDragModule(DragState dragState, ICoordinateService coordinateService)
        {
            _dragState = dragState;
            _coordinateService = coordinateService;
        }

        public void SetPianoRollViewModel(PianoRollViewModel viewModel)
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
            }

            Debug.WriteLine($"��ʼ��ק {_dragState.DraggingNotes.Count} ������");
        }

        /// <summary>
        /// ������ק - ����汾
        /// </summary>
        public void UpdateDrag(Point currentPosition)
        {
            if (!_dragState.IsDragging || _pianoRollViewModel == null) return;

            var deltaX = currentPosition.X - _dragState.DragStartPosition.X;
            var deltaY = currentPosition.Y - _dragState.DragStartPosition.Y;

            // ������ֶ���ֻ����������΢��
            var totalMovement = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (totalMovement < ANTI_SHAKE_PIXEL_THRESHOLD)
            {
                return; // С��1���ص��ƶ�����
            }

            // ����ʱ�������ƫ��
            var timeDeltaInTicks = deltaX / (_pianoRollViewModel.PixelsPerTick * _pianoRollViewModel.Zoom);
            var pitchDelta = -(int)(deltaY / _pianoRollViewModel.KeyHeight);

            // ֱ�Ӹ������б���ק������
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

                    // ֱ�Ӹ���
                    note.StartPosition = newStartPosition;
                    note.Pitch = newPitch;
                    note.InvalidateCache();
                }
            }

            // ��������֪ͨ
            OnDragUpdated?.Invoke();
        }

        /// <summary>
        /// ������ק
        /// </summary>
        public void EndDrag()
        {
            if (_dragState.IsDragging)
            {
                Debug.WriteLine($"������ק {_dragState.DraggingNotes.Count} ������");
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
                foreach (var note in _dragState.DraggingNotes)
                {
                    if (_dragState.OriginalDragPositions.TryGetValue(note, out var originalPos))
                    {
                        note.StartPosition = originalPos.OriginalStartPosition;
                        note.Pitch = originalPos.OriginalPitch;
                        note.InvalidateCache();
                    }
                }
                Debug.WriteLine($"ȡ����ק���ָ� {_dragState.DraggingNotes.Count} ��������ԭʼλ��");
            }

            EndDrag();
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