using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DominoNext.Models.Music;
using DominoNext.Services.Interfaces;
using DominoNext.ViewModels.Editor.Models;
using DominoNext.ViewModels.Editor.Modules;
using DominoNext.ViewModels.Editor.State;
using DominoNext.ViewModels.Editor.Commands;
using Point = Avalonia.Point;
using Rect = Avalonia.Rect;

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
        
        #region 音符时值和网格量化
        [ObservableProperty] private MusicalFraction _gridQuantization = MusicalFraction.SixteenthNote;
        [ObservableProperty] private NoteDurationOption? _currentNoteDuration; // 当前选择的音符时值
        [ObservableProperty] private MusicalFraction _userDefinedNoteDuration = MusicalFraction.QuarterNote;
        #endregion
        [ObservableProperty] private double _verticalZoom = 1.0;
        [ObservableProperty] private double _timelinePosition;
        [ObservableProperty] private double _zoomSliderValue = 50.0;
        [ObservableProperty] private double _verticalZoomSliderValue = 50.0;
        [ObservableProperty] private EditorTool _currentTool = EditorTool.Pencil; // 确保EditorTool在正确命名空间下

        // UI相关属性
        [ObservableProperty] private bool _isNoteDurationDropDownOpen = false;
        [ObservableProperty] private string _customFractionInput = "1/4";
        #endregion

        #region 洋葱皮属性
        [ObservableProperty] private bool _isOnionSkinEnabled = true; // 默认启用洋葱皮功能
        [ObservableProperty] private int _onionSkinPreviousFrames = 1;
        [ObservableProperty] private int _onionSkinNextFrames = 1;
        [ObservableProperty] private double _onionSkinOpacity = 0.3;
        #endregion

        #region 命令属性
        [ObservableProperty] private EditorCommandsViewModel _editorCommands;
        #endregion

        #region 播放控制命令
        [RelayCommand]
        private async Task PlayAsync()
        {
            if (PlaybackService != null)
                await PlaybackService.PlayAsync(this);
        }

        [RelayCommand]
        private async Task PauseAsync()
        {
            if (PlaybackService != null)
                await PlaybackService.PauseAsync();
        }

        [RelayCommand]
        private async Task StopAsync()
        {
            if (PlaybackService != null)
                await PlaybackService.StopAsync();
        }
        #endregion

        #region 网格量化命令
        [RelayCommand]
        private void ToggleNoteDurationDropDown()
        {
            IsNoteDurationDropDownOpen = !IsNoteDurationDropDownOpen;
        }
        #endregion

        #region 洋葱皮命令
        [RelayCommand]
        private void DecreasePreviousOnionSkinFrames()
        {
            OnionSkinPreviousFrames = Math.Max(0, OnionSkinPreviousFrames - 1);
        }

        [RelayCommand]
        private void IncreasePreviousOnionSkinFrames()
        {
            OnionSkinPreviousFrames = Math.Min(10, OnionSkinPreviousFrames + 1);
        }

        [RelayCommand]
        private void DecreaseNextOnionSkinFrames()
        {
            OnionSkinNextFrames = Math.Max(0, OnionSkinNextFrames - 1);
        }

        [RelayCommand]
        private void IncreaseNextOnionSkinFrames()
        {
            OnionSkinNextFrames = Math.Min(10, OnionSkinNextFrames + 1);
        }
        #endregion

        #region 播放服务
        public IPlaybackService PlaybackService { get; }
        #endregion

        #region 其他属性和方法
        public ObservableCollection<NoteViewModel> Notes { get; } = new();
        public ObservableCollection<NoteDurationOption> NoteDurationOptions { get; } = new(); // 网格量化选项

        // 音轨集合与切换
        public ObservableCollection<TrackViewModel> Tracks { get; } = new();
        private TrackViewModel? _selectedTrack;
        public TrackViewModel? SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                if (_selectedTrack != value)
                {
                    // 保存当前轨道的音符
                    SyncNotesToTrack();

                    _selectedTrack = value;

                    // 加载新轨道的音符
                    Notes.Clear();
                    if (_selectedTrack != null)
                    {
                        foreach (var note in _selectedTrack.Notes)
                            Notes.Add(note);
                        
                        // 同步洋葱皮属性
                        IsOnionSkinEnabled = _selectedTrack.IsOnionSkinEnabled;
                        OnionSkinOpacity = _selectedTrack.OnionSkinOpacity;
                        OnionSkinPreviousFrames = _selectedTrack.OnionSkinPreviousFrames;
                        OnionSkinNextFrames = _selectedTrack.OnionSkinNextFrames;
                        
                        System.Diagnostics.Debug.WriteLine($"切换到轨道: {_selectedTrack.Name}, 音符数: {Notes.Count}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("清空当前轨道");
                    }

                    OnPropertyChanged(nameof(SelectedTrack));
                    InvalidateVisual();
                }
            }
        }

        // 将四分音符ticks作为可配置字段（默认为MusicalFraction.QUARTER_NOTE_TICKS）
        [ObservableProperty]
        private int _ticksPerBeat = MusicalFraction.QUARTER_NOTE_TICKS;

        partial void OnTicksPerBeatChanged(int value)
        {
            if (value <= 0)
            {
                TicksPerBeat = MusicalFraction.QUARTER_NOTE_TICKS;
                return;
            }

            // 更新相关计算属性
            OnPropertyChanged(nameof(PixelsPerTick));
            OnPropertyChanged(nameof(MeasureWidth));
            OnPropertyChanged(nameof(BeatWidth));
            OnPropertyChanged(nameof(EighthNoteWidth));
            OnPropertyChanged(nameof(SixteenthNoteWidth));
        }

        public double PixelsPerTick => Zoom; // 每tick的像素，随Zoom变化
        public double KeyHeight => 12.0 * VerticalZoom;
        public double MeasureWidth => BeatsPerMeasure * TicksPerBeat * PixelsPerTick;
        public double BeatWidth => TicksPerBeat * PixelsPerTick;
        public double EighthNoteWidth => (TicksPerBeat / 2.0) * PixelsPerTick;
        public double SixteenthNoteWidth => (TicksPerBeat / 4.0) * PixelsPerTick;

        // 新增：小节相关
        private int _beatsPerMeasure = 4; // standard numerator
        public int BeatsPerMeasure
        {
            get => _beatsPerMeasure;
            set
            {
                if (value <= 0) return;
                if (_beatsPerMeasure != value)
                {
                    _beatsPerMeasure = value;
                    OnPropertyChanged(nameof(BeatsPerMeasure));
                    OnPropertyChanged(nameof(MeasureWidth));
                }
            }
        }

        // 总小节数（由加载MIDI时设置）
        private int _totalMeasures = 1;
        public int TotalMeasures
        {
            get => _totalMeasures;
            set
            {
                if (_totalMeasures != value)
                {
                    _totalMeasures = value;
                    OnPropertyChanged(nameof(TotalMeasures));
                    OnPropertyChanged(nameof(ContentWidth));
                }
            }
        }

        // 内容总宽度（像素），用于绑定画布宽度：MeasureWidth * TotalMeasures
        public double ContentWidth => MeasureWidth * Math.Max(1, TotalMeasures);

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
        public PianoRollViewModel(IPlaybackService playbackService)
        {
            PlaybackService = playbackService;
            
            // 初始化服务
            _coordinateService = new Services.Implementation.CoordinateService();
            
            // 初始化状态
            DragState = new DragState();
            ResizeState = new ResizeState();
            SelectionState = new SelectionState();
            
            // 初始化模块
            DragModule = new NoteDragModule(DragState, _coordinateService);
            ResizeModule = new NoteResizeModule(ResizeState, _coordinateService);
            CreationModule = new NoteCreationModule(_coordinateService);
            SelectionModule = new NoteSelectionModule(SelectionState, _coordinateService);
            PreviewModule = new NotePreviewModule(_coordinateService); // 修复：传递_coordinateService参数
            
            // 设置模块间引用
            DragModule.SetPianoRollViewModel(this);
            ResizeModule.SetPianoRollViewModel(this);
            CreationModule.SetPianoRollViewModel(this);
            SelectionModule.SetPianoRollViewModel(this);
            PreviewModule.SetPianoRollViewModel(this);
            
            // 订阅模块事件
            DragModule.OnDragStarted += () => { OnPropertyChanged(nameof(IsDragging)); };
            DragModule.OnDragEnded += () => { OnPropertyChanged(nameof(IsDragging)); };
            DragModule.OnDragUpdated += () => { OnPropertyChanged(nameof(IsDragging)); };
            
            ResizeModule.OnResizeStarted += () => { OnPropertyChanged(nameof(IsResizing)); };
            ResizeModule.OnResizeEnded += () => { OnPropertyChanged(nameof(IsResizing)); };
            ResizeModule.OnResizeUpdated += () => { OnPropertyChanged(nameof(IsResizing)); };
            
            CreationModule.OnCreationStarted += () => { OnPropertyChanged(nameof(IsCreatingNote)); };
            CreationModule.OnCreationEnded += () => { OnPropertyChanged(nameof(IsCreatingNote)); };
            CreationModule.OnCreationUpdated += () => { OnPropertyChanged(nameof(IsCreatingNote)); };
            
            SelectionModule.OnSelectionStarted += () => { OnPropertyChanged(nameof(IsSelecting)); };
            SelectionModule.OnSelectionEnded += () => { OnPropertyChanged(nameof(IsSelecting)); };
            SelectionModule.OnSelectionUpdated += () => { OnPropertyChanged(nameof(IsSelecting)); };
            
            PreviewModule.OnPreviewUpdated += () => { OnPropertyChanged(nameof(PreviewNote)); };
            
            // 初始化选项
            InitializeNoteDurationOptions();
            
            // 初始化命令
            InitializeCommands();
        }
        
        #endregion

        #region 命令初始化
        private void InitializeCommands()
        {
            // 初始化命令
            
            EditorCommands = new EditorCommandsViewModel(_coordinateService);
            EditorCommands.SetPianoRollViewModel(this);
        }
        #endregion

        #region 工具选择
        [RelayCommand]
        private void SelectPencilTool() => CurrentTool = EditorTool.Pencil;

        [RelayCommand]
        private void SelectSelectionTool() => CurrentTool = EditorTool.Select;

        [RelayCommand]
        private void SelectEraserTool() => CurrentTool = EditorTool.Eraser;

        [RelayCommand]
        private void SelectCutTool() => CurrentTool = EditorTool.Cut;
        #endregion

        #region 音符时值选择
        [RelayCommand]
        private void SelectNoteDuration(object? parameter)
        {
            if (parameter is NoteDurationOption option)
            {
                GridQuantization = option.Duration;
                IsNoteDurationDropDownOpen = false;
                OnPropertyChanged(nameof(CurrentNoteDurationText));
            }
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
        
        [RelayCommand]
        private void ToggleOnionSkin() => IsOnionSkinEnabled = !IsOnionSkinEnabled;
        
        [RelayCommand]
        private void IncreaseOnionSkinOpacity() => OnionSkinOpacity = Math.Min(1.0, OnionSkinOpacity + 0.1);
        
        [RelayCommand]
        private void DecreaseOnionSkinOpacity() => OnionSkinOpacity = Math.Max(0.1, OnionSkinOpacity - 0.1);
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
            System.Diagnostics.Debug.WriteLine($"ZoomSliderValueChanged: {value}");
            // 将0-100的滑块值转换为0.1-5.0的缩放值
            // 50对应1.0倍缩放，0对应0.1倍，100对应5.0倍
            Zoom = ConvertSliderValueToZoom(value);
        }

        partial void OnVerticalZoomSliderValueChanged(double value)
        {
            System.Diagnostics.Debug.WriteLine($"VerticalZoomSliderValueChanged: {value}");
            // 将0-100的滑块值转换为0.5-3.0的垂直缩放值
            // 50对应1.0倍缩放，0对应0.5倍，100对应3.0倍
            VerticalZoom = ConvertSliderValueToVerticalZoom(value);
        }

        partial void OnZoomChanged(double value)
        {
            System.Diagnostics.Debug.WriteLine($"ZoomChanged: {value}");
            // 当Zoom发生变化时，更新滑块值
            ZoomSliderValue = ConvertZoomToSliderValue(value);
            
            // 当Zoom发生变化时，通知所有相关的计算属性
            OnPropertyChanged(nameof(MeasureWidth));
            OnPropertyChanged(nameof(ContentWidth));
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
            System.Diagnostics.Debug.WriteLine($"VerticalZoomChanged: {value}");
            // 当VerticalZoom发生变化时，更新垂直滑块值
            VerticalZoomSliderValue = ConvertVerticalZoomToSliderValue(value);
            
            // 通知所有相关的计算属性
            OnPropertyChanged(nameof(KeyHeight));
            OnPropertyChanged(nameof(TotalHeight));
            
            // 使所有音符的缓存失效
            foreach (var note in Notes)
            {
                note.InvalidateCache();
            }
        }

        partial void OnIsOnionSkinEnabledChanged(bool value)
        {
            if (_selectedTrack != null)
            {
                _selectedTrack.IsOnionSkinEnabled = value;
            }
            InvalidateVisual();
        }

        partial void OnOnionSkinPreviousFramesChanged(int value)
        {
            if (_selectedTrack != null)
            {
                _selectedTrack.OnionSkinPreviousFrames = value;
            }
            InvalidateVisual();
        }

        partial void OnOnionSkinNextFramesChanged(int value)
        {
            if (_selectedTrack != null)
            {
                _selectedTrack.OnionSkinNextFrames = value;
            }
            InvalidateVisual();
        }

        partial void OnOnionSkinOpacityChanged(double value)
        {
            if (_selectedTrack != null)
            {
                _selectedTrack.OnionSkinOpacity = value;
            }
            InvalidateVisual();
        }
        #endregion

        #region 滑块值转换方法
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
        
        private double ConvertZoomToSliderValue(double zoom)
        {
            // 将缩放值转换回滑块值
            if (zoom <= 1.0)
            {
                return (zoom - 0.1) / 0.9 * 50;
            }
            else
            {
                return 50 + (zoom - 1.0) / 4.0 * 50;
            }
        }

        private double ConvertVerticalZoomToSliderValue(double verticalZoom)
        {
            // 将垂直缩放值转换回滑块值
            if (verticalZoom <= 1.0)
            {
                return (verticalZoom - 0.5) / 0.5 * 50;
            }
            else
            {
                return 50 + (verticalZoom - 1.0) / 2.0 * 50;
            }
        }
        #endregion

        #region 选项初始化
        private void InitializeNoteDurationOptions()
        {
            NoteDurationOptions.Add(new NoteDurationOption("全音符", MusicalFraction.WholeNote, ""));
            NoteDurationOptions.Add(new NoteDurationOption("半音符", MusicalFraction.HalfNote, ""));
            NoteDurationOptions.Add(new NoteDurationOption("四分音符", MusicalFraction.QuarterNote, ""));
            NoteDurationOptions.Add(new NoteDurationOption("八分音符", MusicalFraction.EighthNote, ""));
            NoteDurationOptions.Add(new NoteDurationOption("十六分音符", MusicalFraction.SixteenthNote, ""));
        }
        #endregion

        #region 音轨管理
        public void AddTrack(TrackViewModel track)
        {
            Tracks.Add(track);
        }

        public void RemoveTrack(TrackViewModel track)
        {
            Tracks.Remove(track);
        }

        public void LoadMidiTracks(IEnumerable<TrackViewModel> tracks)
        {
            Tracks.Clear();
            foreach (var track in tracks)
            {
                Tracks.Add(track);
            }
            
            // 如果有轨道，选择第一个作为当前轨道
            if (Tracks.Count > 0)
            {
                SelectedTrack = Tracks[0];
            }
        }

        public void SyncNotesToTrack()
        {
            if (_selectedTrack != null)
            {
                _selectedTrack.Notes.Clear();
                foreach (var note in Notes)
                    _selectedTrack.Notes.Add(note);
            }
        }
        #endregion

        #region UI交互
        public void HandleMouseLeftButtonDown(Point position)
        {
            // 处理鼠标左键按下事件
        }

        public void HandleMouseLeftButtonUp(Point position)
        {
            // 处理鼠标左键释放事件
        }

        public void HandleMouseMove(Point position)
        {
            // 处理鼠标移动事件
        }

        public void HandleMouseWheel(double delta)
        {
            // 处理鼠标滚轮事件
        }

        public void HandleKeyDown(Avalonia.Input.Key key)
        {
            // 处理键盘按下事件
        }

        public void HandleKeyUp(Avalonia.Input.Key key)
        {
            // 处理键盘释放事件
        }
        
        private void InvalidateVisual()
        {
            // 通知所有需要刷新的UI属性
            OnPropertyChanged(nameof(ContentWidth));
            OnPropertyChanged(nameof(TotalHeight));
            OnPropertyChanged(nameof(Notes));
            OnPropertyChanged(nameof(Tracks));
            
            // 刷新与音符相关的状态
            OnPropertyChanged(nameof(IsCreatingNote));
            OnPropertyChanged(nameof(IsDragging));
            OnPropertyChanged(nameof(IsResizing));
            OnPropertyChanged(nameof(IsSelecting));
            
            // 刷新洋葱皮相关属性
            OnPropertyChanged(nameof(IsOnionSkinEnabled));
            OnPropertyChanged(nameof(OnionSkinPreviousFrames));
            OnPropertyChanged(nameof(OnionSkinNextFrames));
            OnPropertyChanged(nameof(OnionSkinOpacity));
            
            // 使音符缓存失效
            foreach (var note in Notes)
            {
                note.InvalidateCache();
            }
        }
        #endregion

        #region 事件处理
        private void OnNoteDragStarted(object? sender, object e)
        {
            // 处理拖拽开始事件
        }

        private void OnNoteDragEnded(object? sender, object e)
        {
            // 处理拖拽结束事件
        }

        private void OnNoteResizeStarted(object? sender, object e)
        {
            // 处理调整大小开始事件
        }

        private void OnNoteResizeEnded(object? sender, object e)
        {
            // 处理调整大小结束事件
        }

        private void OnNoteCreationStarted(object? sender, object e)
        {
            // 处理创建音符开始事件
        }

        private void OnNoteCreationEnded(object? sender, object e)
        {
            // 处理创建音符结束事件
        }

        private void OnSelectionStarted(object? sender, object e)
        {
            // 处理选择框开始事件
        }

        private void OnSelectionEnded(object? sender, object e)
        {
            // 处理选择框结束事件
        }

        private void OnPreviewNoteChanged(object? sender, object e)
        {
            // 处理预览音符变化事件
        }
        #endregion

        /// <summary>
        /// 根据Y坐标获取音高
        /// </summary>
        public int GetPitchFromY(double y)
        {
            // 计算MIDI音符编号 (127 - 音符行数)
            // 每个音符的高度是KeyHeight，从上到下是G9(127)到A0(21)
            var noteRow = y / KeyHeight;
            var midiNote = 127 - (int)Math.Floor(noteRow);
            return Math.Max(0, Math.Min(127, midiNote));
        }

        /// <summary>
        /// 判断是否是黑键
        /// </summary>
        public bool IsBlackKey(int midiNote)
        {
            // MIDI音符编号对应钢琴键位
            // 黑键模式：C#(1), D#(3), F#(6), G#(8), A#(10) 在12音阶中的位置
            var noteInOctave = midiNote % 12;
            return noteInOctave == 1 || noteInOctave == 3 || noteInOctave == 6 || 
                   noteInOctave == 8 || noteInOctave == 10;
        }

        /// <summary>
        /// 获取音符名称
        /// </summary>
        public string GetNoteName(int midiNote)
        {
            var noteNames = new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            var octave = midiNote / 12 - 1; // MIDI音符编号转八度
            var noteIndex = midiNote % 12;
            return $"{noteNames[noteIndex]}{octave}";
        }

        /// <summary>
        /// 添加音符
        /// </summary>
        public void AddNote(NoteViewModel note)
        {
            Notes.Add(note);
            InvalidateVisual();
        }

        /// <summary>
        /// 根据位置获取音符
        /// </summary>
        public NoteViewModel? GetNoteAtPosition(Point position)
        {
            // 从后向前遍历，确保选择的是最上层的音符
            for (int i = Notes.Count - 1; i >= 0; i--)
            {
                var note = Notes[i];
                var x = note.GetX(PixelsPerTick, TicksPerBeat);
                var y = note.GetY(KeyHeight);
                var width = note.GetWidth(PixelsPerTick, TicksPerBeat);
                var height = note.GetHeight(KeyHeight);
                var noteRect = new Rect(x, y, width, height);
                
                if (noteRect.Contains(position))
                {
                    return note;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据位置获取调整手柄
        /// </summary>
        public ResizeHandle GetResizeHandleAtPosition(Point position)
        {
            const double HANDLE_SIZE = 8.0;
            
            // 检查所有选中的音符
            foreach (var note in Notes.Where(n => n.IsSelected))
            {
                var x = note.GetX(PixelsPerTick, TicksPerBeat);
                var y = note.GetY(KeyHeight);
                var width = note.GetWidth(PixelsPerTick, TicksPerBeat);
                var height = note.GetHeight(KeyHeight);
                
                // 检查左侧手柄
                var leftHandleRect = new Rect(x - HANDLE_SIZE / 2, y + height / 2 - HANDLE_SIZE / 2, HANDLE_SIZE, HANDLE_SIZE);
                if (leftHandleRect.Contains(position))
                {
                    return ResizeHandle.StartEdge;
                }
                
                // 检查右侧手柄
                var rightHandleRect = new Rect(x + width - HANDLE_SIZE / 2, y + height / 2 - HANDLE_SIZE / 2, HANDLE_SIZE, HANDLE_SIZE);
                if (rightHandleRect.Contains(position))
                {
                    return ResizeHandle.EndEdge;
                }
            }
            
            return ResizeHandle.None;
        }

        /// <summary>
        /// 根据位置和音符获取调整手柄（重载方法）
        /// </summary>
        public ResizeHandle GetResizeHandleAtPosition(Point position, NoteViewModel note)
        {
            const double HANDLE_SIZE = 8.0;
            
            var x = note.GetX(PixelsPerTick, TicksPerBeat);
            var y = note.GetY(KeyHeight);
            var width = note.GetWidth(PixelsPerTick, TicksPerBeat);
            var height = note.GetHeight(KeyHeight);
            
            // 检查左侧手柄
            var leftHandleRect = new Rect(x - HANDLE_SIZE / 2, y + height / 2 - HANDLE_SIZE / 2, HANDLE_SIZE, HANDLE_SIZE);
            if (leftHandleRect.Contains(position))
            {
                return ResizeHandle.StartEdge;
            }
            
            // 检查右侧手柄
            var rightHandleRect = new Rect(x + width - HANDLE_SIZE / 2, y + height / 2 - HANDLE_SIZE / 2, HANDLE_SIZE, HANDLE_SIZE);
            if (rightHandleRect.Contains(position))
            {
                return ResizeHandle.EndEdge;
            }
            
            return ResizeHandle.None;
        }

        /// <summary>
        /// 自动扩展以适应新内容
        /// </summary>
        public void AutoExtendWhenNearEnd(double positionX)
        {
            // 当接近末尾时自动扩展
            var contentEndX = ContentWidth;
            if (positionX > contentEndX - 100) // 距离末尾100像素时扩展
            {
                TotalMeasures = Math.Max(TotalMeasures, (int)(positionX / MeasureWidth) + 5);
            }
        }

        /// <summary>
        /// 计算填充UI所需的小节数
        /// </summary>
        public int CalculateMeasuresToFillUI(double viewportWidth)
        {
            // 计算需要多少小节来填满视口
            return Math.Max(1, (int)Math.Ceiling(viewportWidth / MeasureWidth));
        }

        /// <summary>
        /// 确保有足够的容量容纳新音符
        /// </summary>
        public void EnsureCapacityForNote(NoteViewModel note)
        {
            var noteEndTicks = note.StartPosition.ToTicks(TicksPerBeat) + note.Duration.ToTicks(TicksPerBeat);
            var requiredMeasures = Math.Max(1, (int)(noteEndTicks / (TicksPerBeat * BeatsPerMeasure)) + 1);
            TotalMeasures = Math.Max(TotalMeasures, requiredMeasures);
        }

        #region 时间处理
        /// <summary>
        /// 根据X坐标获取时间（ticks）
        /// </summary>
        public double GetTimeFromX(double x)
        {
            if (PixelsPerTick <= 0) return 0;
            return x / PixelsPerTick;
        }

        /// <summary>
        /// 将时间（ticks）对齐到网格
        /// </summary>
        public double SnapToGridTime(double timeInTicks)
        {
            var gridTicks = GridQuantization.ToTicks(TicksPerBeat);
            return Math.Round(timeInTicks / gridTicks) * gridTicks;
        }

        /// <summary>
        /// 根据音高获取Y坐标
        /// </summary>
        public double GetYFromPitch(int pitch)
        {
            return _coordinateService.GetPitchFromY(pitch, KeyHeight);
        }

        /// <summary>
        /// 选择指定时间范围内的音符
        /// </summary>
        public void SelectNotesInTimeRange(double startTicks, double endTicks)
        {
            foreach (var note in Notes)
            {
                var noteStartTicks = note.StartPosition.ToTicks(TicksPerBeat);
                var noteEndTicks = noteStartTicks + note.Duration.ToTicks(TicksPerBeat);
                if (noteStartTicks < endTicks && noteEndTicks > startTicks)
                {
                    note.IsSelected = true;
                }
            }
            InvalidateVisual();
        }
        
        #endregion
    }
}