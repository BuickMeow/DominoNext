using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DominoNext.Services.Interfaces;
using DominoNext.ViewModels.Editor;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Standards;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Tools;
using DominoNext.Models.Music;

namespace DominoNext.Services.Implementation
{
    public class PlaybackService : IPlaybackService, IDisposable
    {
        private OutputDevice? _outputDevice;
        private Playback? _playback;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _playbackTask;
        private PianoRollViewModel? _pianoRoll;
        
        public PlaybackState State { get; private set; } = PlaybackState.Stopped;
        public long CurrentPosition { get; private set; } = 0;
        public double Tempo { get; set; } = 120.0;
        
        public event EventHandler<long>? PositionChanged;
        public event EventHandler<PlaybackState>? StateChanged;
        
        public async Task PlayAsync(PianoRollViewModel pianoRoll)
        {
            if (State == PlaybackState.Playing)
                return;
                
            _pianoRoll = pianoRoll;
            
            // 创建MIDI输出设备
            if (_outputDevice == null)
            {
                var devices = OutputDevice.GetAll();
                var deviceList = devices.ToList();
                if (deviceList.Count > 0)
                {
                    _outputDevice = deviceList[0];
                }
                else
                {
                    throw new InvalidOperationException("没有可用的MIDI输出设备");
                }
            }
            
            // 创建MIDI文件
            var midiFile = CreateMidiFileFromPianoRoll(pianoRoll);
            
            _playback = midiFile.GetPlayback(_outputDevice);
            _playback.Speed = Tempo / 120.0; // 使用Speed而不是Tempo
            
            // 订阅事件
            _playback.Finished += OnPlaybackFinished;
            _playback.EventPlayed += OnEventPlayed;
            
            State = PlaybackState.Playing;
            StateChanged?.Invoke(this, State);
            
            // 开始播放任务
            _cancellationTokenSource = new CancellationTokenSource();
            _playbackTask = Task.Run(() => PlaybackLoop(_cancellationTokenSource.Token));
            
            // 等待播放任务完成
            if (_playbackTask != null)
                await _playbackTask;
        }
        
        public async Task PauseAsync()
        {
            if (State != PlaybackState.Playing)
                return;
                
            _playback?.Stop();
            State = PlaybackState.Paused;
            StateChanged?.Invoke(this, State);
            
            // 添加一个微小的延迟以确保状态更改被处理
            await Task.Delay(1);
        }
        
        public async Task ResumeAsync()
        {
            if (State != PlaybackState.Paused || _playback == null)
                return;
                
            // 重新开始播放
            _cancellationTokenSource = new CancellationTokenSource();
            _playbackTask = Task.Run(() => PlaybackLoop(_cancellationTokenSource.Token));
            
            State = PlaybackState.Playing;
            StateChanged?.Invoke(this, State);
            
            // 等待播放任务完成
            if (_playbackTask != null)
                await _playbackTask;
        }
        
        public async Task StopAsync()
        {
            if (State == PlaybackState.Stopped)
                return;
                
            _cancellationTokenSource?.Cancel();
            
            // 等待播放任务完成
            if (_playbackTask != null)
            {
                try
                {
                    await _playbackTask;
                }
                catch (OperationCanceledException)
                {
                    // 这是预期的异常，忽略它
                }
            }
            
            _playback?.Stop();
            _playback?.MoveToStart();
            
            CurrentPosition = 0;
            State = PlaybackState.Stopped;
            
            PositionChanged?.Invoke(this, CurrentPosition);
            StateChanged?.Invoke(this, State);
        }
        
        public async Task SeekAsync(long position)
        {
            if (_playback != null)
            {
                // 跳转到指定位置
                var tempoMap = _playback.TempoMap;
                var time = TimeConverter.ConvertTo<MidiTimeSpan>(position, tempoMap);
                _playback.MoveToTime(time);
                CurrentPosition = position;
                PositionChanged?.Invoke(this, CurrentPosition);
                
                // 添加一个微小的延迟以确保操作完成
                await Task.Delay(1);
            }
        }
        
        private MidiFile CreateMidiFileFromPianoRoll(PianoRollViewModel pianoRoll)
        {
            var midiFile = new MidiFile();
            
            // 创建带有名称的音轨
            var trackChunk = new TrackChunk();
            var trackNameEvent = new SequenceTrackNameEvent("Piano Track");
            trackChunk.Events.Add(trackNameEvent);
            
            // 添加 tempo 事件
            var microsecondsPerQuarterNote = (long)(60000000 / Tempo);
            var tempoEvent = new SetTempoEvent(microsecondsPerQuarterNote);
            trackChunk.Events.Add(tempoEvent);
            
            // 添加音符事件
            foreach (var track in pianoRoll.Tracks)
            {
                foreach (var note in track.Notes)
                {
                    // 计算开始和结束时间（以tick为单位）
                    var startTime = (long)(note.StartPosition.ToTicks() * pianoRoll.TicksPerBeat / MusicalFraction.QUARTER_NOTE_TICKS);
                    var duration = (long)(note.Duration.ToTicks() * pianoRoll.TicksPerBeat / MusicalFraction.QUARTER_NOTE_TICKS);
                    var endTime = startTime + duration;
                    
                    // 添加Note On事件
                    var noteOnEvent = new NoteOnEvent((SevenBitNumber)note.Pitch, (SevenBitNumber)note.Velocity);
                    noteOnEvent.Channel = (FourBitNumber)0;
                    noteOnEvent.DeltaTime = startTime;
                    trackChunk.Events.Add(noteOnEvent);
                    
                    // 添加Note Off事件
                    var noteOffEvent = new NoteOffEvent((SevenBitNumber)note.Pitch, (SevenBitNumber)0);
                    noteOffEvent.Channel = (FourBitNumber)0;
                    noteOffEvent.DeltaTime = endTime;
                    trackChunk.Events.Add(noteOffEvent);
                }
            }
            
            // 确保事件排序正确
            var eventList = trackChunk.Events.ToList();
            eventList.Sort(new MidiEventTimeComparer());
            trackChunk.Events.Clear();
            foreach (var evt in eventList)
            {
                trackChunk.Events.Add(evt);
            }
            
            // 添加MIDI文件头信息
            var timeSignature = new TimeSignature(4, 4);
            var timeSignatureEvent = new TimeSignatureEvent()
            {
                DeltaTime = 0
            };
            trackChunk.Events.Insert(0, timeSignatureEvent);
            
            // 添加程序选择事件
            var programChangeEvent = new ProgramChangeEvent((SevenBitNumber)0)
            {
                Channel = (FourBitNumber)0,
                DeltaTime = 0
            };
            trackChunk.Events.Insert(0, programChangeEvent);
            
            midiFile.Chunks.Add(trackChunk);
            
            // 设置MIDI文件格式为标准格式
            midiFile.TimeDivision = new TicksPerQuarterNoteTimeDivision((short)pianoRoll.TicksPerBeat);
            
            return midiFile;
        }
        
        private class MidiEventTimeComparer : IComparer<MidiEvent?>
        {
            public int Compare(MidiEvent? x, MidiEvent? y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return x.DeltaTime.CompareTo(y.DeltaTime);
            }
        }
        
        public TempoMap GetTempoMap()
        {
            // 如果已经有播放实例，直接从播放实例获取
            if (_playback != null)
            {
                return _playback.TempoMap;
            }
            
            // 否则创建一个临时的MidiFile来获取TempoMap
            var midiFile = new MidiFile();
            return midiFile.GetTempoMap();
        }
        
        public long GetTempoInMicrosecondsPerQuarterNote(TempoMap tempoMap)
        {
            // 获取第一个 tempo 事件的值
            var tempo = tempoMap.GetTempoAtTime(new MidiTimeSpan(0));
            return (long)tempo.MicrosecondsPerQuarterNote;
        }
        
        private void OnPlaybackFinished(object? sender, EventArgs e)
        {
            // 播放完成处理
            State = PlaybackState.Stopped;
            PositionChanged?.Invoke(this, CurrentPosition);
            StateChanged?.Invoke(this, State);
        }

        private void OnEventPlayed(object? sender, MidiEventPlayedEventArgs e)
        {
            // 事件播放处理
            if (e.Event is NoteOnEvent noteOnEvent)
            {
                // 处理NoteOn事件
                CurrentPosition = e.Event.DeltaTime;
                PositionChanged?.Invoke(this, CurrentPosition);
            }
        }
        
        private async Task PlaybackLoop(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && State == PlaybackState.Playing)
                {
                    await Task.Delay(10, cancellationToken); // 每10毫秒更新一次
                }
            }
            catch (OperationCanceledException)
            {
                // 任务被取消，正常退出
            }
        }
        
        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _playbackTask?.Wait(1000); // 等待最多1秒
            
            _playback?.Dispose();
            _outputDevice?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}