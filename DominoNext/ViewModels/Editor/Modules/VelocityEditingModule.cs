using Avalonia;
using DominoNext.Services.Interfaces;
using DominoNext.ViewModels.Editor.State;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoNext.ViewModels.Editor.Modules
{
    /// <summary>
    /// 力度编辑模块 - 处理音符力度的交互编辑
    /// </summary>
    public class VelocityEditingModule
    {
        private readonly ICoordinateService _coordinateService;
        private readonly VelocityEditingState _state;
        private PianoRollViewModel? _pianoRollViewModel;

        // 用于铅笔模式的音符处理记录
        private HashSet<NoteViewModel> _processedNotes = new();

        private double _canvasHeight = 100.0; // 缓存画布高度

        public event Action? OnVelocityUpdated;

        public VelocityEditingModule(ICoordinateService coordinateService)
        {
            _coordinateService = coordinateService;
            _state = new VelocityEditingState();
        }

        public void SetPianoRollViewModel(PianoRollViewModel viewModel)
        {
            _pianoRollViewModel = viewModel;
        }

        /// <summary>
        /// 设置画布高度 - 由VelocityViewCanvas调用
        /// </summary>
        public void SetCanvasHeight(double height)
        {
            _canvasHeight = height;
            System.Diagnostics.Debug.WriteLine($"Canvas height set to: {height}");
        }

        #region 公开属性

        public bool IsEditingVelocity => _state.IsEditing;
        public List<NoteViewModel>? EditingNotes => _state.EditingNotes;
        public List<Point> EditingPath => _state.EditingPath;
        public Point? CurrentEditPosition => _state.CurrentPosition;

        #endregion

        #region 力度编辑操作

        /// <summary>
        /// 开始力度编辑
        /// </summary>
        public void StartEditing(Point position)
        {
            if (_pianoRollViewModel == null) return;

            System.Diagnostics.Debug.WriteLine($"StartEditing called with position: {position}, tool: {_pianoRollViewModel.CurrentTool}");

            _state.StartEditing(position);
            _processedNotes.Clear(); // 清空已处理音符记录

            // 根据当前工具模式确定编辑目标
            switch (_pianoRollViewModel.CurrentTool)
            {
                case EditorTool.Select:
                    System.Diagnostics.Debug.WriteLine("Using Select tool mode");
                    // 选择工具：编辑选中的音符
                    StartSelectModeEditing(position);
                    break;
                    
                case EditorTool.Pencil:
                    System.Diagnostics.Debug.WriteLine("Using Pencil tool mode");
                    // 铅笔工具：手绘模式编辑
                    StartPencilModeEditing(position);
                    break;
                    
                default:
                    System.Diagnostics.Debug.WriteLine($"Unsupported tool: {_pianoRollViewModel.CurrentTool}");
                    break;
            }

            OnVelocityUpdated?.Invoke();
        }

        /// <summary>
        /// 更新力度编辑
        /// </summary>
        public void UpdateEditing(Point position)
        {
            if (!_state.IsEditing || _pianoRollViewModel == null) return;

            _state.UpdatePosition(position);

            switch (_pianoRollViewModel.CurrentTool)
            {
                case EditorTool.Select:
                    UpdateSelectModeEditing(position);
                    break;
                    
                case EditorTool.Pencil:
                    UpdatePencilModeEditing(position);
                    break;
            }

            OnVelocityUpdated?.Invoke();
        }

        /// <summary>
        /// 结束力度编辑
        /// </summary>
        public void EndEditing()
        {
            if (!_state.IsEditing) return;

            // 应用最终的力度更改
            ApplyVelocityChanges();
            
            _state.EndEditing();
            _processedNotes.Clear(); // 清空已处理音符记录
            OnVelocityUpdated?.Invoke();
        }

        /// <summary>
        /// 取消力度编辑
        /// </summary>
        public void CancelEditing()
        {
            if (!_state.IsEditing) return;

            // 恢复原始力度值
            RestoreOriginalVelocities();
            
            _state.EndEditing();
            _processedNotes.Clear(); // 清空已处理音符记录
            OnVelocityUpdated?.Invoke();
        }

        #endregion

        #region 选择工具模式

        private void StartSelectModeEditing(Point position)
        {
            if (_pianoRollViewModel == null) return;

            // 获取选中的音符
            var selectedNotes = _pianoRollViewModel.Notes.Where(n => n.IsSelected).ToList();
            
            if (!selectedNotes.Any())
            {
                // 如果没有选中音符，尝试选择点击位置的音符
                var clickedNote = FindNoteAtPosition(position);
                if (clickedNote != null)
                {
                    selectedNotes.Add(clickedNote);
                    clickedNote.IsSelected = true;
                }
            }

            if (selectedNotes.Any())
            {
                _state.SetEditingNotes(selectedNotes);
                _state.SaveOriginalVelocities(selectedNotes);
            }
        }

        private void UpdateSelectModeEditing(Point position)
        {
            if (_state.EditingNotes?.Any() != true) return;

            // 计算力度变化量
            var deltaY = position.Y - _state.StartPosition.Y;
            var velocityChange = CalculateVelocityChange(deltaY);

            // 应用力度变化到所有编辑中的音符
            foreach (var note in _state.EditingNotes)
            {
                if (_state.OriginalVelocities.TryGetValue(note, out var originalVelocity))
                {
                    var newVelocity = Math.Max(1, Math.Min(127, originalVelocity + velocityChange));
                    note.Velocity = newVelocity;
                }
            }
        }

        #endregion

        #region 铅笔工具模式 - 绝对值模式（简化版）

        private void StartPencilModeEditing(Point position)
        {
            if (_pianoRollViewModel == null) return;

            System.Diagnostics.Debug.WriteLine($"Starting pencil mode editing at {position}");
            
            // 清空已处理音符记录
            _processedNotes.Clear();
            
            // 处理当前位置的音符
            ProcessNotesAtPositionSimple(position);
            _state.AddToPath(position);
        }

        private void UpdatePencilModeEditing(Point position)
        {
            if (_pianoRollViewModel == null) return;

            _state.AddToPath(position);

            // 铅笔模式：处理当前位置的音符
            ProcessNotesAtPositionSimple(position);
        }

        /// <summary>
        /// 简化版音符处理 - 直接基于时间位置
        /// </summary>
        private void ProcessNotesAtPositionSimple(Point position)
        {
            if (_pianoRollViewModel == null) return;

            // 计算当前位置对应的绝对力度值
            var velocity = CalculateVelocityFromY(position.Y);
            var timeInTicks = _pianoRollViewModel.GetTimeFromX(position.X);
            
            System.Diagnostics.Debug.WriteLine($"Processing position {position}, velocity: {velocity}, time: {timeInTicks}");

            // 查找在当前时间位置附近的所有音符
            foreach (var note in _pianoRollViewModel.Notes)
            {
                var noteStartTime = note.StartPosition.ToTicks(_pianoRollViewModel.TicksPerBeat);
                var noteEndTime = noteStartTime + note.Duration.ToTicks(_pianoRollViewModel.TicksPerBeat);
                
                // 检查时间是否在音符范围内
                if (timeInTicks >= noteStartTime && timeInTicks <= noteEndTime)
                {
                    // 检查是否在音符开头附近（前25%的时间范围内）
                    var noteDuration = noteEndTime - noteStartTime;
                    var startThreshold = noteDuration * 0.25; // 音符开头25%的时间范围
                    
                    if (timeInTicks <= noteStartTime + startThreshold)
                    {
                        // 检查是否已经处理过这个音符
                        if (!_processedNotes.Contains(note))
                        {
                            System.Diagnostics.Debug.WriteLine($"Updating note velocity from {note.Velocity} to {velocity}");
                            
                            // 保存原始力度（如果还没保存的话）
                            if (_state.EditingNotes?.Contains(note) != true)
                            {
                                _state.AddEditingNote(note);
                                _state.SaveOriginalVelocity(note);
                            }
                            
                            // 直接设置力度值
                            note.Velocity = velocity;
                            _processedNotes.Add(note);
                        }
                    }
                }
            }
        }

        #endregion

        #region 辅助方法

        private NoteViewModel? FindNoteAtPosition(Point position)
        {
            if (_pianoRollViewModel == null) return null;

            return _pianoRollViewModel.GetNoteAtPosition(position);
        }

        private int CalculateVelocityChange(double deltaY)
        {
            // 将像素变化转换为力度变化
            // 假设100像素对应127的力度值
            return (int)Math.Round(-deltaY * 127.0 / 100.0);
        }

        private int CalculateVelocityFromY(double y)
        {
            // 使用VelocityBarRenderer的公开方法来计算力度值
            // 使用实际的画布高度
            return DominoNext.Renderers.VelocityBarRenderer.CalculateVelocityFromY(y, _canvasHeight);
        }

        private void ApplyVelocityChanges()
        {
            // 在这里可以添加撤销/重做支持
            // 当前实现直接应用更改
        }

        private void RestoreOriginalVelocities()
        {
            if (_state.EditingNotes == null) return;

            foreach (var note in _state.EditingNotes)
            {
                if (_state.OriginalVelocities.TryGetValue(note, out var originalVelocity))
                {
                    note.Velocity = originalVelocity;
                }
            }
        }

        #endregion
    }
}