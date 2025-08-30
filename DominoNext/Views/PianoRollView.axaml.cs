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
            
            // ¶©ÔÄÖ÷Ìâ±ä¸üÊÂ¼ş
            SubscribeToThemeChanges();
        }

        private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ScrollViewer? mainScrollViewer = null;

            // å…³è”ä¸»è§†å›¾çš„æ»šåŠ¨äº‹ä»¶
            if (this.FindControl<ScrollViewer>("MainScrollViewer") is ScrollViewer mainScrollViewerControl)
            {
                mainScrollViewer = mainScrollViewerControl;
                mainScrollViewerControl.ScrollChanged += OnMainScrollViewerScrollChanged;
            }

            // å…³è”é’¢ç´é”®åŒºåŸŸçš„æ»šåŠ¨äº‹ä»¶
            if (this.FindControl<ScrollViewer>("PianoKeysScrollViewer") is ScrollViewer pianoKeysScrollViewer)
            {
                pianoKeysScrollViewer.ScrollChanged += OnPianoKeysScrollViewerScrollChanged;
            }

            // å…³è”äº‹ä»¶è§†å›¾çš„æ»šåŠ¨äº‹ä»¶
            if (this.FindControl<ScrollViewer>("EventViewScrollViewer") is ScrollViewer eventViewScrollViewer)
            {
                eventViewScrollViewer.ScrollChanged += OnEventViewScrollViewerScrollChanged;
            }

            // å…³è”åº•éƒ¨æ°´å¹³æ»šåŠ¨æ¡çš„å€¼å˜åŒ–äº‹ä»¶
            if (this.FindControl<ScrollBar>("HorizontalScrollBar") is ScrollBar horizontalScrollBar)
            {
                horizontalScrollBar.ValueChanged += OnHorizontalScrollBarValueChanged;
            }

            // å…³è”å³ä¾§å‚ç›´æ»šåŠ¨æ¡çš„å€¼å˜åŒ–äº‹ä»¶
            if (this.FindControl<ScrollBar>("VerticalScrollBar") is ScrollBar verticalScrollBar)
            {
                verticalScrollBar.ValueChanged += OnVerticalScrollBarValueChanged;
            }

            // åˆå§‹åŒ–æ—¶æ ¹æ®å®é™…è§†å£å®½åº¦è°ƒæ•´å°èŠ‚æ•°
            if (DataContext is PianoRollViewModel pianoRollViewModel && mainScrollViewer != null)
            {
                var viewportWidth = mainScrollViewer.Viewport.Width;
                if (viewportWidth > 0)
                {
                    // ä½¿ç”¨å®é™…è§†å£å®½åº¦é‡æ–°è®¡ç®—å°èŠ‚æ•°
                    pianoRollViewModel.TotalMeasures = pianoRollViewModel.CalculateMeasuresToFillUI(viewportWidth);
                }
            }
        }

        /// <summary>
        /// ¶©ÔÄÖ÷Ìâ±ä¸üÊÂ¼ş
        /// </summary>
        private void SubscribeToThemeChanges()
        {
            try
            {
                // ³¢ÊÔ´Ó·şÎñ¶¨Î»Æ÷»òÒÀÀµ×¢Èë»ñÈ¡ÉèÖÃ·şÎñ
                _settingsService = GetSettingsService();
                
                if (_settingsService != null)
                {
                    _settingsService.SettingsChanged += OnSettingsChanged;
                }

                // ¼àÌıÓ¦ÓÃ³ÌĞò¼¶±ğµÄÖ÷Ìâ±ä¸ü
                if (Application.Current != null)
                {
                    Application.Current.PropertyChanged += OnApplicationPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"¶©ÔÄÖ÷Ìâ±ä¸üÊÂ¼şÊ§°Ü: {ex.Message}");
            }
        }

        /// <summary>
        /// »ñÈ¡ÉèÖÃ·şÎñ£¨¼ò»¯°æ±¾£©
        /// </summary>
        private ISettingsService? GetSettingsService()
        {
            try
            {
                // Èç¹ûÓĞÒÀÀµ×¢ÈëÈİÆ÷£¬¿ÉÒÔ´ÓÕâÀï»ñÈ¡
                // ÕâÀïÊ¹ÓÃ¼ò»¯µÄÊµÏÖ
                return new DominoNext.Services.Implementation.SettingsService();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// ´¦ÀíÉèÖÃ±ä¸üÊÂ¼ş
        /// </summary>
        private void OnSettingsChanged(object? sender, DominoNext.Services.Interfaces.SettingsChangedEventArgs e)
        {
            try
            {
                // µ±ÑÕÉ«Ïà¹ØÉèÖÃ±ä¸üÊ±£¬Ç¿ÖÆË¢ĞÂµ±Ç°ÊÓÍ¼
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
                System.Diagnostics.Debug.WriteLine($"´¦ÀíÉèÖÃ±ä¸üÊ§°Ü: {ex.Message}");
            }
        }

        /// <summary>
        /// ´¦ÀíÓ¦ÓÃ³ÌĞòÊôĞÔ±ä¸üÊÂ¼ş
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
                System.Diagnostics.Debug.WriteLine($"´¦ÀíÓ¦ÓÃ³ÌĞòÊôĞÔ±ä¸üÊ§°Ü: {ex.Message}");
            }
        }

        /// <summary>
        /// Ç¿ÖÆË¢ĞÂÖ÷Ìâ - Õë¶Ô¸ÖÇÙ¾íÁ±ÊÓÍ¼ÓÅ»¯
        /// </summary>
        private void ForceRefreshTheme()
        {
            try
            {
                // Ç¿ÖÆÖØĞÂäÖÈ¾
                this.InvalidateVisual();
                
                // Ç¿ÖÆÖØĞÂ²âÁ¿ºÍÅÅÁĞ
                this.InvalidateMeasure();
                this.InvalidateArrange();

                // ÌØ±ğ´¦Àí×Ô¶¨ÒåCanvas¿Ø¼ş
                RefreshCustomCanvasControls();
                
                // Ë¢ĞÂËùÓĞ×Ó¿Ø¼ş
                RefreshChildControls(this);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ç¿ÖÆË¢ĞÂÖ÷ÌâÊ§°Ü: {ex.Message}");
            }
        }

        /// <summary>
        /// Ë¢ĞÂ×Ô¶¨ÒåCanvas¿Ø¼ş
        /// </summary>
        private void RefreshCustomCanvasControls()
        {
            try
            {
                // Ë¢ĞÂ¸ÖÇÙ¾íÁ±Canvas
                if (this.FindControl<DominoNext.Views.Controls.Canvas.PianoRollCanvas>("PianoRollCanvas") is var pianoRollCanvas && pianoRollCanvas != null)
                {
                    pianoRollCanvas.InvalidateVisual();
                }

                // Ë¢ĞÂĞ¡½ÚÍ·Canvas
                if (this.FindControl<DominoNext.Views.Controls.Canvas.MeasureHeaderCanvas>("MeasureHeaderCanvas") is var measureHeaderCanvas && measureHeaderCanvas != null)
                {
                    measureHeaderCanvas.InvalidateVisual();
                }

                // Ë¢ĞÂÊÂ¼şÊÓÍ¼Canvas
                if (this.FindControl<DominoNext.Views.Controls.Canvas.EventViewCanvas>("EventViewCanvas") is var eventViewCanvas && eventViewCanvas != null)
                {
                    eventViewCanvas.InvalidateVisual();
                }

                // Ë¢ĞÂÁ¦¶ÈÊÓÍ¼Canvas
                if (this.FindControl<DominoNext.Views.Controls.Canvas.VelocityViewCanvas>("VelocityViewCanvas") is var velocityViewCanvas && velocityViewCanvas != null)
                {
                    velocityViewCanvas.InvalidateVisual();
                }

                // Ë¢ĞÂ¸ÖÇÙ¼ü¿Ø¼ş
                if (this.FindControl<DominoNext.Views.Controls.PianoKeysControl>("PianoKeysControl") is var pianoKeysControl && pianoKeysControl != null)
                {
                    pianoKeysControl.InvalidateVisual();
                }

                // Ë¢ĞÂÒô·û±à¼­²ã
                if (this.FindControl<DominoNext.Views.Controls.Editing.NoteEditingLayer>("NoteEditingLayer") is var noteEditingLayer && noteEditingLayer != null)
                {
                    noteEditingLayer.InvalidateVisual();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ë¢ĞÂ×Ô¶¨ÒåCanvas¿Ø¼şÊ§°Ü: {ex.Message}");
            }
        }

        /// <summary>
        /// µİ¹éË¢ĞÂ×Ó¿Ø¼ş
        /// </summary>
        private void RefreshChildControls(Control control)
        {
            try
            {
                // Ç¿ÖÆÖØĞÂäÖÈ¾
                control.InvalidateVisual();
                
                // Ç¿ÖÆÖØĞÂ²âÁ¿ºÍÅÅÁĞ
                control.InvalidateMeasure();
                control.InvalidateArrange();

                // µİ¹é´¦Àí×Ó¿Ø¼ş
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
                System.Diagnostics.Debug.WriteLine($"Ë¢ĞÂ×Ó¿Ø¼şÊ§°Ü: {ex.Message}");
            }
        }

        private void OnMainScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (_isUpdatingScroll) return;

            try
            {
                _isUpdatingScroll = true;

                // æ£€æŸ¥æ˜¯å¦éœ€è¦è‡ªåŠ¨æ‰©å±•é’¢ç´å·å¸˜
                if (DataContext is PianoRollViewModel pianoRollViewModel)
                {
                    // ä½¿ç”¨æ»šåŠ¨ä½ç½®æ›¿ä»£TimelinePosition
                    var scrollViewer = sender as ScrollViewer;
                    if (scrollViewer != null)
                    {
                        pianoRollViewModel.AutoExtendWhenNearEnd(scrollViewer.Offset.X + scrollViewer.Viewport.Width);
                    }
                }

                // åŒæ­¥å°èŠ‚æ ‡é¢˜çš„æ°´å¹³æ»šåŠ¨
                SyncMeasureHeaderScroll();

                // åŒæ­¥äº‹ä»¶è§†å›¾çš„æ°´å¹³æ»šåŠ¨
                SyncEventViewScroll();

                // åŒæ­¥é’¢ç´é”®çš„å‚ç›´æ»šåŠ¨
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

                // åŒæ­¥ä¸»æ»šåŠ¨è§†å›¾çš„å‚ç›´æ»šåŠ¨
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

                // åŒæ­¥ä¸»æ»šåŠ¨è§†å›¾çš„æ°´å¹³æ»šåŠ¨
                if (this.FindControl<ScrollViewer>("MainScrollViewer") is ScrollViewer mainScrollViewer &&
                    sender is ScrollViewer eventViewScrollViewer)
                {
                    var newOffset = new Avalonia.Vector(eventViewScrollViewer.Offset.X, mainScrollViewer.Offset.Y);
                    mainScrollViewer.Offset = newOffset;
                }

                // åŒæ­¥å°èŠ‚æ ‡é¢˜çš„æ°´å¹³æ»šåŠ¨
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

                // å½“æ°´å¹³æ»šåŠ¨æ¡çš„å€¼æ”¹å˜æ—¶ï¼Œæ›´æ–° MainScrollViewer çš„æ°´å¹³åç§»
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

                // å½“å‚ç›´æ»šåŠ¨æ¡çš„å€¼æ”¹å˜æ—¶ï¼Œæ›´æ–° MainScrollViewer çš„å‚ç›´åç§»
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
        /// ÊÍ·Å×ÊÔ´
        /// </summary>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            try
            {
                // È¡Ïû¶©ÔÄÊÂ¼ş
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
                System.Diagnostics.Debug.WriteLine($"ÊÍ·Å×ÊÔ´Ê§°Ü: {ex.Message}");
            }

            base.OnDetachedFromVisualTree(e);
        }
    }
}