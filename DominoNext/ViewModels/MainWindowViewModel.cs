using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DominoNext.Services.Implementation;
using DominoNext.Services.Interfaces;
using DominoNext.ViewModels.Editor;
using DominoNext.ViewModels.Settings;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using DominoNext.Models.Music;
using DominoNext.ViewModels.Editor.Models;

namespace DominoNext.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        #region 属性
        private readonly ISettingsService _settingsService;
        private readonly IProjectStorageService _projectStorageService;
        private readonly IPlaybackService _playbackService;
        
        [ObservableProperty]
        private PianoRollViewModel _pianoRoll;
        
        [ObservableProperty]
        private SettingsWindowViewModel _settingsWindow;
        
        [ObservableProperty]
        private string _currentFilePath;
        
        private string _title = "";

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Greeting { get; } = "Welcome to Avalonia!";
        
        #endregion

        #region 构造函数

        public MainWindowViewModel(ISettingsService settingsService, IProjectStorageService projectStorageService, IPlaybackService playbackService)
        {
            _settingsService = settingsService;
            _projectStorageService = projectStorageService;
            _playbackService = playbackService;
            
            // 初始化钢琴卷帘视图模型
            PianoRoll = new PianoRollViewModel(_playbackService);
            
            // 初始化设置窗口视图模型
            SettingsWindow = new SettingsWindowViewModel(settingsService);
            
            // 初始化当前文件路径
            CurrentFilePath = "";
            
            // 初始化标题
            UpdateTitle();
        }
        
        #endregion

        // 无参构造函数用于设计时
        public MainWindowViewModel() : this(
            new DominoNext.Services.Implementation.SettingsService(), 
            new ProjectStorageService(),
            new PlaybackService())
        {
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            var settingsViewModel = new SettingsWindowViewModel(_settingsService);
            var settingsWindow = new Views.Settings.SettingsWindow
            {
                DataContext = settingsViewModel
            };

            // 安全地获取主窗口
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow != null)
            {
                await settingsWindow.ShowDialog(desktop.MainWindow);
            }
        }

        [RelayCommand]
        private async Task PlayAsync()
        {
            // TODO: 实现播放功能
            await Task.CompletedTask;
        }
        
        [RelayCommand]
        private void Stop()
        {
            // TODO: 实现停止功能
        }

        [RelayCommand]
        private void NewFile()
        {
            // TODO: 实现新建文件功能
        }

        [RelayCommand]
        private async Task OpenFileAsync()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow != null)
            {
                var files = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "打开MIDI文件",
                    AllowMultiple = false,
                    FileTypeFilter = new[] { new FilePickerFileType("MIDI Files") { Patterns = new[] { "*.mid", "*.midi" } } }
                });

                if (files.Count > 0)
                {
                    var file = files[0];
                    var filePath = file.TryGetLocalPath();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        CurrentFilePath = filePath;
                        UpdateTitle();
                        
                        // 加载MIDI文件内容
                        await LoadMidiFileAsync(filePath);
                    }
                }
            }
            
            // 添加这一行以消除CS1998警告
            await Task.CompletedTask;
        }

        private async Task LoadMidiFileAsync(string filePath)
        {
            try
            {
                var midiLoader = new MidiLoader();
                var midiFile = midiLoader.LoadMidi(filePath);
                System.Diagnostics.Debug.WriteLine($"开始加载MIDI文件: {filePath}");

                // 获取MIDI文件的基本信息
                var tempoMap = midiFile.GetTempoMap();
                var timeDivision = midiFile.TimeDivision;
                int ticksPerBeat = MusicalFraction.QUARTER_NOTE_TICKS; // 默认值
                int beatsPerMeasure = 4; // 默认值

                if (timeDivision is TicksPerQuarterNoteTimeDivision tpq)
                {
                    ticksPerBeat = (int)tpq.TicksPerQuarterNote;
                    System.Diagnostics.Debug.WriteLine($"MIDI文件PPQ: {ticksPerBeat}");
                }

                // 读取拍号
                var timeSignature = tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(0));
                if (timeSignature != null)
                {
                    beatsPerMeasure = timeSignature.Numerator;
                    System.Diagnostics.Debug.WriteLine($"MIDI文件拍号: {timeSignature.Numerator}/{timeSignature.Denominator}");
                }

                // 解析轨道
                var tracks = new List<TrackViewModel>();
                long maxEndTick = 0;

                foreach (var trackChunk in midiFile.GetTrackChunks())
                {
                    var trackVm = new TrackViewModel();
                    
                    // 尝试获取轨道名称
                    string? trackName = null;
                    var seqName = trackChunk.Events.OfType<SequenceTrackNameEvent>().FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(seqName?.Text)) trackName = seqName.Text;
                    
                    if (string.IsNullOrWhiteSpace(trackName))
                        trackName = $"轨道 {tracks.Count + 1}";
                    
                    trackVm.Name = trackName;
                    System.Diagnostics.Debug.WriteLine($"解析轨道: {trackName}");

                    // 解析音符
                    var notes = trackChunk.GetNotes();
                    foreach (var note in notes)
                    {
                        // 转换时间为MusicalFraction
                        var startFraction = MusicalFraction.FromTicks((long)note.Time, ticksPerBeat);
                        var durationFraction = MusicalFraction.FromTicks((long)note.Length, ticksPerBeat);

                        var noteVm = new NoteViewModel
                        {
                            Pitch = note.NoteNumber,
                            StartPosition = startFraction,
                            Duration = durationFraction,
                            Velocity = Math.Clamp((int)note.Velocity, 0, 127)
                        };

                        var metricStart = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, tempoMap);
                        var metricEnd = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time + note.Length, tempoMap);
                        noteVm.StartSeconds = metricStart.TotalMicroseconds / 1_000_000.0;
                        noteVm.DurationSeconds = (metricEnd.TotalMicroseconds - metricStart.TotalMicroseconds) / 1_000_000.0;

                        trackVm.Notes.Add(noteVm);
                        var endTick = note.Time + note.Length;
                        if (endTick > maxEndTick) maxEndTick = endTick;
                    }

                    if (trackVm.Notes.Count > 0)
                    {
                        tracks.Add(trackVm);
                    }
                }

                // 计算总小节数
                int totalMeasures = (int)Math.Ceiling(maxEndTick / (double)(ticksPerBeat * beatsPerMeasure));
                System.Diagnostics.Debug.WriteLine($"计算得到总小节数: {totalMeasures}");

                // 将所有数据加载到PianoRoll
                PianoRoll.LoadMidiTracks(tracks);
                PianoRoll.TicksPerBeat = ticksPerBeat;
                PianoRoll.BeatsPerMeasure = beatsPerMeasure;
                PianoRoll.TotalMeasures = Math.Max(1, totalMeasures);

                System.Diagnostics.Debug.WriteLine("MIDI文件加载完成");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载MIDI文件失败: {ex.Message}");
                // TODO: 显示错误提示
            }
        }

        [RelayCommand]
        private void SaveFile()
        {
            // TODO: 实现保存文件功能
        }

        [RelayCommand]
        private void ExitApplication()
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        #region 私有方法
        
        /// <summary>
        /// 更新窗口标题
        /// </summary>
        private void UpdateTitle()
        {
            var appName = "DominoNext";
            Title = string.IsNullOrEmpty(CurrentFilePath) 
                ? $"{appName}" 
                : $"{Path.GetFileName(CurrentFilePath)} - {appName}";
        }

        #endregion
    }
}