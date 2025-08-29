using System;
using Avalonia.Media;
using DominoNext.Models.Settings;
using DominoNext.Services.Interfaces;

namespace DominoNext.Services.Implementation
{
    /// <summary>
    /// 主题服务实现，提供主题颜色的全局访问
    /// </summary>
    public class ThemeService
    {
        private static ThemeService? _instance;
        private readonly ISettingsService _settingsService;

        public static ThemeService Instance => _instance ??= new ThemeService();

        private ThemeService()
        {
            // 这里需要通过依赖注入获取SettingsService，暂时使用默认实现
            _settingsService = new SettingsService();
        }

        public ThemeService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _instance = this;
        }

        public ApplicationThemeColors Colors => _settingsService.Settings.ThemeColors;

        #region 音符颜色
        public SolidColorBrush NormalNoteBrush => ApplicationThemeColors.GetBrush(Colors.NormalNoteColor);
        public Pen NormalNotePen => ApplicationThemeColors.GetPen(Colors.NormalNoteBorderColor, 1);

        public SolidColorBrush SelectedNoteBrush => ApplicationThemeColors.GetBrush(Colors.SelectedNoteColor);
        public Pen SelectedNotePen => ApplicationThemeColors.GetPen(Colors.SelectedNoteBorderColor, 2);

        public SolidColorBrush DraggingNoteBrush => ApplicationThemeColors.GetBrush(Colors.DraggingNoteColor);
        public Pen DraggingNotePen => ApplicationThemeColors.GetPen(Colors.DraggingNoteBorderColor, 2);

        public SolidColorBrush PreviewNoteBrush => new SolidColorBrush(Color.Parse(Colors.PreviewNoteColor), 0.5);
        public Pen PreviewNotePen => new Pen(ApplicationThemeColors.GetBrush(Colors.PreviewNoteBorderColor), 1)
        {
            DashStyle = new DashStyle(new double[] { 3, 3 }, 0)
        };

        public SolidColorBrush CreatingNoteBrush => new SolidColorBrush(Color.Parse(Colors.CreatingNoteColor), 0.85);
        public Pen CreatingNotePen => ApplicationThemeColors.GetPen(Colors.CreatingNoteBorderColor, 2);

        public SolidColorBrush VelocityIndicatorBrush => ApplicationThemeColors.GetBrush(Colors.VelocityIndicatorColor);
        #endregion

        #region 网格线颜色
        public Pen MeasureLinePen => ApplicationThemeColors.GetPen(Colors.MeasureLineColor, 1);
        public Pen BeatLinePen => ApplicationThemeColors.GetPen(Colors.BeatLineColor, 1);
        public Pen EighthNotePen => new Pen(ApplicationThemeColors.GetBrush(Colors.EighthNoteLineColor), 1)
        {
            DashStyle = new DashStyle(new double[] { 2, 2 }, 0)
        };
        public Pen SixteenthNotePen => new Pen(ApplicationThemeColors.GetBrush(Colors.SixteenthNoteLineColor), 1)
        {
            DashStyle = new DashStyle(new double[] { 1, 3 }, 0)
        };
        public Pen HorizontalLinePen => ApplicationThemeColors.GetPen(Colors.HorizontalLineColor, 1);
        public Pen OctaveLinePen => ApplicationThemeColors.GetPen(Colors.OctaveLineColor, 1);
        public Pen BorderLinePen => ApplicationThemeColors.GetPen(Colors.BorderLineColor, 1);
        public Pen TimelinePen => ApplicationThemeColors.GetPen(Colors.TimelineColor, 2);
        #endregion

        #region 钢琴键颜色
        public SolidColorBrush WhiteKeyBrush => ApplicationThemeColors.GetBrush(Colors.WhiteKeyColor);
        public SolidColorBrush BlackKeyBrush => ApplicationThemeColors.GetBrush(Colors.BlackKeyColor);
        public SolidColorBrush WhiteKeyRowBrush => ApplicationThemeColors.GetBrush(Colors.WhiteKeyRowColor);
        public SolidColorBrush BlackKeyRowBrush => ApplicationThemeColors.GetBrush(Colors.BlackKeyRowColor);
        public Pen KeyBorderPen => ApplicationThemeColors.GetPen(Colors.KeyBorderColor, 1);
        #endregion

        #region 背景颜色
        public SolidColorBrush CanvasBackgroundBrush => ApplicationThemeColors.GetBrush(Colors.CanvasBackgroundColor);
        public SolidColorBrush PianoRollBackgroundBrush => ApplicationThemeColors.GetBrush(Colors.PianoRollBackgroundColor);
        public SolidColorBrush HeaderBackgroundBrush => ApplicationThemeColors.GetBrush(Colors.HeaderBackgroundColor);
        #endregion

        /// <summary>
        /// 通知主题已更改，所有使用主题颜色的组件应该刷新
        /// </summary>
        public event EventHandler? ThemeChanged;

        /// <summary>
        /// 触发主题更改事件
        /// </summary>
        public void NotifyThemeChanged()
        {
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}