using System.ComponentModel;
using Avalonia.Media;
using System.Runtime.CompilerServices;

namespace DominoNext.Models.Settings
{
    /// <summary>
    /// Ӧ��������ɫ����
    /// </summary>
    public class ApplicationThemeColors : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region ������ɫ
        private string _normalNoteColor = "#4CAF50";
        public string NormalNoteColor
        {
            get => _normalNoteColor;
            set
            {
                if (_normalNoteColor != value)
                {
                    _normalNoteColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _normalNoteBorderColor = "#2E7D32";
        public string NormalNoteBorderColor
        {
            get => _normalNoteBorderColor;
            set
            {
                if (_normalNoteBorderColor != value)
                {
                    _normalNoteBorderColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _selectedNoteColor = "#FF9800";
        public string SelectedNoteColor
        {
            get => _selectedNoteColor;
            set
            {
                if (_selectedNoteColor != value)
                {
                    _selectedNoteColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _selectedNoteBorderColor = "#F57C00";
        public string SelectedNoteBorderColor
        {
            get => _selectedNoteBorderColor;
            set
            {
                if (_selectedNoteBorderColor != value)
                {
                    _selectedNoteBorderColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _draggingNoteColor = "#2196F3";
        public string DraggingNoteColor
        {
            get => _draggingNoteColor;
            set
            {
                if (_draggingNoteColor != value)
                {
                    _draggingNoteColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _draggingNoteBorderColor = "#1976D2";
        public string DraggingNoteBorderColor
        {
            get => _draggingNoteBorderColor;
            set
            {
                if (_draggingNoteBorderColor != value)
                {
                    _draggingNoteBorderColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _previewNoteColor = "#4CAF50";
        public string PreviewNoteColor
        {
            get => _previewNoteColor;
            set
            {
                if (_previewNoteColor != value)
                {
                    _previewNoteColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _previewNoteBorderColor = "#2E7D32";
        public string PreviewNoteBorderColor
        {
            get => _previewNoteBorderColor;
            set
            {
                if (_previewNoteBorderColor != value)
                {
                    _previewNoteBorderColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _creatingNoteColor = "#8BC34A";
        public string CreatingNoteColor
        {
            get => _creatingNoteColor;
            set
            {
                if (_creatingNoteColor != value)
                {
                    _creatingNoteColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _creatingNoteBorderColor = "#689F38";
        public string CreatingNoteBorderColor
        {
            get => _creatingNoteBorderColor;
            set
            {
                if (_creatingNoteBorderColor != value)
                {
                    _creatingNoteBorderColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _velocityIndicatorColor = "#FFC107";
        public string VelocityIndicatorColor
        {
            get => _velocityIndicatorColor;
            set
            {
                if (_velocityIndicatorColor != value)
                {
                    _velocityIndicatorColor = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region ��������ɫ
        private string _measureLineColor = "#000080";
        public string MeasureLineColor
        {
            get => _measureLineColor;
            set
            {
                if (_measureLineColor != value)
                {
                    _measureLineColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _beatLineColor = "#AFAFAF";
        public string BeatLineColor
        {
            get => _beatLineColor;
            set
            {
                if (_beatLineColor != value)
                {
                    _beatLineColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _eighthNoteLineColor = "#AFAFAF";
        public string EighthNoteLineColor
        {
            get => _eighthNoteLineColor;
            set
            {
                if (_eighthNoteLineColor != value)
                {
                    _eighthNoteLineColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _sixteenthNoteLineColor = "#AFAFAF";
        public string SixteenthNoteLineColor
        {
            get => _sixteenthNoteLineColor;
            set
            {
                if (_sixteenthNoteLineColor != value)
                {
                    _sixteenthNoteLineColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _horizontalLineColor = "#BAD2F2";
        public string HorizontalLineColor
        {
            get => _horizontalLineColor;
            set
            {
                if (_horizontalLineColor != value)
                {
                    _horizontalLineColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _octaveLineColor = "#000000";
        public string OctaveLineColor
        {
            get => _octaveLineColor;
            set
            {
                if (_octaveLineColor != value)
                {
                    _octaveLineColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _borderLineColor = "#000000";
        public string BorderLineColor
        {
            get => _borderLineColor;
            set
            {
                if (_borderLineColor != value)
                {
                    _borderLineColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _timelineColor = "#FF0000";
        public string TimelineColor
        {
            get => _timelineColor;
            set
            {
                if (_timelineColor != value)
                {
                    _timelineColor = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region ���ټ���ɫ
        private string _whiteKeyColor = "#FFFFFF";
        public string WhiteKeyColor
        {
            get => _whiteKeyColor;
            set
            {
                if (_whiteKeyColor != value)
                {
                    _whiteKeyColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _blackKeyColor = "#000000";
        public string BlackKeyColor
        {
            get => _blackKeyColor;
            set
            {
                if (_blackKeyColor != value)
                {
                    _blackKeyColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _whiteKeyRowColor = "#FFFFFF";
        public string WhiteKeyRowColor
        {
            get => _whiteKeyRowColor;
            set
            {
                if (_whiteKeyRowColor != value)
                {
                    _whiteKeyRowColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _blackKeyRowColor = "#F5F5F5";
        public string BlackKeyRowColor
        {
            get => _blackKeyRowColor;
            set
            {
                if (_blackKeyRowColor != value)
                {
                    _blackKeyRowColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _keyBorderColor = "#CCCCCC";
        public string KeyBorderColor
        {
            get => _keyBorderColor;
            set
            {
                if (_keyBorderColor != value)
                {
                    _keyBorderColor = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region ������ɫ
        private string _canvasBackgroundColor = "#FFFFFF";
        public string CanvasBackgroundColor
        {
            get => _canvasBackgroundColor;
            set
            {
                if (_canvasBackgroundColor != value)
                {
                    _canvasBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _pianoRollBackgroundColor = "#FFFFFF";
        public string PianoRollBackgroundColor
        {
            get => _pianoRollBackgroundColor;
            set
            {
                if (_pianoRollBackgroundColor != value)
                {
                    _pianoRollBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _headerBackgroundColor = "#F0F0F0";
        public string HeaderBackgroundColor
        {
            get => _headerBackgroundColor;
            set
            {
                if (_headerBackgroundColor != value)
                {
                    _headerBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        /// <summary>
        /// ����Ϊǳɫ����Ĭ����ɫ
        /// </summary>
        public void ResetToLightTheme()
        {
            // ������ɫ
            NormalNoteColor = "#4CAF50";
            NormalNoteBorderColor = "#2E7D32";
            SelectedNoteColor = "#FF9800";
            SelectedNoteBorderColor = "#F57C00";
            DraggingNoteColor = "#2196F3";
            DraggingNoteBorderColor = "#1976D2";
            PreviewNoteColor = "#4CAF50";
            PreviewNoteBorderColor = "#2E7D32";
            CreatingNoteColor = "#8BC34A";
            CreatingNoteBorderColor = "#689F38";
            VelocityIndicatorColor = "#FFC107";

            // ��������ɫ
            MeasureLineColor = "#000080";
            BeatLineColor = "#AFAFAF";
            EighthNoteLineColor = "#AFAFAF";
            SixteenthNoteLineColor = "#AFAFAF";
            HorizontalLineColor = "#BAD2F2";
            OctaveLineColor = "#000000";
            BorderLineColor = "#000000";
            TimelineColor = "#FF0000";

            // ���ټ���ɫ
            WhiteKeyColor = "#FFFFFF";
            BlackKeyColor = "#000000";
            WhiteKeyRowColor = "#FFFFFF";
            BlackKeyRowColor = "#F5F5F5";
            KeyBorderColor = "#CCCCCC";

            // ������ɫ
            CanvasBackgroundColor = "#FFFFFF";
            PianoRollBackgroundColor = "#FFFFFF";
            HeaderBackgroundColor = "#F0F0F0";
        }

        /// <summary>
        /// ����Ϊ��ɫ����Ĭ����ɫ
        /// </summary>
        public void ResetToDarkTheme()
        {
            // ������ɫ - ��Ӧ��ɫ����
            NormalNoteColor = "#66BB6A";
            NormalNoteBorderColor = "#4CAF50";
            SelectedNoteColor = "#FFB74D";
            SelectedNoteBorderColor = "#FF9800";
            DraggingNoteColor = "#64B5F6";
            DraggingNoteBorderColor = "#2196F3";
            PreviewNoteColor = "#66BB6A";
            PreviewNoteBorderColor = "#4CAF50";
            CreatingNoteColor = "#AED581";
            CreatingNoteBorderColor = "#8BC34A";
            VelocityIndicatorColor = "#FFD54F";

            // ��������ɫ - ��ɫ������ʹ�ý�������ɫ
            MeasureLineColor = "#5C6BC0";
            BeatLineColor = "#757575";
            EighthNoteLineColor = "#757575";
            SixteenthNoteLineColor = "#757575";
            HorizontalLineColor = "#424242";
            OctaveLineColor = "#BDBDBD";
            BorderLineColor = "#BDBDBD";
            TimelineColor = "#F44336";

            // ���ټ���ɫ - ��ɫ����
            WhiteKeyColor = "#FAFAFA";
            BlackKeyColor = "#212121";
            WhiteKeyRowColor = "#303030";
            BlackKeyRowColor = "#424242";
            KeyBorderColor = "#616161";

            // ������ɫ - ��ɫ
            CanvasBackgroundColor = "#303030";
            PianoRollBackgroundColor = "#303030";
            HeaderBackgroundColor = "#424242";
        }

        /// <summary>
        /// ��ȡ��ɫ��Brush����
        /// </summary>
        /// <param name="colorHex">ʮ��������ɫֵ</param>
        /// <returns>SolidColorBrush����</returns>
        public static SolidColorBrush GetBrush(string colorHex)
        {
            try
            {
                return new SolidColorBrush(Color.Parse(colorHex));
            }
            catch
            {
                // �����ɫ����ʧ�ܣ�����Ĭ����ɫ
                return new SolidColorBrush(Colors.Gray);
            }
        }

        /// <summary>
        /// ��ȡ��ɫ��Pen����
        /// </summary>
        /// <param name="colorHex">ʮ��������ɫֵ</param>
        /// <param name="thickness">������ϸ</param>
        /// <returns>Pen����</returns>
        public static Pen GetPen(string colorHex, double thickness = 1.0)
        {
            return new Pen(GetBrush(colorHex), thickness);
        }
    }
}