using CommunityToolkit.Mvvm.ComponentModel;
using DominoNext.Models.Music;
using System;

namespace DominoNext.ViewModels.Editor
{
    public partial class NoteViewModel : ViewModelBase
    {
        // 包装的数据模型
        private readonly Note _note;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private bool _isPreview;

        // 秒级时间（加载MIDI时赋值）
        [ObservableProperty]
        private double _startSeconds;
        [ObservableProperty]
        private double _durationSeconds;

        // 缓存计算结果以提升性能
        // 这些字段在GetX和GetWidth等方法中被使用，用于缓存计算结果避免重复计算
        #pragma warning disable CS0414
        private double _cachedX = double.NaN;
        private double _cachedY = double.NaN;
        private double _cachedWidth = double.NaN;
        private double _cachedHeight = double.NaN;
        private double _lastZoom = double.NaN;
        private double _lastVerticalZoom = double.NaN;
        private double _lastPixelsPerTick = double.NaN;
        #pragma warning restore CS0414

        public NoteViewModel(Note note)
        {
            _note = note ?? throw new ArgumentNullException(nameof(note));
        }

        public NoteViewModel() : this(new Note()) { }

        // 公开属性，直接访问模型数据
        public Guid Id => _note.Id;

        public int Pitch
        {
            get => _note.Pitch;
            set
            {
                if (_note.Pitch != value)
                {
                    _note.Pitch = value;
                    OnPropertyChanged();
                    InvalidateCache();
                }
            }
        }

        public MusicalFraction StartPosition
        {
            get => _note.StartPosition;
            set
            {
                if (_note.StartPosition != value)
                {
                    _note.StartPosition = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StartTime)); // 兼容性
                    InvalidateCache();
                }
            }
        }

        public MusicalFraction Duration
        {
            get => _note.Duration;
            set
            {
                if (_note.Duration != value)
                {
                    _note.Duration = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DurationInTicks)); // 兼容性
                    InvalidateCache();
                }
            }
        }

        public int Velocity
        {
            get => _note.Velocity;
            set
            {
                if (_note.Velocity != value)
                {
                    _note.Velocity = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Lyric
        {
            get => _note.Lyric;
            set
            {
                if (_note.Lyric != value)
                {
                    _note.Lyric = value;
                    OnPropertyChanged();
                }
            }
        }

        // 兼容性属性 - 修复TicksPerBeat不一致的问题
        public double StartTime
        {
            get => StartPosition.ToTicks(MusicalFraction.QUARTER_NOTE_TICKS); // 使用统一的常量
            set
            {
                try
                {
                    // 添加安全检查，避免无效值
                    if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
                    {
                        return; // 忽略无效值
                    }

                    var newPosition = MusicalFraction.FromTicks(value, MusicalFraction.QUARTER_NOTE_TICKS);
                    StartPosition = newPosition;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"设置StartTime错误: {ex.Message}, 值: {value}");
                    // 发生错误时不更新位置
                }
            }
        }

        public double DurationInTicks
        {
            get => Duration.ToTicks(MusicalFraction.QUARTER_NOTE_TICKS); // 使用统一的常量
            set
            {
                try
                {
                    if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
                    {
                        return; // 忽略无效值
                    }

                    var newDuration = MusicalFraction.FromTicks(value, MusicalFraction.QUARTER_NOTE_TICKS);
                    Duration = newDuration;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"设置DurationInTicks错误: {ex.Message}, 值: {value}");
                }
            }
        }

        // 获取底层数据模型
        public Note GetModel() => _note;

        // 现有的UI相关方法保持不变
        public double GetX(double pixelsPerTick, int ticksPerBeat)
        {
            var startTime = StartPosition.ToTicks(ticksPerBeat);
            return startTime * pixelsPerTick;
        }

        public double GetY(double keyHeight)
        {
            if (double.IsNaN(_cachedY) || Math.Abs(_lastVerticalZoom - keyHeight) > 0.001)
            {
                _cachedY = (127 - Pitch) * keyHeight;
                _lastVerticalZoom = keyHeight;
            }
            return _cachedY;
        }

        public double GetWidth(double pixelsPerTick, int ticksPerBeat)
        {
            var duration = Duration.ToTicks(ticksPerBeat);
            return duration * pixelsPerTick;
        }

        public double GetHeight(double keyHeight)
        {
            if (double.IsNaN(_cachedHeight) || Math.Abs(_lastVerticalZoom - keyHeight) > 0.001)
            {
                _cachedHeight = keyHeight;
                _lastVerticalZoom = keyHeight;
            }
            return _cachedHeight;
        }

        public void InvalidateCache()
        {
            _cachedX = double.NaN;
            _cachedY = double.NaN;
            _cachedWidth = double.NaN;
            _cachedHeight = double.NaN;
        }

        /// <summary>
        /// 工厂方法：创建四分音符
        /// </summary>
        public static NoteViewModel CreateQuarterNote(int pitch, MusicalFraction startPosition, int velocity = 100)
        {
            var note = Note.CreateQuarterNote(pitch, startPosition, velocity);
            return new NoteViewModel(note);
        }
    }
}