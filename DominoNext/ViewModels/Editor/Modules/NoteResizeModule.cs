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
    /// 音符调整大小功能模块
    /// </summary>
    public class NoteResizeModule
    {
        private readonly ResizeState _resizeState;
        private readonly ICoordinateService _coordinateService;
        private PianoRollViewModel? _pianoRollViewModel;

        // 拖拽边缘检测阈值
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
        /// 检测鼠标位置是否接近音符的边缘
        /// </summary>
        public ResizeHandle GetResizeHandleAtPosition(Point position, NoteViewModel note)
        {
            if (_pianoRollViewModel?.CurrentTool != EditorTool.Pencil) return ResizeHandle.None;

            var noteRect = _coordinateService.GetNoteRect(note, 
                _pianoRollViewModel.Zoom, 
                _pianoRollViewModel.PixelsPerTick, 
                _pianoRollViewModel.KeyHeight);

            if (!noteRect.Contains(position)) return ResizeHandle.None;

            // 检查是否接近开始边缘
            if (Math.Abs(position.X - noteRect.Left) <= ResizeEdgeThreshold)
            {
                return ResizeHandle.StartEdge;
            }

            // 检查是否接近结束边缘
            if (Math.Abs(position.X - noteRect.Right) <= ResizeEdgeThreshold)
            {
                return ResizeHandle.EndEdge;
            }

            return ResizeHandle.None;
        }

        /// <summary>
        /// 开始调整大小
        /// </summary>
        public void StartResize(Point position, NoteViewModel note, ResizeHandle handle)
        {
            if (handle == ResizeHandle.None || _pianoRollViewModel == null) return;

            _resizeState.StartResize(note, handle);

            // 获取所有选中的音符（包括当前音符）
            _resizeState.ResizingNotes = _pianoRollViewModel.Notes.Where(n => n.IsSelected).ToList();
            if (!_resizeState.ResizingNotes.Contains(note))
            {
                _resizeState.ResizingNotes.Add(note);
            }

            // 记录原始长度和位置
            _resizeState.OriginalDurations.Clear();
            foreach (var n in _resizeState.ResizingNotes)
            {
                _resizeState.OriginalDurations[n] = n.Duration;
                n.PropertyChanged += OnResizingNotePropertyChanged;
            }

            Debug.WriteLine($"开始调整音符长度: Handle={handle}, 选中音符数={_resizeState.ResizingNotes.Count}");
            OnResizeStarted?.Invoke();
        }

        /// <summary>
        /// 更新调整大小
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
                        // 调整开始位置
                        var newStartTime = Math.Min(currentTime, endTime - _pianoRollViewModel.GridQuantization.ToTicks(_pianoRollViewModel.TicksPerBeat));
                        newStartTime = _pianoRollViewModel.SnapToGridTime(newStartTime);

                        var newDurationTicks = endTime - newStartTime;
                        newDuration = MusicalFraction.FromTicks(newDurationTicks, _pianoRollViewModel.TicksPerBeat);
                        newStartPosition = MusicalFraction.FromTicks(newStartTime, _pianoRollViewModel.TicksPerBeat);
                    }
                    else // EndEdge
                    {
                        // 调整结束位置
                        var newEndTime = Math.Max(currentTime, startTime + _pianoRollViewModel.GridQuantization.ToTicks(_pianoRollViewModel.TicksPerBeat));
                        newEndTime = _pianoRollViewModel.SnapToGridTime(newEndTime);

                        var newDurationTicks = newEndTime - startTime;
                        newDuration = MusicalFraction.FromTicks(newDurationTicks, _pianoRollViewModel.TicksPerBeat);
                    }

                    // 应用最小长度约束
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

                    // 只在长度或位置真正改变时更新
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
                Debug.WriteLine($"更新音符长度时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 完成调整大小
        /// </summary>
        public void EndResize()
        {
            if (_resizeState.IsResizing && _resizeState.ResizingNote != null && _pianoRollViewModel != null)
            {
                // 更新用户自定义长度
                _pianoRollViewModel.UserDefinedNoteDuration = _resizeState.ResizingNote.Duration;
                Debug.WriteLine($"调整结束，更新用户自定义长度: {_pianoRollViewModel.UserDefinedNoteDuration}");
            }

            // 取消属性变化监听
            foreach (var note in _resizeState.ResizingNotes)
            {
                note.PropertyChanged -= OnResizingNotePropertyChanged;
            }

            _resizeState.EndResize();
            OnResizeEnded?.Invoke();
        }

        /// <summary>
        /// 取消调整大小
        /// </summary>
        public void CancelResize()
        {
            if (_resizeState.IsResizing && _resizeState.ResizingNotes.Count > 0)
            {
                // 恢复原始长度
                foreach (var note in _resizeState.ResizingNotes)
                {
                    if (_resizeState.OriginalDurations.TryGetValue(note, out var originalDuration))
                    {
                        note.Duration = originalDuration;
                        note.InvalidateCache();
                    }
                    note.PropertyChanged -= OnResizingNotePropertyChanged;
                }

                Debug.WriteLine($"取消音符长度调整，恢复 {_resizeState.ResizingNotes.Count} 个音符的原始长度");
            }

            _resizeState.EndResize();
            OnResizeEnded?.Invoke();
        }

        private void OnResizingNotePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NoteViewModel.Duration) || e.PropertyName == nameof(NoteViewModel.StartPosition))
            {
                OnResizeUpdated?.Invoke();
            }
        }

        // 事件
        public event Action? OnResizeStarted;
        public event Action? OnResizeUpdated;
        public event Action? OnResizeEnded;

        // 只读属性
        public bool IsResizing => _resizeState.IsResizing;
        public ResizeHandle CurrentResizeHandle => _resizeState.CurrentResizeHandle;
        public NoteViewModel? ResizingNote => _resizeState.ResizingNote;
        public System.Collections.Generic.List<NoteViewModel> ResizingNotes => _resizeState.ResizingNotes;
    }
}