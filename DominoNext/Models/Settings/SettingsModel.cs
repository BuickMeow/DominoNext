using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DominoNext.Models.Settings
{
    /// <summary>
    /// 应用程序设置模型
    /// </summary>
    public partial class SettingsModel : ObservableObject
    {
        [ObservableProperty]
        private string _language = "zh-CN";

        [ObservableProperty]
        private ThemeVariant _theme = ThemeVariant.Default;

        [ObservableProperty]
        private bool _autoSave = true;

        [ObservableProperty]
        private int _autoSaveInterval = 5; // 分钟

        [ObservableProperty]
        private bool _showGridLines = true;

        [ObservableProperty]
        private bool _snapToGrid = true;

        [ObservableProperty]
        private double _defaultZoom = 1.0;

        [ObservableProperty]
        private bool _useNativeMenuBar = false;

        [ObservableProperty]
        private int _maxUndoSteps = 100;

        [ObservableProperty]
        private bool _confirmBeforeDelete = true;

        [ObservableProperty]
        private bool _showVelocityBars = true;

        [ObservableProperty]
        private double _pianoKeyWidth = 60.0;

        [ObservableProperty]
        private bool _enableKeyboardShortcuts = true;

        [ObservableProperty]
        private string _customShortcutsJson = "{}";

        /// <summary>
        /// 获取当前语言的显示名称
        /// </summary>
        public string LanguageDisplayName
        {
            get
            {
                return Language switch
                {
                    "zh-CN" => "简体中文",
                    "en-US" => "English",
                    "ja-JP" => "日本語",
                    _ => Language
                };
            }
        }

        /// <summary>
        /// 获取当前主题的显示名称
        /// </summary>
        public string ThemeDisplayName
        {
            get
            {
                if (Theme == ThemeVariant.Default) return "跟随系统";
                if (Theme == ThemeVariant.Light) return "浅色主题";
                if (Theme == ThemeVariant.Dark) return "深色主题";
                return Theme.ToString();
            }
        }

        /// <summary>
        /// 从配置文件加载设置
        /// </summary>
        public void LoadFromFile()
        {
            // TODO: 实现从文件加载设置
        }

        /// <summary>
        /// 保存设置到配置文件
        /// </summary>
        public void SaveToFile()
        {
            // TODO: 实现保存设置到文件
        }

        /// <summary>
        /// 重置为默认设置
        /// </summary>
        public void ResetToDefaults()
        {
            Language = "zh-CN";
            Theme = ThemeVariant.Default;
            AutoSave = true;
            AutoSaveInterval = 5;
            ShowGridLines = true;
            SnapToGrid = true;
            DefaultZoom = 1.0;
            UseNativeMenuBar = false;
            MaxUndoSteps = 100;
            ConfirmBeforeDelete = true;
            ShowVelocityBars = true;
            PianoKeyWidth = 60.0;
            EnableKeyboardShortcuts = true;
            CustomShortcutsJson = "{}";
        }
    }
}