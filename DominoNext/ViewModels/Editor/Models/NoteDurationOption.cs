using DominoNext.Models.Music;

namespace DominoNext.ViewModels.Editor.Models
{
    /// <summary>
    /// 音符时值选项模型
    /// </summary>
    public class NoteDurationOption
    {
        public string Name { get; set; }
        public MusicalFraction Duration { get; set; }
        public string Icon { get; set; }

        public NoteDurationOption(string name, MusicalFraction duration, string icon)
        {
            Name = name;
            Duration = duration;
            Icon = icon;
        }
    }
}