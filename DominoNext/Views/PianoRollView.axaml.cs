using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using DominoNext.ViewModels.Editor;

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
            ScrollViewer? mainScrollViewer = null;

            // 关联主视图的滚动事件
            if (this.FindControl<ScrollViewer>("MainScrollViewer") is ScrollViewer mainScrollViewerControl)
            {
                mainScrollViewer = mainScrollViewerControl;
                mainScrollViewerControl.ScrollChanged += OnMainScrollViewerScrollChanged;
            }

            // 关联钢琴键区域的滚动事件
            if (this.FindControl<ScrollViewer>("PianoKeysScrollViewer") is ScrollViewer pianoKeysScrollViewer)
            {
                pianoKeysScrollViewer.ScrollChanged += OnPianoKeysScrollViewerScrollChanged;
            }

            // 关联事件视图的滚动事件
            if (this.FindControl<ScrollViewer>("EventViewScrollViewer") is ScrollViewer eventViewScrollViewer)
            {
                eventViewScrollViewer.ScrollChanged += OnEventViewScrollViewerScrollChanged;
            }

            // 关联底部水平滚动条的值变化事件
            if (this.FindControl<ScrollBar>("HorizontalScrollBar") is ScrollBar horizontalScrollBar)
            {
                horizontalScrollBar.ValueChanged += OnHorizontalScrollBarValueChanged;
            }

            // 关联右侧垂直滚动条的值变化事件
            if (this.FindControl<ScrollBar>("VerticalScrollBar") is ScrollBar verticalScrollBar)
            {
                verticalScrollBar.ValueChanged += OnVerticalScrollBarValueChanged;
            }

            // 初始化时根据实际视口宽度调整小节数
            if (DataContext is PianoRollViewModel pianoRollViewModel && mainScrollViewer != null)
            {
                var viewportWidth = mainScrollViewer.Viewport.Width;
                if (viewportWidth > 0)
                {
                    // 使用实际视口宽度重新计算小节数
                    pianoRollViewModel.TotalMeasures = pianoRollViewModel.CalculateMeasuresToFillUI(viewportWidth);
                }
            }
        }

        private void OnMainScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (_isUpdatingScroll) return;

            try
            {
                _isUpdatingScroll = true;

                // 检查是否需要自动扩展钢琴卷帘
                if (DataContext is PianoRollViewModel pianoRollViewModel)
                {
                    // 使用滚动位置替代TimelinePosition
                    var scrollViewer = sender as ScrollViewer;
                    if (scrollViewer != null)
                    {
                        pianoRollViewModel.AutoExtendWhenNearEnd(scrollViewer.Offset.X + scrollViewer.Viewport.Width);
                    }
                }

                // 同步小节标题的水平滚动
                SyncMeasureHeaderScroll();

                // 同步事件视图的水平滚动
                SyncEventViewScroll();

                // 同步钢琴键的垂直滚动
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

                // 同步主滚动视图的垂直滚动
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

                // 同步主滚动视图的水平滚动
                if (this.FindControl<ScrollViewer>("MainScrollViewer") is ScrollViewer mainScrollViewer &&
                    sender is ScrollViewer eventViewScrollViewer)
                {
                    var newOffset = new Avalonia.Vector(eventViewScrollViewer.Offset.X, mainScrollViewer.Offset.Y);
                    mainScrollViewer.Offset = newOffset;
                }

                // 同步小节标题的水平滚动
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

                // 当水平滚动条的值改变时，更新 MainScrollViewer 的水平偏移
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

                // 当垂直滚动条的值改变时，更新 MainScrollViewer 的垂直偏移
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