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

<<<<<<< HEAD
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
=======
        private const double PianoKeyWidth = 60;

        // 资源画刷获取助手方法
        private IBrush GetResourceBrush(string key, string fallbackHex)
        {
            try
            {
                if (Application.Current?.Resources.TryGetResource(key, null, out var obj) == true && obj is IBrush brush)
                    return brush;
            }
            catch { }

            try
            {
                return new SolidColorBrush(Color.Parse(fallbackHex));
            }
            catch
            {
                return Brushes.Transparent;
            }
        }

        private IPen GetResourcePen(string brushKey, string fallbackHex, double thickness = 1, DashStyle? dashStyle = null)
        {
            var brush = GetResourceBrush(brushKey, fallbackHex);
            var pen = new Pen(brush, thickness);
            if (dashStyle != null)
                pen.DashStyle = dashStyle;
            return pen;
        }

        // 使用动态资源的画刷和画笔
        private IBrush TimelineBrush => GetResourceBrush("VelocityIndicatorBrush", "#FFFF0000");
        private IBrush WhiteKeyRowBrush => GetResourceBrush("KeyWhiteBrush", "#FFFFFFFF");
        private IBrush BlackKeyRowBrush => GetResourceBrush("AppBackgroundBrush", "#FFedf3fe");
        private IBrush MainBackgroundBrush => GetResourceBrush("MainCanvasBackgroundBrush", "#FFFFFFFF");
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68

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
            context.DrawRectangle(MainBackgroundBrush, null, bounds);

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
                
                // 优化：为深色模式提供更好的黑白键对比
                var rowBrush = isBlackKey ? GetBlackKeyRowBrush() : WhiteKeyRowBrush;
                context.DrawRectangle(rowBrush, null, rowRect);

                // 判断是否是八度分界线（B和C之间）
                // 当前音符是C时（midiNote % 12 == 0），在它的下方画黑线
                var isOctaveBoundary = midiNote % 12 == 0;

                // 绘制水平分界线 - 使用更合适的线条样式
                var pen = isOctaveBoundary 
                    ? GetResourcePen("BorderLineBlackBrush", "#FF000000", 1.5) // 八度线稍微粗一些
                    : GetResourcePen("GridLineBrush", "#FFbad2f2", 0.5); // 普通线更细一些
                
                context.DrawLine(pen, new Point(0, y + keyHeight), new Point(bounds.Width, y + keyHeight));
            }

            // 绘制第一条横线（G9的上边界）
            var horizontalLinePen = GetResourcePen("GridLineBrush", "#FFbad2f2", 0.5);
            context.DrawLine(horizontalLinePen, new Point(0, 0), new Point(bounds.Width, 0));
        }

        /// <summary>
        /// 获取优化的黑键行背景色
        /// </summary>
        private IBrush GetBlackKeyRowBrush()
        {
            // 根据主背景色的亮度动态调整黑键行的颜色
            var mainBg = GetResourceBrush("MainCanvasBackgroundBrush", "#FFFFFFFF");
            
            if (mainBg is SolidColorBrush solidBrush)
            {
                var color = solidBrush.Color;
                var brightness = (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) / 255.0;
                
                if (brightness < 0.5) // 深色主题
                {
                    // 深色主题：使黑键行稍微亮一些
                    return new SolidColorBrush(Color.FromArgb(
                        255,
                        (byte)Math.Min(255, color.R + 15),
                        (byte)Math.Min(255, color.G + 15),
                        (byte)Math.Min(255, color.B + 15)
                    ));
                }
                else // 浅色主题
                {
                    // 浅色主题：使黑键行稍微暗一些
                    return new SolidColorBrush(Color.FromArgb(
                        255,
                        (byte)Math.Max(0, color.R - 25),
                        (byte)Math.Max(0, color.G - 25),
                        (byte)Math.Max(0, color.B - 25)
                    ));
                }
            }
            
            // 回退到预设颜色
            return GetResourceBrush("AppBackgroundBrush", "#FFedf3fe");
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
<<<<<<< HEAD
                var endSixteenth = (int)(ViewModel.ContentWidth / sixteenthWidth) + 1;
                for (int i = 0; i <= endSixteenth; i++)
=======
                var startSixteenth = Math.Max(0, (int)(0 / sixteenthWidth));
                var endSixteenth = (int)(bounds.Width / sixteenthWidth) + 1;

                var sixteenthNotePen = GetResourcePen("GridLineBrush", "#FFafafaf", 0.5, new DashStyle(new double[] { 1, 3 }, 0));

                for (int i = startSixteenth; i <= endSixteenth; i++)
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
                {
                    if (i % 4 == 0) continue; // 跳过拍线位置
                    var x = i * sixteenthWidth;
                    if (x >= startX && x <= endX)
                    {
                        context.DrawLine(sixteenthNotePen, new Point(x, startY), new Point(x, endY));
                    }
                }
            }

            // 绘制八分音符线（虚线）
            if (eighthWidth > 10)
            {
<<<<<<< HEAD
                var endEighth = (int)(ViewModel.ContentWidth / eighthWidth) + 1;
                for (int i = 0; i <= endEighth; i++)
=======
                var startEighth = Math.Max(0, (int)(0 / eighthWidth));
                var endEighth = (int)(bounds.Width / eighthWidth) + 1;

                var eighthNotePen = GetResourcePen("GridLineBrush", "#FFafafaf", 0.7, new DashStyle(new double[] { 2, 2 }, 0));

                for (int i = startEighth; i <= endEighth; i++)
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
                {
                    if (i % 2 == 0) continue; // 跳过拍线位置
                    var x = i * eighthWidth;
                    if (x >= startX && x <= endX)
                    {
<<<<<<< HEAD
                        context.DrawLine(_beatLinePen, new Point(x, startY), new Point(x, endY));
=======
                        context.DrawLine(eighthNotePen, new Point(x, startY), new Point(x, endY));
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
                    }
                }
            }

<<<<<<< HEAD
            // 绘制拍线（实线）
            var endBeat = (int)(ViewModel.ContentWidth / beatWidth) + 1;
            for (int i = 0; i <= endBeat; i++)
=======
            // 绘制二分音符和四分音符线（实线）
            var startBeat = Math.Max(0, (int)(0 / beatWidth));
            var endBeat = (int)(bounds.Width / beatWidth) + 1;

            var beatLinePen = GetResourcePen("GridLineBrush", "#FFafafaf", 0.8);

            for (int i = startBeat; i <= endBeat; i++)
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
            {
                var x = i * beatWidth;
                if (x >= startX && x <= endX)
                {
                    context.DrawLine(beatLinePen, new Point(x, startY), new Point(x, endY));
                }
            }

<<<<<<< HEAD
            // 绘制小节线（粗线）
            var endMeasure = (int)(ViewModel.ContentWidth / measureWidth) + 1;
            for (int i = 0; i <= endMeasure; i++)
=======
            // 绘制小节线（最后绘制，覆盖其他线条）
            var startMeasure = Math.Max(0, (int)(0 / measureWidth));
            var endMeasure = (int)(bounds.Width / measureWidth) + 1;

            var measureLinePen = GetResourcePen("MeasureLineBrush", "#FF000080", 1.2); // 小节线稍微粗一些

            for (int i = startMeasure; i <= endMeasure; i++)
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
            {
                var x = i * measureWidth;
                if (x >= startX && x <= endX)
                {
                    context.DrawLine(measureLinePen, new Point(x, startY), new Point(x, endY));
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
<<<<<<< HEAD
                var note = ViewModel.PreviewNote;
                var x = note.GetX(ViewModel.PixelsPerTick, ViewModel.TicksPerBeat);
                var y = note.GetY(ViewModel.KeyHeight);
                var width = note.GetWidth(ViewModel.PixelsPerTick, ViewModel.TicksPerBeat);
                var height = note.GetHeight(ViewModel.KeyHeight);
                var previewRect = new Rect(x, y, width, height);
                context.DrawRectangle(new SolidColorBrush(Color.Parse("#4CAF50"), 0.4), null, previewRect);
=======
                var timelinePen = new Pen(TimelineBrush, 2);
                context.DrawLine(timelinePen, new Point(x, 0), new Point(x, Math.Min(bounds.Height, totalKeyHeight)));
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68
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