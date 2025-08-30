using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
<<<<<<< HEAD
using DominoNext.ViewModels.Editor;
=======
using Avalonia;
using DominoNext.Services.Interfaces;
>>>>>>> 3e4bb8e91d0e58cf2349304d29317c7768f77c68

namespace DominoNext.Views
{
    public partial class PianoRollView : UserControl
    {
        private bool _isUpdatingScroll = false;
        private ISettingsService? _settingsService;

        public PianoRollView()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            
            // �����������¼�
            SubscribeToThemeChanges();
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

        /// <summary>
        /// �����������¼�
        /// </summary>
        private void SubscribeToThemeChanges()
        {
            try
            {
                // ���Դӷ���λ��������ע���ȡ���÷���
                _settingsService = GetSettingsService();
                
                if (_settingsService != null)
                {
                    _settingsService.SettingsChanged += OnSettingsChanged;
                }

                // ����Ӧ�ó��򼶱��������
                if (Application.Current != null)
                {
                    Application.Current.PropertyChanged += OnApplicationPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"�����������¼�ʧ��: {ex.Message}");
            }
        }

        /// <summary>
        /// ��ȡ���÷��񣨼򻯰汾��
        /// </summary>
        private ISettingsService? GetSettingsService()
        {
            try
            {
                // ���������ע�����������Դ������ȡ
                // ����ʹ�ü򻯵�ʵ��
                return new DominoNext.Services.Implementation.SettingsService();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// �������ñ���¼�
        /// </summary>
        private void OnSettingsChanged(object? sender, DominoNext.Services.Interfaces.SettingsChangedEventArgs e)
        {
            try
            {
                // ����ɫ������ñ��ʱ��ǿ��ˢ�µ�ǰ��ͼ
                if (e.PropertyName?.EndsWith("Color") == true || e.PropertyName == "Theme")
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ForceRefreshTheme();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"�������ñ��ʧ��: {ex.Message}");
            }
        }

        /// <summary>
        /// ����Ӧ�ó������Ա���¼�
        /// </summary>
        private void OnApplicationPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            try
            {
                if (e.Property.Name == nameof(Application.RequestedThemeVariant))
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ForceRefreshTheme();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"����Ӧ�ó������Ա��ʧ��: {ex.Message}");
            }
        }

        /// <summary>
        /// ǿ��ˢ������ - ��Ը��پ�����ͼ�Ż�
        /// </summary>
        private void ForceRefreshTheme()
        {
            try
            {
                // ǿ��������Ⱦ
                this.InvalidateVisual();
                
                // ǿ�����²���������
                this.InvalidateMeasure();
                this.InvalidateArrange();

                // �ر����Զ���Canvas�ؼ�
                RefreshCustomCanvasControls();
                
                // ˢ�������ӿؼ�
                RefreshChildControls(this);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ǿ��ˢ������ʧ��: {ex.Message}");
            }
        }

        /// <summary>
        /// ˢ���Զ���Canvas�ؼ�
        /// </summary>
        private void RefreshCustomCanvasControls()
        {
            try
            {
                // ˢ�¸��پ���Canvas
                if (this.FindControl<DominoNext.Views.Controls.Canvas.PianoRollCanvas>("PianoRollCanvas") is var pianoRollCanvas && pianoRollCanvas != null)
                {
                    pianoRollCanvas.InvalidateVisual();
                }

                // ˢ��С��ͷCanvas
                if (this.FindControl<DominoNext.Views.Controls.Canvas.MeasureHeaderCanvas>("MeasureHeaderCanvas") is var measureHeaderCanvas && measureHeaderCanvas != null)
                {
                    measureHeaderCanvas.InvalidateVisual();
                }

                // ˢ���¼���ͼCanvas
                if (this.FindControl<DominoNext.Views.Controls.Canvas.EventViewCanvas>("EventViewCanvas") is var eventViewCanvas && eventViewCanvas != null)
                {
                    eventViewCanvas.InvalidateVisual();
                }

                // ˢ��������ͼCanvas
                if (this.FindControl<DominoNext.Views.Controls.Canvas.VelocityViewCanvas>("VelocityViewCanvas") is var velocityViewCanvas && velocityViewCanvas != null)
                {
                    velocityViewCanvas.InvalidateVisual();
                }

                // ˢ�¸��ټ��ؼ�
                if (this.FindControl<DominoNext.Views.Controls.PianoKeysControl>("PianoKeysControl") is var pianoKeysControl && pianoKeysControl != null)
                {
                    pianoKeysControl.InvalidateVisual();
                }

                // ˢ�������༭��
                if (this.FindControl<DominoNext.Views.Controls.Editing.NoteEditingLayer>("NoteEditingLayer") is var noteEditingLayer && noteEditingLayer != null)
                {
                    noteEditingLayer.InvalidateVisual();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ˢ���Զ���Canvas�ؼ�ʧ��: {ex.Message}");
            }
        }

        /// <summary>
        /// �ݹ�ˢ���ӿؼ�
        /// </summary>
        private void RefreshChildControls(Control control)
        {
            try
            {
                // ǿ��������Ⱦ
                control.InvalidateVisual();
                
                // ǿ�����²���������
                control.InvalidateMeasure();
                control.InvalidateArrange();

                // �ݹ鴦���ӿؼ�
                if (control is Panel panel)
                {
                    foreach (Control child in panel.Children)
                    {
                        RefreshChildControls(child);
                    }
                }
                else if (control is ContentControl contentControl && contentControl.Content is Control childControl)
                {
                    RefreshChildControls(childControl);
                }
                else if (control is ScrollViewer scrollViewer && scrollViewer.Content is Control scrollContent)
                {
                    RefreshChildControls(scrollContent);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ˢ���ӿؼ�ʧ��: {ex.Message}");
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

        /// <summary>
        /// �ͷ���Դ
        /// </summary>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            try
            {
                // ȡ�������¼�
                if (_settingsService != null)
                {
                    _settingsService.SettingsChanged -= OnSettingsChanged;
                }

                if (Application.Current != null)
                {
                    Application.Current.PropertyChanged -= OnApplicationPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"�ͷ���Դʧ��: {ex.Message}");
            }

            base.OnDetachedFromVisualTree(e);
        }
    }
}