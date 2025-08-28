using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;

namespace DominoNext.Views
{
    public partial class PianoRollView : UserControl
    {
        private bool _isUpdatingScroll = false;

        public PianoRollView()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // ������������ͼ�Ĺ����¼�
            if (this.FindControl<ScrollViewer>("MainScrollViewer") is ScrollViewer mainScrollViewer)
            {
                mainScrollViewer.ScrollChanged += OnMainScrollViewerScrollChanged;
            }

            // ���ĸ��ټ�������ͼ�Ĺ����¼�
            if (this.FindControl<ScrollViewer>("PianoKeysScrollViewer") is ScrollViewer pianoKeysScrollViewer)
            {
                pianoKeysScrollViewer.ScrollChanged += OnPianoKeysScrollViewerScrollChanged;
            }

            // �����¼���ͼ������ͼ�Ĺ����¼�
            if (this.FindControl<ScrollViewer>("EventViewScrollViewer") is ScrollViewer eventViewScrollViewer)
            {
                eventViewScrollViewer.ScrollChanged += OnEventViewScrollViewerScrollChanged;
            }

            // ���ĵײ�ˮƽ��������ֵ�仯�¼�
            if (this.FindControl<ScrollBar>("HorizontalScrollBar") is ScrollBar horizontalScrollBar)
            {
                horizontalScrollBar.ValueChanged += OnHorizontalScrollBarValueChanged;
            }

            // �����Ҳഹֱ��������ֵ�仯�¼�
            if (this.FindControl<ScrollBar>("VerticalScrollBar") is ScrollBar verticalScrollBar)
            {
                verticalScrollBar.ValueChanged += OnVerticalScrollBarValueChanged;
            }
        }

        private void OnMainScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (_isUpdatingScroll) return;

            try
            {
                _isUpdatingScroll = true;

                // ͬ��С�ڱ����ˮƽ����
                SyncMeasureHeaderScroll();

                // ͬ���¼���ͼ��ˮƽ����
                SyncEventViewScroll();

                // ͬ�����ټ��Ĵ�ֱ����
                SyncPianoKeysScroll();
            }
            finally
            {
                _isUpdatingScroll = false;
            }
        }

        private void OnPianoKeysScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (_isUpdatingScroll) return;

            try
            {
                _isUpdatingScroll = true;

                // ͬ������ͼ�Ĵ�ֱ����
                if (this.FindControl<ScrollViewer>("MainScrollViewer") is ScrollViewer mainScrollViewer &&
                    sender is ScrollViewer pianoKeysScrollViewer)
                {
                    var newOffset = new Avalonia.Vector(mainScrollViewer.Offset.X, pianoKeysScrollViewer.Offset.Y);
                    mainScrollViewer.Offset = newOffset;
                }
            }
            finally
            {
                _isUpdatingScroll = false;
            }
        }

        private void OnEventViewScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (_isUpdatingScroll) return;

            try
            {
                _isUpdatingScroll = true;

                // ͬ������ͼ��ˮƽ����
                if (this.FindControl<ScrollViewer>("MainScrollViewer") is ScrollViewer mainScrollViewer &&
                    sender is ScrollViewer eventViewScrollViewer)
                {
                    var newOffset = new Avalonia.Vector(eventViewScrollViewer.Offset.X, mainScrollViewer.Offset.Y);
                    mainScrollViewer.Offset = newOffset;
                }

                // ͬ��С�ڱ����ˮƽ����
                SyncMeasureHeaderScroll();
            }
            finally
            {
                _isUpdatingScroll = false;
            }
        }

        private void OnHorizontalScrollBarValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isUpdatingScroll) return;

            try
            {
                _isUpdatingScroll = true;

                // ��ˮƽ������ֵ�仯ʱ���ֶ����� MainScrollViewer ��ˮƽƫ����
                if (this.FindControl<ScrollViewer>("MainScrollViewer") is ScrollViewer mainScrollViewer &&
                    sender is ScrollBar scrollBar)
                {
                    var newOffset = new Avalonia.Vector(scrollBar.Value, mainScrollViewer.Offset.Y);
                    mainScrollViewer.Offset = newOffset;
                }

                SyncMeasureHeaderScroll();
                SyncEventViewScroll();
            }
            finally
            {
                _isUpdatingScroll = false;
            }
        }

        private void OnVerticalScrollBarValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
        {
            if (_isUpdatingScroll) return;

            try
            {
                _isUpdatingScroll = true;

                // ����ֱ������ֵ�仯ʱ���ֶ����� MainScrollViewer �Ĵ�ֱƫ����
                if (this.FindControl<ScrollViewer>("MainScrollViewer") is ScrollViewer mainScrollViewer &&
                    sender is ScrollBar scrollBar)
                {
                    var newOffset = new Avalonia.Vector(mainScrollViewer.Offset.X, scrollBar.Value);
                    mainScrollViewer.Offset = newOffset;
                }

                SyncPianoKeysScroll();
            }
            finally
            {
                _isUpdatingScroll = false;
            }
        }

        private void SyncMeasureHeaderScroll()
        {
            if (this.FindControl<ScrollViewer>("MainScrollViewer") is ScrollViewer mainScrollViewer &&
                this.FindControl<ScrollViewer>("MeasureHeaderScrollViewer") is ScrollViewer measureHeaderScrollViewer)
            {
                measureHeaderScrollViewer.Offset = new Avalonia.Vector(mainScrollViewer.Offset.X, 0);
            }
        }

        private void SyncEventViewScroll()
        {
            if (this.FindControl<ScrollViewer>("MainScrollViewer") is ScrollViewer mainScrollViewer &&
                this.FindControl<ScrollViewer>("EventViewScrollViewer") is ScrollViewer eventViewScrollViewer)
            {
                eventViewScrollViewer.Offset = new Avalonia.Vector(mainScrollViewer.Offset.X, 0);
            }
        }

        private void SyncPianoKeysScroll()
        {
            if (this.FindControl<ScrollViewer>("MainScrollViewer") is ScrollViewer mainScrollViewer &&
                this.FindControl<ScrollViewer>("PianoKeysScrollViewer") is ScrollViewer pianoKeysScrollViewer)
            {
                pianoKeysScrollViewer.Offset = new Avalonia.Vector(0, mainScrollViewer.Offset.Y);
            }
        }
    }
}