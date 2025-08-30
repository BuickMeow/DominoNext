using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DominoNext.Models.Settings;
using DominoNext.Services.Interfaces;

namespace DominoNext.ViewModels.Settings
{
    /// <summary>
    /// 设置窗口ViewModel
    /// </summary>
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private SettingsPageType _selectedPageType = SettingsPageType.General;

        [ObservableProperty]
        private bool _hasUnsavedChanges = false;

        // 播放列表设置选项
        public ObservableCollection<PlaylistSetting> PlaylistSettings { get; } = new();

        public SettingsModel Settings => _settingsService.Settings;

        public ObservableCollection<SettingsPageInfo> Pages { get; } = new();

        // 语言选项
        public ObservableCollection<LanguageOption> LanguageOptions { get; } = new()
        {
            new LanguageOption { Code = "zh-CN", Name = "简体中文", NativeName = "简体中文" },
            new LanguageOption { Code = "en-US", Name = "English", NativeName = "English" },
            new LanguageOption { Code = "ja-JP", Name = "Japanese", NativeName = "日本語" }
        };

        // 主题选项
        public ObservableCollection<ThemeOption> ThemeOptions { get; } = new()
        {
            new ThemeOption { Key = "Default", Name = "跟随系统", Description = "跟随系统主题设置" },
            new ThemeOption { Key = "Light", Name = "浅色主题", Description = "使用浅色主题" },
            new ThemeOption { Key = "Dark", Name = "深色主题", Description = "使用深色主题" }
        };

        // 快捷键设置
        public ObservableCollection<ShortcutSetting> ShortcutSettings { get; } = new();
        
        // 播放设备选项
        public ObservableCollection<PlaybackDeviceOption> PlaybackDeviceOptions { get; } = new();

        public SettingsWindowViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            InitializePages();
            InitializeShortcutSettings();

            // 加载设置
            LoadSettings();

            // 初始化播放列表设置
            InitializePlaylistSettings();

            // 监听设置变更
            Settings.PropertyChanged += (sender, e) => HasUnsavedChanges = true;
        }
        
        private void InitializePlaylistSettings()
        {
            PlaylistSettings.Clear();
            
            // 添加默认播放列表设置
            PlaylistSettings.Add(new PlaylistSetting
            {
                Name = "主播放列表",
                Description = "主播放列表设置",
                DefaultPlaybackMode = PlaybackMode.Sequential,
                DefaultRepeatMode = RepeatMode.None,
                AutoPlayNext = true
            });
            
            PlaylistSettings.Add(new PlaylistSetting
            {
                Name = "随机播放列表",
                Description = "随机播放列表设置",
                DefaultPlaybackMode = PlaybackMode.Random,
                DefaultRepeatMode = RepeatMode.All,
                AutoPlayNext = false
            });
        }
        
        /// <summary>
        /// 加载播放列表设置
        /// </summary>
        private void LoadPlaylistSettings()
        {
            try
            {
                // 如果播放列表设置存储在 JSON 中，可以从 Settings.PlaylistSettingsJson 解析
                if (!string.IsNullOrEmpty(Settings.PlaylistSettingsJson) && Settings.PlaylistSettingsJson != "{}")
                {
                    var playlistSettings = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<PlaylistSetting>>(Settings.PlaylistSettingsJson) ?? new ObservableCollection<PlaylistSetting>();
                    PlaylistSettings.Clear();
                    foreach (var playlist in playlistSettings)
                    {
                        PlaylistSettings.Add(playlist);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载播放列表设置失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存播放列表设置
        /// </summary>
        private void SavePlaylistSettings()
        {
            try
            {
                // 将播放列表设置保存到 JSON 中
                Settings.PlaylistSettingsJson = System.Text.Json.JsonSerializer.Serialize(PlaylistSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存播放列表设置失败: {ex.Message}");
            }
        }
        
        [RelayCommand]
        private void AddPlaylist()
        {
            // 添加新的播放列表设置
            PlaylistSettings.Add(new PlaylistSetting
            {
                Name = $"播放列表 {PlaylistSettings.Count + 1}",
                Description = "新播放列表",
                DefaultPlaybackMode = PlaybackMode.Sequential,
                DefaultRepeatMode = RepeatMode.None,
                AutoPlayNext = true
            });
            
            HasUnsavedChanges = true;
        }
        
        [RelayCommand]
        private void RemovePlaylist(PlaylistSetting playlist)
        {
            if (playlist != null)
            {
                PlaylistSettings.Remove(playlist);
                HasUnsavedChanges = true;
            }
        }
        
        [RelayCommand]
        private void ResetPlaylistSettings()
        {
            // 重置播放列表设置为默认值
            PlaylistSettings.Clear();
            InitializePlaylistSettings();
            HasUnsavedChanges = true;
        }

        // 设计时使用的无参构造函数
        public SettingsWindowViewModel() : this(new DominoNext.Services.Implementation.SettingsService())
        {
        }

        private void InitializePages()
        {
            Pages.Clear();
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.General,
                Title = "常规",
                Icon = "⚙️",
                Description = "基本应用设置"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Language,
                Title = "语言",
                Icon = "🌐",
                Description = "界面语言设置"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Theme,
                Title = "主题",
                Icon = "🎨",
                Description = "界面主题和外观"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Editor,
                Title = "编辑器",
                Icon = "✏️",
                Description = "编辑器行为设置"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Shortcuts,
                Title = "快捷键",
                Icon = "⌨️",
                Description = "键盘快捷键设置"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Playlist,
                Title = "播放列表",
                Icon = "🎵",
                Description = "播放列表行为设置"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Advanced,
                Title = "高级",
                Icon = "🛠️",
                Description = "高级选项和调试"
            });
            
            // 初始化播放列表设置页面
            InitializePlaylistPage();
        }

        private void InitializeShortcutSettings()
        {
            ShortcutSettings.Clear();

            // 文件操作
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "NewFile",
                Description = "新建文件",
                DefaultShortcut = "Ctrl+N",
                CurrentShortcut = "Ctrl+N",
                Category = "文件"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "OpenFile",
                Description = "打开文件",
                DefaultShortcut = "Ctrl+O",
                CurrentShortcut = "Ctrl+O",
                Category = "文件"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "SaveFile",
                Description = "保存文件",
                DefaultShortcut = "Ctrl+S",
                CurrentShortcut = "Ctrl+S",
                Category = "文件"
            });

            // 编辑操作
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Undo",
                Description = "撤销",
                DefaultShortcut = "Ctrl+Z",
                CurrentShortcut = "Ctrl+Z",
                Category = "编辑"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Redo",
                Description = "重做",
                DefaultShortcut = "Ctrl+Y",
                CurrentShortcut = "Ctrl+Y",
                Category = "编辑"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Copy",
                Description = "复制",
                DefaultShortcut = "Ctrl+C",
                CurrentShortcut = "Ctrl+C",
                Category = "编辑"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Paste",
                Description = "粘贴",
                DefaultShortcut = "Ctrl+V",
                CurrentShortcut = "Ctrl+V",
                Category = "编辑"
            });

            // 工具
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "PencilTool",
                Description = "铅笔工具",
                DefaultShortcut = "P",
                CurrentShortcut = "P",
                Category = "工具"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "EraserTool",
                Description = "橡皮擦工具",
                DefaultShortcut = "E",
                CurrentShortcut = "E",
                Category = "工具"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "SelectTool",
                Description = "选择工具",
                DefaultShortcut = "S",
                CurrentShortcut = "S",
                Category = "工具"
            });

            // 播放控制
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Play",
                Description = "播放",
                DefaultShortcut = "Space",
                CurrentShortcut = "Space",
                Category = "播放"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Pause",
                Description = "暂停",
                DefaultShortcut = "Space",
                CurrentShortcut = "Space",
                Category = "播放"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Stop",
                Description = "停止",
                DefaultShortcut = "Ctrl+Space",
                CurrentShortcut = "Ctrl+Space",
                Category = "播放"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "SeekForward",
                Description = "快进",
                DefaultShortcut = "Right",
                CurrentShortcut = "Right",
                Category = "播放"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "SeekBackward",
                Description = "快退",
                DefaultShortcut = "Left",
                CurrentShortcut = "Left",
                Category = "播放"
            });
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // 加载播放列表设置
                LoadPlaylistSettings();
                
                // 加载快捷键设置
                LoadShortcutSettings();
                
                // 加载播放设备设置
                LoadPlaybackDevices();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
            }
        }
        
        private void LoadPlaybackDevices()
        {
            try
            {
                // 如果播放设备设置存储在 JSON 中，可以从 Settings.PlaybackDevicesJson 解析
                if (!string.IsNullOrEmpty(Settings.PlaybackDevicesJson) && Settings.PlaybackDevicesJson != "{}")
                {
                    var playbackDevices = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<PlaybackDeviceOption>>(Settings.PlaybackDevicesJson) ?? new ObservableCollection<PlaybackDeviceOption>();
                    PlaybackDeviceOptions.Clear();
                    foreach (var device in playbackDevices)
                    {
                        PlaybackDeviceOptions.Add(device);
                    }
                }
                else
                {
                    // 初始化播放设备
                    InitializePlaybackDevices();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载播放设备设置失败: {ex.Message}");
                // 初始化播放设备
                InitializePlaybackDevices();
            }
        }

        private void SavePlaybackDevices()
        {
            try
            {
                // 将播放设备设置保存到 JSON 中
                Settings.PlaybackDevicesJson = System.Text.Json.JsonSerializer.Serialize(PlaybackDeviceOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存播放设备设置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存设置到文件
        /// </summary>
        private void SaveSettingsToFile()
        {
            try
            {
                // 保存快捷键设置到设置模型（如果需要）
                SaveShortcutSettings();
                
                // 保存播放列表设置
                SavePlaylistSettings();
                
                // 保存播放设备设置
                SavePlaybackDevices();

                // 保存到文件
                Settings.SaveToFile();

                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置到文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载快捷键设置
        /// </summary>
        private void LoadShortcutSettings()
        {
            try
            {
                // 如果快捷键设置存储在 JSON 中，可以从 Settings.CustomShortcutsJson 解析
                // 这里可以根据实际需求实现
                /*
                if (!string.IsNullOrEmpty(Settings.CustomShortcutsJson) && Settings.CustomShortcutsJson != "{}")
                {
                    var customShortcuts = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(Settings.CustomShortcutsJson);
                    foreach (var shortcut in ShortcutSettings)
                    {
                        if (customShortcuts.ContainsKey(shortcut.Command))
                        {
                            shortcut.CurrentShortcut = customShortcuts[shortcut.Command];
                        }
                    }
                }
                */
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载快捷键设置失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取播放列表设置页面
        /// </summary>
        private void InitializePlaylistPage()
        {
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Playlist,
                Title = "播放列表",
                Icon = "🎵",
                Description = "播放列表行为设置"
            });
        }

        /// <summary>
        /// 保存快捷键设置
        /// </summary>
        private void SaveShortcutSettings()
        {
            try
            {
                // 如果需要将快捷键设置保存到 JSON 中
                /*
                var customShortcuts = ShortcutSettings.ToDictionary(s => s.Command, s => s.CurrentShortcut);
                Settings.CustomShortcutsJson = System.Text.Json.JsonSerializer.Serialize(customShortcuts);
                */
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存快捷键设置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 应用加载的设置
        /// </summary>
        private void ApplyLoadedSettings()
        {
            // 应用语言设置
            _settingsService.ApplyLanguageSettings();

            // 应用主题设置
            _settingsService.ApplyThemeSettings();
            
            // 应用播放设备设置
            ApplyPlaybackDevicesSettings();

            // 可以在这里应用其他设置
        }
        
        private void ApplyPlaybackDevicesSettings()
        {
            // 应用播放设备设置
            // 这里可以根据实际需求实现
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            try
            {
                // 保存到服务
                await _settingsService.SaveSettingsAsync();

                // 同时保存到文件（在后台线程执行）
                await Task.Run(() => SaveSettingsToFile());

                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
                // 可以在这里显示错误消息给用户
            }
        }

        [RelayCommand]
        private async Task ResetToDefaultsAsync()
        {
            try
            {
                // 重置服务中的设置
                await _settingsService.ResetToDefaultsAsync();

                // 重置快捷键设置
                foreach (var shortcut in ShortcutSettings)
                {
                    shortcut.CurrentShortcut = shortcut.DefaultShortcut;
                }

                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"重置设置失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private void SelectPage(SettingsPageType pageType)
        {
            SelectedPageType = pageType;
        }

        [RelayCommand]
        private void ApplyLanguage(string languageCode)
        {
            Settings.Language = languageCode;
            _settingsService.ApplyLanguageSettings();
            HasUnsavedChanges = true;
        }

        [RelayCommand]
        private void ApplyTheme(string themeKey)
        {
            Settings.Theme = themeKey switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                _ => ThemeVariant.Default
            };
            _settingsService.ApplyThemeSettings();
            HasUnsavedChanges = true;
        }

        [RelayCommand]
        private void ResetShortcut(ShortcutSetting shortcut)
        {
            shortcut.CurrentShortcut = shortcut.DefaultShortcut;
            HasUnsavedChanges = true;
        }

        [RelayCommand]
        private void ResetAllShortcuts()
        {
            foreach (var shortcut in ShortcutSettings)
            {
                shortcut.CurrentShortcut = shortcut.DefaultShortcut;
            }
            HasUnsavedChanges = true;
        }

        /// <summary>
        /// 获取当前选中的语言选项
        /// </summary>
        public LanguageOption? SelectedLanguage =>
            LanguageOptions.FirstOrDefault(x => x.Code == Settings.Language);

        /// <summary>
        /// 获取当前选中的主题选项
        /// </summary>
        public ThemeOption? SelectedTheme =>
            ThemeOptions.FirstOrDefault(x => x.Key == GetThemeKey(Settings.Theme));

        /// <summary>
        /// 获取当前选中的播放设备选项
        /// </summary>
        public PlaybackDeviceOption? SelectedPlaybackDevice =>
            PlaybackDeviceOptions.FirstOrDefault(x => x.IsSelected);

        /// <summary>
        /// 获取主题键值
        /// </summary>
        private string GetThemeKey(ThemeVariant theme)
        {
            return theme.Key switch
            {
                "Default" => "Default",
                "Light" => "Light",
                "Dark" => "Dark",
                _ => "Default"
            };
        }

        private void InitializePlaybackDevices()
        {
            try
            {
                // 获取系统中的MIDI输出设备
                var devices = Melanchall.DryWetMidi.Multimedia.OutputDevice.GetAll();
                var deviceList = devices.ToList();
                for (int i = 0; i < deviceList.Count; i++)
                {
                    PlaybackDeviceOptions.Add(new PlaybackDeviceOption
                    {
                        Id = i.ToString(),
                        Name = deviceList[i].Name,
                        IsSelected = false
                    });
                }
                
                // 如果没有设备，添加默认选项
                if (PlaybackDeviceOptions.Count == 0)
                {
                    PlaybackDeviceOptions.Add(new PlaybackDeviceOption
                    {
                        Id = "-1",
                        Name = "无可用设备",
                        IsSelected = false
                    });
                }
            }
            catch (Exception ex)
            {
                // 如果出现异常，添加错误选项
                PlaybackDeviceOptions.Add(new PlaybackDeviceOption
                {
                    Id = "-1",
                    Name = $"设备加载失败: {ex.Message}",
                    IsSelected = false
                });
            }
        }
    }
}