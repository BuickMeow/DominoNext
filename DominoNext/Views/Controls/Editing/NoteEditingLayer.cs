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
    /// �ع���������༭�� - ����MVVM���ʵ��
    /// View��ֻ������Ⱦ���¼�ת����ҵ���߼���ȫί�и�ViewModel��ģ��
    /// </summary>
    public class NoteEditingLayer : Control
    {
        #region ��������
        public static readonly StyledProperty<PianoRollViewModel?> ViewModelProperty =
            AvaloniaProperty.Register<NoteEditingLayer, PianoRollViewModel?>(nameof(ViewModel));

        public PianoRollViewModel? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        #endregion

        #region ��Ⱦ���
        private readonly NoteRenderer _noteRenderer;
        private readonly DragPreviewRenderer _dragPreviewRenderer;
        private readonly ResizePreviewRenderer _resizePreviewRenderer;
        private readonly CreatingNoteRenderer _creatingNoteRenderer;
        private readonly SelectionBoxRenderer _selectionBoxRenderer;
        #endregion

        #region ���봦�����
        private readonly CursorManager _cursorManager;
        private readonly InputEventRouter _inputEventRouter;
        #endregion

        #region �������
        private readonly Dictionary<NoteViewModel, Rect> _visibleNoteCache = new();
        private bool _cacheInvalid = true;
        private Rect _lastViewport;
        #endregion

        #region �����Ż�
        private readonly System.Timers.Timer _renderTimer;
        private bool _hasPendingRender = false;
        private const double RenderInterval = 16.67; // Լ60FPS
        #endregion

        #region ���캯��
        public NoteEditingLayer()
        {
            Debug.WriteLine("NoteEditingLayer constructor - ģ�黯MVVM�汾");

            // ��ʼ����Ⱦ���
            _noteRenderer = new NoteRenderer();
            _dragPreviewRenderer = new DragPreviewRenderer();
            _resizePreviewRenderer = new ResizePreviewRenderer();
            _creatingNoteRenderer = new CreatingNoteRenderer();
            _selectionBoxRenderer = new SelectionBoxRenderer();

            // ��ʼ�����봦�����
            _cursorManager = new CursorManager(this);
            _inputEventRouter = new InputEventRouter();

            // ���ÿؼ�
            IsHitTestVisible = true;
            Focusable = true;
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;

            // ��ʼ�������Ż�
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

        #region ViewModel��
        private void OnViewModelChanged(PianoRollViewModel? oldViewModel, PianoRollViewModel? newViewModel)
        {
            // ȡ���ɵİ�
            if (oldViewModel != null)
            {
                UnsubscribeFromViewModelEvents(oldViewModel);
            }

            // �����µİ�
            if (newViewModel != null)
            {
                SubscribeToViewModelEvents(newViewModel);
                newViewModel.EditorCommands?.SetPianoRollViewModel(newViewModel);
                Debug.WriteLine($"ViewModel�󶨳ɹ�. ��ǰ����: {newViewModel.CurrentTool}, ��������: {newViewModel.Notes.Count}");
            }

            InvalidateCache();
        }

        private void SubscribeToViewModelEvents(PianoRollViewModel viewModel)
        {
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            viewModel.Notes.CollectionChanged += OnNotesCollectionChanged;

            // ����ģ���¼�
            viewModel.DragModule.OnDragUpdated += OnDragOrResizeUpdated;
            viewModel.DragModule.OnDragEnded += InvalidateVisual;
            viewModel.ResizeModule.OnResizeUpdated += OnDragOrResizeUpdated;
            viewModel.ResizeModule.OnResizeEnded += InvalidateVisual;
            viewModel.CreationModule.OnCreationUpdated += InvalidateVisual;
            viewModel.PreviewModule.OnPreviewUpdated += InvalidateVisual;
            // ���ѡ��ģ���¼����� - �����޸�ѡ�����ʾ�Ĺؼ�
            viewModel.SelectionModule.OnSelectionUpdated += InvalidateVisual;
        }

        private void UnsubscribeFromViewModelEvents(PianoRollViewModel viewModel)
        {
            viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            viewModel.Notes.CollectionChanged -= OnNotesCollectionChanged;

            // ȡ������ģ���¼�
            viewModel.DragModule.OnDragUpdated -= OnDragOrResizeUpdated;
            viewModel.DragModule.OnDragEnded -= InvalidateVisual;
            viewModel.ResizeModule.OnResizeUpdated -= OnDragOrResizeUpdated;
            viewModel.ResizeModule.OnResizeEnded -= InvalidateVisual;
            viewModel.CreationModule.OnCreationUpdated -= InvalidateVisual;
            viewModel.PreviewModule.OnPreviewUpdated -= InvalidateVisual;
            // ȡ������ѡ��ģ���¼�
            viewModel.SelectionModule.OnSelectionUpdated -= InvalidateVisual;
        }

        /// <summary>
        /// ��ק�������Сʱǿ�Ƹ��»���
        /// </summary>
        private void OnDragOrResizeUpdated()
        {
            InvalidateCache(); // ǿ��ˢ�»���
            InvalidateVisual(); // ǿ���ػ�
        }
        #endregion

        #region �¼����� - ί�и�����·����
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
            
            // ����ͣ״̬�仯ʱ��ǿ���ػ��Ը���Ԥ��״̬
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

        #region ��Ⱦ - ί�и���Ⱦ��
        public override void Render(DrawingContext context)
        {
            if (ViewModel == null) return;

            var bounds = Bounds;
            var viewport = new Rect(0, 0, bounds.Width, bounds.Height);

            // ����͸��������ȷ������ָ���¼�
            context.DrawRectangle(Brushes.Transparent, null, viewport);

            // ���¿ɼ���������
            if (_cacheInvalid || !viewport.Equals(_lastViewport))
            {
                UpdateVisibleNotesCache(viewport);
                _lastViewport = viewport;
                _cacheInvalid = false;
            }

            // ʹ����Ⱦ��������Ⱦ
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

            // ֻ���ڲ���ͣ���������Ҳ��ڽ�����������ʱ����ʾԤ������
            if (!ViewModel.CreationModule.IsCreatingNote && 
                !_cursorManager.IsHoveringResizeEdge && 
                !_cursorManager.IsHoveringNote &&  // ��������ͣ��������ʱ����ʾԤ��
                !ViewModel.DragState.IsDragging && 
                !ViewModel.ResizeState.IsResizing)
            {
                _noteRenderer.RenderPreviewNote(context, ViewModel, CalculateNoteRect);
            }

            _selectionBoxRenderer.Render(context, ViewModel);
        }
        #endregion

        #region �������
        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ֻ����Ӱ����Ⱦ�����Ա仯
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

            var x = note.GetX(ViewModel.PixelsPerTick, ViewModel.TicksPerBeat);
            var y = note.GetY(ViewModel.KeyHeight);
            var width = Math.Max(4, note.GetWidth(ViewModel.PixelsPerTick, ViewModel.TicksPerBeat));
            var height = Math.Max(2, note.GetHeight(ViewModel.KeyHeight) - 1);

            return new Rect(x, y, width, height);
        }
        #endregion

        #region �����Ż�
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

    #region Command������

    /// <summary>
    /// �༭����������
    /// </summary>
    public class EditorInteractionArgs
    {
        public Point Position { get; set; }
        public EditorTool Tool { get; set; }
        public KeyModifiers Modifiers { get; set; }
        public EditorInteractionType InteractionType { get; set; }
    }

    /// <summary>
    /// �����������
    /// </summary>
    public class KeyCommandArgs
    {
        public Key Key { get; set; }
        public KeyModifiers Modifiers { get; set; }
    }

    /// <summary>
    /// ��������
    /// </summary>
    public enum EditorInteractionType
    {
        Press,
        Move,
        Release
    }

    #endregion
}