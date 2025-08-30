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

<<<<<<< HEAD
        [ObservableProperty]
        private double _defaultBPM = 120.0;

        [ObservableProperty]
        private PlaybackDeviceOption _playbackDevice = new PlaybackDeviceOption { Id = "-1", Name = "默认设备", IsSelected = false };
        
        [ObservableProperty]
        private string _playlistSettingsJson = "[]";  // 添加播放列表设置JSON属性
        
        [ObservableProperty]
        private string _playbackDevicesJson = "[]";  // 添加播放设备设置JSON属性
=======
        // ���������ɫ��ʹ��˽���ֶβ��ṩ���������Ա����л��ͷ���
        private string _backgroundColor = "#FFFAFAFA"; // ���汳��
        public string BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }

        private string _noteColor = "#FF4CAF50"; // ���������ɫ
        public string NoteColor
        {
            get => _noteColor;
            set => SetProperty(ref _noteColor, value);
        }

        private string _gridLineColor = "#1F000000"; // ��������ɫ����͸���ȣ�
        public string GridLineColor
        {
            get => _gridLineColor;
            set => SetProperty(ref _gridLineColor, value);
        }

        private string _keyWhiteColor = "#FFFFFFFF"; // �׼���ɫ
        public string KeyWhiteColor
        {
            get => _keyWhiteColor;
            set => SetProperty(ref _keyWhiteColor, value);
        }

        private string _keyBlackColor = "#FF1F1F1F"; // �ڼ���ɫ
        public string KeyBlackColor
        {
            get => _keyBlackColor;
            set => SetProperty(ref _keyBlackColor, value);
        }

        private string _selectionColor = "#800099FF"; // ѡ�������ɫ
        public string SelectionColor
        {
            get => _selectionColor;
            set => SetProperty(ref _selectionColor, value);
        }

        // �������������Ԫ����ɫ
        private string _noteSelectedColor = "#FFFF9800"; // ѡ��������ɫ
        public string NoteSelectedColor
        {
            get => _noteSelectedColor;
            set => SetProperty(ref _noteSelectedColor, value);
        }

        private string _noteDraggingColor = "#FF2196F3"; // ��ק������ɫ
        public string NoteDraggingColor
        {
            get => _noteDraggingColor;
            set => SetProperty(ref _noteDraggingColor, value);
        }

        private string _notePreviewColor = "#804CAF50"; // Ԥ��������ɫ
        public string NotePreviewColor
        {
            get => _notePreviewColor;
            set => SetProperty(ref _notePreviewColor, value);
        }

        private string _velocityIndicatorColor = "#FFFFC107"; // ����ָʾ����ɫ
        public string VelocityIndicatorColor
        {
            get => _velocityIndicatorColor;
            set => SetProperty(ref _velocityIndicatorColor, value);
        }

        private string _measureHeaderBackgroundColor = "#FFF5F5F5"; // С��ͷ����ɫ
        public string MeasureHeaderBackgroundColor
        {
            get => _measureHeaderBackgroundColor;
            set => SetProperty(ref _measureHeaderBackgroundColor, value);
        }

        private string _measureLineColor = "#FF000080"; // С������ɫ
        public string MeasureLineColor
        {
            get => _measureLineColor;
            set => SetProperty(ref _measureLineColor, value);
        }

        private string _measureTextColor = "#FF000000"; // С��������ɫ
        public string MeasureTextColor
        {
            get => _measureTextColor;
            set => SetProperty(ref _measureTextColor, value);
        }

        private string _separatorLineColor = "#FFCCCCCC"; // �ָ�����ɫ
        public string SeparatorLineColor
        {
            get => _separatorLineColor;
            set => SetProperty(ref _separatorLineColor, value);
        }

        private string _keyBorderColor = "#FF1F1F1F"; // ���ټ��߿���ɫ
        public string KeyBorderColor
        {
            get => _keyBorderColor;
            set => SetProperty(ref _keyBorderColor, value);
        }

        private string _keyTextWhiteColor = "#FF000000"; // �׼�������ɫ
        public string KeyTextWhiteColor
        {
            get => _keyTextWhiteColor;
            set => SetProperty(ref _keyTextWhiteColor, value);
        }

        private string _keyTextBlackColor = "#FFFFFFFF"; // �ڼ�������ɫ
        public string KeyTextBlackColor
        {
            get => _keyTextBlackColor;
            set => SetProperty(ref _keyTextBlackColor, value);
        }
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68

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
<<<<<<< HEAD
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
=======
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

                        // ����������ɫ
                        _backgroundColor = !string.IsNullOrEmpty(loadedSettings.BackgroundColor) ? loadedSettings.BackgroundColor : _backgroundColor;
                        _noteColor = !string.IsNullOrEmpty(loadedSettings.NoteColor) ? loadedSettings.NoteColor : _noteColor;
                        _gridLineColor = !string.IsNullOrEmpty(loadedSettings.GridLineColor) ? loadedSettings.GridLineColor : _gridLineColor;
                        _keyWhiteColor = !string.IsNullOrEmpty(loadedSettings.KeyWhiteColor) ? loadedSettings.KeyWhiteColor : _keyWhiteColor;
                        _keyBlackColor = !string.IsNullOrEmpty(loadedSettings.KeyBlackColor) ? loadedSettings.KeyBlackColor : _keyBlackColor;
                        _selectionColor = !string.IsNullOrEmpty(loadedSettings.SelectionColor) ? loadedSettings.SelectionColor : _selectionColor;

                        // ��չ�Ľ���Ԫ����ɫ
                        _noteSelectedColor = !string.IsNullOrEmpty(loadedSettings.NoteSelectedColor) ? loadedSettings.NoteSelectedColor : _noteSelectedColor;
                        _noteDraggingColor = !string.IsNullOrEmpty(loadedSettings.NoteDraggingColor) ? loadedSettings.NoteDraggingColor : _noteDraggingColor;
                        _notePreviewColor = !string.IsNullOrEmpty(loadedSettings.NotePreviewColor) ? loadedSettings.NotePreviewColor : _notePreviewColor;
                        _velocityIndicatorColor = !string.IsNullOrEmpty(loadedSettings.VelocityIndicatorColor) ? loadedSettings.VelocityIndicatorColor : _velocityIndicatorColor;
                        _measureHeaderBackgroundColor = !string.IsNullOrEmpty(loadedSettings.MeasureHeaderBackgroundColor) ? loadedSettings.MeasureHeaderBackgroundColor : _measureHeaderBackgroundColor;
                        _measureLineColor = !string.IsNullOrEmpty(loadedSettings.MeasureLineColor) ? loadedSettings.MeasureLineColor : _measureLineColor;
                        _measureTextColor = !string.IsNullOrEmpty(loadedSettings.MeasureTextColor) ? loadedSettings.MeasureTextColor : _measureTextColor;
                        _separatorLineColor = !string.IsNullOrEmpty(loadedSettings.SeparatorLineColor) ? loadedSettings.SeparatorLineColor : _separatorLineColor;
                        _keyBorderColor = !string.IsNullOrEmpty(loadedSettings.KeyBorderColor) ? loadedSettings.KeyBorderColor : _keyBorderColor;
                        _keyTextWhiteColor = !string.IsNullOrEmpty(loadedSettings.KeyTextWhiteColor) ? loadedSettings.KeyTextWhiteColor : _keyTextWhiteColor;
                        _keyTextBlackColor = !string.IsNullOrEmpty(loadedSettings.KeyTextBlackColor) ? loadedSettings.KeyTextBlackColor : _keyTextBlackColor;
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
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
<<<<<<< HEAD
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
=======
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

                        // ����������ɫ
                        _backgroundColor = !string.IsNullOrEmpty(loadedSettings.BackgroundColor) ? loadedSettings.BackgroundColor : _backgroundColor;
                        _noteColor = !string.IsNullOrEmpty(loadedSettings.NoteColor) ? loadedSettings.NoteColor : _noteColor;
                        _gridLineColor = !string.IsNullOrEmpty(loadedSettings.GridLineColor) ? loadedSettings.GridLineColor : _gridLineColor;
                        _keyWhiteColor = !string.IsNullOrEmpty(loadedSettings.KeyWhiteColor) ? loadedSettings.KeyWhiteColor : _keyWhiteColor;
                        _keyBlackColor = !string.IsNullOrEmpty(loadedSettings.KeyBlackColor) ? loadedSettings.KeyBlackColor : _keyBlackColor;
                        _selectionColor = !string.IsNullOrEmpty(loadedSettings.SelectionColor) ? loadedSettings.SelectionColor : _selectionColor;

                        // ��չ�Ľ���Ԫ����ɫ
                        _noteSelectedColor = !string.IsNullOrEmpty(loadedSettings.NoteSelectedColor) ? loadedSettings.NoteSelectedColor : _noteSelectedColor;
                        _noteDraggingColor = !string.IsNullOrEmpty(loadedSettings.NoteDraggingColor) ? loadedSettings.NoteDraggingColor : _noteDraggingColor;
                        _notePreviewColor = !string.IsNullOrEmpty(loadedSettings.NotePreviewColor) ? loadedSettings.NotePreviewColor : _notePreviewColor;
                        _velocityIndicatorColor = !string.IsNullOrEmpty(loadedSettings.VelocityIndicatorColor) ? loadedSettings.VelocityIndicatorColor : _velocityIndicatorColor;
                        _measureHeaderBackgroundColor = !string.IsNullOrEmpty(loadedSettings.MeasureHeaderBackgroundColor) ? loadedSettings.MeasureHeaderBackgroundColor : _measureHeaderBackgroundColor;
                        _measureLineColor = !string.IsNullOrEmpty(loadedSettings.MeasureLineColor) ? loadedSettings.MeasureLineColor : _measureLineColor;
                        _measureTextColor = !string.IsNullOrEmpty(loadedSettings.MeasureTextColor) ? loadedSettings.MeasureTextColor : _measureTextColor;
                        _separatorLineColor = !string.IsNullOrEmpty(loadedSettings.SeparatorLineColor) ? loadedSettings.SeparatorLineColor : _separatorLineColor;
                        _keyBorderColor = !string.IsNullOrEmpty(loadedSettings.KeyBorderColor) ? loadedSettings.KeyBorderColor : _keyBorderColor;
                        _keyTextWhiteColor = !string.IsNullOrEmpty(loadedSettings.KeyTextWhiteColor) ? loadedSettings.KeyTextWhiteColor : _keyTextWhiteColor;
                        _keyTextBlackColor = !string.IsNullOrEmpty(loadedSettings.KeyTextBlackColor) ? loadedSettings.KeyTextBlackColor : _keyTextBlackColor;
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
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

            // ����ɫ�ָ�Ĭ�ϣ�ǳɫ����Ϊ��׼��
            BackgroundColor = "#FFFAFAFA";
            NoteColor = "#FF4CAF50";
            GridLineColor = "#1F000000";
            KeyWhiteColor = "#FFFFFFFF";
            KeyBlackColor = "#FF1F1F1F";
            SelectionColor = "#800099FF";

            // ��չԪ����ɫĬ��ֵ
            NoteSelectedColor = "#FFFF9800";
            NoteDraggingColor = "#FF2196F3";
            NotePreviewColor = "#804CAF50";
            VelocityIndicatorColor = "#FFFFC107";
            MeasureHeaderBackgroundColor = "#FFF5F5F5";
            MeasureLineColor = "#FF000080";
            MeasureTextColor = "#FF000000";
            SeparatorLineColor = "#FFCCCCCC";
            KeyBorderColor = "#FF1F1F1F";
            KeyTextWhiteColor = "#FF000000";
            KeyTextBlackColor = "#FFFFFFFF";
        }

        /// <summary>
        /// Ӧ����ɫ����Ĭ����ɫ - �Ż���
        /// </summary>
        public void ApplyDarkThemeDefaults()
        {
            // ��ɫ������
            BackgroundColor = "#FF1E1E1E";
            NoteColor = "#FF66BB6A";
            GridLineColor = "#40FFFFFF";
            
            // ���ټ��Ż�����߶Աȶ�
            KeyWhiteColor = "#FF2D2D30";  // ���ɫ�׼�
            KeyBlackColor = "#FF0F0F0F";  // ����ĺڼ�
            KeyBorderColor = "#FF404040"; // �߿���ɫ
            KeyTextWhiteColor = "#FFCCCCCC"; // �׼�����
            KeyTextBlackColor = "#FF999999"; // �ڼ�����
            
            SelectionColor = "#8064B5F6";

            // ������ɫ�Ż�
            NoteSelectedColor = "#FFFFB74D";
            NoteDraggingColor = "#FF64B5F6";
            NotePreviewColor = "#8066BB6A";
            VelocityIndicatorColor = "#FFFFCA28";
            
            // ����Ԫ���Ż�
            MeasureHeaderBackgroundColor = "#FF252526";
            MeasureLineColor = "#FF6495ED";
            MeasureTextColor = "#FFE0E0E0";
            SeparatorLineColor = "#FF3E3E42";
        }

        /// <summary>
        /// Ӧ��ǳɫ����Ĭ����ɫ
        /// </summary>
        public void ApplyLightThemeDefaults()
        {
            BackgroundColor = "#FFFAFAFA";
            NoteColor = "#FF4CAF50";
            GridLineColor = "#1F000000";
            KeyWhiteColor = "#FFFFFFFF";
            KeyBlackColor = "#FF1F1F1F";
            SelectionColor = "#800099FF";

            NoteSelectedColor = "#FFFF9800";
            NoteDraggingColor = "#FF2196F3";
            NotePreviewColor = "#804CAF50";
            VelocityIndicatorColor = "#FFFFC107";
            MeasureHeaderBackgroundColor = "#FFF5F5F5";
            MeasureLineColor = "#FF000080";
            MeasureTextColor = "#FF000000";
            SeparatorLineColor = "#FFCCCCCC";
            KeyBorderColor = "#FF1F1F1F";
            KeyTextWhiteColor = "#FF000000";
            KeyTextBlackColor = "#FFFFFFFF";
        }
    }
}