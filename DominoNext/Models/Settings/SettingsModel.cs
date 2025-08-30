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
    /// åº”ç”¨ç¨‹åºè®¾ç½®æ¨¡å‹
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
        private int _autoSaveInterval = 5; // åˆ†é’Ÿ

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
        private PlaybackDeviceOption _playbackDevice = new PlaybackDeviceOption { Id = "-1", Name = "é»˜è®¤è®¾å¤‡", IsSelected = false };
        
        [ObservableProperty]
        private string _playlistSettingsJson = "[]";  // æ·»åŠ æ’­æ”¾åˆ—è¡¨è®¾ç½®JSONå±æ€§
        
        [ObservableProperty]
        private string _playbackDevicesJson = "[]";  // æ·»åŠ æ’­æ”¾è®¾å¤‡è®¾ç½®JSONå±æ€§
=======
        // Ö÷ÌâÏà¹ØÑÕÉ«£ºÊ¹ÓÃË½ÓĞ×Ö¶Î²¢Ìá¹©¹«¿ªÊôĞÔÒÔ±ãĞòÁĞ»¯ºÍ·ÃÎÊ
        private string _backgroundColor = "#FFFAFAFA"; // ½çÃæ±³¾°
        public string BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }

        private string _noteColor = "#FF4CAF50"; // Òô·ûÌî³äÑÕÉ«
        public string NoteColor
        {
            get => _noteColor;
            set => SetProperty(ref _noteColor, value);
        }

        private string _gridLineColor = "#1F000000"; // Íø¸ñÏßÑÕÉ«£¨´øÍ¸Ã÷¶È£©
        public string GridLineColor
        {
            get => _gridLineColor;
            set => SetProperty(ref _gridLineColor, value);
        }

        private string _keyWhiteColor = "#FFFFFFFF"; // °×¼üÑÕÉ«
        public string KeyWhiteColor
        {
            get => _keyWhiteColor;
            set => SetProperty(ref _keyWhiteColor, value);
        }

        private string _keyBlackColor = "#FF1F1F1F"; // ºÚ¼üÑÕÉ«
        public string KeyBlackColor
        {
            get => _keyBlackColor;
            set => SetProperty(ref _keyBlackColor, value);
        }

        private string _selectionColor = "#800099FF"; // Ñ¡Ôñ¸ßÁÁÑÕÉ«
        public string SelectionColor
        {
            get => _selectionColor;
            set => SetProperty(ref _selectionColor, value);
        }

        // ĞÂÔö£º¸ü¶à½çÃæÔªËØÑÕÉ«
        private string _noteSelectedColor = "#FFFF9800"; // Ñ¡ÖĞÒô·ûÑÕÉ«
        public string NoteSelectedColor
        {
            get => _noteSelectedColor;
            set => SetProperty(ref _noteSelectedColor, value);
        }

        private string _noteDraggingColor = "#FF2196F3"; // ÍÏ×§Òô·ûÑÕÉ«
        public string NoteDraggingColor
        {
            get => _noteDraggingColor;
            set => SetProperty(ref _noteDraggingColor, value);
        }

        private string _notePreviewColor = "#804CAF50"; // Ô¤ÀÀÒô·ûÑÕÉ«
        public string NotePreviewColor
        {
            get => _notePreviewColor;
            set => SetProperty(ref _notePreviewColor, value);
        }

        private string _velocityIndicatorColor = "#FFFFC107"; // Á¦¶ÈÖ¸Ê¾Æ÷ÑÕÉ«
        public string VelocityIndicatorColor
        {
            get => _velocityIndicatorColor;
            set => SetProperty(ref _velocityIndicatorColor, value);
        }

        private string _measureHeaderBackgroundColor = "#FFF5F5F5"; // Ğ¡½ÚÍ·±³¾°É«
        public string MeasureHeaderBackgroundColor
        {
            get => _measureHeaderBackgroundColor;
            set => SetProperty(ref _measureHeaderBackgroundColor, value);
        }

        private string _measureLineColor = "#FF000080"; // Ğ¡½ÚÏßÑÕÉ«
        public string MeasureLineColor
        {
            get => _measureLineColor;
            set => SetProperty(ref _measureLineColor, value);
        }

        private string _measureTextColor = "#FF000000"; // Ğ¡½ÚÊı×ÖÑÕÉ«
        public string MeasureTextColor
        {
            get => _measureTextColor;
            set => SetProperty(ref _measureTextColor, value);
        }

        private string _separatorLineColor = "#FFCCCCCC"; // ·Ö¸ôÏßÑÕÉ«
        public string SeparatorLineColor
        {
            get => _separatorLineColor;
            set => SetProperty(ref _separatorLineColor, value);
        }

        private string _keyBorderColor = "#FF1F1F1F"; // ¸ÖÇÙ¼ü±ß¿òÑÕÉ«
        public string KeyBorderColor
        {
            get => _keyBorderColor;
            set => SetProperty(ref _keyBorderColor, value);
        }

        private string _keyTextWhiteColor = "#FF000000"; // °×¼üÎÄ×ÖÑÕÉ«
        public string KeyTextWhiteColor
        {
            get => _keyTextWhiteColor;
            set => SetProperty(ref _keyTextWhiteColor, value);
        }

        private string _keyTextBlackColor = "#FFFFFFFF"; // ºÚ¼üÎÄ×ÖÑÕÉ«
        public string KeyTextBlackColor
        {
            get => _keyTextBlackColor;
            set => SetProperty(ref _keyTextBlackColor, value);
        }
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68

        /// <summary>
        /// è·å–å½“å‰è¯­è¨€çš„æ˜¾ç¤ºåç§°
        /// </summary>
        public string LanguageDisplayName
        {
            get
            {
                return Language switch
                {
                    "zh-CN" => "ä¸­æ–‡ï¼ˆç®€ä½“ï¼‰",
                    "en-US" => "English",
                    "ja-JP" => "æ—¥æœ¬èª",
                    _ => Language
                };
            }
        }

        /// <summary>
        /// è·å–å½“å‰ä¸»é¢˜çš„æ˜¾ç¤ºåç§°
        /// </summary>
        public string ThemeDisplayName
        {
            get
            {
                if (Theme == ThemeVariant.Default) return "ç³»ç»Ÿé»˜è®¤";
                if (Theme == ThemeVariant.Light) return "äº®è‰²ä¸»é¢˜";
                if (Theme == ThemeVariant.Dark) return "æš—è‰²ä¸»é¢˜";
                return Theme.ToString();
            }
        }

        /// <summary>
        /// ä»é»˜è®¤æ–‡ä»¶è·¯å¾„åŠ è½½è®¾ç½®
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
                        // å±æ€§å€¼èµ‹å€¼å¹¶é€šçŸ¥å±æ€§æ›´æ”¹
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
                        // ÊÖ¶¯¸³Öµ±ÜÃâ´¥·¢ÊôĞÔ±ä¸üÍ¨Öª
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

                        // »ù´¡Ö÷ÌâÑÕÉ«
                        _backgroundColor = !string.IsNullOrEmpty(loadedSettings.BackgroundColor) ? loadedSettings.BackgroundColor : _backgroundColor;
                        _noteColor = !string.IsNullOrEmpty(loadedSettings.NoteColor) ? loadedSettings.NoteColor : _noteColor;
                        _gridLineColor = !string.IsNullOrEmpty(loadedSettings.GridLineColor) ? loadedSettings.GridLineColor : _gridLineColor;
                        _keyWhiteColor = !string.IsNullOrEmpty(loadedSettings.KeyWhiteColor) ? loadedSettings.KeyWhiteColor : _keyWhiteColor;
                        _keyBlackColor = !string.IsNullOrEmpty(loadedSettings.KeyBlackColor) ? loadedSettings.KeyBlackColor : _keyBlackColor;
                        _selectionColor = !string.IsNullOrEmpty(loadedSettings.SelectionColor) ? loadedSettings.SelectionColor : _selectionColor;

                        // À©Õ¹µÄ½çÃæÔªËØÑÕÉ«
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
                // å¦‚æœåŠ è½½å¤±è´¥ï¼Œä½¿ç”¨é»˜è®¤è®¾ç½®
                System.Diagnostics.Debug.WriteLine($"åŠ è½½é…ç½®æ–‡ä»¶å¤±è´¥: {ex.Message}");
            }
        }

        /// <summary>
        /// ä»æŒ‡å®šè·¯å¾„åŠ è½½è®¾ç½®
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
                        // å±æ€§å€¼èµ‹å€¼å¹¶é€šçŸ¥å±æ€§æ›´æ”¹
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
                        // ÊÖ¶¯¸³Öµ±ÜÃâ´¥·¢ÊôĞÔ±ä¸üÍ¨Öª
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

                        // »ù´¡Ö÷ÌâÑÕÉ«
                        _backgroundColor = !string.IsNullOrEmpty(loadedSettings.BackgroundColor) ? loadedSettings.BackgroundColor : _backgroundColor;
                        _noteColor = !string.IsNullOrEmpty(loadedSettings.NoteColor) ? loadedSettings.NoteColor : _noteColor;
                        _gridLineColor = !string.IsNullOrEmpty(loadedSettings.GridLineColor) ? loadedSettings.GridLineColor : _gridLineColor;
                        _keyWhiteColor = !string.IsNullOrEmpty(loadedSettings.KeyWhiteColor) ? loadedSettings.KeyWhiteColor : _keyWhiteColor;
                        _keyBlackColor = !string.IsNullOrEmpty(loadedSettings.KeyBlackColor) ? loadedSettings.KeyBlackColor : _keyBlackColor;
                        _selectionColor = !string.IsNullOrEmpty(loadedSettings.SelectionColor) ? loadedSettings.SelectionColor : _selectionColor;

                        // À©Õ¹µÄ½çÃæÔªËØÑÕÉ«
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
                // å¦‚æœåŠ è½½å¤±è´¥ï¼Œä½¿ç”¨é»˜è®¤è®¾ç½®
                System.Diagnostics.Debug.WriteLine($"åŠ è½½é…ç½®æ–‡ä»¶å¤±è´¥: {ex.Message}");
            }
        }

        /// <summary>
        /// ä¿å­˜è®¾ç½®åˆ°é»˜è®¤æ–‡ä»¶è·¯å¾„
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
                System.Diagnostics.Debug.WriteLine($"ä¿å­˜é…ç½®æ–‡ä»¶å¤±è´¥: {ex.Message}");
            }
        }

        /// <summary>
        /// ä¿å­˜è®¾ç½®åˆ°æŒ‡å®šè·¯å¾„
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
                System.Diagnostics.Debug.WriteLine($"ä¿å­˜é…ç½®æ–‡ä»¶å¤±è´¥: {ex.Message}");
            }
        }

        /// <summary>
        /// è·å–é…ç½®æ–‡ä»¶è·¯å¾„
        /// </summary>
        /// <returns>é…ç½®æ–‡ä»¶çš„ç»å¯¹è·¯å¾„</returns>
        private string GetConfigFilePath()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "DominoNext");

            // ç¡®ä¿ç›®å½•å­˜åœ¨
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            return Path.Combine(appFolder, ConfigFileName);
        }

        /// <summary>
        /// é‡ç½®ä¸ºé»˜è®¤è®¾ç½®
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

            // Ö÷ÌâÉ«»Ö¸´Ä¬ÈÏ£¨Ç³É«Ö÷ÌâÎª»ù×¼£©
            BackgroundColor = "#FFFAFAFA";
            NoteColor = "#FF4CAF50";
            GridLineColor = "#1F000000";
            KeyWhiteColor = "#FFFFFFFF";
            KeyBlackColor = "#FF1F1F1F";
            SelectionColor = "#800099FF";

            // À©Õ¹ÔªËØÑÕÉ«Ä¬ÈÏÖµ
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
        /// Ó¦ÓÃÉîÉ«Ö÷ÌâÄ¬ÈÏÑÕÉ« - ÓÅ»¯°æ
        /// </summary>
        public void ApplyDarkThemeDefaults()
        {
            // ÉîÉ«Ö÷½çÃæ
            BackgroundColor = "#FF1E1E1E";
            NoteColor = "#FF66BB6A";
            GridLineColor = "#40FFFFFF";
            
            // ¸ÖÇÙ¼üÓÅ»¯£ºÌá¸ß¶Ô±È¶È
            KeyWhiteColor = "#FF2D2D30";  // Éî»ÒÉ«°×¼ü
            KeyBlackColor = "#FF0F0F0F";  // ¸üÉîµÄºÚ¼ü
            KeyBorderColor = "#FF404040"; // ±ß¿òÑÕÉ«
            KeyTextWhiteColor = "#FFCCCCCC"; // °×¼üÎÄ×Ö
            KeyTextBlackColor = "#FF999999"; // ºÚ¼üÎÄ×Ö
            
            SelectionColor = "#8064B5F6";

            // Òô·ûÑÕÉ«ÓÅ»¯
            NoteSelectedColor = "#FFFFB74D";
            NoteDraggingColor = "#FF64B5F6";
            NotePreviewColor = "#8066BB6A";
            VelocityIndicatorColor = "#FFFFCA28";
            
            // ½çÃæÔªËØÓÅ»¯
            MeasureHeaderBackgroundColor = "#FF252526";
            MeasureLineColor = "#FF6495ED";
            MeasureTextColor = "#FFE0E0E0";
            SeparatorLineColor = "#FF3E3E42";
        }

        /// <summary>
        /// Ó¦ÓÃÇ³É«Ö÷ÌâÄ¬ÈÏÑÕÉ«
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