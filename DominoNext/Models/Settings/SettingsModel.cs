using System;
using System.ComponentModel;
using System.Globalization;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DominoNext.Models.Settings
{
    /// <summary>
    /// Ӧ�ó�������ģ��
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
            // TODO: ʵ�ִ��ļ���������
        }

        /// <summary>
        /// �������õ������ļ�
        /// </summary>
        public void SaveToFile()
        {
            // TODO: ʵ�ֱ������õ��ļ�
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