using DominoNext.Models.Music;

namespace DominoNext.Services.Interfaces
{
    public interface IMusicalTimeService
    {
        /// <summary>
        /// 将分数时间转换为tick
        /// </summary>
        double FractionToTicks(MusicalFraction fraction, int ticksPerBeat = 96);

        /// <summary>
        /// 将tick转换为分数时间
        /// </summary>
        MusicalFraction TicksToFraction(double ticks, int ticksPerBeat = 96);

        /// <summary>
        /// 时间对齐到网格
        /// </summary>
        MusicalFraction SnapToGrid(MusicalFraction time, MusicalFraction gridResolution);

        /// <summary>
        /// 获取小节开始位置
        /// </summary>
        MusicalFraction GetMeasureStart(MusicalFraction time, int beatsPerMeasure = 4);

        /// <summary>
        /// 计算两个时间点的距离
        /// </summary>
        MusicalFraction GetDistance(MusicalFraction from, MusicalFraction to);
    }
}