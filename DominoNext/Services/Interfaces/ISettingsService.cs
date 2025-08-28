using DominoNext.Models.Settings;
using System;
using System.Threading.Tasks;

namespace DominoNext.Services.Interfaces
{
    /// <summary>
    /// 设置服务接口
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// 当前设置
        /// </summary>
        SettingsModel Settings { get; }

        /// <summary>
        /// 设置变更事件
        /// </summary>
        event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

        /// <summary>
        /// 加载设置
        /// </summary>
        Task LoadSettingsAsync();

        /// <summary>
        /// 保存设置
        /// </summary>
        Task SaveSettingsAsync();

        /// <summary>
        /// 重置设置为默认值
        /// </summary>
        Task ResetToDefaultsAsync();

        /// <summary>
        /// 应用语言设置
        /// </summary>
        void ApplyLanguageSettings();

        /// <summary>
        /// 应用主题设置
        /// </summary>
        void ApplyThemeSettings();
    }

    /// <summary>
    /// 设置变更事件参数
    /// </summary>
    public class SettingsChangedEventArgs : EventArgs
    {
        public string PropertyName { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }
}