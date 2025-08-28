using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DominoNext.Services.Interfaces;
using DominoNext.ViewModels.Editor.Commands;
using DominoNext.Views.Controls.Editing;

namespace DominoNext.ViewModels.Editor.Commands
{
    /// <summary>
    /// 重构后的编辑器命令ViewModel - 简化并委托给模块
    /// </summary>
    public partial class EditorCommandsViewModelRefactored : ViewModelBase
    {
        #region 依赖服务
        private readonly INoteEditingService _editingService;
        private readonly ICoordinateService _coordinateService;
        private PianoRollViewModel? _pianoRollViewModel;
        #endregion

        #region 命令处理器
        private readonly PencilToolHandler _pencilToolHandler;
        private readonly SelectToolHandler _selectToolHandler;
        private readonly EraserToolHandler _eraserToolHandler;
        private readonly KeyboardCommandHandler _keyboardCommandHandler;
        #endregion

        #region 性能优化
        private readonly System.Timers.Timer _updateTimer;
        private Point _pendingPosition;
        private bool _hasPendingUpdate;
        private UpdateType _pendingUpdateType;
        private const double UpdateInterval = 16; // 约60FPS限制

        private enum UpdateType { Preview, Drag, Selection, CreatingNote, Resizing }
        #endregion

        #region 构造函数
        public EditorCommandsViewModelRefactored(INoteEditingService editingService, ICoordinateService coordinateService)
        {
            _editingService = editingService;
            _coordinateService = coordinateService;

            // 初始化命令处理器
            _pencilToolHandler = new PencilToolHandler();
            _selectToolHandler = new SelectToolHandler();
            _eraserToolHandler = new EraserToolHandler();
            _keyboardCommandHandler = new KeyboardCommandHandler();

            // 初始化性能优化
            _updateTimer = new System.Timers.Timer(UpdateInterval);
            _updateTimer.Elapsed += OnUpdateTimerElapsed;
            _updateTimer.AutoReset = false;
        }

        public void SetPianoRollViewModel(PianoRollViewModel pianoRollViewModel)
        {
            _pianoRollViewModel = pianoRollViewModel;
            
            // 设置处理器的ViewModel引用
            _pencilToolHandler.SetPianoRollViewModel(pianoRollViewModel);
            _selectToolHandler.SetPianoRollViewModel(pianoRollViewModel);
            _eraserToolHandler.SetPianoRollViewModel(pianoRollViewModel);
            _keyboardCommandHandler.SetPianoRollViewModel(pianoRollViewModel);
        }
        #endregion

        #region 主要交互处理
        [RelayCommand]
        private void HandleInteraction(EditorInteractionArgs args)
        {
            if (_pianoRollViewModel == null) return;

            #if DEBUG
            if (args.InteractionType != EditorInteractionType.Move || 
                _pianoRollViewModel.IsDragging || _pianoRollViewModel.IsResizing)
            {
                Debug.WriteLine($"处理交互: {args.InteractionType}, 工具: {args.Tool}, 位置: {args.Position}");
            }
            #endif

            switch (args.InteractionType)
            {
                case EditorInteractionType.Press:
                    HandlePress(args);
                    break;
                case EditorInteractionType.Move:
                    HandleMove(args);
                    break;
                case EditorInteractionType.Release:
                    HandleRelease(args);
                    break;
            }
        }

        private void HandlePress(EditorInteractionArgs args)
        {
            var clickedNote = _pianoRollViewModel?.GetNoteAtPosition(args.Position);

            switch (args.Tool)
            {
                case EditorTool.Pencil:
                    _pencilToolHandler.HandlePress(args.Position, clickedNote, args.Modifiers);
                    break;
                case EditorTool.Select:
                    _selectToolHandler.HandlePress(args.Position, clickedNote, args.Modifiers);
                    break;
                case EditorTool.Eraser:
                    _eraserToolHandler.HandlePress(clickedNote);
                    break;
                case EditorTool.Cut:
                    // TODO: 实现切割工具
                    break;
            }
        }

        private void HandleMove(EditorInteractionArgs args)
        {
            if (_pianoRollViewModel == null) return;

            if (_pianoRollViewModel.IsResizing)
            {
                ScheduleUpdate(args.Position, UpdateType.Resizing);
            }
            else if (_pianoRollViewModel.IsDragging)
            {
                ScheduleUpdate(args.Position, UpdateType.Drag);
            }
            else if (_pianoRollViewModel.SelectionState.IsSelecting)
            {
                ScheduleUpdate(args.Position, UpdateType.Selection);
            }
            else if (_pianoRollViewModel.IsCreatingNote)
            {
                ScheduleUpdate(args.Position, UpdateType.CreatingNote);
            }
            else if (args.Tool == EditorTool.Pencil)
            {
                ScheduleUpdate(args.Position, UpdateType.Preview);
            }
            else
            {
                _pianoRollViewModel.PreviewModule.ClearPreview();
            }
        }

        private void HandleRelease(EditorInteractionArgs args)
        {
            if (_pianoRollViewModel == null) return;

            if (_pianoRollViewModel.IsResizing)
            {
                _pianoRollViewModel.ResizeModule.EndResize();
            }
            else if (_pianoRollViewModel.IsDragging)
            {
                _pianoRollViewModel.DragModule.EndDrag();
            }
            else if (_pianoRollViewModel.SelectionState.IsSelecting)
            {
                _pianoRollViewModel.SelectionModule.EndSelection(_pianoRollViewModel.Notes);
            }
            else if (_pianoRollViewModel.IsCreatingNote)
            {
                _pianoRollViewModel.CreationModule.FinishCreating();
            }
        }
        #endregion

        #region 键盘命令处理
        [RelayCommand]
        private void HandleKey(KeyCommandArgs args)
        {
            _keyboardCommandHandler.HandleKey(args);
        }
        #endregion

        #region 性能优化 - 统一更新节流系统
        private void ScheduleUpdate(Point position, UpdateType updateType)
        {
            _pendingPosition = position;
            _pendingUpdateType = updateType;
            _hasPendingUpdate = true;

            if (!_updateTimer.Enabled)
            {
                _updateTimer.Start();
            }
        }

        private void OnUpdateTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_hasPendingUpdate && _pianoRollViewModel != null)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    if (_hasPendingUpdate && _pianoRollViewModel != null)
                    {
                        switch (_pendingUpdateType)
                        {
                            case UpdateType.Preview:
                                _pianoRollViewModel.PreviewModule.UpdatePreview(_pendingPosition);
                                break;
                            case UpdateType.Drag:
                                _pianoRollViewModel.DragModule.UpdateDrag(_pendingPosition);
                                break;
                            case UpdateType.Selection:
                                _pianoRollViewModel.SelectionModule.UpdateSelection(_pendingPosition);
                                break;
                            case UpdateType.CreatingNote:
                                _pianoRollViewModel.CreationModule.UpdateCreating(_pendingPosition);
                                break;
                            case UpdateType.Resizing:
                                _pianoRollViewModel.ResizeModule.UpdateResize(_pendingPosition);
                                break;
                        }
                        _hasPendingUpdate = false;
                    }
                });
            }
        }
        #endregion

        #region 预览清除
        [RelayCommand]
        private void ClearPreview()
        {
            _pianoRollViewModel?.PreviewModule.ClearPreview();

            if (_pendingUpdateType == UpdateType.Preview)
            {
                _hasPendingUpdate = false;
                _updateTimer.Stop();
            }
        }
        #endregion

        #region 资源清理
        public void Dispose()
        {
            _updateTimer?.Dispose();
        }
        #endregion
    }
}