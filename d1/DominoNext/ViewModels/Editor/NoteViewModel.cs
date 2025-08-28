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

        // 缓存计算结果以提升性能
        private double _cachedX = double.NaN;
        private double _cachedY = double.NaN;
        private double _cachedWidth = double.NaN;
        private double _cachedHeight = double.NaN;
        private double _lastZoom = double.NaN;
        private double _lastVerticalZoom = double.NaN;
        private double _lastPixelsPerTick = double.NaN;

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

        // 兼容性属性 - 为现有代码提供支持，添加安全检查
        public double StartTime
        {
            get => StartPosition.ToTicks(96); // 使用标准的 96 ticks per beat
            set
            {
                try
                {
                    // 添加安全检查，避免无效值
                    if (double.IsNaN(value) || double.IsInfinity(value) || value < 0)
                    {
                        return; // 忽略无效值
                    }

                    var newPosition = MusicalFraction.FromTicks(value, 96);
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
            get => Duration.ToTicks(96);
            set
            {
                try
                {
                    if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
                    {
                        return; // 忽略无效值
                    }

                    var newDuration = MusicalFraction.FromTicks(value, 96);
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
        public double GetX(double zoom, double pixelsPerTick)
        {
            var currentKey = zoom * pixelsPerTick;
            if (double.IsNaN(_cachedX) || Math.Abs(_lastZoom - currentKey) > 0.001)
            {
                var startTime = StartTime;
                // 添加安全检查
                if (double.IsNaN(startTime) || double.IsInfinity(startTime))
                {
                    _cachedX = 0;
                }
                else
                {
                    _cachedX = startTime * pixelsPerTick * zoom;
                }
                _lastZoom = currentKey;
            }
            return _cachedX;
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

        public double GetWidth(double zoom, double pixelsPerTick)
        {
            var currentKey = zoom * pixelsPerTick;
            if (double.IsNaN(_cachedWidth) || Math.Abs(_lastPixelsPerTick - currentKey) > 0.001)
            {
                var duration = DurationInTicks;
                // 添加安全检查
                if (double.IsNaN(duration) || double.IsInfinity(duration) || duration <= 0)
                {
                    _cachedWidth = 4; // 最小宽度
                }
                else
                {
                    _cachedWidth = duration * pixelsPerTick * zoom;
                }
                _lastPixelsPerTick = currentKey;
            }
            return _cachedWidth;
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