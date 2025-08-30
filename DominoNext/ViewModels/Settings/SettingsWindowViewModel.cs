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
    /// ÉèÖÃ´°¿ÚViewModel
    /// </summary>
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private SettingsPageType _selectedPageType = SettingsPageType.General;

        [ObservableProperty]
        private bool _hasUnsavedChanges = false;

<<<<<<< HEAD
        // æ’­æ”¾åˆ—è¡¨è®¾ç½®é€‰é¡¹
        public ObservableCollection<PlaylistSetting> PlaylistSettings { get; } = new();
=======
        [ObservableProperty]
        private string _selectedThemeKey = "Default";

        [ObservableProperty]
        private string _selectedLanguageCode = "zh-CN";
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68

        public SettingsModel Settings => _settingsService.Settings;

        public ObservableCollection<SettingsPageInfo> Pages { get; } = new();

        // ÓïÑÔÑ¡Ïî
        public ObservableCollection<LanguageOption> LanguageOptions { get; } = new()
        {
            new LanguageOption { Code = "zh-CN", Name = "¼òÌåÖĞÎÄ", NativeName = "¼òÌåÖĞÎÄ" },
            new LanguageOption { Code = "en-US", Name = "English", NativeName = "English" },
            new LanguageOption { Code = "ja-JP", Name = "Japanese", NativeName = "ÈÕ±¾ÕZ" }
        };

        // Ö÷ÌâÑ¡Ïî - °üº¬Ô¤ÉèµÄ¾«ÃÀÖ÷Ìâ
        public ObservableCollection<ThemeOption> ThemeOptions { get; } = new()
        {
            new ThemeOption { Key = "Default", Name = "¸úËæÏµÍ³", Description = "¸úËæÏµÍ³Ö÷ÌâÉèÖÃ" },
            new ThemeOption { Key = "Light", Name = "Ç³É«Ö÷Ìâ", Description = "¾­µäµÄÇ³É«Ö÷Ìâ£¬ÊÊºÏÈÕ¼äÊ¹ÓÃ" },
            new ThemeOption { Key = "Dark", Name = "ÉîÉ«Ö÷Ìâ", Description = "ÉîÉ«Ö÷Ìâ£¬¼õÉÙÑÛ²¿Æ£ÀÍ" },
            new ThemeOption { Key = "Green", Name = "Çà´ºÂÌ", Description = "ÇåĞÂµÄÂÌÉ«Ö÷Ìâ£¬³äÂú»îÁ¦" },
            new ThemeOption { Key = "Blue", Name = "À¶É«¿Æ¼¼", Description = "¿Æ¼¼¸ĞµÄÀ¶É«Ö÷Ìâ£¬ÏÖ´ú¼òÔ¼" },
            new ThemeOption { Key = "Purple", Name = "×ÏÉ«ÃÎ»Ã", Description = "ÃÎ»ÃµÄ×ÏÉ«Ö÷Ìâ£¬ÓÅÑÅÉñÃØ" },
            new ThemeOption { Key = "Custom", Name = "×Ô¶¨Òå", Description = "ÍêÈ«×Ô¶¨ÒåµÄÑÕÉ«Ö÷Ìâ£¬ËæĞÄËùÓû" }
        };

        // ÑÕÉ«ÉèÖÃÏî¼¯ºÏ - °´·ÖÀà×éÖ¯
        public ObservableCollection<ColorSettingGroup> ColorSettingGroups { get; } = new();

        // ¿ì½İ¼üÉèÖÃ
        public ObservableCollection<ShortcutSetting> ShortcutSettings { get; } = new();
        
        // æ’­æ”¾è®¾å¤‡é€‰é¡¹
        public ObservableCollection<PlaybackDeviceOption> PlaybackDeviceOptions { get; } = new();

        /// <summary>
        /// ÊÇ·ñÏÔÊ¾×Ô¶¨ÒåÖ÷ÌâÃæ°å
        /// </summary>
        public bool IsCustomThemeSelected => SelectedThemeKey == "Custom";

        public SettingsWindowViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            InitializePages();
            InitializeShortcutSettings();
            InitializeColorSettingGroups();

            // ¼ÓÔØÉèÖÃ
            LoadSettings();

<<<<<<< HEAD
            // åˆå§‹åŒ–æ’­æ”¾åˆ—è¡¨è®¾ç½®
            InitializePlaylistSettings();

            // ç›‘å¬è®¾ç½®å˜æ›´
            Settings.PropertyChanged += (sender, e) => HasUnsavedChanges = true;
=======
            // ¼àÌıÉèÖÃ±ä¸ü£¬ÊµÏÖ×Ô¶¯±£´æ
            Settings.PropertyChanged += (sender, e) => 
            {
                HasUnsavedChanges = true;
                AutoSave();
            };
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
        }
        
        private void InitializePlaylistSettings()
        {
            PlaylistSettings.Clear();
            
            // æ·»åŠ é»˜è®¤æ’­æ”¾åˆ—è¡¨è®¾ç½®
            PlaylistSettings.Add(new PlaylistSetting
            {
                Name = "ä¸»æ’­æ”¾åˆ—è¡¨",
                Description = "ä¸»æ’­æ”¾åˆ—è¡¨è®¾ç½®",
                DefaultPlaybackMode = PlaybackMode.Sequential,
                DefaultRepeatMode = RepeatMode.None,
                AutoPlayNext = true
            });
            
            PlaylistSettings.Add(new PlaylistSetting
            {
                Name = "éšæœºæ’­æ”¾åˆ—è¡¨",
                Description = "éšæœºæ’­æ”¾åˆ—è¡¨è®¾ç½®",
                DefaultPlaybackMode = PlaybackMode.Random,
                DefaultRepeatMode = RepeatMode.All,
                AutoPlayNext = false
            });
        }
        
        /// <summary>
        /// åŠ è½½æ’­æ”¾åˆ—è¡¨è®¾ç½®
        /// </summary>
        private void LoadPlaylistSettings()
        {
            try
            {
                // å¦‚æœæ’­æ”¾åˆ—è¡¨è®¾ç½®å­˜å‚¨åœ¨ JSON ä¸­ï¼Œå¯ä»¥ä» Settings.PlaylistSettingsJson è§£æ
                if (!string.IsNullOrEmpty(Settings.PlaylistSettingsJson) && Settings.PlaylistSettingsJson != "{}")
                {
                    var playlistSettings = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<PlaylistSetting>>(Settings.PlaylistSettingsJson) ?? new ObservableCollection<PlaylistSetting>();
                    PlaylistSettings.Clear();
                    foreach (var playlist in playlistSettings)
                    {
                        PlaylistSettings.Add(playlist);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"åŠ è½½æ’­æ”¾åˆ—è¡¨è®¾ç½®å¤±è´¥: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ä¿å­˜æ’­æ”¾åˆ—è¡¨è®¾ç½®
        /// </summary>
        private void SavePlaylistSettings()
        {
            try
            {
                // å°†æ’­æ”¾åˆ—è¡¨è®¾ç½®ä¿å­˜åˆ° JSON ä¸­
                Settings.PlaylistSettingsJson = System.Text.Json.JsonSerializer.Serialize(PlaylistSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ä¿å­˜æ’­æ”¾åˆ—è¡¨è®¾ç½®å¤±è´¥: {ex.Message}");
            }
        }
        
        [RelayCommand]
        private void AddPlaylist()
        {
            // æ·»åŠ æ–°çš„æ’­æ”¾åˆ—è¡¨è®¾ç½®
            PlaylistSettings.Add(new PlaylistSetting
            {
                Name = $"æ’­æ”¾åˆ—è¡¨ {PlaylistSettings.Count + 1}",
                Description = "æ–°æ’­æ”¾åˆ—è¡¨",
                DefaultPlaybackMode = PlaybackMode.Sequential,
                DefaultRepeatMode = RepeatMode.None,
                AutoPlayNext = true
            });
            
            HasUnsavedChanges = true;
        }
        
        [RelayCommand]
        private void RemovePlaylist(PlaylistSetting playlist)
        {
            if (playlist != null)
            {
                PlaylistSettings.Remove(playlist);
                HasUnsavedChanges = true;
            }
        }
        
        [RelayCommand]
        private void ResetPlaylistSettings()
        {
            // é‡ç½®æ’­æ”¾åˆ—è¡¨è®¾ç½®ä¸ºé»˜è®¤å€¼
            PlaylistSettings.Clear();
            InitializePlaylistSettings();
            HasUnsavedChanges = true;
        }

        // Éè¼ÆÊ±Ê¹ÓÃµÄÎŞ²Î¹¹Ôìº¯Êı
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
                Title = "³£¹æ",
                Icon = "??",
                Description = "»ù±¾Ó¦ÓÃÉèÖÃ"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Language,
                Title = "ÓïÑÔ",
                Icon = "??",
                Description = "½çÃæÓïÑÔÉèÖÃ"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Theme,
                Title = "Ö÷Ìâ",
                Icon = "??",
                Description = "½çÃæÖ÷ÌâºÍÍâ¹Û"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Editor,
                Title = "±à¼­Æ÷",
                Icon = "??",
                Description = "±à¼­Æ÷ĞĞÎªÉèÖÃ"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Shortcuts,
                Title = "¿ì½İ¼ü",
                Icon = "??",
                Description = "¼üÅÌ¿ì½İ¼üÉèÖÃ"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Playlist,
                Title = "æ’­æ”¾åˆ—è¡¨",
                Icon = "ğŸµ",
                Description = "æ’­æ”¾åˆ—è¡¨è¡Œä¸ºè®¾ç½®"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Advanced,
                Title = "¸ß¼¶",
                Icon = "???",
                Description = "¸ß¼¶Ñ¡ÏîºÍµ÷ÊÔ"
            });
            
            // åˆå§‹åŒ–æ’­æ”¾åˆ—è¡¨è®¾ç½®é¡µé¢
            InitializePlaylistPage();
        }

        private void InitializeColorSettingGroups()
        {
            ColorSettingGroups.Clear();

            // »ù´¡½çÃæÑÕÉ«×é
            var interfaceGroup = new ColorSettingGroup("½çÃæ", "Ö÷½çÃæÏà¹ØµÄÑÕÉ«ÉèÖÃ");
            interfaceGroup.Items.Add(new ColorSettingItem("½çÃæ±³¾°", "BackgroundColor", "Ö÷½çÃæµÄ±³¾°ÑÕÉ«", "Interface"));
            interfaceGroup.Items.Add(new ColorSettingItem("Íø¸ñÏß", "GridLineColor", "±à¼­Æ÷Íø¸ñÏßÑÕÉ«", "Interface"));
            interfaceGroup.Items.Add(new ColorSettingItem("Ñ¡Ôñ¿ò", "SelectionColor", "Ñ¡Ôñ¿òµÄÑÕÉ«", "Interface"));
            interfaceGroup.Items.Add(new ColorSettingItem("·Ö¸ôÏß", "SeparatorLineColor", "¸÷ÖÖ·Ö¸ôÏßµÄÑÕÉ«", "Interface"));
            ColorSettingGroups.Add(interfaceGroup);

            // ¸ÖÇÙ¼üÑÕÉ«×é
            var pianoGroup = new ColorSettingGroup("¸ÖÇÙ¼ü", "¸ÖÇÙ¼üÅÌÏà¹ØµÄÑÕÉ«ÉèÖÃ");
            pianoGroup.Items.Add(new ColorSettingItem("°×¼ü", "KeyWhiteColor", "¸ÖÇÙ°×¼üÑÕÉ«", "Piano"));
            pianoGroup.Items.Add(new ColorSettingItem("ºÚ¼ü", "KeyBlackColor", "¸ÖÇÙºÚ¼üÑÕÉ«", "Piano"));
            pianoGroup.Items.Add(new ColorSettingItem("¼üÅÌ±ß¿ò", "KeyBorderColor", "¸ÖÇÙ¼ü±ß¿òÑÕÉ«", "Piano"));
            pianoGroup.Items.Add(new ColorSettingItem("°×¼üÎÄ×Ö", "KeyTextWhiteColor", "°×¼üÉÏµÄÎÄ×ÖÑÕÉ«", "Piano"));
            pianoGroup.Items.Add(new ColorSettingItem("ºÚ¼üÎÄ×Ö", "KeyTextBlackColor", "ºÚ¼üÉÏµÄÎÄ×ÖÑÕÉ«", "Piano"));
            ColorSettingGroups.Add(pianoGroup);

            // Òô·ûÑÕÉ«×é
            var noteGroup = new ColorSettingGroup("Òô·û", "Òô·ûÏà¹ØµÄÑÕÉ«ÉèÖÃ");
            noteGroup.Items.Add(new ColorSettingItem("ÆÕÍ¨Òô·û", "NoteColor", "ÆÕÍ¨Òô·ûµÄÌî³äÑÕÉ«", "Note"));
            noteGroup.Items.Add(new ColorSettingItem("Ñ¡ÖĞÒô·û", "NoteSelectedColor", "Ñ¡ÖĞÒô·ûµÄÑÕÉ«", "Note"));
            noteGroup.Items.Add(new ColorSettingItem("ÍÏ×§Òô·û", "NoteDraggingColor", "ÍÏ×§ÖĞÒô·ûµÄÑÕÉ«", "Note"));
            noteGroup.Items.Add(new ColorSettingItem("Ô¤ÀÀÒô·û", "NotePreviewColor", "Ô¤ÀÀÒô·ûµÄÑÕÉ«", "Note"));
            noteGroup.Items.Add(new ColorSettingItem("Á¦¶ÈÖ¸Ê¾Æ÷", "VelocityIndicatorColor", "Òô·ûÁ¦¶ÈÖ¸Ê¾Æ÷ÑÕÉ«", "Note"));
            ColorSettingGroups.Add(noteGroup);

            // Ğ¡½ÚºÍÎÄ×Ö×é
            var measureGroup = new ColorSettingGroup("Ğ¡½Ú", "Ğ¡½ÚºÍÎÄ×ÖÏà¹ØµÄÑÕÉ«ÉèÖÃ");
            measureGroup.Items.Add(new ColorSettingItem("Ğ¡½ÚÍ·±³¾°", "MeasureHeaderBackgroundColor", "Ğ¡½ÚÍ·µÄ±³¾°ÑÕÉ«", "Measure"));
            measureGroup.Items.Add(new ColorSettingItem("Ğ¡½ÚÏß", "MeasureLineColor", "Ğ¡½Ú·Ö¸ôÏßÑÕÉ«", "Measure"));
            measureGroup.Items.Add(new ColorSettingItem("Ğ¡½ÚÎÄ×Ö", "MeasureTextColor", "Ğ¡½ÚÊı×ÖµÄÑÕÉ«", "Measure"));
            ColorSettingGroups.Add(measureGroup);
        }

        private void InitializeShortcutSettings()
        {
            ShortcutSettings.Clear();

            // ÎÄ¼ş²Ù×÷
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "NewFile",
                Description = "ĞÂ½¨ÎÄ¼ş",
                DefaultShortcut = "Ctrl+N",
                CurrentShortcut = "Ctrl+N",
                Category = "ÎÄ¼ş"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "OpenFile",
                Description = "´ò¿ªÎÄ¼ş",
                DefaultShortcut = "Ctrl+O",
                CurrentShortcut = "Ctrl+O",
                Category = "ÎÄ¼ş"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "SaveFile",
                Description = "±£´æÎÄ¼ş",
                DefaultShortcut = "Ctrl+S",
                CurrentShortcut = "Ctrl+S",
                Category = "ÎÄ¼ş"
            });

            // ±à¼­²Ù×÷
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Undo",
                Description = "³·Ïú",
                DefaultShortcut = "Ctrl+Z",
                CurrentShortcut = "Ctrl+Z",
                Category = "±à¼­"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Redo",
                Description = "ÖØ×ö",
                DefaultShortcut = "Ctrl+Y",
                CurrentShortcut = "Ctrl+Y",
                Category = "±à¼­"
            });

            // ¹¤¾ß
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "PencilTool",
                Description = "Ç¦±Ê¹¤¾ß",
                DefaultShortcut = "P",
                CurrentShortcut = "P",
<<<<<<< HEAD
                Category = "å·¥å…·"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "EraserTool",
                Description = "æ©¡çš®æ“¦å·¥å…·",
                DefaultShortcut = "E",
                CurrentShortcut = "E",
                Category = "å·¥å…·"
=======
                Category = "¹¤¾ß"
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "SelectTool",
                Description = "é€‰æ‹©å·¥å…·",
                DefaultShortcut = "S",
                CurrentShortcut = "S",
                Category = "å·¥å…·"
            });

            // æ’­æ”¾æ§åˆ¶
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Play",
                Description = "æ’­æ”¾",
                DefaultShortcut = "Space",
                CurrentShortcut = "Space",
                Category = "æ’­æ”¾"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Pause",
                Description = "æš‚åœ",
                DefaultShortcut = "Space",
                CurrentShortcut = "Space",
                Category = "æ’­æ”¾"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Stop",
                Description = "åœæ­¢",
                DefaultShortcut = "Ctrl+Space",
                CurrentShortcut = "Ctrl+Space",
                Category = "æ’­æ”¾"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "SeekForward",
                Description = "å¿«è¿›",
                DefaultShortcut = "Right",
                CurrentShortcut = "Right",
                Category = "æ’­æ”¾"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "SeekBackward",
                Description = "å¿«é€€",
                DefaultShortcut = "Left",
                CurrentShortcut = "Left",
                Category = "æ’­æ”¾"
            });
        }

        /// <summary>
<<<<<<< HEAD
        /// åŠ è½½è®¾ç½®
=======
        /// ´ÓÅäÖÃÎÄ¼ş¼ÓÔØÉèÖÃ
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
        /// </summary>
        private void LoadSettings()
        {
            try
            {
<<<<<<< HEAD
                // åŠ è½½æ’­æ”¾åˆ—è¡¨è®¾ç½®
                LoadPlaylistSettings();
                
                // åŠ è½½å¿«æ·é”®è®¾ç½®
                LoadShortcutSettings();
                
                // åŠ è½½æ’­æ”¾è®¾å¤‡è®¾ç½®
                LoadPlaybackDevices();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"åŠ è½½è®¾ç½®å¤±è´¥: {ex.Message}");
            }
        }
        
        private void LoadPlaybackDevices()
        {
            try
            {
                // å¦‚æœæ’­æ”¾è®¾å¤‡è®¾ç½®å­˜å‚¨åœ¨ JSON ä¸­ï¼Œå¯ä»¥ä» Settings.PlaybackDevicesJson è§£æ
                if (!string.IsNullOrEmpty(Settings.PlaybackDevicesJson) && Settings.PlaybackDevicesJson != "{}")
                {
                    var playbackDevices = System.Text.Json.JsonSerializer.Deserialize<ObservableCollection<PlaybackDeviceOption>>(Settings.PlaybackDevicesJson) ?? new ObservableCollection<PlaybackDeviceOption>();
                    PlaybackDeviceOptions.Clear();
                    foreach (var device in playbackDevices)
                    {
                        PlaybackDeviceOptions.Add(device);
                    }
                }
                else
                {
                    // åˆå§‹åŒ–æ’­æ”¾è®¾å¤‡
                    InitializePlaybackDevices();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"åŠ è½½æ’­æ”¾è®¾å¤‡è®¾ç½®å¤±è´¥: {ex.Message}");
                // åˆå§‹åŒ–æ’­æ”¾è®¾å¤‡
                InitializePlaybackDevices();
            }
        }

        private void SavePlaybackDevices()
        {
            try
            {
                // å°†æ’­æ”¾è®¾å¤‡è®¾ç½®ä¿å­˜åˆ° JSON ä¸­
                Settings.PlaybackDevicesJson = System.Text.Json.JsonSerializer.Serialize(PlaybackDeviceOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ä¿å­˜æ’­æ”¾è®¾å¤‡è®¾ç½®å¤±è´¥: {ex.Message}");
=======
                // ´ÓÎÄ¼ş¼ÓÔØÉèÖÃ£¨µ«²»ÖØĞÂÓ¦ÓÃ£¬±ÜÃâ¸²¸Çµ±Ç°ÔËĞĞ×´Ì¬£©
                Settings.LoadFromFile();

                // ¸üĞÂµ±Ç°Ñ¡Ôñ×´Ì¬
                UpdateCurrentSelections();

                // ²»ÒªÖØĞÂÓ¦ÓÃÉèÖÃ£¬ÒòÎªÕâ»á¸²¸Çµ±Ç°ÔËĞĞµÄÖ÷Ìâ
                // ApplyLoadedSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"¼ÓÔØÉèÖÃÊ§°Ü: {ex.Message}");
                // Ê¹ÓÃÄ¬ÈÏÉèÖÃ
                UpdateCurrentSelections();
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
            }
        }

        /// <summary>
<<<<<<< HEAD
        /// ä¿å­˜è®¾ç½®åˆ°æ–‡ä»¶
=======
        /// ¸üĞÂµ±Ç°Ñ¡Ôñ×´Ì¬
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
        /// </summary>
        public void UpdateCurrentSelections()
        {
<<<<<<< HEAD
            try
            {
                // ä¿å­˜å¿«æ·é”®è®¾ç½®åˆ°è®¾ç½®æ¨¡å‹ï¼ˆå¦‚æœéœ€è¦ï¼‰
                SaveShortcutSettings();
                
                // ä¿å­˜æ’­æ”¾åˆ—è¡¨è®¾ç½®
                SavePlaylistSettings();
                
                // ä¿å­˜æ’­æ”¾è®¾å¤‡è®¾ç½®
                SavePlaybackDevices();
=======
            // ¸üĞÂÑ¡ÖĞµÄÓïÑÔ
            SelectedLanguageCode = Settings.Language;
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68

            // ¸üĞÂÑ¡ÖĞµÄÖ÷Ìâ - »ùÓÚµ±Ç°ÉèÖÃÅĞ¶ÏÖ÷ÌâÀàĞÍ
            SelectedThemeKey = DetermineCurrentThemeKey();

            // Í¨ÖªÊôĞÔ±ä¸ü
            OnPropertyChanged(nameof(IsCustomThemeSelected));
        }

        /// <summary>
        /// ¸ù¾İµ±Ç°ÑÕÉ«ÉèÖÃÅĞ¶ÏÖ÷ÌâÀàĞÍ
        /// </summary>
        private string DetermineCurrentThemeKey()
        {
            // ¼ì²éÊÇ·ñÆ¥ÅäÔ¤ÉèÖ÷Ìâ
            if (IsMatchingLightTheme()) return "Light";
            if (IsMatchingDarkTheme()) return "Dark";
            if (IsMatchingGreenTheme()) return "Green";
            if (IsMatchingBlueTheme()) return "Blue";
            if (IsMatchingPurpleTheme()) return "Purple";
            
            // Èç¹û²»Æ¥ÅäÈÎºÎÔ¤ÉèÖ÷Ìâ£¬ÔòÎª×Ô¶¨Òå
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
        /// è·å–æ’­æ”¾åˆ—è¡¨è®¾ç½®é¡µé¢
        /// </summary>
        private void InitializePlaylistPage()
        {
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Playlist,
                Title = "æ’­æ”¾åˆ—è¡¨",
                Icon = "ğŸµ",
                Description = "æ’­æ”¾åˆ—è¡¨è¡Œä¸ºè®¾ç½®"
            });
        }

        /// <summary>
        /// Ó¦ÓÃ¼ÓÔØµÄÉèÖÃ
        /// </summary>
        private void ApplyLoadedSettings()
        {
            // Ó¦ÓÃÓïÑÔÉèÖÃ
            _settingsService.ApplyLanguageSettings();

            // Ó¦ÓÃÖ÷ÌâÉèÖÃ
            _settingsService.ApplyThemeSettings();
<<<<<<< HEAD
            
            // åº”ç”¨æ’­æ”¾è®¾å¤‡è®¾ç½®
            ApplyPlaybackDevicesSettings();

            // å¯ä»¥åœ¨è¿™é‡Œåº”ç”¨å…¶ä»–è®¾ç½®
=======
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
        }
        
        private void ApplyPlaybackDevicesSettings()
        {
            // åº”ç”¨æ’­æ”¾è®¾å¤‡è®¾ç½®
            // è¿™é‡Œå¯ä»¥æ ¹æ®å®é™…éœ€æ±‚å®ç°
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            try
            {
                // ±£´æµ½·şÎñ
                await _settingsService.SaveSettingsAsync();
                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"±£´æÉèÖÃÊ§°Ü: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ResetToDefaultsAsync()
        {
            try
            {
                // ÖØÖÃ·şÎñÖĞµÄÉèÖÃ
                await _settingsService.ResetToDefaultsAsync();

                // ÖØÖÃ¿ì½İ¼üÉèÖÃ
                foreach (var shortcut in ShortcutSettings)
                {
                    shortcut.CurrentShortcut = shortcut.DefaultShortcut;
                }

                // ¸üĞÂµ±Ç°Ñ¡Ôñ×´Ì¬
                UpdateCurrentSelections();

                // ×Ô¶¯±£´æ»áÓÉSettingsÊôĞÔ±ä¸ü´¥·¢£¬²»ĞèÒªÊÖ¶¯µ÷ÓÃ
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ÖØÖÃÉèÖÃÊ§°Ü: {ex.Message}");
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
            
            // ×Ô¶¯±£´æ
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

            // ¸ù¾İÖ÷ÌâÓ¦ÓÃ¶ÔÓ¦µÄÑÕÉ«ÉèÖÃ
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
                    // ×Ô¶¨ÒåÖ÷Ìâ²»×Ô¶¯Ó¦ÓÃÈÎºÎÑÕÉ«£¬±£³ÖÓÃ»§ÉèÖÃ
                    break;
            }

            _settingsService.ApplyThemeSettings();
            
            // ×Ô¶¯±£´æ
            AutoSave();
        }

        /// <summary>
        /// ×Ô¶¯±£´æÉèÖÃ
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
                System.Diagnostics.Debug.WriteLine($"×Ô¶¯±£´æÉèÖÃÊ§°Ü: {ex.Message}");
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
            
            // ×Ô¶¯±£´æ
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
        /// »ñÈ¡Ö¸¶¨ÑÕÉ«ÉèÖÃÏî¶ÔÓ¦µÄÑÕÉ«Öµ
        /// </summary>
<<<<<<< HEAD
        public LanguageOption? SelectedLanguage =>
            LanguageOptions.FirstOrDefault(x => x.Code == Settings.Language);

        /// <summary>
        /// è·å–å½“å‰é€‰ä¸­çš„ä¸»é¢˜é€‰é¡¹
        /// </summary>
        public ThemeOption? SelectedTheme =>
            ThemeOptions.FirstOrDefault(x => x.Key == GetThemeKey(Settings.Theme));

        /// <summary>
        /// è·å–å½“å‰é€‰ä¸­çš„æ’­æ”¾è®¾å¤‡é€‰é¡¹
        /// </summary>
        public PlaybackDeviceOption? SelectedPlaybackDevice =>
            PlaybackDeviceOptions.FirstOrDefault(x => x.IsSelected);

        /// <summary>
        /// è·å–ä¸»é¢˜é”®å€¼
        /// </summary>
        private string GetThemeKey(ThemeVariant theme)
=======
        public string GetColorValue(string propertyName)
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
        {
            var property = typeof(SettingsModel).GetProperty(propertyName);
            return property?.GetValue(Settings) as string ?? "#FFFFFFFF";
        }

        /// <summary>
        /// ÉèÖÃÖ¸¶¨ÑÕÉ«ÉèÖÃÏîµÄÑÕÉ«Öµ
        /// </summary>
        public void SetColorValue(string propertyName, string colorValue)
        {
            var property = typeof(SettingsModel).GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(Settings, colorValue);
                
                // Èç¹ûÓÃ»§ĞŞ¸ÄÁËÑÕÉ«£¬×Ô¶¯ÇĞ»»µ½×Ô¶¨ÒåÖ÷Ìâ
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
        /// ÖØÖÃËùÓĞÑÕÉ«Îªµ±Ç°Ö÷ÌâµÄÄ¬ÈÏÖµ
        /// </summary>
        [RelayCommand]
        private void ResetAllColors()
        {
            ApplyTheme(SelectedThemeKey);
        }

        /// <summary>
        /// ÎªÌØ¶¨ÑÕÉ«ÊôĞÔ´´½¨°ó¶¨ÓÃµÄCommand
        /// </summary>
        [RelayCommand]
        private void UpdateColor(object parameter)
        {
            if (parameter is (string propertyName, string colorValue))
            {
                SetColorValue(propertyName, colorValue);
            }
        }

        // ÎªÃ¿¸öÑÕÉ«ÊôĞÔ´´½¨ÌØ¶¨µÄÊôĞÔ
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

        private void InitializePlaybackDevices()
        {
            try
            {
                // è·å–ç³»ç»Ÿä¸­çš„MIDIè¾“å‡ºè®¾å¤‡
                var devices = Melanchall.DryWetMidi.Multimedia.OutputDevice.GetAll();
                var deviceList = devices.ToList();
                for (int i = 0; i < deviceList.Count; i++)
                {
                    PlaybackDeviceOptions.Add(new PlaybackDeviceOption
                    {
                        Id = i.ToString(),
                        Name = deviceList[i].Name,
                        IsSelected = false
                    });
                }
                
                // å¦‚æœæ²¡æœ‰è®¾å¤‡ï¼Œæ·»åŠ é»˜è®¤é€‰é¡¹
                if (PlaybackDeviceOptions.Count == 0)
                {
                    PlaybackDeviceOptions.Add(new PlaybackDeviceOption
                    {
                        Id = "-1",
                        Name = "æ— å¯ç”¨è®¾å¤‡",
                        IsSelected = false
                    });
                }
            }
            catch (Exception ex)
            {
                // å¦‚æœå‡ºç°å¼‚å¸¸ï¼Œæ·»åŠ é”™è¯¯é€‰é¡¹
                PlaybackDeviceOptions.Add(new PlaybackDeviceOption
                {
                    Id = "-1",
                    Name = $"è®¾å¤‡åŠ è½½å¤±è´¥: {ex.Message}",
                    IsSelected = false
                });
            }
        }
    }
}