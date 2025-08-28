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
    /// ����������С����ģ��
    /// </summary>
    public class NoteResizeModule
    {
        private readonly ResizeState _resizeState;
        private readonly ICoordinateService _coordinateService;
        private PianoRollViewModel? _pianoRollViewModel;

        // ��ק��Ե�����ֵ
        private const double ResizeEdgeThreshold = 8.0;

        public NoteResizeModule(ResizeState resizeState, ICoordinateService coordinateService)
        {
            _resizeState = resizeState;
            _coordinateService = coordinateService;
        }

        public void SetPianoRollViewModel(PianoRollViewModel viewModel)
        {
            _pianoRollViewModel = viewModel;
        }

        /// <summary>
        /// ������λ���Ƿ�ӽ������ı�Ե
        /// </summary>
        public ResizeHandle GetResizeHandleAtPosition(Point position, NoteViewModel note)
        {
            if (_pianoRollViewModel?.CurrentTool != EditorTool.Pencil) return ResizeHandle.None;

            var noteRect = _coordinateService.GetNoteRect(note, 
                _pianoRollViewModel.Zoom, 
                _pianoRollViewModel.PixelsPerTick, 
                _pianoRollViewModel.KeyHeight);

            if (!noteRect.Contains(position)) return ResizeHandle.None;

            // ����Ƿ�ӽ���ʼ��Ե
            if (Math.Abs(position.X - noteRect.Left) <= ResizeEdgeThreshold)
            {
                return ResizeHandle.StartEdge;
            }

            // ����Ƿ�ӽ�������Ե
            if (Math.Abs(position.X - noteRect.Right) <= ResizeEdgeThreshold)
            {
                return ResizeHandle.EndEdge;
            }

            return ResizeHandle.None;
        }

        /// <summary>
        /// ��ʼ������С
        /// </summary>
        public void StartResize(Point position, NoteViewModel note, ResizeHandle handle)
        {
            if (handle == ResizeHandle.None || _pianoRollViewModel == null) return;

            _resizeState.StartResize(note, handle);

            // ��ȡ����ѡ�е�������������ǰ������
            _resizeState.ResizingNotes = _pianoRollViewModel.Notes.Where(n => n.IsSelected).ToList();
            if (!_resizeState.ResizingNotes.Contains(note))
            {
                _resizeState.ResizingNotes.Add(note);
            }

            // ��¼ԭʼ���Ⱥ�λ��
            _resizeState.OriginalDurations.Clear();
            foreach (var n in _resizeState.ResizingNotes)
            {
                _resizeState.OriginalDurations[n] = n.Duration;
                n.PropertyChanged += OnResizingNotePropertyChanged;
            }

            Debug.WriteLine($"��ʼ������������: Handle={handle}, ѡ��������={_resizeState.ResizingNotes.Count}");
            OnResizeStarted?.Invoke();
        }

        /// <summary>
        /// ���µ�����С
        /// </summary>
        public void UpdateResize(Point currentPosition)
        {
            if (!_resizeState.IsResizing || _resizeState.ResizingNote == null || 
                _resizeState.ResizingNotes.Count == 0 || _pianoRollViewModel == null) return;

            try
            {
                var currentTime = _pianoRollViewModel.GetTimeFromX(currentPosition.X);
                bool anyNoteChanged = false;

                foreach (var note in _resizeState.ResizingNotes)
                {
                    var startTime = note.StartPosition.ToTicks(_pianoRollViewModel.TicksPerBeat);
                    var endTime = startTime + note.Duration.ToTicks(_pianoRollViewModel.TicksPerBeat);
                    var originalDuration = _resizeState.OriginalDurations[note];

                    MusicalFraction newDuration;
                    MusicalFraction newStartPosition = note.StartPosition;

                    if (_resizeState.CurrentResizeHandle == ResizeHandle.StartEdge)
                    {
                        // ������ʼλ��
                        var newStartTime = Math.Min(currentTime, endTime - _pianoRollViewModel.GridQuantization.ToTicks(_pianoRollViewModel.TicksPerBeat));
                        newStartTime = _pianoRollViewModel.SnapToGridTime(newStartTime);

                        var newDurationTicks = endTime - newStartTime;
                        newDuration = MusicalFraction.FromTicks(newDurationTicks, _pianoRollViewModel.TicksPerBeat);
                        newStartPosition = MusicalFraction.FromTicks(newStartTime, _pianoRollViewModel.TicksPerBeat);
                    }
                    else // EndEdge
                    {
                        // ��������λ��
                        var newEndTime = Math.Max(currentTime, startTime + _pianoRollViewModel.GridQuantization.ToTicks(_pianoRollViewModel.TicksPerBeat));
                        newEndTime = _pianoRollViewModel.SnapToGridTime(newEndTime);

                        var newDurationTicks = newEndTime - startTime;
                        newDuration = MusicalFraction.FromTicks(newDurationTicks, _pianoRollViewModel.TicksPerBeat);
                    }

                    // Ӧ����С����Լ��
                    var minDuration = _pianoRollViewModel.GridQuantization;
                    if (originalDuration.CompareTo(minDuration) < 0)
                    {
                        newDuration = originalDuration;
                    }
                    else
                    {
                        if (newDuration.CompareTo(minDuration) < 0)
                        {
                            newDuration = minDuration;
                        }
                    }

                    // ֻ�ڳ��Ȼ�λ�������ı�ʱ����
                    bool durationChanged = !note.Duration.Equals(newDuration);
                    bool positionChanged = _resizeState.CurrentResizeHandle == ResizeHandle.StartEdge && !note.StartPosition.Equals(newStartPosition);

                    if (durationChanged || positionChanged)
                    {
                        if (positionChanged) note.StartPosition = newStartPosition;
                        if (durationChanged) note.Duration = newDuration;

                        note.InvalidateCache();
                        anyNoteChanged = true;
                    }
                }

                if (anyNoteChanged)
                {
                    OnResizeUpdated?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"������������ʱ����: {ex.Message}");
            }
        }

        /// <summary>
        /// ��ɵ�����С
        /// </summary>
        public void EndResize()
        {
            if (_resizeState.IsResizing && _resizeState.ResizingNote != null && _pianoRollViewModel != null)
            {
                // �����û��Զ��峤��
                _pianoRollViewModel.UserDefinedNoteDuration = _resizeState.ResizingNote.Duration;
                Debug.WriteLine($"����������ȵ����������û��Զ��峤��: {_pianoRollViewModel.UserDefinedNoteDuration}");
            }

            // ȡ�����Ա仯����
            foreach (var note in _resizeState.ResizingNotes)
            {
                note.PropertyChanged -= OnResizingNotePropertyChanged;
            }

            _resizeState.EndResize();
            OnResizeEnded?.Invoke();
        }

        /// <summary>
        /// ȡ��������С
        /// </summary>
        public void CancelResize()
        {
            if (_resizeState.IsResizing && _resizeState.ResizingNotes.Count > 0)
            {
                // �ָ�ԭʼ����
                foreach (var note in _resizeState.ResizingNotes)
                {
                    if (_resizeState.OriginalDurations.TryGetValue(note, out var originalDuration))
                    {
                        note.Duration = originalDuration;
                        note.InvalidateCache();
                    }
                    note.PropertyChanged -= OnResizingNotePropertyChanged;
                }

                Debug.WriteLine($"ȡ���������ȵ������ָ� {_resizeState.ResizingNotes.Count} ��������ԭʼ����");
            }

            EndResize();
        }

        private void OnResizingNotePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NoteViewModel.Duration) || e.PropertyName == nameof(NoteViewModel.StartPosition))
            {
                OnResizeUpdated?.Invoke();
            }
        }

        // �¼�
        public event Action? OnResizeStarted;
        public event Action? OnResizeUpdated;
        public event Action? OnResizeEnded;

        // ֻ������
        public bool IsResizing => _resizeState.IsResizing;
        public ResizeHandle CurrentResizeHandle => _resizeState.CurrentResizeHandle;
        public NoteViewModel? ResizingNote => _resizeState.ResizingNote;
        public System.Collections.Generic.List<NoteViewModel> ResizingNotes => _resizeState.ResizingNotes;
    }
}