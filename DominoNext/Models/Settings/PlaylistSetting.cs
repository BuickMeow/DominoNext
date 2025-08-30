using System;
using System.Collections.ObjectModel;

namespace DominoNext.Models.Settings
{
    public enum PlaybackMode
    {
        Sequential,
        Random,
        Single
    }
    
    public enum RepeatMode
    {
        None,
        One,
        All
    }
    
    public class PlaylistSetting
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";  // 添加Description属性
        public ObservableCollection<string> Files { get; } = new();
        public PlaybackMode DefaultPlaybackMode { get; set; } = PlaybackMode.Sequential;
        public RepeatMode DefaultRepeatMode { get; set; } = RepeatMode.None;
        public bool AutoPlayNext { get; set; } = true;
    }
}