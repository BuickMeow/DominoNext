using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DominoNext.Models.Settings;
using DominoNext.Services.Interfaces;
using DominoNext.Services.Implementation;

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
                Type = SettingsPageType.Colors, 
                Title = "颜色", 
                Icon = "🌈", 
                Description = "自定义界面颜色" 
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

        [RelayCommand]
        private void SelectPage(SettingsPageType pageType)
        {
            SelectedPageType = pageType;
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            await _settingsService.SaveSettingsAsync();
            HasUnsavedChanges = false;
        }

        [RelayCommand]
        private async Task ResetToDefaultsAsync()
        {
            await _settingsService.ResetToDefaultsAsync();
            HasUnsavedChanges = false;
        }

        [RelayCommand]
        private void ApplyLanguage(string languageCode)
        {
            Settings.Language = languageCode;
            _settingsService.ApplyLanguageSettings();
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

        [RelayCommand]
        private void ResetColorsToLight()
        {
            Settings.ThemeColors.ResetToLightTheme();
            ThemeService.Instance.NotifyThemeChanged();
            HasUnsavedChanges = true;
        }

        [RelayCommand]
        private void ResetColorsToDark()
        {
            Settings.ThemeColors.ResetToDarkTheme();
            ThemeService.Instance.NotifyThemeChanged();
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
            ThemeOptions.FirstOrDefault(x => x.Key == Settings.Theme.ToString());
    }
}