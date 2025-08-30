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
        private bool _useNativeMenuBar = true;

        [ObservableProperty]
        private int _maxUndoSteps = 50;

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

        [ObservableProperty]
        private double _defaultBPM = 120.0;

        [ObservableProperty]
        private PlaybackDeviceOption _playbackDevice = new PlaybackDeviceOption { Id = "-1", Name = "默认设备", IsSelected = false };
        
        [ObservableProperty]
        private string _playlistSettingsJson = "[]";  // 添加播放列表设置JSON属性
        
        [ObservableProperty]
        private string _playbackDevicesJson = "[]";  // 添加播放设备设置JSON属性

        /// <summary>
        /// 获取当前语言的显示名称
        /// </summary>
        public string LanguageDisplayName
        {
            get
            {
                return Language switch
                {
                    "zh-CN" => "中文（简体）",
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
                if (Theme == ThemeVariant.Default) return "系统默认";
                if (Theme == ThemeVariant.Light) return "亮色主题";
                if (Theme == ThemeVariant.Dark) return "暗色主题";
                return Theme.ToString();
            }
        }

        /// <summary>
        /// 从默认文件路径加载设置
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
                        // 属性值赋值并通知属性更改
                        Language = loadedSettings.Language;
                        Theme = loadedSettings.Theme;
                        AutoSave = loadedSettings.AutoSave;
                        AutoSaveInterval = loadedSettings.AutoSaveInterval;
                        ShowGridLines = loadedSettings.ShowGridLines;
                        SnapToGrid = loadedSettings.SnapToGrid;
                        DefaultZoom = loadedSettings.DefaultZoom;
                        UseNativeMenuBar = loadedSettings.UseNativeMenuBar;
                        MaxUndoSteps = loadedSettings.MaxUndoSteps;
                        ConfirmBeforeDelete = loadedSettings.ConfirmBeforeDelete;
                        ShowVelocityBars = loadedSettings.ShowVelocityBars;
                        PianoKeyWidth = loadedSettings.PianoKeyWidth;
                        EnableKeyboardShortcuts = loadedSettings.EnableKeyboardShortcuts;
                        CustomShortcutsJson = loadedSettings.CustomShortcutsJson;
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
                        // 属性值赋值并通知属性更改
                        Language = loadedSettings.Language;
                        Theme = loadedSettings.Theme;
                        AutoSave = loadedSettings.AutoSave;
                        AutoSaveInterval = loadedSettings.AutoSaveInterval;
                        ShowGridLines = loadedSettings.ShowGridLines;
                        SnapToGrid = loadedSettings.SnapToGrid;
                        DefaultZoom = loadedSettings.DefaultZoom;
                        UseNativeMenuBar = loadedSettings.UseNativeMenuBar;
                        MaxUndoSteps = loadedSettings.MaxUndoSteps;
                        ConfirmBeforeDelete = loadedSettings.ConfirmBeforeDelete;
                        ShowVelocityBars = loadedSettings.ShowVelocityBars;
                        PianoKeyWidth = loadedSettings.PianoKeyWidth;
                        EnableKeyboardShortcuts = loadedSettings.EnableKeyboardShortcuts;
                        CustomShortcutsJson = loadedSettings.CustomShortcutsJson;
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
        /// 保存设置到默认文件路径
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
        /// <returns>配置文件的绝对路径</returns>
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