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

        // 主题选项
        public ObservableCollection<ThemeOption> ThemeOptions { get; } = new()
        {
            new ThemeOption { Key = "Default", Name = "跟随系统", Description = "跟随系统主题设置" },
            new ThemeOption { Key = "Light", Name = "浅色主题", Description = "使用浅色主题" },
            new ThemeOption { Key = "Dark", Name = "深色主题", Description = "使用深色主题" }
        };

        // 快捷键设置
        public ObservableCollection<ShortcutSetting> ShortcutSettings { get; } = new();

        public SettingsWindowViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            InitializePages();
            InitializeShortcutSettings();

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
    }
}