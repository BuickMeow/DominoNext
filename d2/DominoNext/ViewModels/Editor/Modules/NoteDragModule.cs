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
    /// 音符拖拽功能模块
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
        /// 开始拖拽音符
        /// </summary>
        public void StartDrag(NoteViewModel note, Point startPosition)
        {
            if (_pianoRollViewModel == null) return;

            _dragState.StartDrag(note, startPosition);
            
            // 获取所有选中的音符进行拖拽
            _dragState.DraggingNotes = _pianoRollViewModel.Notes.Where(n => n.IsSelected).ToList();

            // 记录所有被拖拽音符的原始位置
            _dragState.OriginalDragPositions.Clear();
            foreach (var dragNote in _dragState.DraggingNotes)
            {
                _dragState.OriginalDragPositions[dragNote] = (dragNote.StartPosition, dragNote.Pitch);
                dragNote.PropertyChanged += OnDraggingNotePropertyChanged;
            }

            Debug.WriteLine($"开始拖拽 {_dragState.DraggingNotes.Count} 个音符");
        }

        /// <summary>
        /// 更新拖拽
        /// </summary>
        public void UpdateDrag(Point currentPosition)
        {
            if (!_dragState.IsDragging || _pianoRollViewModel == null) return;

            try
            {
                var deltaX = currentPosition.X - _dragState.DragStartPosition.X;
                var deltaY = currentPosition.Y - _dragState.DragStartPosition.Y;

                // 计算时间和音高偏移
                var timeDeltaInTicks = deltaX / (_pianoRollViewModel.PixelsPerTick * _pianoRollViewModel.Zoom);
                var pitchDelta = -(int)(deltaY / _pianoRollViewModel.KeyHeight);

                bool anyNoteChanged = false;

                // 更新所有被拖拽的音符
                foreach (var note in _dragState.DraggingNotes)
                {
                    if (_dragState.OriginalDragPositions.TryGetValue(note, out var originalPos))
                    {
                        var originalTimeInTicks = originalPos.OriginalStartPosition.ToTicks(_pianoRollViewModel.TicksPerBeat);
                        var newTimeInTicks = Math.Max(0, originalTimeInTicks + timeDeltaInTicks);
                        var newPitch = Math.Max(0, Math.Min(127, originalPos.OriginalPitch + pitchDelta));

                        // 量化新位置
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
                    // 触发属性变更通知
                    OnDragUpdated?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"拖拽更新错误: {ex.Message}");
                // 出错时恢复原始位置
                RestoreOriginalPositions();
            }
        }

        /// <summary>
        /// 结束拖拽
        /// </summary>
        public void EndDrag()
        {
            if (_dragState.IsDragging)
            {
                Debug.WriteLine($"完成拖拽 {_dragState.DraggingNotes.Count} 个音符");
                
                // 取消属性变化监听
                foreach (var note in _dragState.DraggingNotes)
                {
                    note.PropertyChanged -= OnDraggingNotePropertyChanged;
                }
            }

            _dragState.EndDrag();
            OnDragEnded?.Invoke();
        }

        /// <summary>
        /// 取消拖拽，恢复原始位置
        /// </summary>
        public void CancelDrag()
        {
            if (_dragState.IsDragging && _dragState.DraggingNotes.Count > 0)
            {
                RestoreOriginalPositions();
                Debug.WriteLine($"取消拖拽，恢复 {_dragState.DraggingNotes.Count} 个音符的原始位置");
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

        // 事件
        public event Action? OnDragUpdated;
        public event Action? OnDragEnded;

        // 只读属性
        public bool IsDragging => _dragState.IsDragging;
        public NoteViewModel? DraggingNote => _dragState.DraggingNote;
        public System.Collections.Generic.List<NoteViewModel> DraggingNotes => _dragState.DraggingNotes;
    }
}