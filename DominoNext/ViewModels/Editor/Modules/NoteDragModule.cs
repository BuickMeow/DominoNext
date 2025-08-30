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
    /// 拖拽模块 - 改进版本
    /// </summary>
    public class NoteDragModule
    {
        private readonly DragState _dragState;
        private readonly ICoordinateService _coordinateService;
        private PianoRollViewModel? _pianoRollViewModel;

        // 防抖动像素阈值
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
        /// 开始拖拽
        /// </summary>
        public void StartDrag(NoteViewModel note, Point startPosition)
        {
            if (_pianoRollViewModel == null) return;

            _dragState.StartDrag(note, startPosition);
            
            // 获取所有选中的音符进行拖拽
            _dragState.DraggingNotes = _pianoRollViewModel.Notes.Where(n => n.IsSelected).ToList();

            // 记录音符列表的拖拽初始位置
            _dragState.OriginalDragPositions.Clear();
            foreach (var dragNote in _dragState.DraggingNotes)
            {
                _dragState.OriginalDragPositions[dragNote] = (dragNote.StartPosition, dragNote.Pitch);
            }

            Debug.WriteLine($"开始拖拽 {_dragState.DraggingNotes.Count} 个音符");
            OnDragStarted?.Invoke();
        }

        /// <summary>
        /// 更新拖拽 - 改进版本
        /// </summary>
        public void UpdateDrag(Point currentPosition)
        {
            if (!_dragState.IsDragging || _pianoRollViewModel == null) return;

            var deltaX = currentPosition.X - _dragState.DragStartPosition.X;
            var deltaY = currentPosition.Y - _dragState.DragStartPosition.Y;

            // 检查是否需要自动扩展钢琴卷帘
            _pianoRollViewModel.AutoExtendWhenNearEnd(currentPosition.X);

            // 当移动距离过小时不处理，防止意外微调
            var totalMovement = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            if (totalMovement < ANTI_SHAKE_PIXEL_THRESHOLD)
            {
                return; // 小于1像素的移动忽略
            }

            // 计算时间轴上的偏移
            var timeDeltaInTicks = deltaX / (_pianoRollViewModel.PixelsPerTick * _pianoRollViewModel.Zoom);
            var pitchDelta = -(int)(deltaY / _pianoRollViewModel.KeyHeight);

            // 直接更新所有被拖拽的音符
            foreach (var note in _dragState.DraggingNotes)
            {
                if (_dragState.OriginalDragPositions.TryGetValue(note, out var originalPos))
                {
                    var originalTimeInTicks = originalPos.OriginalStartPosition.ToTicks(_pianoRollViewModel.TicksPerBeat);
                    var newTimeInTicks = Math.Max(0, originalTimeInTicks + timeDeltaInTicks);
                    var newPitch = Math.Max(0, Math.Min(127, originalPos.OriginalPitch + pitchDelta));

                    // 量化到网格位置
                    var quantizedTimeInTicks = _pianoRollViewModel.SnapToGridTime(newTimeInTicks);
                    var newStartPosition = MusicalFraction.FromTicks(quantizedTimeInTicks, _pianoRollViewModel.TicksPerBeat);

                    // 直接更新
                    note.StartPosition = newStartPosition;
                    note.Pitch = newPitch;
                    note.InvalidateCache();
                }
            }

            // 发出更新通知
            OnDragUpdated?.Invoke();
        }

        /// <summary>
        /// 结束拖拽
        /// </summary>
        public void EndDrag()
        {
            if (_dragState.IsDragging)
            {
                Debug.WriteLine($"结束拖拽 {_dragState.DraggingNotes.Count} 个音符");
            }

            _dragState.EndDrag();
            OnDragEnded?.Invoke();
        }

        /// <summary>
        /// 取消拖拽
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
                Debug.WriteLine($"取消拖拽 {_dragState.DraggingNotes.Count} 个音符的初始位置");
            }

            EndDrag();
        }

        // 事件
        public event Action? OnDragStarted;
        public event Action? OnDragUpdated;
        public event Action? OnDragEnded;

        // 只读
        public bool IsDragging => _dragState.IsDragging;
        public NoteViewModel? DraggingNote => _dragState.DraggingNote;
        public System.Collections.Generic.List<NoteViewModel> DraggingNotes => _dragState.DraggingNotes;
    }
}