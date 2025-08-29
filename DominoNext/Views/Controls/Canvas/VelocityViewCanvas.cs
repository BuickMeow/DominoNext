using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using DominoNext.Renderers;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace DominoNext.Views.Controls.Canvas
{
    /// <summary>
    /// ������ͼ���� - ��ʾ�ͱ༭����������
    /// </summary>
    public class VelocityViewCanvas : Control
    {
        public static readonly StyledProperty<PianoRollViewModel?> ViewModelProperty =
            AvaloniaProperty.Register<VelocityViewCanvas, PianoRollViewModel?>(nameof(ViewModel));

        public PianoRollViewModel? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private readonly VelocityBarRenderer _velocityRenderer;

        public VelocityViewCanvas()
        {
            _velocityRenderer = new VelocityBarRenderer();
            
            // ��������¼�
            IsHitTestVisible = true;
        }

        static VelocityViewCanvas()
        {
            ViewModelProperty.Changed.AddClassHandler<VelocityViewCanvas>((canvas, e) =>
            {
                if (e.OldValue is PianoRollViewModel oldVm)
                {
                    canvas.UnsubscribeFromViewModel(oldVm);
                }

                if (e.NewValue is PianoRollViewModel newVm)
                {
                    canvas.SubscribeToViewModel(newVm);
                }

                canvas.InvalidateVisual();
            });
        }

        private void SubscribeToViewModel(PianoRollViewModel viewModel)
        {
            // ����ViewModel���Ա仯
            viewModel.PropertyChanged += OnViewModelPropertyChanged;
            
            // �����������ϱ仯
            if (viewModel.Notes is INotifyCollectionChanged notesCollection)
            {
                notesCollection.CollectionChanged += OnNotesCollectionChanged;
            }

            // ����ÿ�����������Ա仯
            foreach (var note in viewModel.Notes)
            {
                note.PropertyChanged += OnNotePropertyChanged;
            }

            // �������ȱ༭ģ���¼�
            if (viewModel.VelocityEditingModule != null)
            {
                viewModel.VelocityEditingModule.OnVelocityUpdated += OnVelocityUpdated;
            }
        }

        private void UnsubscribeFromViewModel(PianoRollViewModel viewModel)
        {
            // ȡ������ViewModel���Ա仯
            viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            
            // ȡ�������������ϱ仯
            if (viewModel.Notes is INotifyCollectionChanged notesCollection)
            {
                notesCollection.CollectionChanged -= OnNotesCollectionChanged;
            }

            // ȡ������ÿ�����������Ա仯
            foreach (var note in viewModel.Notes)
            {
                note.PropertyChanged -= OnNotePropertyChanged;
            }

            // ȡ���������ȱ༭ģ���¼�
            if (viewModel.VelocityEditingModule != null)
            {
                viewModel.VelocityEditingModule.OnVelocityUpdated -= OnVelocityUpdated;
            }
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PianoRollViewModel.Zoom) ||
                e.PropertyName == nameof(PianoRollViewModel.VerticalZoom) ||
                e.PropertyName == nameof(PianoRollViewModel.TimelinePosition))
            {
                InvalidateVisual();
            }
        }

        private void OnNotesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // ���������Ϸ����仯ʱ����Ҫ�����¼�����
            if (e.OldItems != null)
            {
                foreach (NoteViewModel note in e.OldItems)
                {
                    note.PropertyChanged -= OnNotePropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (NoteViewModel note in e.NewItems)
                {
                    note.PropertyChanged += OnNotePropertyChanged;
                }
            }

            // ˢ����ͼ
            InvalidateVisual();
        }

        private void OnNotePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ���κ����������Է����仯ʱ��ˢ��������ͼ
            if (e.PropertyName == nameof(NoteViewModel.Velocity) ||
                e.PropertyName == nameof(NoteViewModel.StartPosition) ||
                e.PropertyName == nameof(NoteViewModel.Duration) ||
                e.PropertyName == nameof(NoteViewModel.Pitch) ||
                e.PropertyName == nameof(NoteViewModel.IsSelected))
            {
                InvalidateVisual();
            }
        }

        private void OnVelocityUpdated()
        {
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            if (ViewModel == null) return;

            var bounds = Bounds;
            
            // �������ȱ༭ģ��Ļ����߶�
            if (ViewModel.VelocityEditingModule != null)
            {
                ViewModel.VelocityEditingModule.SetCanvasHeight(bounds.Height);
            }
            
            // ���Ʊ���
            var backgroundBrush = GetResourceBrush("VelocityViewBackgroundBrush", "#20000000");
            context.DrawRectangle(backgroundBrush, null, bounds);

            // ����������
            DrawVelocityBars(context, bounds);
            
            // ���������ߣ���ѡ��
            DrawGridLines(context, bounds);
        }

        private void DrawVelocityBars(DrawingContext context, Rect bounds)
        {
            if (ViewModel?.Notes == null) return;

            foreach (var note in ViewModel.Notes)
            {
                var noteRect = ViewModel.GetNoteRect(note);
                
                // ֻ��Ⱦ����ͼ��Χ�ڵ�����
                if (noteRect.Right < 0 || noteRect.Left > bounds.Width) continue;

                // ��������״̬ѡ����Ⱦ����
                var renderType = GetVelocityRenderType(note);
                
                _velocityRenderer.DrawVelocityBar(context, note, bounds, 
                    ViewModel.Zoom, ViewModel.PixelsPerTick, renderType);
            }

            // �������ڱ༭������Ԥ��
            if (ViewModel.VelocityEditingModule?.IsEditingVelocity == true)
            {
                _velocityRenderer.DrawEditingPreview(context, bounds, 
                    ViewModel.VelocityEditingModule, ViewModel.Zoom, ViewModel.PixelsPerTick);
            }
        }

        private VelocityRenderType GetVelocityRenderType(NoteViewModel note)
        {
            if (ViewModel?.VelocityEditingModule?.EditingNotes?.Contains(note) == true)
                return VelocityRenderType.Editing;
            
            if (note.IsSelected)
                return VelocityRenderType.Selected;
                
            if (ViewModel?.DraggingNotes?.Contains(note) == true)
                return VelocityRenderType.Dragging;
                
            return VelocityRenderType.Normal;
        }

        private void DrawGridLines(DrawingContext context, Rect bounds)
        {
            if (ViewModel == null) return;

            // ����ˮƽ�ο��� (25%, 50%, 75%, 100%)
            var linePen = GetResourcePen("VelocityGridLineBrush", "#30808080", 1);
            
            var quarterHeight = bounds.Height / 4.0;
            for (int i = 1; i <= 3; i++)
            {
                var y = bounds.Height - (i * quarterHeight);
                context.DrawLine(linePen, new Point(0, y), new Point(bounds.Width, y));
            }
        }

        #region ����¼�����

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (ViewModel?.VelocityEditingModule == null) return;

            var position = e.GetPosition(this);
            var properties = e.GetCurrentPoint(this).Properties;

            if (properties.IsLeftButtonPressed)
            {
                ViewModel.VelocityEditingModule.StartEditing(position);
                e.Handled = true;
            }

            base.OnPointerPressed(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (ViewModel?.VelocityEditingModule == null) return;

            var position = e.GetPosition(this);
            
            // ֻ�����ڱ༭ʱ�����ƶ��¼�
            if (ViewModel.VelocityEditingModule.IsEditingVelocity)
            {
                // ����λ���ڻ�����Χ��
                var clampedPosition = new Point(
                    Math.Max(0, Math.Min(Bounds.Width, position.X)),
                    Math.Max(0, Math.Min(Bounds.Height, position.Y))
                );
                
                ViewModel.VelocityEditingModule.UpdateEditing(clampedPosition);
            }

            base.OnPointerMoved(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (ViewModel?.VelocityEditingModule == null) return;
            
            ViewModel.VelocityEditingModule.EndEditing();
            e.Handled = true;

            base.OnPointerReleased(e);
        }

        #endregion

        #region ��Դ��������

        private IBrush GetResourceBrush(string key, string fallbackHex)
        {
            try
            {
                if (Application.Current?.Resources.TryGetResource(key, null, out var obj) == true && obj is IBrush brush)
                    return brush;
            }
            catch { }

            try
            {
                return new SolidColorBrush(Color.Parse(fallbackHex));
            }
            catch
            {
                return Brushes.Transparent;
            }
        }

        private IPen GetResourcePen(string brushKey, string fallbackHex, double thickness = 1, DashStyle? dashStyle = null)
        {
            var brush = GetResourceBrush(brushKey, fallbackHex);
            var pen = new Pen(brush, thickness);
            if (dashStyle != null)
                pen.DashStyle = dashStyle;
            return pen;
        }

        #endregion
    }
}