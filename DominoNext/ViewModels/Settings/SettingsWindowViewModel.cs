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

        public SettingsModel Settings => _settingsService.Settings;

        public ObservableCollection<SettingsPageInfo> Pages { get; } = new();

        // 语言选项
        public ObservableCollection<LanguageOption> LanguageOptions { get; } = new()
        {
            new LanguageOption { Code = "zh-CN", Name = "简体中文", NativeName = "简体中文" },
            new LanguageOption { Code = "en-US", Name = "English", NativeName = "English" },
            new LanguageOption { Code = "ja-JP", Name = "Japanese", NativeName = "日本語" }
        };

        // 主题选项 - 扩展以支持更多主题
        public ObservableCollection<ThemeOption> ThemeOptions { get; } = new()
        {
            new ThemeOption { Key = "Default", Name = "跟随系统", Description = "跟随系统主题设置" },
            new ThemeOption { Key = "Light", Name = "浅色主题", Description = "使用浅色主题" },
            new ThemeOption { Key = "Dark", Name = "深色主题", Description = "使用深色主题" },
            new ThemeOption { Key = "HighContrast", Name = "高对比度", Description = "高对比度主题，提高可访问性" },
            new ThemeOption { Key = "Custom", Name = "自定义", Description = "完全自定义的颜色主题" }
        };

        // 颜色设置项集合 - 新增
        public ObservableCollection<ColorSettingItem> ColorSettings { get; } = new();

        // 预设主题集合 - 新增
        public ObservableCollection<PresetTheme> PresetThemes { get; } = new();

        // 快捷键设置
        public ObservableCollection<ShortcutSetting> ShortcutSettings { get; } = new();

        public SettingsWindowViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            InitializePages();
            InitializeShortcutSettings();
            InitializeColorSettings();
            InitializePresetThemes();

            // 加载设置
            LoadSettings();

            // 监听设置变更
            Settings.PropertyChanged += (sender, e) => HasUnsavedChanges = true;
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
                Type = SettingsPageType.Advanced,
                Title = "高级",
                Icon = "🛠️",
                Description = "高级选项和调试"
            });
        }

        private void InitializeColorSettings()
        {
            ColorSettings.Clear();

            // 基础界面颜色
            ColorSettings.Add(new ColorSettingItem("界面背景", "BackgroundColor", "主界面的背景颜色", "Interface"));
            ColorSettings.Add(new ColorSettingItem("网格线", "GridLineColor", "编辑器网格线颜色", "Interface"));
            ColorSettings.Add(new ColorSettingItem("选择框", "SelectionColor", "选择框的颜色", "Interface"));

            // 钢琴键颜色
            ColorSettings.Add(new ColorSettingItem("白键", "KeyWhiteColor", "钢琴白键颜色", "Piano"));
            ColorSettings.Add(new ColorSettingItem("黑键", "KeyBlackColor", "钢琴黑键颜色", "Piano"));
            ColorSettings.Add(new ColorSettingItem("键盘边框", "KeyBorderColor", "钢琴键边框颜色", "Piano"));
            ColorSettings.Add(new ColorSettingItem("白键文字", "KeyTextWhiteColor", "白键上的文字颜色", "Piano"));
            ColorSettings.Add(new ColorSettingItem("黑键文字", "KeyTextBlackColor", "黑键上的文字颜色", "Piano"));

            // 音符颜色
            ColorSettings.Add(new ColorSettingItem("普通音符", "NoteColor", "普通音符的填充颜色", "Note"));
            ColorSettings.Add(new ColorSettingItem("选中音符", "NoteSelectedColor", "选中音符的颜色", "Note"));
            ColorSettings.Add(new ColorSettingItem("拖拽音符", "NoteDraggingColor", "拖拽中音符的颜色", "Note"));
            ColorSettings.Add(new ColorSettingItem("预览音符", "NotePreviewColor", "预览音符的颜色", "Note"));
            ColorSettings.Add(new ColorSettingItem("力度指示器", "VelocityIndicatorColor", "音符力度指示器颜色", "Note"));

            // 小节和文字
            ColorSettings.Add(new ColorSettingItem("小节头背景", "MeasureHeaderBackgroundColor", "小节头的背景颜色", "Measure"));
            ColorSettings.Add(new ColorSettingItem("小节线", "MeasureLineColor", "小节分隔线颜色", "Measure"));
            ColorSettings.Add(new ColorSettingItem("小节文字", "MeasureTextColor", "小节数字的颜色", "Measure"));
            ColorSettings.Add(new ColorSettingItem("分隔线", "SeparatorLineColor", "各种分隔线的颜色", "Measure"));
        }

        private void InitializePresetThemes()
        {
            PresetThemes.Clear();

            PresetThemes.Add(new PresetTheme 
            { 
                Name = "经典浅色", 
                Description = "经典的浅色主题，适合日间使用",
                ApplyAction = () => Settings.ApplyLightThemeDefaults()
            });

            PresetThemes.Add(new PresetTheme 
            { 
                Name = "深色护眼", 
                Description = "深色主题，减少眼部疲劳",
                ApplyAction = () => Settings.ApplyDarkThemeDefaults()
            });

            PresetThemes.Add(new PresetTheme 
            { 
                Name = "青春绿", 
                Description = "清新的绿色主题",
                ApplyAction = () => ApplyGreenTheme()
            });

            PresetThemes.Add(new PresetTheme 
            { 
                Name = "蓝色科技", 
                Description = "科技感的蓝色主题",
                ApplyAction = () => ApplyBlueTheme()
            });

            PresetThemes.Add(new PresetTheme 
            { 
                Name = "紫色梦幻", 
                Description = "梦幻的紫色主题",
                ApplyAction = () => ApplyPurpleTheme()
            });
        }

        private void ApplyGreenTheme()
        {
            Settings.BackgroundColor = "#FFF1F8E9";
            Settings.NoteColor = "#FF66BB6A";
            Settings.NoteSelectedColor = "#FFFF8A65";
            Settings.NoteDraggingColor = "#FF26A69A";
            Settings.NotePreviewColor = "#8066BB6A";
            Settings.GridLineColor = "#20388E3C";
            Settings.KeyWhiteColor = "#FFFAFAFA";
            Settings.KeyBlackColor = "#FF2E7D32";
            Settings.SelectionColor = "#8026A69A";
            Settings.MeasureHeaderBackgroundColor = "#FFE8F5E8";
            Settings.MeasureLineColor = "#FF4CAF50";
            Settings.MeasureTextColor = "#FF1B5E20";
        }

        private void ApplyBlueTheme()
        {
            Settings.BackgroundColor = "#FFE3F2FD";
            Settings.NoteColor = "#FF42A5F5";
            Settings.NoteSelectedColor = "#FFFF7043";
            Settings.NoteDraggingColor = "#FF1E88E5";
            Settings.NotePreviewColor = "#8042A5F5";
            Settings.GridLineColor = "#201976D2";
            Settings.KeyWhiteColor = "#FFFAFAFA";
            Settings.KeyBlackColor = "#FF0D47A1";
            Settings.SelectionColor = "#801E88E5";
            Settings.MeasureHeaderBackgroundColor = "#FFE1F5FE";
            Settings.MeasureLineColor = "#FF2196F3";
            Settings.MeasureTextColor = "#FF0D47A1";
        }

        private void ApplyPurpleTheme()
        {
            Settings.BackgroundColor = "#FFF3E5F5";
            Settings.NoteColor = "#FFAB47BC";
            Settings.NoteSelectedColor = "#FFFF8A65";
            Settings.NoteDraggingColor = "#FF8E24AA";
            Settings.NotePreviewColor = "#80AB47BC";
            Settings.GridLineColor = "#204A148C";
            Settings.KeyWhiteColor = "#FFFAFAFA";
            Settings.KeyBlackColor = "#FF4A148C";
            Settings.SelectionColor = "#808E24AA";
            Settings.MeasureHeaderBackgroundColor = "#FFEDE7F6";
            Settings.MeasureLineColor = "#FF9C27B0";
            Settings.MeasureTextColor = "#FF4A148C";
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
                Command = "SelectTool",
                Description = "选择工具",
                DefaultShortcut = "S",
                CurrentShortcut = "S",
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
        }

        /// <summary>
        /// 从配置文件加载设置
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // 从文件加载设置
                Settings.LoadFromFile();

                // 更新快捷键设置（如果存储在设置中）
                LoadShortcutSettings();

                // 应用加载的设置
                ApplyLoadedSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
                // 使用默认设置
            }
        }

        /// <summary>
        /// 保存设置到配置文件（同步版本）
        /// </summary>
        private void SaveSettingsToFile()
        {
            try
            {
                // 保存快捷键设置到设置模型（如果需要）
                SaveShortcutSettings();

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

            // 可以在这里应用其他设置
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

            // 根据主题自动应用对应的默认颜色
            if (themeKey == "Light")
            {
                Settings.ApplyLightThemeDefaults();
            }
            else if (themeKey == "Dark")
            {
                Settings.ApplyDarkThemeDefaults();
            }

            _settingsService.ApplyThemeSettings();
            HasUnsavedChanges = true;
        }

        [RelayCommand]
        private void ApplyPresetTheme(PresetTheme presetTheme)
        {
            presetTheme.ApplyAction?.Invoke();
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

        /// <summary>
        /// 获取指定颜色设置项对应的颜色值
        /// </summary>
        public string GetColorValue(string propertyName)
        {
            var property = typeof(SettingsModel).GetProperty(propertyName);
            return property?.GetValue(Settings) as string ?? "#FFFFFFFF";
        }

        /// <summary>
        /// 设置指定颜色设置项的颜色值
        /// </summary>
        public void SetColorValue(string propertyName, string colorValue)
        {
            var property = typeof(SettingsModel).GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(Settings, colorValue);
                HasUnsavedChanges = true;
            }
        }
    }
}