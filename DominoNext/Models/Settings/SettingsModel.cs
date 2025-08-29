using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DominoNext.Models.Settings
{
    /// <summary>
    /// 应用程序设置模型
    /// </summary>
    public partial class SettingsModel : ObservableObject
    {
        private static readonly string ConfigFileName = "appsettings.json";

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
            try
            {
                string configPath = GetConfigFilePath();
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var loadedSettings = JsonSerializer.Deserialize<SettingsModel>(json, options);
                    if (loadedSettings != null)
                    {
                        // 手动赋值避免触发属性变更通知
                        _language = loadedSettings.Language;
                        _theme = loadedSettings.Theme;
                        _autoSave = loadedSettings.AutoSave;
                        _autoSaveInterval = loadedSettings.AutoSaveInterval;
                        _showGridLines = loadedSettings.ShowGridLines;
                        _snapToGrid = loadedSettings.SnapToGrid;
                        _defaultZoom = loadedSettings.DefaultZoom;
                        _useNativeMenuBar = loadedSettings.UseNativeMenuBar;
                        _maxUndoSteps = loadedSettings.MaxUndoSteps;
                        _confirmBeforeDelete = loadedSettings.ConfirmBeforeDelete;
                        _showVelocityBars = loadedSettings.ShowVelocityBars;
                        _pianoKeyWidth = loadedSettings.PianoKeyWidth;
                        _enableKeyboardShortcuts = loadedSettings.EnableKeyboardShortcuts;
                        _customShortcutsJson = loadedSettings.CustomShortcutsJson;
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果加载失败，使用默认设置
                System.Diagnostics.Debug.WriteLine($"加载配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从指定路径加载设置
        /// </summary>
        public void LoadFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var loadedSettings = JsonSerializer.Deserialize<SettingsModel>(json, options);
                    if (loadedSettings != null)
                    {
                        // 手动赋值避免触发属性变更通知
                        _language = loadedSettings.Language;
                        _theme = loadedSettings.Theme;
                        _autoSave = loadedSettings.AutoSave;
                        _autoSaveInterval = loadedSettings.AutoSaveInterval;
                        _showGridLines = loadedSettings.ShowGridLines;
                        _snapToGrid = loadedSettings.SnapToGrid;
                        _defaultZoom = loadedSettings.DefaultZoom;
                        _useNativeMenuBar = loadedSettings.UseNativeMenuBar;
                        _maxUndoSteps = loadedSettings.MaxUndoSteps;
                        _confirmBeforeDelete = loadedSettings.ConfirmBeforeDelete;
                        _showVelocityBars = loadedSettings.ShowVelocityBars;
                        _pianoKeyWidth = loadedSettings.PianoKeyWidth;
                        _enableKeyboardShortcuts = loadedSettings.EnableKeyboardShortcuts;
                        _customShortcutsJson = loadedSettings.CustomShortcutsJson;
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果加载失败，使用默认设置
                System.Diagnostics.Debug.WriteLine($"加载配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存设置到配置文件
        /// </summary>
        public void SaveToFile()
        {
            try
            {
                string configPath = GetConfigFilePath();
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存设置到指定路径
        /// </summary>
        public void SaveToFile(string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        /// <returns>配置文件完整路径</returns>
        private string GetConfigFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "DominoNext");

            // 确保目录存在
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            return Path.Combine(appFolder, ConfigFileName);
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