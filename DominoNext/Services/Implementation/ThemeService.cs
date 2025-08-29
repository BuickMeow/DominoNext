using System;
using Avalonia.Media;
using DominoNext.Models.Settings;
using DominoNext.Services.Interfaces;

namespace DominoNext.Services.Implementation
{
    /// <summary>
    /// �������ʵ�֣��ṩ������ɫ��ȫ�ַ���
    /// </summary>
    public class ThemeService
    {
        private static ThemeService? _instance;
        private readonly ISettingsService _settingsService;

        public static ThemeService Instance => _instance ??= new ThemeService();

        private ThemeService()
        {
            // ������Ҫͨ������ע���ȡSettingsService����ʱʹ��Ĭ��ʵ��
            _settingsService = new SettingsService();
        }

        public ThemeService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _instance = this;
        }

        public ApplicationThemeColors Colors => _settingsService.Settings.ThemeColors;

        #region ������ɫ
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

        #region ��������ɫ
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

        #region ���ټ���ɫ
        public SolidColorBrush WhiteKeyBrush => ApplicationThemeColors.GetBrush(Colors.WhiteKeyColor);
        public SolidColorBrush BlackKeyBrush => ApplicationThemeColors.GetBrush(Colors.BlackKeyColor);
        public SolidColorBrush WhiteKeyRowBrush => ApplicationThemeColors.GetBrush(Colors.WhiteKeyRowColor);
        public SolidColorBrush BlackKeyRowBrush => ApplicationThemeColors.GetBrush(Colors.BlackKeyRowColor);
        public Pen KeyBorderPen => ApplicationThemeColors.GetPen(Colors.KeyBorderColor, 1);
        #endregion

        #region ������ɫ
        public SolidColorBrush CanvasBackgroundBrush => ApplicationThemeColors.GetBrush(Colors.CanvasBackgroundColor);
        public SolidColorBrush PianoRollBackgroundBrush => ApplicationThemeColors.GetBrush(Colors.PianoRollBackgroundColor);
        public SolidColorBrush HeaderBackgroundBrush => ApplicationThemeColors.GetBrush(Colors.HeaderBackgroundColor);
        #endregion

        /// <summary>
        /// ֪ͨ�����Ѹ��ģ�����ʹ��������ɫ�����Ӧ��ˢ��
        /// </summary>
        public event EventHandler? ThemeChanged;

        /// <summary>
        /// ������������¼�
        /// </summary>
        public void NotifyThemeChanged()
        {
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}