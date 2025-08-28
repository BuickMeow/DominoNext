using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DominoNext.Models.Music;
using DominoNext.Services.Interfaces;
using DominoNext.ViewModels.Editor.Commands;
using DominoNext.ViewModels.Editor.Modules;
using DominoNext.ViewModels.Editor.State;
using DominoNext.ViewModels.Editor.Models;

namespace DominoNext.ViewModels.Editor
{
    /// <summary>
    /// 重构后的钢琴卷帘ViewModel - 符合MVVM最佳实践
    /// 主要负责协调各个模块，业务逻辑委托给专门的模块处理
    /// </summary>
    public partial class PianoRollViewModel : ViewModelBase
    {
        #region 服务依赖
        private readonly ICoordinateService _coordinateService;
        #endregion

        #region 核心模块
        public NoteDragModule DragModule { get; }
        public NoteResizeModule ResizeModule { get; }
        public NoteCreationModule CreationModule { get; }
        public NoteSelectionModule SelectionModule { get; }
        public NotePreviewModule PreviewModule { get; }
        #endregion

        #region 状态管理
        public DragState DragState { get; }
        public ResizeState ResizeState { get; }
        public SelectionState SelectionState { get; }
        #endregion

        #region 基本属性
        [ObservableProperty] private double _zoom = 1.0;
        [ObservableProperty] private double _verticalZoom = 1.0;
        [ObservableProperty] private double _timelinePosition;
        [ObservableProperty] private double _zoomSliderValue = 50.0;
        [ObservableProperty] private double _verticalZoomSliderValue = 50.0;
        [ObservableProperty] private EditorTool _currentTool = EditorTool.Pencil;
        [ObservableProperty] private MusicalFraction _gridQuantization = MusicalFraction.SixteenthNote;
        [ObservableProperty] private MusicalFraction _userDefinedNoteDuration = MusicalFraction.QuarterNote;
        [ObservableProperty] private EditorCommandsViewModel _editorCommands;

        // UI相关属性
        [ObservableProperty] private bool _isNoteDurationDropDownOpen = false;
        [ObservableProperty] private string _customFractionInput = "1/4";
        #endregion

        #region 集合
        public ObservableCollection<NoteViewModel> Notes { get; } = new();
        public ObservableCollection<NoteDurationOption> NoteDurationOptions { get; } = new(); // 网格量化选项
        #endregion

        #region 计算属性
        public int TicksPerBeat => MusicalFraction.QUARTER_NOTE_TICKS;
        public double PixelsPerTick => 100.0 / TicksPerBeat;
        public double KeyHeight => 12.0 * VerticalZoom;
        public double MeasureWidth => (4 * TicksPerBeat) * PixelsPerTick * Zoom;
        public double BeatWidth => TicksPerBeat * PixelsPerTick * Zoom;

        // 新增：音符宽度计算
        public double EighthNoteWidth => (TicksPerBeat / 2) * PixelsPerTick * Zoom;
        public double SixteenthNoteWidth => (TicksPerBeat / 4) * PixelsPerTick * Zoom;

        // 新增：小节相关
        public int BeatsPerMeasure => 4; // 标准4/4拍

        // UI相关计算属性
        public string CurrentNoteDurationText => GridQuantization.ToString(); // 显示当前网格量化而不是音符时值
        public string CurrentNoteTimeValueText => UserDefinedNoteDuration.ToString(); // 显示当前音符时值
        public double TotalHeight => 128 * KeyHeight; // 128个MIDI音符
        #endregion

        #region 代理属性 - 简化访问
        #region 便捷属性 - 简化访问
        // 拖拽相关
        public bool IsDragging => DragState.IsDragging;
        public NoteViewModel? DraggingNote => DragState.DraggingNote;
        public List<NoteViewModel> DraggingNotes => DragState.DraggingNotes;

        // 调整大小相关
        public bool IsResizing => ResizeState.IsResizing;
        public ResizeHandle CurrentResizeHandle => ResizeState.CurrentResizeHandle;
        public NoteViewModel? ResizingNote => ResizeState.ResizingNote;
        public List<NoteViewModel> ResizingNotes => ResizeState.ResizingNotes;

        // 创建音符
        public bool IsCreatingNote => CreationModule.IsCreatingNote;
        public NoteViewModel? CreatingNote => CreationModule.CreatingNote;

        // 选择框
        public bool IsSelecting => SelectionState.IsSelecting;
        public Point? SelectionStart => SelectionState.SelectionStart;
        public Point? SelectionEnd => SelectionState.SelectionEnd;

        // 预览音符
        public NoteViewModel? PreviewNote => PreviewModule.PreviewNote;
        #endregion
        #endregion

        #region 构造函数
        public PianoRollViewModel() : this(null) { }

        public PianoRollViewModel(ICoordinateService? coordinateService)
        {
            _coordinateService = coordinateService ?? new DominoNext.Services.Implementation.CoordinateService();

            // 初始化状态
            DragState = new DragState();
            ResizeState = new ResizeState();
            SelectionState = new SelectionState();

            // 初始化模块
            DragModule = new NoteDragModule(DragState, _coordinateService);
            ResizeModule = new NoteResizeModule(ResizeState, _coordinateService);
            CreationModule = new NoteCreationModule(_coordinateService);
            SelectionModule = new NoteSelectionModule(SelectionState, _coordinateService);
            PreviewModule = new NotePreviewModule(_coordinateService);

            // 设置模块引用
            DragModule.SetPianoRollViewModel(this);
            ResizeModule.SetPianoRollViewModel(this);
            CreationModule.SetPianoRollViewModel(this);
            SelectionModule.SetPianoRollViewModel(this);
            PreviewModule.SetPianoRollViewModel(this);

            // 简化初始化命令
            _editorCommands = new EditorCommandsViewModel(_coordinateService);
            _editorCommands.SetPianoRollViewModel(this);

            // 订阅模块事件
            SubscribeToModuleEvents();

            // 初始化选项
            InitializeNoteDurationOptions();
        }
        #endregion

        #region 模块事件订阅
        private void SubscribeToModuleEvents()
        {
            // 拖拽模块事件（避免nameof冲突）
            DragModule.OnDragUpdated += InvalidateVisual;
            DragModule.OnDragEnded += InvalidateVisual;

            ResizeModule.OnResizeUpdated += InvalidateVisual;
            ResizeModule.OnResizeEnded += InvalidateVisual;

            CreationModule.OnCreationUpdated += InvalidateVisual;
            CreationModule.OnCreationCompleted += OnNoteCreated; // 订阅音符创建完成事件

            // 选择模块事件
            SelectionModule.OnSelectionUpdated += InvalidateVisual;

            // 订阅选择状态变更事件
            SelectionState.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(SelectionState.SelectionStart) ||
                    e.PropertyName == nameof(SelectionState.SelectionEnd) ||
                    e.PropertyName == nameof(SelectionState.IsSelecting))
                {
                    // 当选择框状态变化时，通知UI更新
                    OnPropertyChanged(nameof(SelectionStart));
                    OnPropertyChanged(nameof(SelectionEnd));
                    OnPropertyChanged(nameof(IsSelecting));
                    InvalidateVisual();
                }
            };
        }

        private void InvalidateVisual()
        {
            // 触发UI更新的方法，由View层实现
        }

        /// <summary>
        /// 音符创建完成后，同步更新用户定义的音符时值
        /// </summary>
        private void OnNoteCreated()
        {
            InvalidateVisual();
            
            // 同步最新创建音符的时值到UI显示
            if (Notes.Count > 0)
            {
                var lastNote = Notes.Last();
                if (!lastNote.Duration.Equals(UserDefinedNoteDuration))
                {
                    UserDefinedNoteDuration = lastNote.Duration;
                    OnPropertyChanged(nameof(CurrentNoteTimeValueText));
                }
            }
        }
        #endregion

        #region 初始化方法
        private void InitializeNoteDurationOptions()
        {
            // 网格量化选项 - 控制音符可以放置在多细的网格上
            NoteDurationOptions.Add(new NoteDurationOption("全音符网格 (1/1)", MusicalFraction.WholeNote, "𝅝"));
            NoteDurationOptions.Add(new NoteDurationOption("二分音符网格 (1/2)", MusicalFraction.HalfNote, "𝅗𝅥"));
            NoteDurationOptions.Add(new NoteDurationOption("三连二分音符网格 (1/3)", MusicalFraction.TripletHalf, "𝅗𝅥"));
            NoteDurationOptions.Add(new NoteDurationOption("四分音符网格 (1/4)", MusicalFraction.QuarterNote, "𝅘𝅥"));
            NoteDurationOptions.Add(new NoteDurationOption("三连四分音符网格 (1/6)", MusicalFraction.TripletQuarter, "𝅘𝅥"));
            NoteDurationOptions.Add(new NoteDurationOption("八分音符网格 (1/8)", MusicalFraction.EighthNote, "𝅘𝅥𝅮"));
            NoteDurationOptions.Add(new NoteDurationOption("三连八分音符网格 (1/12)", MusicalFraction.TripletEighth, "𝅘𝅥𝅮"));
            NoteDurationOptions.Add(new NoteDurationOption("十六分音符网格 (1/16)", MusicalFraction.SixteenthNote, "𝅘𝅥𝅯"));
            NoteDurationOptions.Add(new NoteDurationOption("三连十六分音符网格 (1/24)", MusicalFraction.TripletSixteenth, "𝅘𝅥𝅯"));
            NoteDurationOptions.Add(new NoteDurationOption("三十二分音符网格 (1/32)", MusicalFraction.ThirtySecondNote, "𝅘𝅥𝅰"));
            NoteDurationOptions.Add(new NoteDurationOption("三连三十二分音符网格 (1/48)", new MusicalFraction(1, 48), "𝅘𝅥𝅰"));
            NoteDurationOptions.Add(new NoteDurationOption("六十四分音符网格 (1/64)", new MusicalFraction(1, 64), "𝅘𝅥𝅱"));
        }
        #endregion

        #region 坐标转换委托
        public int GetPitchFromY(double y) => _coordinateService.GetPitchFromY(y, KeyHeight);
        public double GetTimeFromX(double x) => _coordinateService.GetTimeFromX(x, Zoom, PixelsPerTick);
        public Point GetPositionFromNote(NoteViewModel note) => _coordinateService.GetPositionFromNote(note, Zoom, PixelsPerTick, KeyHeight);
        public Rect GetNoteRect(NoteViewModel note) => _coordinateService.GetNoteRect(note, Zoom, PixelsPerTick, KeyHeight);
        #endregion

        #region 公共方法委托给模块
        public void StartCreatingNote(Point position) => CreationModule.StartCreating(position);
        public void UpdateCreatingNote(Point position) => CreationModule.UpdateCreating(position);
        public void FinishCreatingNote() => CreationModule.FinishCreating();
        public void CancelCreatingNote() => CreationModule.CancelCreating();

        public void StartNoteDrag(NoteViewModel note, Point startPoint) => DragModule.StartDrag(note, startPoint);
        public void UpdateNoteDrag(Point currentPoint, Point startPoint) => DragModule.UpdateDrag(currentPoint);
        public void EndNoteDrag() => DragModule.EndDrag();

        public void StartNoteResize(Point position, NoteViewModel note, ResizeHandle handle) => ResizeModule.StartResize(position, note, handle);
        public void UpdateNoteResize(Point currentPosition) => ResizeModule.UpdateResize(currentPosition);
        public void EndNoteResize() => ResizeModule.EndResize();

        public ResizeHandle GetResizeHandleAtPosition(Point position, NoteViewModel note) => ResizeModule.GetResizeHandleAtPosition(position, note);

        public NoteViewModel? GetNoteAtPosition(Point position) => SelectionModule.GetNoteAtPosition(position, Notes, Zoom, PixelsPerTick, KeyHeight);
        #endregion

        #region 工具方法
        public double SnapToGridTime(double time) => MusicalFraction.QuantizeToGrid(time, GridQuantization, TicksPerBeat);

        // 新增：音符名称和键盘相关方法
        public bool IsBlackKey(int midiNote)
        {
            var noteInOctave = midiNote % 12;
            return noteInOctave == 1 || noteInOctave == 3 || noteInOctave == 6 || noteInOctave == 8 || noteInOctave == 10;
        }

        public string GetNoteName(int midiNote)
        {
            var noteNames = new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            var octave = midiNote / 12 - 1;
            var noteIndex = midiNote % 12;
            return $"{noteNames[noteIndex]}{octave}";
        }

        public void AddNote(int pitch, double startTime, double duration = -1, int velocity = 100)
        {
            var quantizedStartTime = SnapToGridTime(startTime);
            var quantizedPosition = MusicalFraction.FromTicks(quantizedStartTime, TicksPerBeat);
            var noteDuration = duration < 0 ? UserDefinedNoteDuration : MusicalFraction.FromTicks(duration, TicksPerBeat);

            var note = new NoteViewModel
            {
                Pitch = pitch,
                StartPosition = quantizedPosition,
                Duration = noteDuration,
                Velocity = velocity
            };
            Notes.Add(note);
        }
        #endregion

        #region 命令
        [RelayCommand]
        private void SelectPencilTool() => CurrentTool = EditorTool.Pencil;

        [RelayCommand]
        private void SelectSelectionTool() => CurrentTool = EditorTool.Select;

        [RelayCommand]
        private void SelectEraserTool() => CurrentTool = EditorTool.Eraser;

        [RelayCommand]
        private void SelectCutTool() => CurrentTool = EditorTool.Cut;

        [RelayCommand]
        private void ToggleNoteDurationDropDown() => IsNoteDurationDropDownOpen = !IsNoteDurationDropDownOpen;

        [RelayCommand]
        private void SelectNoteDuration(NoteDurationOption option)
        {
            // 这里应该更改网格量化，而不是用户定义的音符时值
            GridQuantization = option.Duration;
            IsNoteDurationDropDownOpen = false;
            
            // 手动触发UI更新
            OnPropertyChanged(nameof(CurrentNoteDurationText));
        }

        [RelayCommand]
        private void ApplyCustomFraction()
        {
            try
            {
                // 简单的分数解析
                var parts = CustomFractionInput.Split('/');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int numerator) &&
                    int.TryParse(parts[1], out int denominator) &&
                    numerator > 0 && denominator > 0)
                {
                    // 这里应该更改网格量化，而不是用户定义的音符时值
                    GridQuantization = new MusicalFraction(numerator, denominator);
                    IsNoteDurationDropDownOpen = false;
                    OnPropertyChanged(nameof(CurrentNoteDurationText));
                }
            }
            catch
            {
                // 解析失败，保持原值
            }
        }

        [RelayCommand]
        private void SelectAll() => SelectionModule.SelectAll(Notes);
        #endregion

        #region 清理
        public void Cleanup()
        {
            DragModule.EndDrag();
            ResizeModule.EndResize();
            CreationModule.CancelCreating();
            SelectionModule.ClearSelection(Notes);
            PreviewModule.ClearPreview();
            Notes.Clear();
        }
        #endregion

        #region 属性变更处理
        partial void OnZoomSliderValueChanged(double value)
        {
            // 将0-100的滑块值转换为0.1-5.0的缩放值
            // 50对应1.0倍缩放，0对应0.1倍，100对应5.0倍
            Zoom = ConvertSliderValueToZoom(value);
        }

        partial void OnVerticalZoomSliderValueChanged(double value)
        {
            // 将0-100的滑块值转换为0.5-3.0的垂直缩放值
            // 50对应1.0倍缩放，0对应0.5倍，100对应3.0倍
            VerticalZoom = ConvertSliderValueToVerticalZoom(value);
        }

        partial void OnZoomChanged(double value)
        {
            // 当Zoom发生变化时，通知所有相关的计算属性
            OnPropertyChanged(nameof(MeasureWidth));
            OnPropertyChanged(nameof(BeatWidth));
            OnPropertyChanged(nameof(EighthNoteWidth));
            OnPropertyChanged(nameof(SixteenthNoteWidth));
            
            // 使所有音符的缓存失效
            foreach (var note in Notes)
            {
                note.InvalidateCache();
            }
        }

        partial void OnVerticalZoomChanged(double value)
        {
            // 当VerticalZoom发生变化时，通知所有相关的计算属性
            OnPropertyChanged(nameof(KeyHeight));
            OnPropertyChanged(nameof(TotalHeight));
            
            // 使所有音符的缓存失效
            foreach (var note in Notes)
            {
                note.InvalidateCache();
            }
        }

        private double ConvertSliderValueToZoom(double sliderValue)
        {
            // 确保滑块值在有效范围内
            sliderValue = Math.Max(0, Math.Min(100, sliderValue));
            
            // 水平缩放：0-100 -> 0.1-5.0
            // 使用指数函数实现更好的缩放体验
            if (sliderValue <= 50)
            {
                // 0-50对应0.1-1.0
                return 0.1 + (sliderValue / 50.0) * 0.9;
            }
            else
            {
                // 50-100对应1.0-5.0
                return 1.0 + ((sliderValue - 50) / 50.0) * 4.0;
            }
        }

        private double ConvertSliderValueToVerticalZoom(double sliderValue)
        {
            // 确保滑块值在有效范围内
            sliderValue = Math.Max(0, Math.Min(100, sliderValue));
            
            // 垂直缩放：0-100 -> 0.5-3.0
            if (sliderValue <= 50)
            {
                // 0-50对应0.5-1.0
                return 0.5 + (sliderValue / 50.0) * 0.5;
            }
            else
            {
                // 50-100对应1.0-3.0
                return 1.0 + ((sliderValue - 50) / 50.0) * 2.0;
            }
        }
        #endregion
    }
}