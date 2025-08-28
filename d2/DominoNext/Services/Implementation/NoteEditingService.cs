using System;
using System.Linq;
using System.Collections.Generic;
using Avalonia;
using DominoNext.Services.Interfaces;
using DominoNext.ViewModels.Editor;
using DominoNext.Models.Music;

namespace DominoNext.Services.Implementation
{
    public class NoteEditingService : INoteEditingService
    {
        private readonly PianoRollViewModel _viewModel;
        private readonly ICoordinateService _coordinateService;

        private NoteViewModel? _draggingNote;
        private Point _dragStartPoint;
        private MusicalFraction _originalStartPosition;
        private int _originalPitch;

        public NoteEditingService(PianoRollViewModel viewModel, ICoordinateService coordinateService)
        {
            _viewModel = viewModel;
            _coordinateService = coordinateService;
        }

        public void CreateNoteAtPosition(Point position)
        {
            var pitch = _coordinateService.GetPitchFromY(position.Y, _viewModel.KeyHeight);
            var startTime = _coordinateService.GetTimeFromX(position.X, _viewModel.Zoom, _viewModel.PixelsPerTick);

            if (IsValidNotePosition(pitch, startTime))
            {
                _viewModel.AddNote(pitch, startTime);
            }
        }

        public void StartNoteDrag(NoteViewModel note, Point startPosition)
        {
            _draggingNote = note;
            _dragStartPoint = startPosition;

            // 保存原始位置，避免累积误差
            _originalStartPosition = note.StartPosition;
            _originalPitch = note.Pitch;
        }

        public void UpdateNoteDrag(Point currentPosition)
        {
            if (_draggingNote == null) return;

            try
            {
                var deltaX = currentPosition.X - _dragStartPoint.X;
                var deltaY = currentPosition.Y - _dragStartPoint.Y;

                // 计算时间偏移（以tick为单位）
                var timeDeltaInTicks = deltaX / (_viewModel.PixelsPerTick * _viewModel.Zoom);
                var pitchDelta = -(int)(deltaY / _viewModel.KeyHeight);

                // 基于原始位置计算新位置，避免累积误差
                var originalTimeInTicks = _originalStartPosition.ToTicks(_viewModel.TicksPerBeat);
                var newTimeInTicks = Math.Max(0, originalTimeInTicks + timeDeltaInTicks);
                var newPitch = Math.Max(0, Math.Min(127, _originalPitch + pitchDelta));

                // 量化新位置
                var quantizedTimeInTicks = _viewModel.SnapToGridTime(newTimeInTicks);

                // 安全地转换为 MusicalFraction
                var newStartPosition = MusicalFraction.FromTicks(quantizedTimeInTicks, _viewModel.TicksPerBeat);

                // 更新音符位置
                _draggingNote.StartPosition = newStartPosition;
                _draggingNote.Pitch = newPitch;
            }
            catch (Exception ex)
            {
                // 记录错误并恢复到原始位置
                System.Diagnostics.Debug.WriteLine($"拖拽更新错误: {ex.Message}");

                if (_draggingNote != null)
                {
                    _draggingNote.StartPosition = _originalStartPosition;
                    _draggingNote.Pitch = _originalPitch;
                }
            }
        }

        public void EndNoteDrag()
        {
            _draggingNote = null;
        }

        public void SelectNotesInArea(Rect area)
        {
            foreach (var note in _viewModel.Notes)
            {
                var noteRect = _coordinateService.GetNoteRect(note, _viewModel.Zoom, _viewModel.PixelsPerTick, _viewModel.KeyHeight);
                note.IsSelected = area.Intersects(noteRect);
            }
        }

        public void ClearSelection()
        {
            foreach (var note in _viewModel.Notes)
            {
                note.IsSelected = false;
            }
        }

        public void DeleteSelectedNotes()
        {
            var notesToRemove = _viewModel.Notes.Where(n => n.IsSelected).ToList();
            foreach (var note in notesToRemove)
            {
                _viewModel.Notes.Remove(note);
            }
        }

        public void DuplicateSelectedNotes()
        {
            var selectedNotes = _viewModel.Notes.Where(n => n.IsSelected).ToList();

            // 先取消原有音符的选择
            foreach (var note in selectedNotes)
            {
                note.IsSelected = false;
            }

            // 创建复制的音符
            foreach (var note in selectedNotes)
            {
                var newNote = new NoteViewModel
                {
                    Pitch = note.Pitch,
                    StartPosition = note.StartPosition + note.Duration, // 放在原音符之后
                    Duration = note.Duration,
                    Velocity = note.Velocity,
                    IsSelected = true // 选中新创建的音符
                };
                _viewModel.Notes.Add(newNote);
            }
        }

        public void QuantizeSelectedNotes()
        {
            foreach (var note in _viewModel.Notes.Where(n => n.IsSelected))
            {
                try
                {
                    // 使用 MusicalFraction 进行量化
                    var currentTicks = note.StartPosition.ToTicks(_viewModel.TicksPerBeat);
                    var quantizedTicks = _viewModel.SnapToGridTime(currentTicks);
                    note.StartPosition = MusicalFraction.FromTicks(quantizedTicks, _viewModel.TicksPerBeat);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"量化音符错误: {ex.Message}");
                    // 发生错误时跳过该音符
                }
            }
        }

        public bool IsValidNotePosition(int pitch, double startTime)
        {
            return pitch >= 0 && pitch <= 127 && startTime >= 0 && !double.IsNaN(startTime) && !double.IsInfinity(startTime);
        }
    }
}