using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using System;
using System.Collections.Specialized;

namespace DominoNext.Views.Controls.Canvas
{
    public class EventViewCanvas : Control
    {
        public static readonly StyledProperty<PianoRollViewModel?> ViewModelProperty =
            AvaloniaProperty.Register<EventViewCanvas, PianoRollViewModel?>(nameof(ViewModel));

        public PianoRollViewModel? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        // 网格线颜色
        private readonly IPen _measureLinePen = new Pen(new SolidColorBrush(Color.Parse("#000080")), 1);
        private readonly IPen _beatLinePen = new Pen(new SolidColorBrush(Color.Parse("#afafaf")), 1);
        private readonly IPen _eighthNotePen = new Pen(new SolidColorBrush(Color.Parse("#afafaf")), 1) { DashStyle = new DashStyle(new double[] { 2, 2 }, 0) };
        private readonly IPen _sixteenthNotePen = new Pen(new SolidColorBrush(Color.Parse("#afafaf")), 1) { DashStyle = new DashStyle(new double[] { 1, 3 }, 0) };

        // 水平分界线颜色
        private readonly IPen _horizontalLinePen = new Pen(new SolidColorBrush(Color.Parse("#BAD2F2")), 1);

        // 边界线颜色
        private readonly IPen _borderLinePen = new Pen(new SolidColorBrush(Color.Parse("#000000")), 1);

        // 时间线
        private readonly IPen _timelinePen = new Pen(Brushes.Red, 2);

        static EventViewCanvas()
        {
            ViewModelProperty.Changed.AddClassHandler<EventViewCanvas>((canvas, e) =>
            {
                if (e.OldValue is PianoRollViewModel oldVm)
                {
                    oldVm.PropertyChanged -= canvas.OnViewModelPropertyChanged;
                }

                if (e.NewValue is PianoRollViewModel newVm)
                {
                    newVm.PropertyChanged += canvas.OnViewModelPropertyChanged;
                }

                canvas.InvalidateVisual();
            });
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PianoRollViewModel.Zoom) ||
                e.PropertyName == nameof(PianoRollViewModel.VerticalZoom) ||
                e.PropertyName == nameof(PianoRollViewModel.TimelinePosition))
            {
                InvalidateVisual();
            }
        }

        public override void Render(DrawingContext context)
        {
            if (ViewModel == null) return;

            var bounds = Bounds;

            // 绘制背景
            context.DrawRectangle(Brushes.White, null, bounds);

            DrawHorizontalGridLines(context, bounds);
            DrawVerticalGridLines(context, bounds);
            DrawTimeline(context, bounds);
        }

        private void DrawHorizontalGridLines(DrawingContext context, Rect bounds)
        {
            // 将事件视图高度分为4等份，在1/4、1/2、3/4处画横线
            var quarterHeight = bounds.Height / 4.0;

            // 绘制1/4、1/2、3/4位置的横线
            for (int i = 1; i <= 3; i++)
            {
                var y = i * quarterHeight;
                context.DrawLine(_horizontalLinePen,
                    new Point(0, y), new Point(bounds.Width, y));
            }
        }

        private void DrawVerticalGridLines(DrawingContext context, Rect bounds)
        {
            var measureWidth = ViewModel!.MeasureWidth;
            var beatWidth = ViewModel.BeatWidth;
            var eighthWidth = ViewModel.EighthNoteWidth;
            var sixteenthWidth = ViewModel.SixteenthNoteWidth;

            var startX = 0;
            var endX = bounds.Width;
            var startY = 0;
            var endY = bounds.Height;

            // 绘制十六分音符线（最稀疏的虚线）
            if (sixteenthWidth > 5)
            {
                var startSixteenth = Math.Max(0, (int)(0 / sixteenthWidth));
                var endSixteenth = (int)(bounds.Width / sixteenthWidth) + 1;

                for (int i = startSixteenth; i <= endSixteenth; i++)
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
                var startEighth = Math.Max(0, (int)(0 / eighthWidth));
                var endEighth = (int)(bounds.Width / eighthWidth) + 1;

                for (int i = startEighth; i <= endEighth; i++)
                {
                    if (i % 2 == 0) continue; // 跳过拍线位置

                    var x = i * eighthWidth;
                    if (x >= startX && x <= endX)
                    {
                        context.DrawLine(_eighthNotePen, new Point(x, startY), new Point(x, endY));
                    }
                }
            }

            // 绘制二分音符和四分音符线（实线）
            var startBeat = Math.Max(0, (int)(0 / beatWidth));
            var endBeat = (int)(bounds.Width / beatWidth) + 1;

            for (int i = startBeat; i <= endBeat; i++)
            {
                if (i % ViewModel.BeatsPerMeasure == 0) continue; // 跳过小节线位置

                var x = i * beatWidth;
                if (x >= startX && x <= endX)
                {
                    context.DrawLine(_beatLinePen, new Point(x, startY), new Point(x, endY));
                }
            }

            // 绘制小节线（最后绘制，覆盖其他线条）
            // 修复：支持无限扩展长度，不再依赖TotalMeasures限制
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
            var x = ViewModel!.TimelinePosition;

            if (x >= 0 && x <= bounds.Width)
            {
                context.DrawLine(_timelinePen, new Point(x, 0), new Point(x, bounds.Height));
            }
        }
    }
}