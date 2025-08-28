using System;

namespace DominoNext.Models.Music
{
    /// <summary>
    /// 音符数据模型 - 纯数据，不包含UI逻辑
    /// </summary>
    public class Note
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Pitch { get; set; }
        public MusicalFraction StartPosition { get; set; }
        public MusicalFraction Duration { get; set; }
        public int Velocity { get; set; } = 100;
        public string? Lyric { get; set; }

        /// <summary>
        /// 创建四分音符
        /// </summary>
        public static Note CreateQuarterNote(int pitch, MusicalFraction startPosition, int velocity = 100)
        {
            return new Note
            {
                Pitch = pitch,
                StartPosition = startPosition,
                Duration = MusicalFraction.QuarterNote,
                Velocity = velocity
            };
        }

        /// <summary>
        /// 克隆音符
        /// </summary>
        public Note Clone()
        {
            return new Note
            {
                Id = Guid.NewGuid(), // 新的ID
                Pitch = Pitch,
                StartPosition = StartPosition,
                Duration = Duration,
                Velocity = Velocity,
                Lyric = Lyric
            };
        }

        public override string ToString()
        {
            return $"Note(Pitch:{Pitch}, Start:{StartPosition}, Duration:{Duration})";
        }
    }
}