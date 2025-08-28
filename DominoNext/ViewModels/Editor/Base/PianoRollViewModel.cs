using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DominoNext.Services.Interfaces;
using DominoNext.Models.Music;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DominoNext.ViewModels.Editor
{
    public enum EditorTool
    {
        Pencil,     // 铅笔工具 - 添加/编辑音符
        Select,     // 选择工具 - 选择和移动音符
        Eraser,     // 橡皮工具 - 删除音符
        Cut         // 切割工具 - 分割音符
    }

    public enum ResizeHandle
    {
        None,
        StartEdge,  // 音符开始边缘
        EndEdge     // 音符结束边缘
    }

    public partial class PianoRollViewModel : ViewModelBase
    {
        private readonly ICoordinateService _coordinateService;

        [ObservableProperty]
        private double _zoom = 1.0;

        [ObservableProperty]
        private double _verticalZoom = 1.0;

        [ObservableProperty]
        private double _timelinePosition;

        [ObservableProperty]
        private double _eventViewHeight = 100.0;

        [ObservableProperty]
        private double _zoomSliderValue = 50.0;

        [ObservableProperty]
        private double _verticalZoomSliderValue = 50.0;

        // 工具选择
        [ObservableProperty]
        private EditorTool _currentTool = EditorTool.Pencil;

        // 添加预览音符支持
        [ObservableProperty]
        private NoteViewModel? _previewNote;

        // 编辑器命令 ViewModel
        [ObservableProperty]
        private EditorCommandsViewModel _editorCommands;

        // 选择相关属性
        [ObservableProperty]
        private Point? _selectionStart;

        [ObservableProperty]
        private Point? _selectionEnd;

        [ObservableProperty]
        private bool _isDragging;

        [ObservableProperty]
        private NoteViewModel? _draggingNote;

        // 音符长度调整相关属性
        [ObservableProperty]
        private bool _isResizing;

        [ObservableProperty]
        private ResizeHandle _currentResizeHandle = ResizeHandle.None;

        [ObservableProperty]
        private NoteViewModel? _resizingNote;

        [ObservableProperty]
        private List<NoteViewModel> _resizingNotes = new();

        // 记录原始长度用于约束
        private Dictionary<NoteViewModel, MusicalFraction> _originalDurations = new();

        // 网格量化设置 - 控制位置贴合，保持为1/16（这个影响工具栏显示）
        [ObservableProperty]
        private MusicalFraction _gridQuantization = MusicalFraction.SixteenthNote;

        // 默认音符长度 - 仅作为备用，不直接使用
        [ObservableProperty]
        private MusicalFraction _defaultNoteDuration = MusicalFraction.SixteenthNote;

        // 用户自定义音符长度 - 记住用户拉长或缩短后的长度，开局为四分音符
        [ObservableProperty]
        private MusicalFraction _userDefinedNoteDuration = MusicalFraction.QuarterNote;

        [ObservableProperty]
        private bool _isNoteDurationDropDownOpen = false;

        [ObservableProperty]
        private string _customFractionInput = "1/16";

        // 拖拽创建音符的状态
        [ObservableProperty]
        private bool _isCreatingNote = false;

        [ObservableProperty]
        private NoteViewModel? _creatingNote;

        [ObservableProperty]
        private Point _creatingStartPosition;

        public ObservableCollection<NoteViewModel> Notes { get; } = new();

        // 更新预定义的音符时值选项（使用您修改的符号字符，基于四分音符基准）
        public ObservableCollection<NoteDurationOption> NoteDurationOptions { get; } = new()
        {
            new NoteDurationOption("全音符 (1/1)", MusicalFraction.WholeNote, "𝅝"),
            new NoteDurationOption("二分音符 (1/2)", MusicalFraction.HalfNote, "𝅗𝅥"),
            new NoteDurationOption("三连二分音符 (1/3)", MusicalFraction.TripletHalf, "𝅗𝅥"),
            new NoteDurationOption("四分音符 (1/4)", MusicalFraction.QuarterNote, "𝅘𝅥"),
            new NoteDurationOption("三连四分音符 (1/6)", MusicalFraction.TripletQuarter, "𝅘𝅥"),
            new NoteDurationOption("八分音符 (1/8)", MusicalFraction.EighthNote, "𝅘𝅥𝅮"),
            new NoteDurationOption("三连八分音符 (1/12)", MusicalFraction.TripletEighth, "𝅘𝅥𝅮"),
            new NoteDurationOption("十六分音符 (1/16)", MusicalFraction.SixteenthNote, "𝅘𝅥𝅯"),
            new NoteDurationOption("三连十六分音符 (1/24)", MusicalFraction.TripletSixteenth, "𝅘𝅥𝅯"),
            new NoteDurationOption("三十二分音符 (1/32)", MusicalFraction.ThirtySecondNote, "𝅘𝅥𝅰"),
            new NoteDurationOption("三连三十二分音符 (1/48)", new MusicalFraction(1, 48), "𝅘𝅥𝅰"),
            new NoteDurationOption("六十四分音符 (1/64)", new MusicalFraction(1, 64), "𝅘𝅥𝅱"),
        };

        // 当前网格量化显示文本 - 这个在工具栏显示，保持为1/16
        public string CurrentGridQuantizationText => GridQuantization.ToString();

        // 当前用户自定义音符长度显示文本 - 这个可以单独显示或用于调试
        public string CurrentUserDefinedDurationText => UserDefinedNoteDuration.ToString();

        // 音乐时间相关属性
        public int BeatsPerMeasure => 4;
        public int TicksPerBeat => MusicalFraction.QUARTER_NOTE_TICKS; // 使用定义的常量
        public int TicksPerMeasure => BeatsPerMeasure * TicksPerBeat;
        public double PixelsPerTick => 100.0 / TicksPerBeat;

        public int KeyCount => 128;
        public double KeyHeight => 12.0 * VerticalZoom;
        public double GridWidth => 20.0 * Zoom;
        public double TotalHeight => KeyCount * KeyHeight;

        // 小节相关计算
        public double MeasureWidth => TicksPerMeasure * PixelsPerTick * Zoom;
        public double BeatWidth => TicksPerBeat * PixelsPerTick * Zoom;
        public double EighthNoteWidth => (TicksPerBeat / 2) * PixelsPerTick * Zoom;
        public double SixteenthNoteWidth => (TicksPerBeat / 4) * PixelsPerTick * Zoom;

        // 拖拽边缘检测阈值
        private const double ResizeEdgeThreshold = 8.0; // 像素

        // 默认构造函数（用于设计时）
        public PianoRollViewModel() : this(null)
        {
        }

        // 主构造函数
        public PianoRollViewModel(ICoordinateService? coordinateService)
        {
            _coordinateService = coordinateService ?? new DominoNext.Services.Implementation.CoordinateService();

            // 修复CS7036错误：提供正确的构造函数参数
            var noteEditingService = new DominoNext.Services.Implementation.NoteEditingService(this, _coordinateService);
            _editorCommands = new EditorCommandsViewModel(noteEditingService, _coordinateService);

            // 设置双向引用
            _editorCommands.SetPianoRollViewModel(this);

            //AddSampleNotes();
            UpdateZoomSliderValue();
            UpdateVerticalZoomSliderValue();
        }

        // 重要修复：确保CreatingNote属性变化时触发UI更新
        partial void OnCreatingNoteChanged(NoteViewModel? value)
        {
            // 确保UI重新渲染
            System.Diagnostics.Debug.WriteLine($"CreatingNote changed: {value?.Duration}");
        }

        // 新增：监听用户自定义音符长度变化
        partial void OnUserDefinedNoteDurationChanged(MusicalFraction value)
        {
            OnPropertyChanged(nameof(CurrentUserDefinedDurationText));
            System.Diagnostics.Debug.WriteLine($"用户自定义音符长度已更新: {value}");
        }

        #region 音符长度调整功能

        /// <summary>
        /// 检测鼠标位置是否接近音符的边缘
        /// </summary>
        public ResizeHandle GetResizeHandleAtPosition(Point position, NoteViewModel note)
        {
            if (CurrentTool != EditorTool.Pencil) return ResizeHandle.None;

            var noteRect = GetNoteRect(note);

            // 检查是否在音符内部
            if (!noteRect.Contains(position)) return ResizeHandle.None;

            // 检查是否接近开始边缘
            if (Math.Abs(position.X - noteRect.Left) <= ResizeEdgeThreshold)
            {
                return ResizeHandle.StartEdge;
            }

            // 检查是否接近结束边缘
            if (Math.Abs(position.X - noteRect.Right) <= ResizeEdgeThreshold)
            {
                return ResizeHandle.EndEdge;
            }

            return ResizeHandle.None;
        }

        /// <summary>
        /// 开始音符长度调整
        /// </summary>
        public void StartNoteResize(Point position, NoteViewModel note, ResizeHandle handle)
        {
            if (handle == ResizeHandle.None) return;

            IsResizing = true;
            CurrentResizeHandle = handle;
            ResizingNote = note;

            // 获取所有选中的音符（包括当前音符）
            ResizingNotes = Notes.Where(n => n.IsSelected).ToList();
            if (!ResizingNotes.Contains(note))
            {
                ResizingNotes.Add(note);
            }

            // 记录原始长度
            _originalDurations.Clear();
            foreach (var n in ResizingNotes)
            {
                _originalDurations[n] = n.Duration;
            }

            System.Diagnostics.Debug.WriteLine($"开始调整音符长度: Handle={handle}, 选中音符数={ResizingNotes.Count}");
        }

        /// <summary>
        /// 更新音符长度调整
        /// </summary>
        public void UpdateNoteResize(Point currentPosition)
        {
            if (!IsResizing || ResizingNote == null || ResizingNotes.Count == 0) return;

            try
            {
                var currentTime = GetTimeFromX(currentPosition.X);
                var noteStartTime = ResizingNote.StartPosition.ToTicks(TicksPerBeat);
                var noteEndTime = noteStartTime + ResizingNote.Duration.ToTicks(TicksPerBeat);

                foreach (var note in ResizingNotes)
                {
                    var startTime = note.StartPosition.ToTicks(TicksPerBeat);
                    var endTime = startTime + note.Duration.ToTicks(TicksPerBeat);
                    var originalDuration = _originalDurations[note];

                    MusicalFraction newDuration;

                    if (CurrentResizeHandle == ResizeHandle.StartEdge)
                    {
                        // 调整开始位置（缩短或延长音符开头）
                        var newStartTime = Math.Min(currentTime, endTime - GridQuantization.ToTicks(TicksPerBeat));
                        newStartTime = SnapToGridTime(newStartTime);

                        var newDurationTicks = endTime - newStartTime;
                        newDuration = MusicalFraction.FromTicks(newDurationTicks, TicksPerBeat);

                        // 更新开始位置
                        note.StartPosition = MusicalFraction.FromTicks(newStartTime, TicksPerBeat);
                    }
                    else // EndEdge
                    {
                        // 调整结束位置（延长或缩短音符结尾）
                        var newEndTime = Math.Max(currentTime, startTime + GridQuantization.ToTicks(TicksPerBeat));
                        newEndTime = SnapToGridTime(newEndTime);

                        var newDurationTicks = newEndTime - startTime;
                        newDuration = MusicalFraction.FromTicks(newDurationTicks, TicksPerBeat);
                    }

                    // 应用最小长度约束
                    var minDuration = GridQuantization;
                    if (originalDuration.CompareTo(minDuration) < 0)
                    {
                        // 如果原始长度小于网格量化，保持原始长度
                        newDuration = originalDuration;
                    }
                    else
                    {
                        // 否则不能小于网格量化
                        if (newDuration.CompareTo(minDuration) < 0)
                        {
                            newDuration = minDuration;
                        }
                    }

                    note.Duration = newDuration;
                    note.InvalidateCache();
                }

                System.Diagnostics.Debug.WriteLine($"更新音符长度: {ResizingNote.Duration}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新音符长度时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 完成音符长度调整
        /// </summary>
        public void EndNoteResize()
        {
            if (IsResizing && ResizingNote != null)
            {
                // 更新用户自定义长度为最后调整的音符长度
                UserDefinedNoteDuration = ResizingNote.Duration;
                System.Diagnostics.Debug.WriteLine($"完成音符长度调整，更新用户自定义长度: {UserDefinedNoteDuration}");
            }

            IsResizing = false;
            CurrentResizeHandle = ResizeHandle.None;
            ResizingNote = null;
            ResizingNotes.Clear();
            _originalDurations.Clear();
        }

        /// <summary>
        /// 取消音符长度调整
        /// </summary>
        public void CancelNoteResize()
        {
            if (IsResizing && ResizingNotes.Count > 0)
            {
                // 恢复原始长度
                foreach (var note in ResizingNotes)
                {
                    if (_originalDurations.TryGetValue(note, out var originalDuration))
                    {
                        note.Duration = originalDuration;
                        note.InvalidateCache();
                    }
                }
            }

            EndNoteResize();
        }

        /// <summary>
        /// 获取当前鼠标位置应该显示的光标类型
        /// </summary>
        public string GetCursorForPosition(Point position)
        {
            if (CurrentTool != EditorTool.Pencil) return "Default";

            var note = GetNoteAtPosition(position);
            if (note != null)
            {
                var handle = GetResizeHandleAtPosition(position, note);
                return handle switch
                {
                    ResizeHandle.StartEdge or ResizeHandle.EndEdge => "SizeWE", // 水平调整光标
                    _ => "Hand" // 移动光标
                };
            }

            return "Cross"; // 创建音符光标
        }

        #endregion

        // 工具选择命令
        [RelayCommand]
        private void SelectPencilTool() => CurrentTool = EditorTool.Pencil;

        [RelayCommand]
        private void SelectSelectionTool() => CurrentTool = EditorTool.Select;

        [RelayCommand]
        private void SelectEraserTool() => CurrentTool = EditorTool.Eraser;

        [RelayCommand]
        private void SelectCutTool() => CurrentTool = EditorTool.Cut;

        // 音符时值选择命令 - 恢复原来的行为，同时更新网格量化和默认长度
        [RelayCommand]
        private void ToggleNoteDurationDropDown()
        {
            IsNoteDurationDropDownOpen = !IsNoteDurationDropDownOpen;
        }

        [RelayCommand]
        private void SelectNoteDuration(NoteDurationOption option)
        {
            GridQuantization = option.Fraction;
            DefaultNoteDuration = option.Fraction; // 恢复原来的行为
            IsNoteDurationDropDownOpen = false;
            OnPropertyChanged(nameof(CurrentGridQuantizationText));
            OnPropertyChanged(nameof(CurrentNoteDurationText));
        }

        [RelayCommand]
        private void ApplyCustomFraction()
        {
            try
            {
                var parts = CustomFractionInput.Split('/');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int numerator) &&
                    int.TryParse(parts[1], out int denominator))
                {
                    GridQuantization = new MusicalFraction(numerator, denominator);
                    DefaultNoteDuration = GridQuantization; // 恢复原来的行为
                    IsNoteDurationDropDownOpen = false;
                    OnPropertyChanged(nameof(CurrentGridQuantizationText));
                    OnPropertyChanged(nameof(CurrentNoteDurationText));
                }
            }
            catch
            {
                // 解析失败，保持原值
            }
        }

        // 改进的量化方法
        public double SnapToGridTime(double time)
        {
            return MusicalFraction.QuantizeToGrid(time, GridQuantization, TicksPerBeat);
        }

        // 修改 AddNote 方法，使用用户自定义音符长度
        public void AddNote(int pitch, double startTime, double duration = -1, int velocity = 100)
        {
            var quantizedStartTime = SnapToGridTime(startTime);
            var quantizedPosition = MusicalFraction.FromTicks(quantizedStartTime, TicksPerBeat);

            // 如果没有指定长度，使用用户自定义长度
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

        // 新增：开始拖拽创建音符，使用用户自定义长度
        public void StartCreatingNote(Point position)
        {
            var pitch = GetPitchFromY(position.Y);
            var startTime = GetTimeFromX(position.X);

            if (IsValidNotePosition(pitch, startTime))
            {
                var quantizedStartTime = SnapToGridTime(startTime);
                var quantizedPosition = MusicalFraction.FromTicks(quantizedStartTime, TicksPerBeat);

                CreatingNote = new NoteViewModel
                {
                    Pitch = pitch,
                    StartPosition = quantizedPosition,
                    Duration = UserDefinedNoteDuration, // 使用用户自定义长度作为初始长度
                    Velocity = 100,
                    IsPreview = true
                };

                CreatingStartPosition = position;
                IsCreatingNote = true;

                System.Diagnostics.Debug.WriteLine($"开始创建音符: Pitch={pitch}, Duration={CreatingNote.Duration}");
            }
        }

        // 修改：更新拖拽创建的音符长度
        public void UpdateCreatingNote(Point currentPosition)
        {
            if (!IsCreatingNote || CreatingNote == null) return;

            var currentTime = GetTimeFromX(currentPosition.X);
            var startTime = CreatingNote.StartPosition.ToTicks(TicksPerBeat);

            // 计算量化后的长度
            var minDuration = GridQuantization.ToTicks(TicksPerBeat);
            var actualDuration = Math.Max(minDuration, currentTime - startTime);

            if (actualDuration > 0)
            {
                var duration = MusicalFraction.CalculateQuantizedDuration(
                    startTime, startTime + actualDuration, GridQuantization, TicksPerBeat);

                // 只在长度真正改变时更新
                if (!CreatingNote.Duration.Equals(duration))
                {
                    System.Diagnostics.Debug.WriteLine($"实时更新音符长度: {CreatingNote.Duration} -> {duration}");
                    CreatingNote.Duration = duration;
                    CreatingNote.InvalidateCache();

                    // 手动触发属性变更通知
                    OnPropertyChanged(nameof(CreatingNote));
                }
            }
        }

        // 修改：完成拖拽创建音符，记住用户自定义的长度
        public void FinishCreatingNote()
        {
            if (IsCreatingNote && CreatingNote != null)
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

                Notes.Add(finalNote);

                // 重要：记住用户拉长后的音符长度，用于下一个音符
                UserDefinedNoteDuration = CreatingNote.Duration;

                System.Diagnostics.Debug.WriteLine($"完成创建音符: {finalNote.Duration}，已更新用户自定义长度: {UserDefinedNoteDuration}");
            }

            // 清理状态
            IsCreatingNote = false;
            CreatingNote = null;
        }

        // 新增：取消拖拽创建音符
        public void CancelCreatingNote()
        {
            IsCreatingNote = false;
            CreatingNote = null;
        }

        // 缩放相关方法
        partial void OnZoomSliderValueChanged(double value)
        {
            var normalizedValue = value / 100.0;
            var logMin = Math.Log10(0.1);
            var logMax = Math.Log10(5.0);
            var logValue = logMin + normalizedValue * (logMax - logMin);
            var newZoom = Math.Pow(10, logValue);

            if (Math.Abs(Zoom - newZoom) > 0.001)
            {
                Zoom = newZoom;
            }
        }

        partial void OnVerticalZoomSliderValueChanged(double value)
        {
            var normalizedValue = value / 100.0;
            var logMin = Math.Log10(0.5);
            var logMax = Math.Log10(3.0);
            var logValue = logMin + normalizedValue * (logMax - logMin);
            var newVerticalZoom = Math.Pow(10, logValue);

            if (Math.Abs(VerticalZoom - newVerticalZoom) > 0.001)
            {
                VerticalZoom = newVerticalZoom;
            }
        }

        partial void OnZoomChanged(double value)
        {
            // 通知所有音符重新计算缓存
            foreach (var note in Notes)
            {
                note.InvalidateCache();
            }
            PreviewNote?.InvalidateCache();
            CreatingNote?.InvalidateCache();

            UpdateZoomSliderValue();
            OnPropertyChanged(nameof(MeasureWidth));
            OnPropertyChanged(nameof(BeatWidth));
            OnPropertyChanged(nameof(EighthNoteWidth));
            OnPropertyChanged(nameof(SixteenthNoteWidth));
        }

        partial void OnVerticalZoomChanged(double value)
        {
            // 通知所有音符重新计算缓存
            foreach (var note in Notes)
            {
                note.InvalidateCache();
            }
            PreviewNote?.InvalidateCache();
            CreatingNote?.InvalidateCache();

            UpdateVerticalZoomSliderValue();
            OnPropertyChanged(nameof(KeyHeight));
            OnPropertyChanged(nameof(TotalHeight));
        }

        private void UpdateZoomSliderValue()
        {
            var logMin = Math.Log10(0.1);
            var logMax = Math.Log10(5.0);
            var logZoom = Math.Log10(Math.Max(0.1, Math.Min(5.0, Zoom)));
            var normalizedValue = (logZoom - logMin) / (logMax - logMin);
            var sliderValue = normalizedValue * 100.0;

            if (Math.Abs(ZoomSliderValue - sliderValue) > 0.1)
            {
                ZoomSliderValue = sliderValue;
            }
        }

        private void UpdateVerticalZoomSliderValue()
        {
            var logMin = Math.Log10(0.5);
            var logMax = Math.Log10(3.0);
            var logZoom = Math.Log10(Math.Max(0.5, Math.Min(3.0, VerticalZoom)));
            var normalizedValue = (logZoom - logMin) / (logMax - logMin);
            var sliderValue = normalizedValue * 100.0;

            if (Math.Abs(VerticalZoomSliderValue - sliderValue) > 0.1)
            {
                VerticalZoomSliderValue = sliderValue;
            }
        }

        private void AddSampleNotes()
        {
            Notes.Add(new NoteViewModel
            {
                Pitch = 60,
                StartPosition = MusicalFraction.FromTicks(0, TicksPerBeat),
                Duration = MusicalFraction.QuarterNote,
                Velocity = 100
            });
            Notes.Add(new NoteViewModel
            {
                Pitch = 64,
                StartPosition = MusicalFraction.FromTicks(TicksPerBeat, TicksPerBeat),
                Duration = MusicalFraction.EighthNote,
                Velocity = 80
            });
            Notes.Add(new NoteViewModel
            {
                Pitch = 67,
                StartPosition = MusicalFraction.FromTicks(TicksPerBeat * 1.5, TicksPerBeat),
                Duration = MusicalFraction.EighthNote,
                Velocity = 90
            });
        }

        public void DeleteSelectedNotes()
        {
            for (int i = Notes.Count - 1; i >= 0; i--)
            {
                if (Notes[i].IsSelected)
                {
                    Notes.RemoveAt(i);
                }
            }
        }

        public void ClearSelection()
        {
            foreach (var note in Notes)
            {
                note.IsSelected = false;
            }
        }

        // 坐标转换方法 (通过服务)
        public int GetPitchFromY(double y)
        {
            return _coordinateService.GetPitchFromY(y, KeyHeight);
        }

        public double GetTimeFromX(double x)
        {
            return _coordinateService.GetTimeFromX(x, Zoom, PixelsPerTick);
        }

        public Point GetPositionFromNote(NoteViewModel note)
        {
            return _coordinateService.GetPositionFromNote(note, Zoom, PixelsPerTick, KeyHeight);
        }

        public Rect GetNoteRect(NoteViewModel note)
        {
            return _coordinateService.GetNoteRect(note, Zoom, PixelsPerTick, KeyHeight);
        }

        // 选择和拖拽相关方法
        public void StartSelection(Point point)
        {
            SelectionStart = point;
            SelectionEnd = point;
        }

        public void UpdateSelection(Point point)
        {
            SelectionEnd = point;
        }

        public void EndSelection()
        {
            if (SelectionStart.HasValue && SelectionEnd.HasValue)
            {
                var selectionRect = new Rect(SelectionStart.Value, SelectionEnd.Value);
                SelectNotesInArea(selectionRect);
            }

            SelectionStart = null;
            SelectionEnd = null;
        }

        public void SelectNotesInArea(Rect area)
        {
            foreach (var note in Notes)
            {
                var noteRect = GetNoteRect(note);
                if (area.Intersects(noteRect))
                {
                    note.IsSelected = true;
                }
            }
        }

        public void StartNoteDrag(NoteViewModel note, Point startPoint)
        {
            DraggingNote = note;
            IsDragging = true;
        }

        public void UpdateNoteDrag(Point currentPoint, Point startPoint)
        {
            if (DraggingNote == null) return;

            var deltaX = currentPoint.X - startPoint.X;
            var deltaY = currentPoint.Y - startPoint.Y;

            var timeDelta = deltaX / (PixelsPerTick * Zoom);
            var pitchDelta = -(int)(deltaY / KeyHeight);

            var newStartTime = Math.Max(0, DraggingNote.StartTime + timeDelta);
            var newPitch = Math.Max(0, Math.Min(127, DraggingNote.Pitch + pitchDelta));

            newStartTime = SnapToGridTime(newStartTime);

            DraggingNote.StartTime = newStartTime;
            DraggingNote.Pitch = newPitch;
        }

        public void EndNoteDrag()
        {
            DraggingNote = null;
            IsDragging = false;
        }

        public NoteViewModel? GetNoteAtPosition(Point position)
        {
            foreach (var note in Notes)
            {
                var noteRect = GetNoteRect(note);
                if (noteRect.Contains(position))
                {
                    return note;
                }
            }
            return null;
        }

        // 修改预览音符逻辑，使用用户自定义长度
        public void UpdatePreviewNote(Point position)
        {
            // 如果正在创建音符，不显示普通预览
            if (IsCreatingNote)
            {
                ClearPreviewNote();
                return;
            }

            if (CurrentTool != EditorTool.Pencil)
            {
                ClearPreviewNote();
                return;
            }

            var pitch = GetPitchFromY(position.Y);
            var startTime = GetTimeFromX(position.X);

            if (IsValidNotePosition(pitch, startTime))
            {
                var quantizedStartTime = SnapToGridTime(startTime);
                var quantizedPosition = MusicalFraction.FromTicks(quantizedStartTime, TicksPerBeat);

                if (PreviewNote == null)
                {
                    PreviewNote = new NoteViewModel();
                }

                PreviewNote.Pitch = pitch;
                PreviewNote.StartPosition = quantizedPosition;
                PreviewNote.Duration = UserDefinedNoteDuration; // 使用用户自定义长度
                PreviewNote.Velocity = 100;
                PreviewNote.IsPreview = true;
            }
            else
            {
                PreviewNote = null;
            }
        }

        public void ClearPreviewNote()
        {
            PreviewNote = null;
        }

        public bool IsValidNotePosition(int pitch, double startTime)
        {
            return pitch >= 0 && pitch <= 127 && startTime >= 0;
        }

        // 缩放命令
        [RelayCommand]
        private void ZoomIn() => Zoom = Math.Min(Zoom * 1.2, 5.0);

        [RelayCommand]
        private void ZoomOut() => Zoom = Math.Max(Zoom / 1.2, 0.1);

        [RelayCommand]
        private void VerticalZoomIn() => VerticalZoom = Math.Min(VerticalZoom * 1.2, 3.0);

        [RelayCommand]
        private void VerticalZoomOut() => VerticalZoom = Math.Max(VerticalZoom / 1.2, 0.5);

        // 编辑命令
        [RelayCommand]
        private void CreateNote()
        {
            AddNote(60, 0);
        }

        [RelayCommand]
        private void CreateNoteAtCursor(Point position)
        {
            var pitch = GetPitchFromY(position.Y);
            var startTime = GetTimeFromX(position.X);

            if (IsValidNotePosition(pitch, startTime))
            {
                AddNote(pitch, startTime);
            }
        }

        [RelayCommand]
        private void DeleteSelected()
        {
            DeleteSelectedNotes();
        }

        [RelayCommand]
        private void ClearNoteSelection()
        {
            ClearSelection();
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (var note in Notes)
            {
                note.IsSelected = true;
            }
        }

        [RelayCommand]
        private void DuplicateSelected()
        {
            var selectedNotes = new List<NoteViewModel>();
            foreach (var note in Notes)
            {
                if (note.IsSelected)
                {
                    selectedNotes.Add(note);
                }
            }

            foreach (var note in selectedNotes)
            {
                var newNote = new NoteViewModel
                {
                    Pitch = note.Pitch,
                    StartPosition = note.StartPosition + note.Duration,
                    Duration = note.Duration,
                    Velocity = note.Velocity,
                    IsSelected = true
                };
                Notes.Add(newNote);
                note.IsSelected = false;
            }
        }

        [RelayCommand]
        private void QuantizeSelected()
        {
            foreach (var note in Notes)
            {
                if (note.IsSelected)
                {
                    var currentTicks = note.StartPosition.ToTicks(TicksPerBeat);
                    var quantizedTicks = SnapToGridTime(currentTicks);
                    note.StartPosition = MusicalFraction.FromTicks(quantizedTicks, TicksPerBeat);
                }
            }
        }

        // 时间线控制
        [RelayCommand]
        private void SetTimelinePosition(double position)
        {
            TimelinePosition = Math.Max(0, position);
        }

        [RelayCommand]
        private void PlayFromTimeline()
        {
            // TODO: 实现播放功能
        }

        [RelayCommand]
        private void StopPlayback()
        {
            // TODO: 实现停止播放功能
        }

        // 键盘和音符相关辅助方法
        public string GetNoteName(int midiNote)
        {
            var noteNames = new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            var octave = midiNote / 12 - 1;
            var noteIndex = midiNote % 12;
            return $"{noteNames[noteIndex]}{octave}";
        }

        public bool IsBlackKey(int midiNote)
        {
            var noteIndex = midiNote % 12;
            return noteIndex == 1 || noteIndex == 3 || noteIndex == 6 || noteIndex == 8 || noteIndex == 10;
        }

        // 网格和对齐设置 - 保持兼容性
        [ObservableProperty]
        private bool _snapToGrid = true;

        // 兼容性属性：将新的网格量化映射到旧的GridResolution
        public int GridResolution => (int)(TicksPerBeat / GridQuantization.GetGridUnit(TicksPerBeat) * TicksPerBeat);

        public double GetGridSnapTime(double time)
        {
            if (!SnapToGrid) return time;
            return SnapToGridTime(time);
        }

        // 兼容性属性，映射到网格量化系统（恢复原来的行为）
        public MusicalFraction CurrentNoteDuration
        {
            get => GridQuantization; // 恢复显示网格量化值
            set => GridQuantization = value;
        }

        public string CurrentNoteDurationText => CurrentGridQuantizationText; // 恢复显示网格量化文本

        // 撤销/重做支持
        [RelayCommand(CanExecute = nameof(CanUndo))]
        private void Undo()
        {
            // TODO: 实现撤销功能
        }

        [RelayCommand(CanExecute = nameof(CanRedo))]
        private void Redo()
        {
            // TODO: 实现重做功能
        }

        public bool CanUndo => false;
        public bool CanRedo => false;

        public void SaveViewState()
        {
            // TODO: 保存当前视图状态
        }

        public void RestoreViewState()
        {
            // TODO: 恢复保存的视图状态
        }

        public void BeginUpdate()
        {
            // TODO: 暂停通知更新
        }

        public void EndUpdate()
        {
            // TODO: 恢复通知更新并刷新
        }

        public void Cleanup()
        {
            ClearSelection();
            Notes.Clear();
            PreviewNote = null;
            CancelCreatingNote();
        }
    }

    /// <summary>
    /// 音符时值选项
    /// </summary>
    public class NoteDurationOption
    {
        public string Name { get; }
        public MusicalFraction Fraction { get; }
        public string Icon { get; }

        public NoteDurationOption(string name, MusicalFraction fraction, string icon)
        {
            Name = name;
            Fraction = fraction;
            Icon = icon;
        }
    }
}