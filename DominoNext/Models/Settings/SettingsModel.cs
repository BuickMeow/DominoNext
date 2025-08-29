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
    /// Ӧ�ó�������ģ��
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
        private int _autoSaveInterval = 5; // ����

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
        /// ��ȡ��ǰ���Ե���ʾ����
        /// </summary>
        public string LanguageDisplayName
        {
            get
            {
                return Language switch
                {
                    "zh-CN" => "��������",
                    "en-US" => "English",
                    "ja-JP" => "�ձ��Z",
                    _ => Language
                };
            }
        }

        /// <summary>
        /// ��ȡ��ǰ�������ʾ����
        /// </summary>
        public string ThemeDisplayName
        {
            get
            {
                if (Theme == ThemeVariant.Default) return "����ϵͳ";
                if (Theme == ThemeVariant.Light) return "ǳɫ����";
                if (Theme == ThemeVariant.Dark) return "��ɫ����";
                return Theme.ToString();
            }
        }

        /// <summary>
        /// �������ļ���������
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
                        // �ֶ���ֵ���ⴥ�����Ա��֪ͨ
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
                // �������ʧ�ܣ�ʹ��Ĭ������
                System.Diagnostics.Debug.WriteLine($"���������ļ�ʧ��: {ex.Message}");
            }
        }

        /// <summary>
        /// ��ָ��·����������
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
                        // �ֶ���ֵ���ⴥ�����Ա��֪ͨ
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
                // �������ʧ�ܣ�ʹ��Ĭ������
                System.Diagnostics.Debug.WriteLine($"���������ļ�ʧ��: {ex.Message}");
            }
        }

        /// <summary>
        /// �������õ������ļ�
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
                System.Diagnostics.Debug.WriteLine($"���������ļ�ʧ��: {ex.Message}");
            }
        }

        /// <summary>
        /// �������õ�ָ��·��
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
                System.Diagnostics.Debug.WriteLine($"���������ļ�ʧ��: {ex.Message}");
            }
        }

        /// <summary>
        /// ��ȡ�����ļ�·��
        /// </summary>
        /// <returns>�����ļ�����·��</returns>
        private string GetConfigFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "DominoNext");

            // ȷ��Ŀ¼����
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            return Path.Combine(appFolder, ConfigFileName);
        }

        /// <summary>
        /// ����ΪĬ������
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