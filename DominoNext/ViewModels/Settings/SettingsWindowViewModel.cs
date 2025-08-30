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
    /// ���ô���ViewModel
    /// </summary>
    public partial class SettingsWindowViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;

        [ObservableProperty]
        private SettingsPageType _selectedPageType = SettingsPageType.General;

        [ObservableProperty]
        private bool _hasUnsavedChanges = false;

<<<<<<< HEAD
        // 播放列表设置选项
        public ObservableCollection<PlaylistSetting> PlaylistSettings { get; } = new();
=======
        [ObservableProperty]
        private string _selectedThemeKey = "Default";

        [ObservableProperty]
        private string _selectedLanguageCode = "zh-CN";
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68

        public SettingsModel Settings => _settingsService.Settings;

        public ObservableCollection<SettingsPageInfo> Pages { get; } = new();

        // ����ѡ��
        public ObservableCollection<LanguageOption> LanguageOptions { get; } = new()
        {
            new LanguageOption { Code = "zh-CN", Name = "��������", NativeName = "��������" },
            new LanguageOption { Code = "en-US", Name = "English", NativeName = "English" },
            new LanguageOption { Code = "ja-JP", Name = "Japanese", NativeName = "�ձ��Z" }
        };

        // ����ѡ�� - ����Ԥ��ľ�������
        public ObservableCollection<ThemeOption> ThemeOptions { get; } = new()
        {
            new ThemeOption { Key = "Default", Name = "����ϵͳ", Description = "����ϵͳ��������" },
            new ThemeOption { Key = "Light", Name = "ǳɫ����", Description = "�����ǳɫ���⣬�ʺ��ռ�ʹ��" },
            new ThemeOption { Key = "Dark", Name = "��ɫ����", Description = "��ɫ���⣬�����۲�ƣ��" },
            new ThemeOption { Key = "Green", Name = "�ഺ��", Description = "���µ���ɫ���⣬��������" },
            new ThemeOption { Key = "Blue", Name = "��ɫ�Ƽ�", Description = "�Ƽ��е���ɫ���⣬�ִ���Լ" },
            new ThemeOption { Key = "Purple", Name = "��ɫ�λ�", Description = "�λõ���ɫ���⣬��������" },
            new ThemeOption { Key = "Custom", Name = "�Զ���", Description = "��ȫ�Զ������ɫ���⣬��������" }
        };

        // ��ɫ������� - ��������֯
        public ObservableCollection<ColorSettingGroup> ColorSettingGroups { get; } = new();

        // ��ݼ�����
        public ObservableCollection<ShortcutSetting> ShortcutSettings { get; } = new();
        
        // 播放设备选项
        public ObservableCollection<PlaybackDeviceOption> PlaybackDeviceOptions { get; } = new();

        /// <summary>
        /// �Ƿ���ʾ�Զ����������
        /// </summary>
        public bool IsCustomThemeSelected => SelectedThemeKey == "Custom";

        public SettingsWindowViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            InitializePages();
            InitializeShortcutSettings();
            InitializeColorSettingGroups();

            // ��������
            LoadSettings();

<<<<<<< HEAD
            // 初始化播放列表设置
            InitializePlaylistSettings();

            // 监听设置变更
            Settings.PropertyChanged += (sender, e) => HasUnsavedChanges = true;
=======
            // �������ñ����ʵ���Զ�����
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
            
            // 添加默认播放列表设置
            PlaylistSettings.Add(new PlaylistSetting
            {
                Name = "主播放列表",
                Description = "主播放列表设置",
                DefaultPlaybackMode = PlaybackMode.Sequential,
                DefaultRepeatMode = RepeatMode.None,
                AutoPlayNext = true
            });
            
            PlaylistSettings.Add(new PlaylistSetting
            {
                Name = "随机播放列表",
                Description = "随机播放列表设置",
                DefaultPlaybackMode = PlaybackMode.Random,
                DefaultRepeatMode = RepeatMode.All,
                AutoPlayNext = false
            });
        }
        
        /// <summary>
        /// 加载播放列表设置
        /// </summary>
        private void LoadPlaylistSettings()
        {
            try
            {
                // 如果播放列表设置存储在 JSON 中，可以从 Settings.PlaylistSettingsJson 解析
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
                System.Diagnostics.Debug.WriteLine($"加载播放列表设置失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 保存播放列表设置
        /// </summary>
        private void SavePlaylistSettings()
        {
            try
            {
                // 将播放列表设置保存到 JSON 中
                Settings.PlaylistSettingsJson = System.Text.Json.JsonSerializer.Serialize(PlaylistSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存播放列表设置失败: {ex.Message}");
            }
        }
        
        [RelayCommand]
        private void AddPlaylist()
        {
            // 添加新的播放列表设置
            PlaylistSettings.Add(new PlaylistSetting
            {
                Name = $"播放列表 {PlaylistSettings.Count + 1}",
                Description = "新播放列表",
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
            // 重置播放列表设置为默认值
            PlaylistSettings.Clear();
            InitializePlaylistSettings();
            HasUnsavedChanges = true;
        }

        // ���ʱʹ�õ��޲ι��캯��
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
                Title = "����",
                Icon = "??",
                Description = "����Ӧ������"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Language,
                Title = "����",
                Icon = "??",
                Description = "������������"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Theme,
                Title = "����",
                Icon = "??",
                Description = "������������"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Editor,
                Title = "�༭��",
                Icon = "??",
                Description = "�༭����Ϊ����"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Shortcuts,
                Title = "��ݼ�",
                Icon = "??",
                Description = "���̿�ݼ�����"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Playlist,
                Title = "播放列表",
                Icon = "🎵",
                Description = "播放列表行为设置"
            });
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Advanced,
                Title = "�߼�",
                Icon = "???",
                Description = "�߼�ѡ��͵���"
            });
            
            // 初始化播放列表设置页面
            InitializePlaylistPage();
        }

        private void InitializeColorSettingGroups()
        {
            ColorSettingGroups.Clear();

            // ����������ɫ��
            var interfaceGroup = new ColorSettingGroup("����", "��������ص���ɫ����");
            interfaceGroup.Items.Add(new ColorSettingItem("���汳��", "BackgroundColor", "������ı�����ɫ", "Interface"));
            interfaceGroup.Items.Add(new ColorSettingItem("������", "GridLineColor", "�༭����������ɫ", "Interface"));
            interfaceGroup.Items.Add(new ColorSettingItem("ѡ���", "SelectionColor", "ѡ������ɫ", "Interface"));
            interfaceGroup.Items.Add(new ColorSettingItem("�ָ���", "SeparatorLineColor", "���ַָ��ߵ���ɫ", "Interface"));
            ColorSettingGroups.Add(interfaceGroup);

            // ���ټ���ɫ��
            var pianoGroup = new ColorSettingGroup("���ټ�", "���ټ�����ص���ɫ����");
            pianoGroup.Items.Add(new ColorSettingItem("�׼�", "KeyWhiteColor", "���ٰ׼���ɫ", "Piano"));
            pianoGroup.Items.Add(new ColorSettingItem("�ڼ�", "KeyBlackColor", "���ٺڼ���ɫ", "Piano"));
            pianoGroup.Items.Add(new ColorSettingItem("���̱߿�", "KeyBorderColor", "���ټ��߿���ɫ", "Piano"));
            pianoGroup.Items.Add(new ColorSettingItem("�׼�����", "KeyTextWhiteColor", "�׼��ϵ�������ɫ", "Piano"));
            pianoGroup.Items.Add(new ColorSettingItem("�ڼ�����", "KeyTextBlackColor", "�ڼ��ϵ�������ɫ", "Piano"));
            ColorSettingGroups.Add(pianoGroup);

            // ������ɫ��
            var noteGroup = new ColorSettingGroup("����", "������ص���ɫ����");
            noteGroup.Items.Add(new ColorSettingItem("��ͨ����", "NoteColor", "��ͨ�����������ɫ", "Note"));
            noteGroup.Items.Add(new ColorSettingItem("ѡ������", "NoteSelectedColor", "ѡ����������ɫ", "Note"));
            noteGroup.Items.Add(new ColorSettingItem("��ק����", "NoteDraggingColor", "��ק����������ɫ", "Note"));
            noteGroup.Items.Add(new ColorSettingItem("Ԥ������", "NotePreviewColor", "Ԥ����������ɫ", "Note"));
            noteGroup.Items.Add(new ColorSettingItem("����ָʾ��", "VelocityIndicatorColor", "��������ָʾ����ɫ", "Note"));
            ColorSettingGroups.Add(noteGroup);

            // С�ں�������
            var measureGroup = new ColorSettingGroup("С��", "С�ں�������ص���ɫ����");
            measureGroup.Items.Add(new ColorSettingItem("С��ͷ����", "MeasureHeaderBackgroundColor", "С��ͷ�ı�����ɫ", "Measure"));
            measureGroup.Items.Add(new ColorSettingItem("С����", "MeasureLineColor", "С�ڷָ�����ɫ", "Measure"));
            measureGroup.Items.Add(new ColorSettingItem("С������", "MeasureTextColor", "С�����ֵ���ɫ", "Measure"));
            ColorSettingGroups.Add(measureGroup);
        }

        private void InitializeShortcutSettings()
        {
            ShortcutSettings.Clear();

            // �ļ�����
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "NewFile",
                Description = "�½��ļ�",
                DefaultShortcut = "Ctrl+N",
                CurrentShortcut = "Ctrl+N",
                Category = "�ļ�"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "OpenFile",
                Description = "���ļ�",
                DefaultShortcut = "Ctrl+O",
                CurrentShortcut = "Ctrl+O",
                Category = "�ļ�"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "SaveFile",
                Description = "�����ļ�",
                DefaultShortcut = "Ctrl+S",
                CurrentShortcut = "Ctrl+S",
                Category = "�ļ�"
            });

            // �༭����
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Undo",
                Description = "����",
                DefaultShortcut = "Ctrl+Z",
                CurrentShortcut = "Ctrl+Z",
                Category = "�༭"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Redo",
                Description = "����",
                DefaultShortcut = "Ctrl+Y",
                CurrentShortcut = "Ctrl+Y",
                Category = "�༭"
            });

            // ����
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "PencilTool",
                Description = "Ǧ�ʹ���",
                DefaultShortcut = "P",
                CurrentShortcut = "P",
<<<<<<< HEAD
                Category = "工具"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "EraserTool",
                Description = "橡皮擦工具",
                DefaultShortcut = "E",
                CurrentShortcut = "E",
                Category = "工具"
=======
                Category = "����"
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "SelectTool",
                Description = "选择工具",
                DefaultShortcut = "S",
                CurrentShortcut = "S",
                Category = "工具"
            });

            // 播放控制
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Play",
                Description = "播放",
                DefaultShortcut = "Space",
                CurrentShortcut = "Space",
                Category = "播放"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Pause",
                Description = "暂停",
                DefaultShortcut = "Space",
                CurrentShortcut = "Space",
                Category = "播放"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "Stop",
                Description = "停止",
                DefaultShortcut = "Ctrl+Space",
                CurrentShortcut = "Ctrl+Space",
                Category = "播放"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "SeekForward",
                Description = "快进",
                DefaultShortcut = "Right",
                CurrentShortcut = "Right",
                Category = "播放"
            });
            ShortcutSettings.Add(new ShortcutSetting
            {
                Command = "SeekBackward",
                Description = "快退",
                DefaultShortcut = "Left",
                CurrentShortcut = "Left",
                Category = "播放"
            });
        }

        /// <summary>
<<<<<<< HEAD
        /// 加载设置
=======
        /// �������ļ���������
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
        /// </summary>
        private void LoadSettings()
        {
            try
            {
<<<<<<< HEAD
                // 加载播放列表设置
                LoadPlaylistSettings();
                
                // 加载快捷键设置
                LoadShortcutSettings();
                
                // 加载播放设备设置
                LoadPlaybackDevices();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
            }
        }
        
        private void LoadPlaybackDevices()
        {
            try
            {
                // 如果播放设备设置存储在 JSON 中，可以从 Settings.PlaybackDevicesJson 解析
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
                    // 初始化播放设备
                    InitializePlaybackDevices();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载播放设备设置失败: {ex.Message}");
                // 初始化播放设备
                InitializePlaybackDevices();
            }
        }

        private void SavePlaybackDevices()
        {
            try
            {
                // 将播放设备设置保存到 JSON 中
                Settings.PlaybackDevicesJson = System.Text.Json.JsonSerializer.Serialize(PlaybackDeviceOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存播放设备设置失败: {ex.Message}");
=======
                // ���ļ��������ã���������Ӧ�ã����⸲�ǵ�ǰ����״̬��
                Settings.LoadFromFile();

                // ���µ�ǰѡ��״̬
                UpdateCurrentSelections();

                // ��Ҫ����Ӧ�����ã���Ϊ��Ḳ�ǵ�ǰ���е�����
                // ApplyLoadedSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"��������ʧ��: {ex.Message}");
                // ʹ��Ĭ������
                UpdateCurrentSelections();
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
            }
        }

        /// <summary>
<<<<<<< HEAD
        /// 保存设置到文件
=======
        /// ���µ�ǰѡ��״̬
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
        /// </summary>
        public void UpdateCurrentSelections()
        {
<<<<<<< HEAD
            try
            {
                // 保存快捷键设置到设置模型（如果需要）
                SaveShortcutSettings();
                
                // 保存播放列表设置
                SavePlaylistSettings();
                
                // 保存播放设备设置
                SavePlaybackDevices();
=======
            // ����ѡ�е�����
            SelectedLanguageCode = Settings.Language;
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68

            // ����ѡ�е����� - ���ڵ�ǰ�����ж���������
            SelectedThemeKey = DetermineCurrentThemeKey();

            // ֪ͨ���Ա��
            OnPropertyChanged(nameof(IsCustomThemeSelected));
        }

        /// <summary>
        /// ���ݵ�ǰ��ɫ�����ж���������
        /// </summary>
        private string DetermineCurrentThemeKey()
        {
            // ����Ƿ�ƥ��Ԥ������
            if (IsMatchingLightTheme()) return "Light";
            if (IsMatchingDarkTheme()) return "Dark";
            if (IsMatchingGreenTheme()) return "Green";
            if (IsMatchingBlueTheme()) return "Blue";
            if (IsMatchingPurpleTheme()) return "Purple";
            
            // �����ƥ���κ�Ԥ�����⣬��Ϊ�Զ���
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
        /// 获取播放列表设置页面
        /// </summary>
        private void InitializePlaylistPage()
        {
            Pages.Add(new SettingsPageInfo
            {
                Type = SettingsPageType.Playlist,
                Title = "播放列表",
                Icon = "🎵",
                Description = "播放列表行为设置"
            });
        }

        /// <summary>
        /// Ӧ�ü��ص�����
        /// </summary>
        private void ApplyLoadedSettings()
        {
            // Ӧ����������
            _settingsService.ApplyLanguageSettings();

            // Ӧ����������
            _settingsService.ApplyThemeSettings();
<<<<<<< HEAD
            
            // 应用播放设备设置
            ApplyPlaybackDevicesSettings();

            // 可以在这里应用其他设置
=======
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
        }
        
        private void ApplyPlaybackDevicesSettings()
        {
            // 应用播放设备设置
            // 这里可以根据实际需求实现
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            try
            {
                // ���浽����
                await _settingsService.SaveSettingsAsync();
                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"��������ʧ��: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ResetToDefaultsAsync()
        {
            try
            {
                // ���÷����е�����
                await _settingsService.ResetToDefaultsAsync();

                // ���ÿ�ݼ�����
                foreach (var shortcut in ShortcutSettings)
                {
                    shortcut.CurrentShortcut = shortcut.DefaultShortcut;
                }

                // ���µ�ǰѡ��״̬
                UpdateCurrentSelections();

                // �Զ��������Settings���Ա������������Ҫ�ֶ�����
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"��������ʧ��: {ex.Message}");
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
            
            // �Զ�����
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

            // ��������Ӧ�ö�Ӧ����ɫ����
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
                    // �Զ������ⲻ�Զ�Ӧ���κ���ɫ�������û�����
                    break;
            }

            _settingsService.ApplyThemeSettings();
            
            // �Զ�����
            AutoSave();
        }

        /// <summary>
        /// �Զ���������
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
                System.Diagnostics.Debug.WriteLine($"�Զ���������ʧ��: {ex.Message}");
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
            
            // �Զ�����
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
        /// ��ȡָ����ɫ�������Ӧ����ɫֵ
        /// </summary>
<<<<<<< HEAD
        public LanguageOption? SelectedLanguage =>
            LanguageOptions.FirstOrDefault(x => x.Code == Settings.Language);

        /// <summary>
        /// 获取当前选中的主题选项
        /// </summary>
        public ThemeOption? SelectedTheme =>
            ThemeOptions.FirstOrDefault(x => x.Key == GetThemeKey(Settings.Theme));

        /// <summary>
        /// 获取当前选中的播放设备选项
        /// </summary>
        public PlaybackDeviceOption? SelectedPlaybackDevice =>
            PlaybackDeviceOptions.FirstOrDefault(x => x.IsSelected);

        /// <summary>
        /// 获取主题键值
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
        /// ����ָ����ɫ���������ɫֵ
        /// </summary>
        public void SetColorValue(string propertyName, string colorValue)
        {
            var property = typeof(SettingsModel).GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(Settings, colorValue);
                
                // ����û��޸�����ɫ���Զ��л����Զ�������
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
        /// ����������ɫΪ��ǰ�����Ĭ��ֵ
        /// </summary>
        [RelayCommand]
        private void ResetAllColors()
        {
            ApplyTheme(SelectedThemeKey);
        }

        /// <summary>
        /// Ϊ�ض���ɫ���Դ������õ�Command
        /// </summary>
        [RelayCommand]
        private void UpdateColor(object parameter)
        {
            if (parameter is (string propertyName, string colorValue))
            {
                SetColorValue(propertyName, colorValue);
            }
        }

        // Ϊÿ����ɫ���Դ����ض�������
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
                // 获取系统中的MIDI输出设备
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
                
                // 如果没有设备，添加默认选项
                if (PlaybackDeviceOptions.Count == 0)
                {
                    PlaybackDeviceOptions.Add(new PlaybackDeviceOption
                    {
                        Id = "-1",
                        Name = "无可用设备",
                        IsSelected = false
                    });
                }
            }
            catch (Exception ex)
            {
                // 如果出现异常，添加错误选项
                PlaybackDeviceOptions.Add(new PlaybackDeviceOption
                {
                    Id = "-1",
                    Name = $"设备加载失败: {ex.Message}",
                    IsSelected = false
                });
            }
        }
    }
}