using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using System;
using System.Globalization;

namespace DominoNext.Views.Controls.Canvas
{
    public class MeasureHeaderCanvas : Control
    {
        public static readonly StyledProperty<PianoRollViewModel?> ViewModelProperty =
            AvaloniaProperty.Register<MeasureHeaderCanvas, PianoRollViewModel?>(nameof(ViewModel));

        public PianoRollViewModel? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        // 使用默认字体系列
        private readonly Typeface _typeface = new Typeface(FontFamily.Default);

        static MeasureHeaderCanvas()
        {
            ViewModelProperty.Changed.AddClassHandler<MeasureHeaderCanvas>((canvas, e) =>
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
            if (e.PropertyName == nameof(PianoRollViewModel.Zoom))
            {
                InvalidateVisual();
            }
        }

        public override void Render(DrawingContext context)
        {
            if (ViewModel == null) return;

            var bounds = Bounds;

            // 绘制背景
            context.DrawRectangle(new SolidColorBrush(Color.Parse("#F5F5F5")), null, bounds);

            var measureWidth = ViewModel.MeasureWidth;
            var startMeasure = Math.Max(1, (int)(0 / measureWidth) + 1);
            var endMeasure = (int)(bounds.Width / measureWidth) + 2;

            for (int measure = startMeasure; measure <= endMeasure; measure++)
            {
                var x = (measure - 1) * measureWidth;
                if (x >= 0 && x <= bounds.Width)
                {
                    // 绘制小节数字
                    var measureText = new FormattedText(
                        measure.ToString(),
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _typeface,
                        12,
                        Brushes.Black);

                    var textPoint = new Point(x + 5, 5);
                    context.DrawText(measureText, textPoint);

                    // 绘制小节线
                    if (measure > 1)
                    {
                        var measureLinePen = new Pen(new SolidColorBrush(Color.Parse("#000080")), 1);
                        context.DrawLine(measureLinePen, new Point(x, 0), new Point(x, bounds.Height));
                    }
                }
            }

            // 绘制底部分隔线
            context.DrawLine(new Pen(new SolidColorBrush(Color.Parse("#CCCCCC")), 1),
                new Point(0, bounds.Height - 1),
                new Point(bounds.Width, bounds.Height - 1));
        }
    }
}