using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using System;
using System.Globalization;

namespace DominoNext.Views.Controls
{
    public class PianoKeysControl : Control
    {
        public static readonly StyledProperty<PianoRollViewModel?> ViewModelProperty =
            AvaloniaProperty.Register<PianoKeysControl, PianoRollViewModel?>(nameof(ViewModel));

        public PianoRollViewModel? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private const double PianoKeyWidth = 60;

        // 画刷将从应用资源获取，若不存在则使用默认颜色
        private IBrush GetResourceBrush(string key, string fallbackHex)
        {
            try
            {
                if (Application.Current?.Resources.TryGetResource(key, null, out var obj) == true && obj is IBrush b)
                    return b;
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

        private IPen GetResourcePen(string brushKey, string fallbackHex, double thickness = 1)
        {
            var brush = GetResourceBrush(brushKey, fallbackHex);
            return new Pen(brush, thickness);
        }

        private IBrush BlackKeyBrush => GetResourceBrush("KeyBlackBrush", "#FF1F1F1F");
        private IBrush WhiteKeyBrush => GetResourceBrush("KeyWhiteBrush", "#FFFFFFFF");
        private IBrush KeyAreaBrush => GetResourceBrush("AppBackgroundBrush", "#FFFFFFFF");

        // 使用默认字体系列
        private readonly Typeface _typeface = new Typeface(FontFamily.Default);

        static PianoKeysControl()
        {
            ViewModelProperty.Changed.AddClassHandler<PianoKeysControl>((control, e) =>
            {
                if (e.OldValue is PianoRollViewModel oldVm)
                {
                    oldVm.PropertyChanged -= control.OnViewModelPropertyChanged;
                }

                if (e.NewValue is PianoRollViewModel newVm)
                {
                    newVm.PropertyChanged += control.OnViewModelPropertyChanged;
                }

                control.InvalidateVisual();
            });
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PianoRollViewModel.VerticalZoom) ||
                e.PropertyName == nameof(PianoRollViewModel.KeyHeight) ||
                e.PropertyName == nameof(PianoRollViewModel.TotalHeight))
            {
                InvalidateVisual();
            }
        }

        public override void Render(DrawingContext context)
        {
            if (ViewModel == null) return;

            var bounds = Bounds;
            var keyHeight = ViewModel.KeyHeight;
            var totalKeyHeight = 128 * keyHeight;

            // 绘制钢琴键区域背景
            var keyAreaRect = new Rect(0, 0, PianoKeyWidth, Math.Min(bounds.Height, totalKeyHeight));
            context.DrawRectangle(KeyAreaBrush, null, keyAreaRect);

            // 绘制所有128个键
            for (int i = 0; i < 128; i++)
            {
                var midiNote = 127 - i; // MIDI音符号（从127到0）
                var isBlackKey = ViewModel.IsBlackKey(midiNote);
                var y = i * keyHeight;

                // 只绘制可见的键
                if (y + keyHeight >= 0 && y <= bounds.Height)
                {
                    var keyRect = new Rect(0, y, PianoKeyWidth, keyHeight);

                    // 绘制键盘 - 使用资源中的边框颜色
                    var keyBorderPen = GetResourcePen("KeyBorderBrush", "#FF1F1F1F", 1);
                    context.DrawRectangle(isBlackKey ? BlackKeyBrush : WhiteKeyBrush, keyBorderPen, keyRect);

                    // 绘制音符名称 - 使用资源中的文字颜色
                    var noteName = ViewModel.GetNoteName(midiNote);
                    var textBrush = isBlackKey 
                        ? GetResourceBrush("KeyTextBlackBrush", "#FFFFFFFF")
                        : GetResourceBrush("KeyTextWhiteBrush", "#FF000000");

                    var formattedText = new FormattedText(
                        noteName,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _typeface,
                        8,
                        textBrush);

                    var textPoint = new Point(
                        PianoKeyWidth / 2 - formattedText.Width / 2,
                        y + keyHeight / 2 - formattedText.Height / 2);

                    context.DrawText(formattedText, textPoint);
                }
            }
        }
    }
}