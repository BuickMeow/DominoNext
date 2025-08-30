using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using System.Collections.Specialized;

namespace DominoNext.Views.Controls.Canvas
{
    public class PianoRollCanvas : UserControl
    {
        public static readonly StyledProperty<PianoRollViewModel?> ViewModelProperty =
            AvaloniaProperty.Register<PianoRollCanvas, PianoRollViewModel?>(nameof(ViewModel));

        public PianoRollViewModel? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        // 音符颜色
        private readonly Color _noteColor = Color.Parse("#4CAF50");
        private readonly IPen _noteBorderPen = new Pen(new SolidColorBrush(Color.Parse("#388E3C")), 1);
        private readonly IPen _selectedNoteBorderPen = new Pen(Brushes.White, 2);
        private readonly IPen _horizontalLinePen = new Pen(new SolidColorBrush(Color.Parse("#E0E0E0")), 1);
        private readonly IPen _octaveLinePen = new Pen(new SolidColorBrush(Color.Parse("#000000")), 1);
        private readonly IPen _measureLinePen = new Pen(new SolidColorBrush(Color.Parse("#000080")), 1);
        private readonly IPen _beatLinePen = new Pen(new SolidColorBrush(Color.Parse("#4444FF")), 1);
        private readonly IPen _sixteenthNotePen = new Pen(Brushes.Gray, 1) { DashStyle = DashStyle.Dash };
        private readonly IBrush _whiteKeyRowBrush = new SolidColorBrush(Color.Parse("#FFFFFF"));
        private readonly IBrush _blackKeyRowBrush = new SolidColorBrush(Color.Parse("#F5F5F5"));

        static PianoRollCanvas()
        {
            ViewModelProperty.Changed.AddClassHandler<PianoRollCanvas>((canvas, e) =>
            {
                if (e.OldValue is PianoRollViewModel oldVm)
                {
                    oldVm.PropertyChanged -= canvas.OnViewModelPropertyChanged;
                    if (oldVm.Notes is INotifyCollectionChanged oldCollection)
                        oldCollection.CollectionChanged -= canvas.OnNotesCollectionChanged;
                }

                if (e.NewValue is PianoRollViewModel newVm)
                {
                    newVm.PropertyChanged += canvas.OnViewModelPropertyChanged;
                    if (newVm.Notes is INotifyCollectionChanged newCollection)
                        newCollection.CollectionChanged += canvas.OnNotesCollectionChanged;
                }

                canvas.InvalidateVisual();
            });
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 监听与绘制相关的属性变化
            if (e.PropertyName == nameof(PianoRollViewModel.Zoom) ||
                e.PropertyName == nameof(PianoRollViewModel.VerticalZoom) ||
                e.PropertyName == nameof(PianoRollViewModel.KeyHeight) ||
                e.PropertyName == nameof(PianoRollViewModel.TimelinePosition) ||
                e.PropertyName == nameof(PianoRollViewModel.IsOnionSkinEnabled) ||
                e.PropertyName == nameof(PianoRollViewModel.OnionSkinPreviousFrames) ||
                e.PropertyName == nameof(PianoRollViewModel.OnionSkinNextFrames) ||
                e.PropertyName == nameof(PianoRollViewModel.OnionSkinOpacity))
            {
                InvalidateVisual();
            }
        }

        private void OnNotesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            if (ViewModel == null) return;

            var bounds = Bounds;

            // 绘制背景
            context.DrawRectangle(Brushes.White, null, bounds);

            DrawHorizontalGridLines(context, bounds);
            DrawVerticalGridLines(context, bounds);
            DrawNotes(context, bounds);
            DrawTimeline(context, bounds);
        }

        private void DrawHorizontalGridLines(DrawingContext context, Rect bounds)
        {
            var keyHeight = ViewModel!.KeyHeight;
            var totalKeyHeight = 128 * keyHeight;

            // 绘制所有128行的背景和分界线
            for (int i = 0; i < 128; i++)
            {
                var midiNote = 127 - i; // MIDI音符号
                var y = i * keyHeight;
                var isBlackKey = ViewModel.IsBlackKey(midiNote);

                // 绘制行背景，从左边开始（不再需要 PianoKeyWidth 偏移）
                var rowRect = new Rect(0, y, bounds.Width, keyHeight);
                context.DrawRectangle(isBlackKey ? _blackKeyRowBrush : _whiteKeyRowBrush, null, rowRect);

                // 判断是否是八度分界线（B和C之间）
                // 当前音符是C时（midiNote % 12 == 0），在它的下方画黑线
                var isOctaveBoundary = midiNote % 12 == 0;

                // 绘制水平分界线
                var pen = isOctaveBoundary ? _octaveLinePen : _horizontalLinePen;
                context.DrawLine(pen, new Point(0, y + keyHeight), new Point(bounds.Width, y + keyHeight));
            }

            // 绘制第一条横线（G9的上边界）
            context.DrawLine(_horizontalLinePen, new Point(0, 0), new Point(bounds.Width, 0));
        }

        private void DrawVerticalGridLines(DrawingContext context, Rect bounds)
        {
            var measureWidth = ViewModel!.MeasureWidth;
            var beatWidth = ViewModel.BeatWidth;
            var eighthWidth = ViewModel.EighthNoteWidth;
            var sixteenthWidth = ViewModel.SixteenthNoteWidth;
            var totalKeyHeight = 128 * ViewModel.KeyHeight;

            var startX = 0; // 从左边开始
            var endX = bounds.Width;
            var startY = 0;
            var endY = Math.Min(bounds.Height, totalKeyHeight);

            // 绘制十六分音符线（最稀疏的虚线）
            if (sixteenthWidth > 5)
            {
                var endSixteenth = (int)(ViewModel.ContentWidth / sixteenthWidth) + 1;
                for (int i = 0; i <= endSixteenth; i++)
                {
                    if (i % 4 == 0) continue; // 跳过拍线位置
                    var x = i * sixteenthWidth;
                    if (x >= startX && x <= endX)
                    {
                        context.DrawLine(_sixteenthNotePen, new Point(x, startY), new Point(x, endY));
                    }
                }
            }

            // 绘制八分音符线（虚线）
            if (eighthWidth > 10)
            {
                var endEighth = (int)(ViewModel.ContentWidth / eighthWidth) + 1;
                for (int i = 0; i <= endEighth; i++)
                {
                    if (i % 2 == 0) continue; // 跳过拍线位置
                    var x = i * eighthWidth;
                    if (x >= startX && x <= endX)
                    {
                        context.DrawLine(_beatLinePen, new Point(x, startY), new Point(x, endY));
                    }
                }
            }

            // 绘制拍线（实线）
            var endBeat = (int)(ViewModel.ContentWidth / beatWidth) + 1;
            for (int i = 0; i <= endBeat; i++)
            {
                var x = i * beatWidth;
                if (x >= startX && x <= endX)
                {
                    context.DrawLine(_beatLinePen, new Point(x, startY), new Point(x, endY));
                }
            }

            // 绘制小节线（粗线）
            var endMeasure = (int)(ViewModel.ContentWidth / measureWidth) + 1;
            for (int i = 0; i <= endMeasure; i++)
            {
                var x = i * measureWidth;
                if (x >= startX && x <= endX)
                {
                    context.DrawLine(_measureLinePen, new Point(x, startY), new Point(x, endY));
                }
            }
        }

        private void DrawTimeline(DrawingContext context, Rect bounds)
        {
            // 绘制时间线（顶部）
            context.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, bounds.Width, 20));
        }

        private void DrawNotes(DrawingContext context, Rect bounds)
        {
            if (ViewModel == null) return;
            
            // 绘制洋葱皮音符（其他轨道）
            if (ViewModel.IsOnionSkinEnabled)
            {
                DrawOnionSkinNotes(context, bounds);
            }
            
            // 绘制当前轨道的音符
            foreach (var note in ViewModel.Notes)
            {
                var x = note.GetX(ViewModel.PixelsPerTick, ViewModel.TicksPerBeat);
                var y = note.GetY(ViewModel.KeyHeight);
                var width = note.GetWidth(ViewModel.PixelsPerTick, ViewModel.TicksPerBeat);
                var height = note.GetHeight(ViewModel.KeyHeight);
                var noteRect = new Rect(x, y, width, height);
                if (noteRect.Intersects(bounds))
                {
                    var opacity = Math.Max(0.3, note.Velocity / 127.0);
                    var brush = new SolidColorBrush(_noteColor, opacity);
                    context.DrawRectangle(brush, _noteBorderPen, noteRect);
                    
                    // 绘制选中状态
                    if (note.IsSelected)
                    {
                        context.DrawRectangle(null, _selectedNoteBorderPen, noteRect);
                    }
                }
            }
            
            // 预览音符渲染
            if (ViewModel.PreviewNote != null)
            {
                var note = ViewModel.PreviewNote;
                var x = note.GetX(ViewModel.PixelsPerTick, ViewModel.TicksPerBeat);
                var y = note.GetY(ViewModel.KeyHeight);
                var width = note.GetWidth(ViewModel.PixelsPerTick, ViewModel.TicksPerBeat);
                var height = note.GetHeight(ViewModel.KeyHeight);
                var previewRect = new Rect(x, y, width, height);
                context.DrawRectangle(new SolidColorBrush(Color.Parse("#4CAF50"), 0.4), null, previewRect);
            }
        }

        /// <summary>
        /// 绘制洋葱皮音符
        /// </summary>
        /// <param name="context">绘制上下文</param>
        /// <param name="bounds">绘制边界</param>
        private void DrawOnionSkinNotes(DrawingContext context, Rect bounds)
        {
            if (ViewModel == null) return;
            
            // 遍历所有轨道（除了当前选中的轨道）
            foreach (var track in ViewModel.Tracks)
            {
                // 检查轨道是否被选中用于洋葱皮显示
                if (track != ViewModel.SelectedTrack && track.IsOnionSkinSelected)
                {
                    // 绘制该轨道的所有音符（与当前轨道对齐，无偏移）
                    foreach (var note in track.Notes)
                    {
                        var x = note.GetX(ViewModel.PixelsPerTick, ViewModel.TicksPerBeat);
                        var y = note.GetY(ViewModel.KeyHeight);
                        var width = note.GetWidth(ViewModel.PixelsPerTick, ViewModel.TicksPerBeat);
                        var height = note.GetHeight(ViewModel.KeyHeight);
                        var noteRect = new Rect(x, y, width, height);
                        
                        if (noteRect.Intersects(bounds))
                        {
                            // 使用不同的颜色和透明度来区分洋葱皮音符
                            var brush = new SolidColorBrush(Colors.Gray, ViewModel.OnionSkinOpacity);
                            context.DrawRectangle(brush, null, noteRect);
                        }
                    }
                }
            }
        }
    }
}