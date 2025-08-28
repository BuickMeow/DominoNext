using System;
using Avalonia;
using DominoNext.Services.Interfaces;
using DominoNext.Models.Music;
using System.Diagnostics;

namespace DominoNext.ViewModels.Editor.Modules
{
    /// <summary>
    /// ������������ģ�� - �򻯷��ֶ��汾
    /// </summary>
    public class NoteCreationModule
    {
        private readonly ICoordinateService _coordinateService;
        private PianoRollViewModel? _pianoRollViewModel;

        // ����״̬
        public bool IsCreatingNote { get; private set; }
        public NoteViewModel? CreatingNote { get; private set; }
        public Point CreatingStartPosition { get; private set; }
        
        // �򻯷��ֶ����ƣ�ֻ����ʱ���ж�
        private DateTime _creationStartTime;
        
        // �ɵ����ķ��ֶ�ʱ����ֵ�����룩
        // �����Ҫ�޸ķ��ֶ�ʱ�䣬���޸��������
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
        /// ��ʼ��������
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

                Debug.WriteLine($"��ʼ��������: Pitch={pitch}, Duration={CreatingNote.Duration}");
                OnCreationStarted?.Invoke();
            }
        }

        /// <summary>
        /// ���´����е���������
        /// </summary>
        public void UpdateCreating(Point currentPosition)
        {
            if (!IsCreatingNote || CreatingNote == null || _pianoRollViewModel == null) return;

            var currentTime = _pianoRollViewModel.GetTimeFromX(currentPosition.X);
            var startTime = CreatingNote.StartPosition.ToTicks(_pianoRollViewModel.TicksPerBeat);

            // ���������ĳ���
            var minDuration = _pianoRollViewModel.GridQuantization.ToTicks(_pianoRollViewModel.TicksPerBeat);
            var actualDuration = Math.Max(minDuration, currentTime - startTime);

            if (actualDuration > 0)
            {
                var duration = MusicalFraction.CalculateQuantizedDuration(
                    startTime, startTime + actualDuration, _pianoRollViewModel.GridQuantization, _pianoRollViewModel.TicksPerBeat);

                // ֻ�ڳ��������ı�ʱ����
                if (!CreatingNote.Duration.Equals(duration))
                {
                    Debug.WriteLine($"ʵʱ������������: {CreatingNote.Duration} -> {duration}");
                    CreatingNote.Duration = duration;
                    CreatingNote.InvalidateCache();

                    OnCreationUpdated?.Invoke();
                }
            }
        }

        /// <summary>
        /// ��ɴ������� - ������ֶ��汾
        /// </summary>
        public void FinishCreating()
        {
            if (IsCreatingNote && CreatingNote != null && _pianoRollViewModel != null)
            {
                var holdTimeMs = (DateTime.Now - _creationStartTime).TotalMilliseconds;
                
                MusicalFraction finalDuration;

                // �����жϣ�ֻ���ڰ�סʱ��
                if (holdTimeMs < ANTI_SHAKE_THRESHOLD_MS)
                {
                    // �̰���ʹ���û�Ԥ���ʱֵ
                    finalDuration = _pianoRollViewModel.UserDefinedNoteDuration;
                    Debug.WriteLine($"�̰��������� ({holdTimeMs:F0}ms < {ANTI_SHAKE_THRESHOLD_MS}ms)��ʹ��Ԥ��ʱֵ: {finalDuration}");
                }
                else
                {
                    // ������ʹ����ק���ĳ���
                    finalDuration = CreatingNote.Duration;
                    Debug.WriteLine($"������������ ({holdTimeMs:F0}ms >= {ANTI_SHAKE_THRESHOLD_MS}ms)��ʹ����קʱֵ: {finalDuration}");
                }

                // ������������
                var finalNote = new NoteViewModel
                {
                    Pitch = CreatingNote.Pitch,
                    StartPosition = CreatingNote.StartPosition,
                    Duration = finalDuration,
                    Velocity = CreatingNote.Velocity,
                    IsPreview = false
                };

                _pianoRollViewModel.Notes.Add(finalNote);

                // ֻ�г�����קʱ�Ÿ����û�Ԥ��
                if (holdTimeMs >= ANTI_SHAKE_THRESHOLD_MS)
                {
                    _pianoRollViewModel.UserDefinedNoteDuration = CreatingNote.Duration;
                    Debug.WriteLine($"�����û��Զ��峤��Ϊ: {_pianoRollViewModel.UserDefinedNoteDuration}");
                }

                Debug.WriteLine($"��ɴ�������: {finalNote.Duration}");
            }

            ClearCreating();
            OnCreationCompleted?.Invoke();
        }

        /// <summary>
        /// ȡ����������
        /// </summary>
        public void CancelCreating()
        {
            if (IsCreatingNote)
            {
                Debug.WriteLine("ȡ����������");
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

        // �¼�
        public event Action? OnCreationStarted;
        public event Action? OnCreationUpdated;
        public event Action? OnCreationCompleted;
        public event Action? OnCreationCancelled;
    }
}