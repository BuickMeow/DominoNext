using System;
using System.Threading.Tasks;
using DominoNext.ViewModels.Editor;

namespace DominoNext.Services.Interfaces
{
    public interface IPlaybackService
    {
        /// <summary>
        /// 播放状态
        /// </summary>
        PlaybackState State { get; }
        
        /// <summary>
        /// 当前播放位置（以tick为单位）
        /// </summary>
        long CurrentPosition { get; }
        
        /// <summary>
        /// 播放速度（BPM）
        /// </summary>
        double Tempo { get; set; }
        
        /// <summary>
        /// 开始播放
        /// </summary>
        Task PlayAsync(PianoRollViewModel pianoRoll);
        
        /// <summary>
        /// 暂停播放
        /// </summary>
        Task PauseAsync();
        
        /// <summary>
        /// 停止播放
        /// </summary>
        Task StopAsync();
        
        /// <summary>
        /// 跳转到指定位置
        /// </summary>
        /// <param name="position">位置（以tick为单位）</param>
        Task SeekAsync(long position);
        
        /// <summary>
        /// 播放位置变化事件
        /// </summary>
        event EventHandler<long> PositionChanged;
        
        /// <summary>
        /// 播放状态变化事件
        /// </summary>
        event EventHandler<PlaybackState> StateChanged;
    }
    
    public enum PlaybackState
    {
        Stopped,
        Playing,
        Paused
    }
}