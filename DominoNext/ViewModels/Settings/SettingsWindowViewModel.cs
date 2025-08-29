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

        [ObservableProperty]
        private string _selectedThemeKey = "Default";

        [ObservableProperty]
        private string _selectedLanguageCode = "zh-CN";

        public SettingsModel Settings => _settingsService.Settings;

        public ObservableCollection<SettingsPageInfo> Pages { get; } = new();

        // 语言选项
        public ObservableCollection<LanguageOption> LanguageOptions { get; } = new()
        {
            new LanguageOption { Code = "zh-CN", Name = "简体中文", NativeName = "简体中文" },
            new LanguageOption { Code = "en-US", Name = "English", NativeName = "English" },
            new LanguageOption { Code = "ja-JP", Name = "Japanese", NativeName = "日本語" }
        };

        // 主题选项 - 包含预设的精美主题
        public ObservableCollection<ThemeOption> ThemeOptions { get; } = new()
        {
            new ThemeOption { Key = "Default", Name = "跟随系统", Description = "跟随系统主题设置" },
            new ThemeOption { Key = "Light", Name = "浅色主题", Description = "经典的浅色主题，适合日间使用" },
            new ThemeOption { Key = "Dark", Name = "深色主题", Description = "深色主题，减少眼部疲劳" },
            new ThemeOption { Key = "Green", Name = "青春绿", Description = "清新的绿色主题，充满活力" },
            new ThemeOption { Key = "Blue", Name = "蓝色科技", Description = "科技感的蓝色主题，现代简约" },
            new ThemeOption { Key = "Purple", Name = "紫色梦幻", Description = "梦幻的紫色主题，优雅神秘" },
            new ThemeOption { Key = "Custom", Name = "自定义", Description = "完全自定义的颜色主题，随心所欲" }
        };

        // 颜色设置项集合 - 按分类组织
        public ObservableCollection<ColorSettingGroup> ColorSettingGroups { get; } = new();

        // 快捷键设置
        public ObservableCollection<ShortcutSetting> ShortcutSettings { get; } = new();

        /// <summary>
        /// 是否显示自定义主题面板
        /// </summary>
        public bool IsCustomThemeSelected => SelectedThemeKey == "Custom";

        public SettingsWindowViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            InitializePages();
            InitializeShortcutSettings();
            InitializeColorSettingGroups();

            // 加载设置
            LoadSettings();

            // 监听设置变更，实现自动保存
            Settings.PropertyChanged += (sender, e) => 
            {
                HasUnsavedChanges = true;
                AutoSave();
            };
        }

        // 设计时使用的无参构造函数
        public SettingsWindowViewModel() : this(new DominoNext.Services.Implementation.SettingsService())
        {
        }

        partial void OnSelectedThemeKeyChanged(string value)
        {
            OnPropertyChanged(nameof(IsCustomThemeSelected));
        }

        private void InitializePages()
        {
            Pages.Clear();
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.General,
                Title = "常规",
                Icon = "??",
                Description = "基本应用设置"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Language,
                Title = "语言",
                Icon = "??",
                Description = "界面语言设置"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Theme,
                Title = "主题",
                Icon = "??",
                Description = "界面主题和外观"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Editor,
                Title = "编辑器",
                Icon = "??",
                Description = "编辑器行为设置"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Shortcuts,
                Title = "快捷键",
                Icon = "??",
                Description = "键盘快捷键设置"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Advanced,
                Title = "高级",
                Icon = "???",
                Description = "高级选项和调试"
            });
        }

        private void InitializeColorSettingGroups()
        {
            ColorSettingGroups.Clear();

            // 基础界面颜色组
            var interfaceGroup = new ColorSettingGroup("界面", "主界面相关的颜色设置");
            interfaceGroup.Items.Add(new ColorSettingItem("界面背景", "BackgroundColor", "主界面的背景颜色", "Interface"));
            interfaceGroup.Items.Add(new ColorSettingItem("网格线", "GridLineColor", "编辑器网格线颜色", "Interface"));
            interfaceGroup.Items.Add(new ColorSettingItem("选择框", "SelectionColor", "选择框的颜色", "Interface"));
            interfaceGroup.Items.Add(new ColorSettingItem("分隔线", "SeparatorLineColor", "各种分隔线的颜色", "Interface"));
            ColorSettingGroups.Add(interfaceGroup);

            // 钢琴键颜色组
            var pianoGroup = new ColorSettingGroup("钢琴键", "钢琴键盘相关的颜色设置");
            pianoGroup.Items.Add(new ColorSettingItem("白键", "KeyWhiteColor", "钢琴白键颜色", "Piano"));
            pianoGroup.Items.Add(new ColorSettingItem("黑键", "KeyBlackColor", "钢琴黑键颜色", "Piano"));
            pianoGroup.Items.Add(new ColorSettingItem("键盘边框", "KeyBorderColor", "钢琴键边框颜色", "Piano"));
            pianoGroup.Items.Add(new ColorSettingItem("白键文字", "KeyTextWhiteColor", "白键上的文字颜色", "Piano"));
            pianoGroup.Items.Add(new ColorSettingItem("黑键文字", "KeyTextBlackColor", "黑键上的文字颜色", "Piano"));
            ColorSettingGroups.Add(pianoGroup);

            // 音符颜色组
            var noteGroup = new ColorSettingGroup("音符", "音符相关的颜色设置");
            noteGroup.Items.Add(new ColorSettingItem("普通音符", "NoteColor", "普通音符的填充颜色", "Note"));
            noteGroup.Items.Add(new ColorSettingItem("选中音符", "NoteSelectedColor", "选中音符的颜色", "Note"));
            noteGroup.Items.Add(new ColorSettingItem("拖拽音符", "NoteDraggingColor", "拖拽中音符的颜色", "Note"));
            noteGroup.Items.Add(new ColorSettingItem("预览音符", "NotePreviewColor", "预览音符的颜色", "Note"));
            noteGroup.Items.Add(new ColorSettingItem("力度指示器", "VelocityIndicatorColor", "音符力度指示器颜色", "Note"));
            ColorSettingGroups.Add(noteGroup);

            // 小节和文字组
            var measureGroup = new ColorSettingGroup("小节", "小节和文字相关的颜色设置");
            measureGroup.Items.Add(new ColorSettingItem("小节头背景", "MeasureHeaderBackgroundColor", "小节头的背景颜色", "Measure"));
            measureGroup.Items.Add(new ColorSettingItem("小节线", "MeasureLineColor", "小节分隔线颜色", "Measure"));
            measureGroup.Items.Add(new ColorSettingItem("小节文字", "MeasureTextColor", "小节数字的颜色", "Measure"));
            ColorSettingGroups.Add(measureGroup);
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

            // 工具
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "PencilTool",
                Description = "铅笔工具",
                DefaultShortcut = "P",
                CurrentShortcut = "P",
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
                // 从文件加载设置（但不重新应用，避免覆盖当前运行状态）
                Settings.LoadFromFile();

                // 更新当前选择状态
                UpdateCurrentSelections();

                // 不要重新应用设置，因为这会覆盖当前运行的主题
                // ApplyLoadedSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
                // 使用默认设置
                UpdateCurrentSelections();
            }
        }

        /// <summary>
        /// 更新当前选择状态
        /// </summary>
        public void UpdateCurrentSelections()
        {
            // 更新选中的语言
            SelectedLanguageCode = Settings.Language;

            // 更新选中的主题 - 基于当前设置判断主题类型
            SelectedThemeKey = DetermineCurrentThemeKey();

            // 通知属性变更
            OnPropertyChanged(nameof(IsCustomThemeSelected));
        }

        /// <summary>
        /// 根据当前颜色设置判断主题类型
        /// </summary>
        private string DetermineCurrentThemeKey()
        {
            // 检查是否匹配预设主题
            if (IsMatchingLightTheme()) return "Light";
            if (IsMatchingDarkTheme()) return "Dark";
            if (IsMatchingGreenTheme()) return "Green";
            if (IsMatchingBlueTheme()) return "Blue";
            if (IsMatchingPurpleTheme()) return "Purple";
            
            // 如果不匹配任何预设主题，则为自定义
            return "Custom";
        }

        private bool IsMatchingLightTheme()
        {
            return Settings.BackgroundColor == "#FFFAFAFA" &&
                   Settings.NoteColor == "#FF4CAF50" &&
                   Settings.KeyWhiteColor == "#FFFFFFFF" &&
                   Settings.KeyBlackColor == "#FF1F1F1F";
        }

        private bool IsMatchingDarkTheme()
        {
            return Settings.BackgroundColor == "#FF1E1E1E" &&
                   Settings.NoteColor == "#FF66BB6A" &&
                   Settings.KeyWhiteColor == "#FF2D2D30" &&
                   Settings.KeyBlackColor == "#FF0F0F0F";
        }

        private bool IsMatchingGreenTheme()
        {
            return Settings.BackgroundColor == "#FFF1F8E9" &&
                   Settings.NoteColor == "#FF66BB6A" &&
                   Settings.KeyWhiteColor == "#FFFAFAFA" &&
                   Settings.KeyBlackColor == "#FF2E7D32";
        }

        private bool IsMatchingBlueTheme()
        {
            return Settings.BackgroundColor == "#FFE3F2FD" &&
                   Settings.NoteColor == "#FF42A5F5" &&
                   Settings.KeyWhiteColor == "#FFFAFAFA" &&
                   Settings.KeyBlackColor == "#FF0D47A1";
        }

        private bool IsMatchingPurpleTheme()
        {
            return Settings.BackgroundColor == "#FFF3E5F5" &&
                   Settings.NoteColor == "#FFAB47BC" &&
                   Settings.KeyWhiteColor == "#FFFAFAFA" &&
                   Settings.KeyBlackColor == "#FF4A148C";
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
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            try
            {
                // 保存到服务
                await _settingsService.SaveSettingsAsync();
                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
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

                // 更新当前选择状态
                UpdateCurrentSelections();

                // 自动保存会由Settings属性变更触发，不需要手动调用
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
            SelectedLanguageCode = languageCode;
            _settingsService.ApplyLanguageSettings();
            
            // 自动保存
            AutoSave();
        }

        [RelayCommand]
        private void ApplyTheme(string themeKey)
        {
            SelectedThemeKey = themeKey;

            Settings.Theme = themeKey switch
            {
                "Light" => ThemeVariant.Light,
                "Dark" => ThemeVariant.Dark,
                "Green" => ThemeVariant.Default,
                "Blue" => ThemeVariant.Default,
                "Purple" => ThemeVariant.Default,
                "Custom" => ThemeVariant.Default,
                _ => ThemeVariant.Default
            };

            // 根据主题应用对应的颜色设置
            switch (themeKey)
            {
                case "Light":
                    Settings.ApplyLightThemeDefaults();
                    break;
                case "Dark":
                    Settings.ApplyDarkThemeDefaults();
                    break;
                case "Green":
                    ApplyGreenTheme();
                    break;
                case "Blue":
                    ApplyBlueTheme();
                    break;
                case "Purple":
                    ApplyPurpleTheme();
                    break;
                case "Custom":
                    // 自定义主题不自动应用任何颜色，保持用户设置
                    break;
            }

            _settingsService.ApplyThemeSettings();
            
            // 自动保存
            AutoSave();
        }

        /// <summary>
        /// 自动保存设置
        /// </summary>
        private async void AutoSave()
        {
            try
            {
                await _settingsService.SaveSettingsAsync();
                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"自动保存设置失败: {ex.Message}");
                HasUnsavedChanges = true;
            }
        }

        private void AutoSwitchToCustomTheme()
        {
            if (SelectedThemeKey != "Custom")
            {
                SelectedThemeKey = "Custom";
                OnPropertyChanged(nameof(IsCustomThemeSelected));
            }
            
            _settingsService.ApplyThemeSettings();
            
            // 自动保存
            AutoSave();
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
            Settings.SeparatorLineColor = "#FF81C784";
            Settings.KeyBorderColor = "#FF1B5E20";
            Settings.KeyTextWhiteColor = "#FF1B5E20";
            Settings.KeyTextBlackColor = "#FFFFFFFF";
            Settings.VelocityIndicatorColor = "#FF8BC34A";
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
            Settings.SeparatorLineColor = "#FF64B5F6";
            Settings.KeyBorderColor = "#FF0D47A1";
            Settings.KeyTextWhiteColor = "#FF0D47A1";
            Settings.KeyTextBlackColor = "#FFFFFFFF";
            Settings.VelocityIndicatorColor = "#FF03A9F4";
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
            Settings.SeparatorLineColor = "#FFCE93D8";
            Settings.KeyBorderColor = "#FF4A148C";
            Settings.KeyTextWhiteColor = "#FF4A148C";
            Settings.KeyTextBlackColor = "#FFFFFFFF";
            Settings.VelocityIndicatorColor = "#FFBA68C8";
        }

        [RelayCommand]
        private void ResetShortcut(ShortcutSetting shortcut)
        {
            shortcut.CurrentShortcut = shortcut.DefaultShortcut;
            AutoSave();
        }

        [RelayCommand]
        private void ResetAllShortcuts()
        {
            foreach (var shortcut in ShortcutSettings)
            {
                shortcut.CurrentShortcut = shortcut.DefaultShortcut;
            }
            AutoSave();
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
                
                // 如果用户修改了颜色，自动切换到自定义主题
                if (SelectedThemeKey != "Custom")
                {
                    SelectedThemeKey = "Custom";
                    OnPropertyChanged(nameof(IsCustomThemeSelected));
                }
                
                _settingsService.ApplyThemeSettings();
                HasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// 重置所有颜色为当前主题的默认值
        /// </summary>
        [RelayCommand]
        private void ResetAllColors()
        {
            ApplyTheme(SelectedThemeKey);
        }

        /// <summary>
        /// 为特定颜色属性创建绑定用的Command
        /// </summary>
        [RelayCommand]
        private void UpdateColor(object parameter)
        {
            if (parameter is (string propertyName, string colorValue))
            {
                SetColorValue(propertyName, colorValue);
            }
        }

        // 为每个颜色属性创建特定的属性
        public string BackgroundColorValue 
        { 
            get => Settings.BackgroundColor; 
            set { Settings.BackgroundColor = value; OnPropertyChanged(); }
        }

        public string NoteColorValue 
        { 
            get => Settings.NoteColor; 
            set { Settings.NoteColor = value; OnPropertyChanged(); }
        }

        public string GridLineColorValue 
        { 
            get => Settings.GridLineColor; 
            set { Settings.GridLineColor = value; OnPropertyChanged(); }
        }

        public string KeyWhiteColorValue 
        { 
            get => Settings.KeyWhiteColor; 
            set { Settings.KeyWhiteColor = value; OnPropertyChanged(); }
        }

        public string KeyBlackColorValue 
        { 
            get => Settings.KeyBlackColor; 
            set { Settings.KeyBlackColor = value; OnPropertyChanged(); }
        }

        public string SelectionColorValue 
        { 
            get => Settings.SelectionColor; 
            set { Settings.SelectionColor = value; OnPropertyChanged(); }
        }

        public string NoteSelectedColorValue 
        { 
            get => Settings.NoteSelectedColor; 
            set { Settings.NoteSelectedColor = value; OnPropertyChanged(); }
        }

        public string NoteDraggingColorValue 
        { 
            get => Settings.NoteDraggingColor; 
            set { Settings.NoteDraggingColor = value; OnPropertyChanged(); }
        }

        public string NotePreviewColorValue 
        { 
            get => Settings.NotePreviewColor; 
            set { Settings.NotePreviewColor = value; OnPropertyChanged(); }
        }

        public string VelocityIndicatorColorValue 
        { 
            get => Settings.VelocityIndicatorColor; 
            set { Settings.VelocityIndicatorColor = value; OnPropertyChanged(); }
        }

        public string MeasureHeaderBackgroundColorValue 
        { 
            get => Settings.MeasureHeaderBackgroundColor; 
            set { Settings.MeasureHeaderBackgroundColor = value; OnPropertyChanged(); }
        }

        public string MeasureLineColorValue 
        { 
            get => Settings.MeasureLineColor; 
            set { Settings.MeasureLineColor = value; OnPropertyChanged(); }
        }

        public string MeasureTextColorValue 
        { 
            get => Settings.MeasureTextColor; 
            set { Settings.MeasureTextColor = value; OnPropertyChanged(); }
        }

        public string SeparatorLineColorValue 
        { 
            get => Settings.SeparatorLineColor; 
            set { Settings.SeparatorLineColor = value; OnPropertyChanged(); }
        }

        public string KeyBorderColorValue 
        { 
            get => Settings.KeyBorderColor; 
            set { Settings.KeyBorderColor = value; OnPropertyChanged(); }
        }

        public string KeyTextWhiteColorValue 
        { 
            get => Settings.KeyTextWhiteColor; 
            set { Settings.KeyTextWhiteColor = value; OnPropertyChanged(); }
        }

        public string KeyTextBlackColorValue 
        { 
            get => Settings.KeyTextBlackColor; 
            set { Settings.KeyTextBlackColor = value; OnPropertyChanged(); }
        }
    }
}