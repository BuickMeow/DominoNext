using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using DominoNext.Views.Controls.Editing.Input;
using DominoNext.Views.Controls.Editing.Rendering;

namespace DominoNext.Views.Controls.Editing
{
    /// <summary>
    /// 重构后的音符编辑层 - 符合MVVM最佳实践
    /// View层只负责渲染和事件转发，业务逻辑完全委托给ViewModel和模块
    /// </summary>
    public class NoteEditingLayer : Control
    {
        #region 依赖属性
        public static readonly StyledProperty<PianoRollViewModel?> ViewModelProperty =
            AvaloniaProperty.Register<NoteEditingLayer, PianoRollViewModel?>(nameof(ViewModel));

        public PianoRollViewModel? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        #endregion

        #region 渲染组件
        private readonly NoteRenderer _noteRenderer;
        private readonly DragPreviewRenderer _dragPreviewRenderer;
        private readonly ResizePreviewRenderer _resizePreviewRenderer;
        private readonly CreatingNoteRenderer _creatingNoteRenderer;
        private readonly SelectionBoxRenderer _selectionBoxRenderer;
        #endregion

        #region 输入处理组件
        private readonly CursorManager _cursorManager;
        private readonly InputEventRouter _inputEventRouter;
        #endregion

        #region 缓存管理
        private readonly Dictionary<NoteViewModel, Rect> _visibleNoteCache = new();
        private bool _cacheInvalid = true;
        private Rect _lastViewport;
        #endregion

        #region 性能优化
        private readonly System.Timers.Timer _renderTimer;
        private bool _hasPendingRender = false;
        private const double RenderInterval = 16.67; // 约60FPS
        #endregion

        #region 构造函数
        public NoteEditingLayer()
        {
            Debug.WriteLine("NoteEditingLayer constructor - 模块化MVVM版本");

            // 初始化渲染组件
            _noteRenderer = new NoteRenderer();
            _dragPreviewRenderer = new DragPreviewRenderer();
            _resizePreviewRenderer = new ResizePreviewRenderer();
            _creatingNoteRenderer = new CreatingNoteRenderer();
            _selectionBoxRenderer = new SelectionBoxRenderer();

            // 初始化输入处理组件
            _cursorManager = new CursorManager(this);
            _inputEventRouter = new InputEventRouter();

            // 配置控件
            IsHitTestVisible = true;
            Focusable = true;
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;

            // 初始化性能优化
            _renderTimer = new System.Timers.Timer(RenderInterval);
            _renderTimer.Elapsed += OnRenderTimerElapsed;
            _renderTimer.AutoReset = false;
        }

        static NoteEditingLayer()
        {
            ViewModelProperty.Changed.AddClassHandler<NoteEditingLayer>((layer, e) =>
            {
                Debug.WriteLine($"ViewModel changed: {e.OldValue} -> {e.NewValue}");
                layer.OnViewModelChanged(e.OldValue as PianoRollViewModel, e.NewValue as PianoRollViewModel);
            });
        }
        #endregion

        #region ViewModel绑定
        private void OnViewModelChanged(PianoRollViewModel? oldViewModel, PianoRollViewModel? newViewModel)
        {
            // 取消旧的绑定
            if (oldViewModel != null)
            {
                UnsubscribeFromViewModelEvents(oldViewModel);
            }

            // 建立新的绑定
            if (newViewModel != null)
            {
                SubscribeToViewModelEvents(newViewModel);
                newViewModel.EditorCommands?.SetPianoRollViewModel(newViewModel);
                Debug.WriteLine($"ViewModel绑定成功. 当前工具: {newViewModel.CurrentTool}, 音符数量: {newViewModel.Notes.Count}");
            }

            InvalidateCache();
        }

        private void SubscribeToViewModelEvents(PianoRollViewModel viewModel)
        {
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModel.Notes.CollectionChanged += OnNotesCollectionChanged;

            // 订阅模块事件
            viewModel.DragModule.OnDragUpdated += OnDragOrResizeUpdated;
            viewModel.DragModule.OnDragEnded += InvalidateVisual;
            viewModel.ResizeModule.OnResizeUpdated += OnDragOrResizeUpdated;
            viewModel.ResizeModule.OnResizeEnded += InvalidateVisual;
            viewModel.CreationModule.OnCreationUpdated += InvalidateVisual;
            viewModel.PreviewModule.OnPreviewUpdated += InvalidateVisual;
            // 添加选择模块事件订阅 - 这是修复选择框显示的关键
            viewModel.SelectionModule.OnSelectionUpdated += InvalidateVisual;
        }

        private void UnsubscribeFromViewModelEvents(PianoRollViewModel viewModel)
        {
            viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            viewModel.Notes.CollectionChanged -= OnNotesCollectionChanged;

            // 取消订阅模块事件
            viewModel.DragModule.OnDragUpdated -= OnDragOrResizeUpdated;
            viewModel.DragModule.OnDragEnded -= InvalidateVisual;
            viewModel.ResizeModule.OnResizeUpdated -= OnDragOrResizeUpdated;
            viewModel.ResizeModule.OnResizeEnded -= InvalidateVisual;
            viewModel.CreationModule.OnCreationUpdated -= InvalidateVisual;
            viewModel.PreviewModule.OnPreviewUpdated -= InvalidateVisual;
            // 取消订阅选择模块事件
            viewModel.SelectionModule.OnSelectionUpdated -= InvalidateVisual;
        }

        /// <summary>
        /// 拖拽或调整大小时强制更新缓存
        /// </summary>
        private void OnDragOrResizeUpdated()
        {
            InvalidateCache(); // 强制刷新缓存
            InvalidateVisual(); // 强制重绘
        }
        #endregion

        #region 事件处理 - 委托给输入路由器
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            _inputEventRouter.HandlePointerPressed(e, ViewModel, this);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            
            var position = e.GetPosition(this);
            _cursorManager.UpdateCursorForPosition(position, ViewModel);
            
            // 当悬停状态变化时，强制重绘以更新预览状态
            if (_cursorManager.HoveringStateChanged)
            {
                InvalidateVisual();
            }

            _inputEventRouter.HandlePointerMoved(e, ViewModel);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            _inputEventRouter.HandlePointerReleased(e, ViewModel);
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            _cursorManager.Reset();
            ViewModel?.PreviewModule?.ClearPreview();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            _inputEventRouter.HandleKeyDown(e, ViewModel);
        }
        #endregion

        #region 渲染 - 委托给渲染器
        public override void Render(DrawingContext context)
        {
            if (ViewModel == null) return;

            var bounds = Bounds;
            var viewport = new Rect(0, 0, bounds.Width, bounds.Height);

            // 绘制透明背景以确保接收指针事件
            context.DrawRectangle(Brushes.Transparent, null, viewport);

            // 更新可见音符缓存
            if (_cacheInvalid || !viewport.Equals(_lastViewport))
            {
                UpdateVisibleNotesCache(viewport);
                _lastViewport = viewport;
                _cacheInvalid = false;
            }

            // 使用渲染器进行渲染
            _noteRenderer.RenderNotes(context, ViewModel, _visibleNoteCache);

            if (ViewModel.DragState.IsDragging)
            {
                _dragPreviewRenderer.Render(context, ViewModel, CalculateNoteRect);
            }

            if (ViewModel.ResizeState.IsResizing)
            {
                _resizePreviewRenderer.Render(context, ViewModel, CalculateNoteRect);
            }

            if (ViewModel.CreationModule.IsCreatingNote)
            {
                _creatingNoteRenderer.Render(context, ViewModel, CalculateNoteRect);
            }

            // 只有在不悬停在音符上且不在进行其他操作时才显示预览音符
            if (!ViewModel.CreationModule.IsCreatingNote && 
                !_cursorManager.IsHoveringResizeEdge && 
                !_cursorManager.IsHoveringNote &&  // 新增：悬停在音符上时不显示预览
                !ViewModel.DragState.IsDragging && 
                !ViewModel.ResizeState.IsResizing)
            {
                _noteRenderer.RenderPreviewNote(context, ViewModel, CalculateNoteRect);
            }

            _selectionBoxRenderer.Render(context, ViewModel);
        }
        #endregion

        #region 缓存管理
        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // 只处理影响渲染的属性变化
            var renderingProperties = new[]
            {
                nameof(PianoRollViewModel.Zoom),
                nameof(PianoRollViewModel.VerticalZoom),
                nameof(PianoRollViewModel.CurrentTool),
                nameof(PianoRollViewModel.GridQuantization)
            };

            if (Array.Exists(renderingProperties, prop => prop == e.PropertyName))
            {
                InvalidateCache();
            }
        }

        private void OnNotesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            InvalidateCache();
        }

        private void InvalidateCache()
        {
            _cacheInvalid = true;
            ThrottledInvalidateVisual();
        }

        private void UpdateVisibleNotesCache(Rect viewport)
        {
            _visibleNoteCache.Clear();
            if (ViewModel?.Notes == null) return;

            var expandedViewport = viewport.Inflate(100);

            foreach (var note in ViewModel.Notes)
            {
                var noteRect = CalculateNoteRect(note);
                if (noteRect.Intersects(expandedViewport))
                {
                    _visibleNoteCache[note] = noteRect;
                }
            }
        }

        private Rect CalculateNoteRect(NoteViewModel note)
        {
            if (ViewModel == null) return default;

            var x = note.GetX(ViewModel.Zoom, ViewModel.PixelsPerTick);
            var y = note.GetY(ViewModel.KeyHeight);
            var width = Math.Max(4, note.GetWidth(ViewModel.Zoom, ViewModel.PixelsPerTick));
            var height = Math.Max(2, note.GetHeight(ViewModel.KeyHeight) - 1);

            return new Rect(x, y, width, height);
        }
        #endregion

        #region 性能优化
        private void ThrottledInvalidateVisual()
        {
            _hasPendingRender = true;
            if (!_renderTimer.Enabled)
            {
                _renderTimer.Start();
            }
        }

        private void OnRenderTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_hasPendingRender)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    if (_hasPendingRender)
                    {
                        InvalidateVisual();
                        _hasPendingRender = false;
                    }
                });
            }
        }
        #endregion
    }

    #region Command参数类

    /// <summary>
    /// 编辑器交互参数
    /// </summary>
    public class EditorInteractionArgs
    {
        public Point Position { get; set; }
        public EditorTool Tool { get; set; }
        public KeyModifiers Modifiers { get; set; }
        public EditorInteractionType InteractionType { get; set; }
    }

    /// <summary>
    /// 键盘命令参数
    /// </summary>
    public class KeyCommandArgs
    {
        public Key Key { get; set; }
        public KeyModifiers Modifiers { get; set; }
    }

    /// <summary>
    /// 交互类型
    /// </summary>
    public enum EditorInteractionType
    {
        Press,
        Move,
        Release
    }

    #endregion
}