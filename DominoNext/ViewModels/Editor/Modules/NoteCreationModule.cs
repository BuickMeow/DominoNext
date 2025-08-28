using System;
using Avalonia;
using DominoNext.Services.Interfaces;
using DominoNext.Models.Music;
using System.Diagnostics;

namespace DominoNext.ViewModels.Editor.Modules
{
    /// <summary>
    /// 音符创建功能模块 - 简化防手抖版本
    /// </summary>
    public class NoteCreationModule
    {
        private readonly ICoordinateService _coordinateService;
        private PianoRollViewModel? _pianoRollViewModel;

        // 创建状态
        public bool IsCreatingNote { get; private set; }
        public NoteViewModel? CreatingNote { get; private set; }
        public Point CreatingStartPosition { get; private set; }
        
        // 简化防手抖机制：只基于时间判断
        private DateTime _creationStartTime;
        
        // 可调整的防手抖时间阈值（毫秒）
        // 如果需要修改防手抖时间，请修改这个常量
        private const double ANTI_SHAKE_THRESHOLD_MS = 100.0;

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
                _creationStartTime = DateTime.Now;

                Debug.WriteLine($"开始创建音符: Pitch={pitch}, Duration={CreatingNote.Duration}");
                OnCreationStarted?.Invoke();
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

            // 计算音符的长度
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
        /// 完成创建音符 - 极简防手抖版本
        /// </summary>
        public void FinishCreating()
        {
            if (IsCreatingNote && CreatingNote != null && _pianoRollViewModel != null)
            {
                var holdTimeMs = (DateTime.Now - _creationStartTime).TotalMilliseconds;
                
                MusicalFraction finalDuration;

                // 极简判断：只基于按住时长
                if (holdTimeMs < ANTI_SHAKE_THRESHOLD_MS)
                {
                    // 短按：使用用户预设的时值
                    finalDuration = _pianoRollViewModel.UserDefinedNoteDuration;
                    Debug.WriteLine($"短按创建音符 ({holdTimeMs:F0}ms < {ANTI_SHAKE_THRESHOLD_MS}ms)，使用预设时值: {finalDuration}");
                }
                else
                {
                    // 长按：使用拖拽出的长度
                    finalDuration = CreatingNote.Duration;
                    Debug.WriteLine($"长按创建音符 ({holdTimeMs:F0}ms >= {ANTI_SHAKE_THRESHOLD_MS}ms)，使用拖拽时值: {finalDuration}");
                }

                // 创建最终音符
                var finalNote = new NoteViewModel
                {
                    Pitch = CreatingNote.Pitch,
                    StartPosition = CreatingNote.StartPosition,
                    Duration = finalDuration,
                    Velocity = CreatingNote.Velocity,
                    IsPreview = false
                };

                _pianoRollViewModel.Notes.Add(finalNote);

                // 只有长按拖拽时才更新用户预设
                if (holdTimeMs >= ANTI_SHAKE_THRESHOLD_MS)
                {
                    _pianoRollViewModel.UserDefinedNoteDuration = CreatingNote.Duration;
                    Debug.WriteLine($"更新用户自定义长度为: {_pianoRollViewModel.UserDefinedNoteDuration}");
                }

                Debug.WriteLine($"完成创建音符: {finalNote.Duration}");
            }

            ClearCreating();
            OnCreationCompleted?.Invoke();
        }

        /// <summary>
        /// 取消创建音符
        /// </summary>
        public void CancelCreating()
        {
            if (IsCreatingNote)
            {
                Debug.WriteLine("取消创建音符");
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