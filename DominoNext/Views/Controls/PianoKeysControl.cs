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

        // 钢琴键相关画刷
        private readonly IBrush _blackKeyBrush = new SolidColorBrush(Color.Parse("#1F1F1F"));
        private readonly IBrush _whiteKeyBrush = new SolidColorBrush(Color.Parse("#FFFFFF"));
        private readonly IBrush _keyAreaBrush = new SolidColorBrush(Color.Parse("#FFFFFF"));

        // 使用默认字体系列
        private readonly Typeface _typeface = new Typeface(FontFamily.Default);

        static PianoKeysControl()
        {
            ViewModelProperty.Changed.AddClassHandler<PianoKeysControl>((control, e) =>
            {
                control.InvalidateVisual();
            });
        }

        public override void Render(DrawingContext context)
        {
            if (ViewModel == null) return;

            var bounds = Bounds;
            var keyHeight = ViewModel.KeyHeight;
            var totalKeyHeight = 128 * keyHeight;

            // 绘制钢琴键区域背景
            var keyAreaRect = new Rect(0, 0, PianoKeyWidth, Math.Min(bounds.Height, totalKeyHeight));
            context.DrawRectangle(_keyAreaBrush, null, keyAreaRect);

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

                    // 绘制键盘
                    context.DrawRectangle(isBlackKey ? _blackKeyBrush : _whiteKeyBrush,
                                        new Pen(new SolidColorBrush(Color.Parse("#1F1F1F")), 1), keyRect);

                    // 绘制音符名称
                    var noteName = ViewModel.GetNoteName(midiNote);
                    var formattedText = new FormattedText(
                        noteName,
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        _typeface,
                        8,
                        isBlackKey ? Brushes.White : Brushes.Black);

                    var textPoint = new Point(
                        PianoKeyWidth / 2 - formattedText.Width / 2,
                        y + keyHeight / 2 - formattedText.Height / 2);

                    context.DrawText(formattedText, textPoint);
                }
            }
        }
    }
}