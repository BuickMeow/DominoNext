using System;
using Avalonia;
using DominoNext.Services.Interfaces;
using DominoNext.Models.Music;
using System.Diagnostics;

namespace DominoNext.ViewModels.Editor.Modules
{
    /// <summary>
    /// ����Ԥ������ģ��
    /// </summary>
    public class NotePreviewModule : IRequiresPianoRollViewModel
    {
        private readonly ICoordinateService _coordinateService;
        private PianoRollViewModel? _pianoRollViewModel;

        public NoteViewModel? PreviewNote { get; private set; }

        public NotePreviewModule(ICoordinateService coordinateService)
        {
            _coordinateService = coordinateService;
        }

        public void SetPianoRollViewModel(PianoRollViewModel viewModel)
        {
            _pianoRollViewModel = viewModel;
        }

        /// <summary>
        /// ����Ԥ������
        /// </summary>
        public void UpdatePreview(Point position)
        {
            if (_pianoRollViewModel == null) return;

            // ������ڴ�������������ʾ��ͨԤ��
            if (_pianoRollViewModel.IsCreatingNote)
            {
                ClearPreview();
                return;
            }

            // ������ڵ���������С������ʾ��ͨԤ��
            if (_pianoRollViewModel.IsResizing)
            {
                ClearPreview();
                return;
            }

            if (_pianoRollViewModel.CurrentTool != EditorTool.Pencil)
            {
                ClearPreview();
                return;
            }

            // ����Ƿ���ͣ�ڿɵ�����С��������Ե��
            var note = _pianoRollViewModel.GetNoteAtPosition(position);
            if (note != null)
            {
                var handle = _pianoRollViewModel.GetResizeHandleAtPosition(position, note);
                if (handle == ResizeHandle.StartEdge || handle == ResizeHandle.EndEdge)
                {
                    // ��ͣ�ڵ�����Ե�ϣ�����ʾԤ������
                    ClearPreview();
                    return;
                }
            }

            var pitch = _pianoRollViewModel.GetPitchFromY(position.Y);
            var startTime = _pianoRollViewModel.GetTimeFromX(position.X);

            if (IsValidNotePosition(pitch, startTime))
            {
                var quantizedStartTime = _pianoRollViewModel.SnapToGridTime(startTime);
                var quantizedPosition = MusicalFraction.FromTicks(quantizedStartTime, _pianoRollViewModel.TicksPerBeat);

                // ֻ��Ԥ������ʵ�ʸı�ʱ�Ÿ���
                if (PreviewNote == null ||
                    PreviewNote.Pitch != pitch ||
                    !PreviewNote.StartPosition.Equals(quantizedPosition))
                {
                    PreviewNote = new NoteViewModel
                    {
                        Pitch = pitch,
                        StartPosition = quantizedPosition,
                        Duration = _pianoRollViewModel.UserDefinedNoteDuration,
                        Velocity = 100,
                        IsPreview = true
                    };

                    OnPreviewUpdated?.Invoke();
                }
            }
            else
            {
                ClearPreview();
            }
        }

        /// <summary>
        /// ���Ԥ������
        /// </summary>
        public void ClearPreview()
        {
            if (PreviewNote != null)
            {
                PreviewNote = null;
                OnPreviewUpdated?.Invoke();
            }
        }

        private bool IsValidNotePosition(int pitch, double startTime)
        {
            return pitch >= 0 && pitch <= 127 && startTime >= 0;
        }

        // �¼�
        public event Action? OnPreviewUpdated;
    }
}