using System;
using Avalonia;
using DominoNext.Services.Interfaces;
using DominoNext.Models.Music;
using System.Diagnostics;

namespace DominoNext.ViewModels.Editor.Modules
{
    /// <summary>
    /// 音符创建功能模块
    /// </summary>
    public class NoteCreationModule
    {
        private readonly ICoordinateService _coordinateService;
        private PianoRollViewModel? _pianoRollViewModel;

        // 创建状态
        public bool IsCreatingNote { get; private set; }
        public NoteViewModel? CreatingNote { get; private set; }
        public Point CreatingStartPosition { get; private set; }

        public NoteCreationModule(ICoordinateService coordinateService)
        {
            _coordinateService = coordinateService;
        }

        public void SetPianoRollViewModel(PianoRollViewModel viewModel)
        {
            _pianoRollViewModel = viewModel;
        }

        /// <summary>
        /// 开始创建音符
        /// </summary>
        public void StartCreating(Point position)
        {
            if (_pianoRollViewModel == null) return;

            var pitch = _pianoRollViewModel.GetPitchFromY(position.Y);
            var startTime = _pianoRollViewModel.GetTimeFromX(position.X);

            Debug.WriteLine("=== StartCreatingNote ===");
            Debug.WriteLine($"鼠标位置: {position}");
            Debug.WriteLine($"计算得到的pitch: {pitch}");
            Debug.WriteLine($"计算得到的startTime: {startTime}");

            if (IsValidNotePosition(pitch, startTime))
            {
                var quantizedStartTime = _pianoRollViewModel.SnapToGridTime(startTime);
                var quantizedPosition = MusicalFraction.FromTicks(quantizedStartTime, _pianoRollViewModel.TicksPerBeat);

                CreatingNote = new NoteViewModel
                {
                    Pitch = pitch,
                    StartPosition = quantizedPosition,
                    Duration = _pianoRollViewModel.UserDefinedNoteDuration,
                    Velocity = 100,
                    IsPreview = true
                };

                CreatingStartPosition = position;
                IsCreatingNote = true;

                Debug.WriteLine($"开始创建音符: Pitch={pitch}, Duration={CreatingNote.Duration}");
                OnCreationStarted?.Invoke();
            }
            else
            {
                Debug.WriteLine($"无效的音符位置: pitch={pitch}, startTime={startTime}");
            }
        }

        /// <summary>
        /// 更新创建中的音符长度
        /// </summary>
        public void UpdateCreating(Point currentPosition)
        {
            if (!IsCreatingNote || CreatingNote == null || _pianoRollViewModel == null) return;

            var currentTime = _pianoRollViewModel.GetTimeFromX(currentPosition.X);
            var startTime = CreatingNote.StartPosition.ToTicks(_pianoRollViewModel.TicksPerBeat);

            // 计算量化后的长度
            var minDuration = _pianoRollViewModel.GridQuantization.ToTicks(_pianoRollViewModel.TicksPerBeat);
            var actualDuration = Math.Max(minDuration, currentTime - startTime);

            if (actualDuration > 0)
            {
                var duration = MusicalFraction.CalculateQuantizedDuration(
                    startTime, startTime + actualDuration, _pianoRollViewModel.GridQuantization, _pianoRollViewModel.TicksPerBeat);

                // 只在长度真正改变时更新
                if (!CreatingNote.Duration.Equals(duration))
                {
                    Debug.WriteLine($"实时更新音符长度: {CreatingNote.Duration} -> {duration}");
                    CreatingNote.Duration = duration;
                    CreatingNote.InvalidateCache();

                    OnCreationUpdated?.Invoke();
                }
            }
        }

        /// <summary>
        /// 完成音符创建
        /// </summary>
        public void FinishCreating()
        {
            if (IsCreatingNote && CreatingNote != null && _pianoRollViewModel != null)
            {
                // 将预览音符转换为正式音符
                var finalNote = new NoteViewModel
                {
                    Pitch = CreatingNote.Pitch,
                    StartPosition = CreatingNote.StartPosition,
                    Duration = CreatingNote.Duration,
                    Velocity = CreatingNote.Velocity,
                    IsPreview = false
                };

                _pianoRollViewModel.Notes.Add(finalNote);

                // 记住用户拉长后的音符长度
                _pianoRollViewModel.UserDefinedNoteDuration = CreatingNote.Duration;

                Debug.WriteLine($"完成创建音符: {finalNote.Duration}，已更新用户自定义长度: {_pianoRollViewModel.UserDefinedNoteDuration}");
            }

            ClearCreating();
            OnCreationCompleted?.Invoke();
        }

        /// <summary>
        /// 取消音符创建
        /// </summary>
        public void CancelCreating()
        {
            if (IsCreatingNote)
            {
                Debug.WriteLine("取消音符创建");
            }

            ClearCreating();
            OnCreationCancelled?.Invoke();
        }

        private void ClearCreating()
        {
            IsCreatingNote = false;
            CreatingNote = null;
        }

        private bool IsValidNotePosition(int pitch, double startTime)
        {
            return pitch >= 0 && pitch <= 127 && startTime >= 0;
        }

        // 事件
        public event Action? OnCreationStarted;
        public event Action? OnCreationUpdated;
        public event Action? OnCreationCompleted;
        public event Action? OnCreationCancelled;
    }
}