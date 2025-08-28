using DominoNext.Models.Settings;
using DominoNext.Services.Interfaces;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;

namespace DominoNext.Services.Implementation
{
    /// <summary>
    /// 设置服务实现
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private const string SettingsFileName = "settings.json";
        private readonly string _settingsFilePath;

        public SettingsModel Settings { get; private set; }
        public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

        public SettingsService()
        {
            // 设置文件保存在用户数据目录
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "DominoNext");
            Directory.CreateDirectory(appFolder);
            _settingsFilePath = Path.Combine(appFolder, SettingsFileName);

            Settings = new SettingsModel();
            Settings.PropertyChanged += OnSettingsPropertyChanged;
        }

        private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != null)
            {
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs
                {
                    PropertyName = e.PropertyName
                });

                // 自动保存设置
                _ = Task.Run(SaveSettingsAsync);
            }
        }

        public async Task LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    // 首次运行，使用默认设置
                    await SaveSettingsAsync();
                    return;
                }

                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var loadedSettings = JsonSerializer.Deserialize<SettingsModel>(json);
                
                if (loadedSettings != null)
                {
                    // 复制属性值
                    Settings.Language = loadedSettings.Language;
                    Settings.Theme = loadedSettings.Theme;
                    Settings.AutoSave = loadedSettings.AutoSave;
                    Settings.AutoSaveInterval = loadedSettings.AutoSaveInterval;
                    Settings.ShowGridLines = loadedSettings.ShowGridLines;
                    Settings.SnapToGrid = loadedSettings.SnapToGrid;
                    Settings.DefaultZoom = loadedSettings.DefaultZoom;
                    Settings.UseNativeMenuBar = loadedSettings.UseNativeMenuBar;
                    Settings.MaxUndoSteps = loadedSettings.MaxUndoSteps;
                    Settings.ConfirmBeforeDelete = loadedSettings.ConfirmBeforeDelete;
                    Settings.ShowVelocityBars = loadedSettings.ShowVelocityBars;
                    Settings.PianoKeyWidth = loadedSettings.PianoKeyWidth;
                    Settings.EnableKeyboardShortcuts = loadedSettings.EnableKeyboardShortcuts;
                    Settings.CustomShortcutsJson = loadedSettings.CustomShortcutsJson;
                }

                // 应用设置
                ApplyLanguageSettings();
                ApplyThemeSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
                // 如果加载失败，使用默认设置
                Settings.ResetToDefaults();
            }
        }

        public async Task SaveSettingsAsync()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(Settings, options);
                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
            }
        }

        public async Task ResetToDefaultsAsync()
        {
            Settings.ResetToDefaults();
            await SaveSettingsAsync();
            
            ApplyLanguageSettings();
            ApplyThemeSettings();
        }

        public void ApplyLanguageSettings()
        {
            try
            {
                var culture = new CultureInfo(Settings.Language);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"应用语言设置失败: {ex.Message}");
            }
        }

        public void ApplyThemeSettings()
        {
            try
            {
                if (Application.Current != null)
                {
                    Application.Current.RequestedThemeVariant = Settings.Theme;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"应用主题设置失败: {ex.Message}");
            }
        }
    }
}