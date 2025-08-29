using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DominoNext.Models.Settings
{
    /// <summary>
    /// ����ҳ������
    /// </summary>
    public enum SettingsPageType
    {
        General,      // ��������
        Language,     // ��������
        Theme,        // ��������
        Editor,       // �༭������
        Shortcuts,    // ��ݼ�����
        Advanced      // �߼�����
    }

    /// <summary>
    /// ����ҳ����Ϣ
    /// </summary>
    public class SettingsPageInfo
    {
        public SettingsPageType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// ����ѡ��
    /// </summary>
    public class LanguageOption
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
    }

    /// <summary>
    /// ����ѡ��
    /// </summary>
    public class ThemeOption
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// ��ݼ�����
    /// </summary>
    public partial class ShortcutSetting : ObservableObject
    {
        [ObservableProperty]
        private string _command = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _defaultShortcut = string.Empty;

        [ObservableProperty]
        private string _currentShortcut = string.Empty;

        [ObservableProperty]
        private string _category = string.Empty;
    }

    /// <summary>
    /// ��ɫ������
    /// </summary>
    public class ColorSettingItem
    {
        public string DisplayName { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        public ColorSettingItem() { }

        public ColorSettingItem(string displayName, string propertyName, string description, string category)
        {
            DisplayName = displayName;
            PropertyName = propertyName;
            Description = description;
            Category = category;
        }
    }

    /// <summary>
    /// Ԥ������
    /// </summary>
    public class PresetTheme
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Action? ApplyAction { get; set; }
    }
}