using System;

namespace DominoNext.Models.Settings
{
    /// <summary>
    /// 播放设备选项
    /// </summary>
    public class PlaybackDeviceOption
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// 设备名称
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected { get; set; } = false;
    }
}