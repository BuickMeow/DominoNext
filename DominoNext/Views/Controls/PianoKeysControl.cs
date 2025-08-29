using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using DominoNext.Services.Implementation;
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
        private readonly ThemeService _themeService;
        private readonly Typeface _typeface = new Typeface(FontFamily.Default);

        public PianoKeysControl()
        {
            _themeService = ThemeService.Instance;
            _themeService.ThemeChanged += OnThemeChanged;
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            InvalidateVisual();
        }

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
            context.DrawRectangle(_themeService.WhiteKeyBrush, null, keyAreaRect);

            // 绘制所有128个键
            for (int i = 0; i < 128; i++)
            {
                var midiNote = 127 - i; // MIDI音符号（从127到0）
                var isBlackKey = ViewModel.IsBlackKey(midiNote);
                var y = i * keyHeight;

                // 只绘制可见的键
                if (y + keyHeight >= 0 && y <= bounds.Height)
                {
                    DrawPianoKey(context, midiNote, y, keyHeight, isBlackKey);
                }
            }

            // 绘制右边界线
            context.DrawLine(_themeService.KeyBorderPen,
                new Point(PianoKeyWidth - 1, 0),
                new Point(PianoKeyWidth - 1, Math.Min(bounds.Height, totalKeyHeight)));
        }

        private void DrawPianoKey(DrawingContext context, int midiNote, double y, double keyHeight, bool isBlackKey)
        {
            var keyRect = new Rect(0, y, PianoKeyWidth, keyHeight);

            // 绘制键的背景
            var keyBrush = isBlackKey ? _themeService.BlackKeyBrush : _themeService.WhiteKeyBrush;
            context.DrawRectangle(keyBrush, _themeService.KeyBorderPen, keyRect);

            // 绘制音符名称（仅对C音符）
            if (midiNote % 12 == 0 && keyHeight > 12)
            {
                var octave = midiNote / 12 - 1;
                var noteText = $"C{octave}";

                var formattedText = new FormattedText(
                    noteText,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    _typeface,
                    Math.Min(10, keyHeight * 0.6),
                    isBlackKey ? Brushes.White : Brushes.Black);

                var textX = keyRect.X + 3;
                var textY = keyRect.Y + (keyRect.Height - formattedText.Height) / 2;
                context.DrawText(formattedText, new Point(textX, textY));
            }

            // 绘制水平分界线
            context.DrawLine(_themeService.KeyBorderPen,
                new Point(0, y + keyHeight),
                new Point(PianoKeyWidth, y + keyHeight));
        }
    }
}