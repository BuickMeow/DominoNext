using Avalonia;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using DominoNext.Models.Music;
using DominoNext.Services.Interfaces;
using DominoNext.ViewModels;
using DominoNext.ViewModels.Editor;
using DominoNext.Views.Controls.Editing;
using System;
using System.Diagnostics;

public partial class EditorCommandsViewModel : ViewModelBase
{
    private readonly INoteEditingService _editingService;
    private readonly ICoordinateService _coordinateService;
    private PianoRollViewModel? _pianoRollViewModel;

    // 交互状态
    private bool _isDragging;
    private bool _isSelecting;
    private Point _dragStartPosition;
    private NoteViewModel? _draggedNote;

    // 性能优化：统一的更新节流系统
    private readonly System.Timers.Timer _updateTimer;
    private Point _pendingPosition;
    private bool _hasPendingUpdate;
    private UpdateType _pendingUpdateType;
    private const double UpdateInterval = 16; // 约60FPS限制

    // 更新类型枚举
    private enum UpdateType
    {
        Preview,
        Drag,
        Selection,
        CreatingNote,  // 新增：正在创建音符的更新类型
        Resizing       // 新增：音符长度调整的更新类型
    }

    public EditorCommandsViewModel(INoteEditingService editingService, ICoordinateService coordinateService)
    {
        _editingService = editingService;
        _coordinateService = coordinateService;

        // 初始化统一更新定时器
        _updateTimer = new System.Timers.Timer(UpdateInterval);
        _updateTimer.Elapsed += OnUpdateTimerElapsed;
        _updateTimer.AutoReset = false;
    }

    public void SetPianoRollViewModel(PianoRollViewModel pianoRollViewModel)
    {
        _pianoRollViewModel = pianoRollViewModel;
    }

    #region 统一交互处理Command

    [RelayCommand]
    private void HandleInteraction(EditorInteractionArgs args)
    {
        if (_pianoRollViewModel == null) return;

        // 移除或条件化Debug输出以提升性能
#if DEBUG
        if (args.InteractionType != EditorInteractionType.Move || _isDragging || _isSelecting || _pianoRollViewModel.IsResizing)
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
        var clickedNote = GetNoteAtPosition(args.Position);

        switch (args.Tool)
        {
            case EditorTool.Pencil:
                HandlePencilPress(args.Position, clickedNote, args.Modifiers);
                break;
            case EditorTool.Select:
                HandleSelectPress(args.Position, clickedNote, args.Modifiers);
                break;
            case EditorTool.Eraser:
                HandleEraserPress(clickedNote);
                break;
            case EditorTool.Cut:
                HandleCutPress(args.Position, clickedNote);
                break;
        }
    }

    private void HandleMove(EditorInteractionArgs args)
    {
        if (_pianoRollViewModel.IsResizing)
        {
            // 使用节流更新音符长度调整
            ScheduleUpdate(args.Position, UpdateType.Resizing);
        }
        else if (_isDragging)
        {
            // 使用节流更新拖拽
            ScheduleUpdate(args.Position, UpdateType.Drag);
        }
        else if (_isSelecting)
        {
            // 使用节流更新选择框
            ScheduleUpdate(args.Position, UpdateType.Selection);
        }
        else if (_pianoRollViewModel?.IsCreatingNote == true)
        {
            // 使用节流更新正在创建的音符长度 - 实现实时预览
            ScheduleUpdate(args.Position, UpdateType.CreatingNote);
        }
        else
        {
            // 优化：节流预览更新
            if (args.Tool == EditorTool.Pencil && _pianoRollViewModel != null)
            {
                ScheduleUpdate(args.Position, UpdateType.Preview);
            }
            else
            {
                ClearPreview();
            }
        }
    }

    private void HandleRelease(EditorInteractionArgs args)
    {
        if (_pianoRollViewModel.IsResizing)
        {
            _pianoRollViewModel.EndNoteResize();
        }
        else if (_isDragging)
        {
            EndDragInternal();
        }
        else if (_isSelecting)
        {
            EndSelectionInternal();
        }
        else if (_pianoRollViewModel?.IsCreatingNote == true)
        {
            // 完成拖拽创建音符 - 这会自动记住用户拉长的长度
            _pianoRollViewModel.FinishCreatingNote();
        }
    }

    #endregion

    #region 性能优化：统一更新节流系统

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
            // 在UI线程上执行更新
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (_hasPendingUpdate && _pianoRollViewModel != null)
                {
                    switch (_pendingUpdateType)
                    {
                        case UpdateType.Preview:
                            UpdatePreviewNoteInternal(_pendingPosition);
                            break;
                        case UpdateType.Drag:
                            UpdateDragInternal(_pendingPosition);
                            break;
                        case UpdateType.Selection:
                            UpdateSelectionInternal(_pendingPosition);
                            break;
                        case UpdateType.CreatingNote:
                            UpdateCreatingNoteInternal(_pendingPosition);
                            break;
                        case UpdateType.Resizing:
                            UpdateResizingInternal(_pendingPosition);
                            break;
                    }
                    _hasPendingUpdate = false;
                }
            });
        }
    }

    private void UpdatePreviewNoteInternal(Point position)
    {
        if (_pianoRollViewModel == null) return;

        var pitch = _coordinateService.GetPitchFromY(position.Y, _pianoRollViewModel.KeyHeight);
        var startTime = _coordinateService.GetTimeFromX(position.X, _pianoRollViewModel.Zoom, _pianoRollViewModel.PixelsPerTick);

        if (_editingService.IsValidNotePosition(pitch, startTime))
        {
            // 使用网格量化
            var quantizedTicks = _pianoRollViewModel.SnapToGridTime(startTime);
            var quantizedPosition = MusicalFraction.FromTicks(quantizedTicks, _pianoRollViewModel.TicksPerBeat);

            // 只在预览音符实际改变时才更新
            var currentPreview = _pianoRollViewModel.PreviewNote;
            if (currentPreview == null ||
                currentPreview.Pitch != pitch ||
                !currentPreview.StartPosition.Equals(quantizedPosition))
            {
                _pianoRollViewModel.PreviewNote = new NoteViewModel
                {
                    Pitch = pitch,
                    StartPosition = quantizedPosition,
                    Duration = _pianoRollViewModel.UserDefinedNoteDuration, // 使用用户自定义长度
                    Velocity = 100,
                    IsPreview = true
                };
            }
        }
        else
        {
            _pianoRollViewModel.PreviewNote = null;
        }
    }

    // 简化：直接委托给PianoRollViewModel处理
    private void UpdateCreatingNoteInternal(Point position)
    {
        _pianoRollViewModel?.UpdateCreatingNote(position);
    }

    private void UpdateResizingInternal(Point position)
    {
        _pianoRollViewModel?.UpdateNoteResize(position);
    }

    #endregion

    #region 工具特定处理方法

    private void HandlePencilPress(Point position, NoteViewModel? clickedNote, KeyModifiers modifiers)
    {
        if (clickedNote == null)
        {
            // 开始拖拽创建新音符，而不是立即创建
            Debug.WriteLine("铅笔工具: 开始拖拽创建新音符");
            _pianoRollViewModel?.StartCreatingNote(position);
        }
        else
        {
            // 检查是否点击在音符边缘以调整长度
            var resizeHandle = _pianoRollViewModel.GetResizeHandleAtPosition(position, clickedNote);

            if (resizeHandle != ResizeHandle.None)
            {
                // 开始音符长度调整
                Debug.WriteLine($"铅笔工具: 开始调整音符长度 - {resizeHandle}");

                // 如果音符未选中，先选中它
                if (!clickedNote.IsSelected)
                {
                    if (!modifiers.HasFlag(KeyModifiers.Control))
                    {
                        _editingService.ClearSelection();
                    }
                    clickedNote.IsSelected = true;
                }

                _pianoRollViewModel.StartNoteResize(position, clickedNote, resizeHandle);
            }
            else
            {
                // 选择并开始拖拽现有音符
                Debug.WriteLine("铅笔工具: 开始拖拽现有音符");
                if (!modifiers.HasFlag(KeyModifiers.Control))
                {
                    _editingService.ClearSelection();
                }
                clickedNote.IsSelected = true;
                StartDragInternal(clickedNote, position);
            }
        }
    }

    private void HandleSelectPress(Point position, NoteViewModel? clickedNote, KeyModifiers modifiers)
    {
        if (clickedNote != null)
        {
            // 选择音符并开始拖拽
            Debug.WriteLine("选择工具: 选择音符");
            if (!modifiers.HasFlag(KeyModifiers.Control))
            {
                _editingService.ClearSelection();
            }
            clickedNote.IsSelected = true;
            StartDragInternal(clickedNote, position);
        }
        else
        {
            // 开始框选
            Debug.WriteLine("选择工具: 开始框选");
            if (!modifiers.HasFlag(KeyModifiers.Control))
            {
                _editingService.ClearSelection();
            }
            StartSelectionInternal(position);
        }
    }

    private void HandleEraserPress(NoteViewModel? clickedNote)
    {
        if (clickedNote != null)
        {
            Debug.WriteLine("橡皮工具: 删除音符");
            _pianoRollViewModel?.Notes.Remove(clickedNote);
        }
    }

    private void HandleCutPress(Point position, NoteViewModel? clickedNote)
    {
        if (clickedNote != null)
        {
            Debug.WriteLine("切割工具: 功能待实现");
            // TODO: 实现音符切割功能
        }
    }

    #endregion

    #region 拖拽处理

    private void StartDragInternal(NoteViewModel note, Point position)
    {
        _isDragging = true;
        _draggedNote = note;
        _dragStartPosition = position;
        _editingService.StartNoteDrag(note, position);
        Debug.WriteLine($"开始拖拽音符: Pitch={note.Pitch}, StartTime={note.StartTime}");
    }

    private void UpdateDragInternal(Point position)
    {
        _editingService.UpdateNoteDrag(position);
    }

    private void EndDragInternal()
    {
        _isDragging = false;
        _draggedNote = null;
        _editingService.EndNoteDrag();

        // 确保取消任何待处理的拖拽更新
        if (_pendingUpdateType == UpdateType.Drag)
        {
            _hasPendingUpdate = false;
            _updateTimer.Stop();
        }

        Debug.WriteLine("结束拖拽");
    }

    #endregion

    #region 选择框处理

    private void StartSelectionInternal(Point position)
    {
        _isSelecting = true;
        if (_pianoRollViewModel != null)
        {
            _pianoRollViewModel.SelectionStart = position;
            _pianoRollViewModel.SelectionEnd = position;
        }
        Debug.WriteLine($"开始框选: {position}");
    }

    private void UpdateSelectionInternal(Point position)
    {
        if (_pianoRollViewModel != null)
        {
            _pianoRollViewModel.SelectionEnd = position;
        }
    }

    private void EndSelectionInternal()
    {
        _isSelecting = false;

        // 确保取消任何待处理的选择框更新
        if (_pendingUpdateType == UpdateType.Selection)
        {
            _hasPendingUpdate = false;
            _updateTimer.Stop();
        }

        if (_pianoRollViewModel?.SelectionStart != null && _pianoRollViewModel?.SelectionEnd != null)
        {
            var start = _pianoRollViewModel.SelectionStart.Value;
            var end = _pianoRollViewModel.SelectionEnd.Value;

            var selectionRect = new Rect(
                Math.Min(start.X, end.X),
                Math.Min(start.Y, end.Y),
                Math.Abs(end.X - start.X),
                Math.Abs(end.Y - start.Y)
            );

            _editingService.SelectNotesInArea(selectionRect);
        }

        // 清除选择框显示
        if (_pianoRollViewModel != null)
        {
            _pianoRollViewModel.SelectionStart = null;
            _pianoRollViewModel.SelectionEnd = null;
        }
        Debug.WriteLine("结束框选");
    }

    #endregion

    #region 键盘快捷键处理

    [RelayCommand]
    private void HandleKey(KeyCommandArgs args)
    {
        if (_pianoRollViewModel == null) return;

        switch (args.Key)
        {
            case Key.Delete:
                _editingService.DeleteSelectedNotes();
                break;

            case Key.A when args.Modifiers.HasFlag(KeyModifiers.Control):
                SelectAllNotes();
                break;

            case Key.D when args.Modifiers.HasFlag(KeyModifiers.Control):
                _editingService.DuplicateSelectedNotes();
                break;

            case Key.Q:
                _editingService.QuantizeSelectedNotes();
                break;

            // 工具快捷键
            case Key.D1:
            case Key.P:
                _pianoRollViewModel.CurrentTool = EditorTool.Pencil;
                break;

            case Key.D2:
            case Key.S:
                _pianoRollViewModel.CurrentTool = EditorTool.Select;
                break;

            case Key.D3:
            case Key.E:
                _pianoRollViewModel.CurrentTool = EditorTool.Eraser;
                break;

            case Key.D4:
            case Key.C:
                _pianoRollViewModel.CurrentTool = EditorTool.Cut;
                break;

            // 支持ESC键取消正在进行的操作
            case Key.Escape:
                if (_pianoRollViewModel.IsCreatingNote)
                {
                    _pianoRollViewModel.CancelCreatingNote();
                }
                else if (_pianoRollViewModel.IsResizing)
                {
                    _pianoRollViewModel.CancelNoteResize();
                }
                break;
        }
    }

    private void SelectAllNotes()
    {
        if (_pianoRollViewModel?.Notes != null)
        {
            foreach (var note in _pianoRollViewModel.Notes)
            {
                note.IsSelected = true;
            }
        }
    }

    #endregion

    #region 预览处理 - 保留原有方法以兼容性

    // 保留原有的UpdatePreviewNote方法，但现在通过节流调用
    private void UpdatePreviewNote(Point position)
    {
        ScheduleUpdate(position, UpdateType.Preview);
    }

    [RelayCommand]
    private void ClearPreview()
    {
        if (_pianoRollViewModel != null)
        {
            _pianoRollViewModel.PreviewNote = null;
        }

        // 取消待处理的预览更新
        if (_pendingUpdateType == UpdateType.Preview)
        {
            _hasPendingUpdate = false;
            _updateTimer.Stop();
        }
    }

    #endregion

    #region 辅助方法

    private NoteViewModel? GetNoteAtPosition(Point position)
    {
        if (_pianoRollViewModel?.Notes == null) return null;

        foreach (var note in _pianoRollViewModel.Notes)
        {
            var noteRect = _coordinateService.GetNoteRect(note,
                _pianoRollViewModel.Zoom,
                _pianoRollViewModel.PixelsPerTick,
                _pianoRollViewModel.KeyHeight);

            if (noteRect.Contains(position))
            {
                return note;
            }
        }
        return null;
    }

    #endregion

    #region 原有Commands保持兼容性

    [RelayCommand]
    private void CreateNote(Point position)
    {
        _editingService.CreateNoteAtPosition(position);
    }

    [RelayCommand]
    private void StartDrag(object parameter)
    {
        if (parameter is (NoteViewModel note, Point position))
        {
            _editingService.StartNoteDrag(note, position);
        }
    }

    [RelayCommand]
    private void UpdateDrag(Point position)
    {
        _editingService.UpdateNoteDrag(position);
    }

    [RelayCommand]
    private void EndDrag()
    {
        _editingService.EndNoteDrag();
    }

    [RelayCommand]
    private void SelectArea(Rect area)
    {
        _editingService.SelectNotesInArea(area);
    }

    [RelayCommand]
    private void ClearSelection()
    {
        _editingService.ClearSelection();
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        _editingService.DeleteSelectedNotes();
    }

    [RelayCommand]
    private void DuplicateSelected()
    {
        _editingService.DuplicateSelectedNotes();
    }

    [RelayCommand]
    private void QuantizeSelected()
    {
        _editingService.QuantizeSelectedNotes();
    }

    #endregion

    #region 资源清理

    public void Dispose()
    {
        _updateTimer?.Dispose();
    }

    #endregion
}