using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Styling;
using System.Runtime.CompilerServices;

namespace DominoNext.Models.Settings
{
    /// <summary>
    /// 应用程序设置模型
    /// </summary>
    public class SettingsModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _language = "zh-CN";
        public string Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    OnPropertyChanged();
                }
            }
        }

        private ThemeVariant _theme = ThemeVariant.Default;
        public ThemeVariant Theme
        {
            get => _theme;
            set
            {
                if (_theme != value)
                {
                    _theme = value;
                    OnPropertyChanged();
                    OnThemeChanged(value);
                }
            }
        }

        private bool _autoSave = true;
        public bool AutoSave
        {
            get => _autoSave;
            set
            {
                if (_autoSave != value)
                {
                    _autoSave = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _autoSaveInterval = 5;
        public int AutoSaveInterval
        {
            get => _autoSaveInterval;
            set
            {
                if (_autoSaveInterval != value)
                {
                    _autoSaveInterval = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _showGridLines = true;
        public bool ShowGridLines
        {
            get => _showGridLines;
            set
            {
                if (_showGridLines != value)
                {
                    _showGridLines = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _snapToGrid = true;
        public bool SnapToGrid
        {
            get => _snapToGrid;
            set
            {
                if (_snapToGrid != value)
                {
                    _snapToGrid = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _defaultZoom = 1.0;
        public double DefaultZoom
        {
            get => _defaultZoom;
            set
            {
                if (_defaultZoom != value)
                {
                    _defaultZoom = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _useNativeMenuBar = false;
        public bool UseNativeMenuBar
        {
            get => _useNativeMenuBar;
            set
            {
                if (_useNativeMenuBar != value)
                {
                    _useNativeMenuBar = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _maxUndoSteps = 100;
        public int MaxUndoSteps
        {
            get => _maxUndoSteps;
            set
            {
                if (_maxUndoSteps != value)
                {
                    _maxUndoSteps = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _confirmBeforeDelete = true;
        public bool ConfirmBeforeDelete
        {
            get => _confirmBeforeDelete;
            set
            {
                if (_confirmBeforeDelete != value)
                {
                    _confirmBeforeDelete = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _showVelocityBars = true;
        public bool ShowVelocityBars
        {
            get => _showVelocityBars;
            set
            {
                if (_showVelocityBars != value)
                {
                    _showVelocityBars = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _pianoKeyWidth = 60.0;
        public double PianoKeyWidth
        {
            get => _pianoKeyWidth;
            set
            {
                if (_pianoKeyWidth != value)
                {
                    _pianoKeyWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _enableKeyboardShortcuts = true;
        public bool EnableKeyboardShortcuts
        {
            get => _enableKeyboardShortcuts;
            set
            {
                if (_enableKeyboardShortcuts != value)
                {
                    _enableKeyboardShortcuts = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _customShortcutsJson = "{}";
        public string CustomShortcutsJson
        {
            get => _customShortcutsJson;
            set
            {
                if (_customShortcutsJson != value)
                {
                    _customShortcutsJson = value;
                    OnPropertyChanged();
                }
            }
        }

        private ApplicationThemeColors _themeColors = new();
        public ApplicationThemeColors ThemeColors
        {
            get => _themeColors;
            set
            {
                if (_themeColors != value)
                {
                    _themeColors = value;
                    OnPropertyChanged();
                }
            }
        }

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
            
            // 重置主题颜色
            ThemeColors = new();
            ApplyThemeColors();
        }

        /// <summary>
        /// 应用主题颜色
        /// </summary>
        public void ApplyThemeColors()
        {
            if (Theme == ThemeVariant.Dark)
            {
                ThemeColors.ResetToDarkTheme();
            }
            else
            {
                ThemeColors.ResetToLightTheme();
            }
        }

        private void OnThemeChanged(ThemeVariant value)
        {
            ApplyThemeColors();
        }
    }
}