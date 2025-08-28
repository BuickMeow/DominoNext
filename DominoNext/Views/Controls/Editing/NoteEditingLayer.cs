using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using DominoNext.ViewModels.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DominoNext.Views.Controls.Editing
{
    /// <summary>
    /// 符合MVVM标准的音符编辑层
    /// View层只负责渲染和事件转发，所有业务逻辑委托给ViewModel和Command
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

        #region 渲染资源 - 仅用于显示

        private readonly Color _noteColor = Color.Parse("#4CAF50");
        private readonly IPen _noteBorderPen = new Pen(new SolidColorBrush(Color.Parse("#2E7D32")), 2);
        private readonly Color _selectedNoteColor = Color.Parse("#FF9800");
        private readonly IPen _selectedNoteBorderPen = new Pen(new SolidColorBrush(Color.Parse("#F57C00")), 2);
        private readonly Color _previewNoteColor = Color.Parse("#81C784");

        // 预览音符使用实线边框
        private readonly IPen _previewNoteBorderPen = new Pen(new SolidColorBrush(Color.Parse("#66BB6A")), 2);

        // 正在创建的音符样式 - 使用更明显的颜色和实线边框
        private readonly Color _creatingNoteColor = Color.Parse("#8BC34A");
        private readonly IPen _creatingNoteBorderPen = new Pen(new SolidColorBrush(Color.Parse("#689F38")), 2);

        // 选择框使用实线边框
        private readonly IPen _selectionBoxPen = new Pen(new SolidColorBrush(Color.Parse("#2196F3")), 2);
        private readonly IBrush _selectionBoxBrush = new SolidColorBrush(Color.Parse("#2196F3"), 0.3);

        #endregion

        #region 缓存和性能优化

        private readonly Dictionary<NoteViewModel, Rect> _visibleNoteCache = new();
        private bool _cacheInvalid = true;
        private Rect _lastViewport;

        #endregion

        #region 构造函数和初始化

        public NoteEditingLayer()
        {
            Debug.WriteLine("NoteEditingLayer constructor - MVVM version");

            // 关键修复：确保能接收指针事件 - 使用正确的Avalonia属性
            //this.Background = Brushes.Transparent;  // 修复CS0103错误
            IsHitTestVisible = true;
            Focusable = true;

            // 确保控件能够拉伸填充父容器
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        }

        static NoteEditingLayer()
        {
            // 监听ViewModel变化，但只进行UI相关的绑定和清理
            ViewModelProperty.Changed.AddClassHandler<NoteEditingLayer>((layer, e) =>
            {
                Debug.WriteLine($"ViewModel changed: {e.OldValue} -> {e.NewValue}");
                layer.OnViewModelChanged(e.OldValue as PianoRollViewModel, e.NewValue as PianoRollViewModel);
            });
        }

        private void OnViewModelChanged(PianoRollViewModel? oldViewModel, PianoRollViewModel? newViewModel)
        {
            // 取消旧的绑定
            if (oldViewModel != null)
            {
                oldViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                oldViewModel.Notes.CollectionChanged -= OnNotesCollectionChanged;  // 修复CS8602警告
            }

            // 建立新的绑定
            if (newViewModel != null)
            {
                newViewModel.PropertyChanged += OnViewModelPropertyChanged;
                newViewModel.Notes.CollectionChanged += OnNotesCollectionChanged;

                Debug.WriteLine($"ViewModel绑定成功. 当前工具: {newViewModel.CurrentTool}, 音符数量: {newViewModel.Notes.Count}");

                // 设置PianoRollViewModel到EditorCommands
                newViewModel.EditorCommands?.SetPianoRollViewModel(newViewModel);
            }

            InvalidateCache();
        }

        #endregion

        #region 事件处理 - 仅转发到Commands

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            Debug.WriteLine("=== OnPointerPressed (MVVM) ===");
            Debug.WriteLine($"控件边界: {Bounds}");
            Debug.WriteLine($"IsHitTestVisible: {IsHitTestVisible}");

            base.OnPointerPressed(e);

            if (ViewModel?.EditorCommands == null)
            {
                Debug.WriteLine("警告: ViewModel或EditorCommands为空");
                return;
            }

            var position = e.GetPosition(this);
            var properties = e.GetCurrentPoint(this).Properties;

            Debug.WriteLine($"指针位置: {position}, 工具: {ViewModel.CurrentTool}");
            Debug.WriteLine($"左键按下: {properties.IsLeftButtonPressed}");

            if (properties.IsLeftButtonPressed)
            {
                // 使用Command模式处理不同工具的操作
                var commandParameter = new EditorInteractionArgs
                {
                    Position = position,
                    Tool = ViewModel.CurrentTool,
                    Modifiers = e.KeyModifiers,
                    InteractionType = EditorInteractionType.Press
                };

                // 委托给EditorCommands处理
                if (ViewModel.EditorCommands.HandleInteractionCommand.CanExecute(commandParameter))
                {
                    ViewModel.EditorCommands.HandleInteractionCommand.Execute(commandParameter);

                    // 获得焦点和指针捕获
                    this.Focus();
                    e.Pointer.Capture(this);
                    e.Handled = true;
                }
            }

            Debug.WriteLine("=== OnPointerPressed End ===");
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            if (ViewModel?.EditorCommands == null) return;

            var position = e.GetPosition(this);

            var commandParameter = new EditorInteractionArgs
            {
                Position = position,
                Tool = ViewModel.CurrentTool,
                InteractionType = EditorInteractionType.Move
            };

            // 委托给Command处理鼠标移动
            if (ViewModel.EditorCommands.HandleInteractionCommand.CanExecute(commandParameter))
            {
                ViewModel.EditorCommands.HandleInteractionCommand.Execute(commandParameter);
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (ViewModel?.EditorCommands == null) return;

            var position = e.GetPosition(this);

            var commandParameter = new EditorInteractionArgs
            {
                Position = position,
                Tool = ViewModel.CurrentTool,
                InteractionType = EditorInteractionType.Release
            };

            // 委托给Command处理鼠标释放
            if (ViewModel.EditorCommands.HandleInteractionCommand.CanExecute(commandParameter))
            {
                ViewModel.EditorCommands.HandleInteractionCommand.Execute(commandParameter);
            }

            // 释放指针捕获
            e.Pointer.Capture(null);
            e.Handled = true;
        }

        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);

            // 清除预览状态
            ViewModel?.EditorCommands?.ClearPreviewCommand?.Execute(null);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (ViewModel?.EditorCommands == null) return;

            // 使用Command模式处理键盘快捷键
            var keyCommandParameter = new KeyCommandArgs
            {
                Key = e.Key,
                Modifiers = e.KeyModifiers
            };

            if (ViewModel.EditorCommands.HandleKeyCommand.CanExecute(keyCommandParameter))
            {
                ViewModel.EditorCommands.HandleKeyCommand.Execute(keyCommandParameter);
                e.Handled = true;
            }
        }

        #endregion

        #region 渲染 - 纯UI展示逻辑

        public override void Render(DrawingContext context)
        {
            if (ViewModel == null) return;

            var bounds = Bounds;

            // 关键修复：绘制透明背景以确保接收指针事件
            context.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, bounds.Width, bounds.Height));

            var viewport = new Rect(0, 0, bounds.Width, bounds.Height);

            Debug.WriteLine($"渲染NoteEditingLayer. 音符数: {ViewModel.Notes?.Count ?? 0}");

            // 更新可见音符缓存
            if (_cacheInvalid || !viewport.Equals(_lastViewport))
            {
                UpdateVisibleNotesCache(viewport);
                _lastViewport = viewport;
                _cacheInvalid = false;
            }

            // 渲染音符
            RenderNotes(context);

            // 渲染正在创建的音符（优先级高于预览音符）
            RenderCreatingNote(context);

            // 渲染预览音符（只在不创建音符时显示）
            if (!ViewModel.IsCreatingNote)
            {
                RenderPreviewNote(context);
            }

            // 渲染选择框
            RenderSelectionBox(context);
        }

        // 优化：渲染正在创建的音符，支持实时长度预览
        private void RenderCreatingNote(DrawingContext context)
        {
            if (ViewModel?.CreatingNote == null || !ViewModel.IsCreatingNote) return;

            var creatingRect = CalculateNoteRect(ViewModel.CreatingNote);
            if (creatingRect.Width > 0 && creatingRect.Height > 0)
            {
                // 使用专门的创建音符样式，实线边框，比普通音符稍透明
                var brush = new SolidColorBrush(_creatingNoteColor, 0.85);
                context.DrawRectangle(brush, _creatingNoteBorderPen, creatingRect);

                // 显示当前长度信息，实现所见即所得
                var durationText = ViewModel.CreatingNote.Duration.ToString();
                var typeface = new Typeface(FontFamily.Default);
                var formattedText = new FormattedText(
                    durationText,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    12, // 稍大的字体
                    Brushes.Black);

                var textPosition = new Point(
                    creatingRect.X + (creatingRect.Width - formattedText.Width) / 2,
                    creatingRect.Y + (creatingRect.Height - formattedText.Height) / 2);

                // 只在音符足够大时显示文本
                if (creatingRect.Width > formattedText.Width + 4 && creatingRect.Height > formattedText.Height + 2)
                {
                    // 绘制文本背景以提高可读性
                    var textBounds = new Rect(
                        textPosition.X - 2,
                        textPosition.Y - 1,
                        formattedText.Width + 4,
                        formattedText.Height + 2);
                    context.DrawRectangle(new SolidColorBrush(Colors.White, 0.8), null, textBounds);

                    context.DrawText(formattedText, textPosition);
                }
            }
        }

        private void RenderNotes(DrawingContext context)
        {
            int drawnNotes = 0;
            foreach (var kvp in _visibleNoteCache)
            {
                var note = kvp.Key;
                var rect = kvp.Value;

                if (rect.Width > 0 && rect.Height > 0)
                {
                    DrawNote(context, note, rect);
                    drawnNotes++;
                }
            }

            Debug.WriteLine($"绘制了 {drawnNotes} 个可见音符");
        }

        private void RenderPreviewNote(DrawingContext context)
        {
            if (ViewModel?.PreviewNote == null) return;

            var previewRect = CalculateNoteRect(ViewModel.PreviewNote);
            if (previewRect.Width > 0 && previewRect.Height > 0)
            {
                // 使用半透明填充，实线边框，实现所见即所得效果
                var brush = new SolidColorBrush(_previewNoteColor, 0.6);
                context.DrawRectangle(brush, _previewNoteBorderPen, previewRect);

                // 可选：在预览音符上显示时值信息
                if (ViewModel.PreviewNote.Duration != null)
                {
                    var durationText = ViewModel.PreviewNote.Duration.ToString();
                    var typeface = new Typeface(FontFamily.Default);
                    var formattedText = new FormattedText(
                        durationText,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        10,
                        Brushes.Black);

                    // 在音符中央显示时值
                    var textPosition = new Point(
                        previewRect.X + (previewRect.Width - formattedText.Width) / 2,
                        previewRect.Y + (previewRect.Height - formattedText.Height) / 2);

                    if (previewRect.Width > formattedText.Width + 2 && previewRect.Height > formattedText.Height + 2)
                    {
                        context.DrawText(formattedText, textPosition);
                    }
                }
            }
        }

        private void RenderSelectionBox(DrawingContext context)
        {
            if (ViewModel?.SelectionStart == null || ViewModel?.SelectionEnd == null) return;

            var start = ViewModel.SelectionStart.Value;
            var end = ViewModel.SelectionEnd.Value;

            var x = Math.Min(start.X, end.X);
            var y = Math.Min(start.Y, end.Y);
            var width = Math.Abs(end.X - start.X);
            var height = Math.Abs(end.Y - start.Y);

            var selectionRect = new Rect(x, y, width, height);
            // 使用实线边框而不是虚线
            context.DrawRectangle(_selectionBoxBrush, _selectionBoxPen, selectionRect);
        }

        private void DrawNote(DrawingContext context, NoteViewModel note, Rect rect)
        {
            var opacity = Math.Max(0.7, note.Velocity / 127.0);

            IBrush brush;
            IPen pen;

            if (note.IsSelected)
            {
                brush = new SolidColorBrush(_selectedNoteColor, opacity);
                pen = _selectedNoteBorderPen;
            }
            else
            {
                brush = new SolidColorBrush(_noteColor, opacity);
                pen = _noteBorderPen;
            }

            context.DrawRectangle(brush, pen, rect);
        }

        #endregion

        #region 缓存管理

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PianoRollViewModel.Zoom) ||
                e.PropertyName == nameof(PianoRollViewModel.VerticalZoom) ||
                e.PropertyName == nameof(PianoRollViewModel.CurrentTool) ||
                e.PropertyName == nameof(PianoRollViewModel.SelectionStart) ||
                e.PropertyName == nameof(PianoRollViewModel.SelectionEnd) ||
                e.PropertyName == nameof(PianoRollViewModel.PreviewNote) ||
                e.PropertyName == nameof(PianoRollViewModel.GridQuantization) ||
                e.PropertyName == nameof(PianoRollViewModel.IsCreatingNote) ||
                e.PropertyName == nameof(PianoRollViewModel.CreatingNote))
            {
                InvalidateCache();
            }
        }

        private void OnNotesCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("音符集合发生变化");
            InvalidateCache();
        }

        private void InvalidateCache()
        {
            _cacheInvalid = true;
            InvalidateVisual();
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

            Debug.WriteLine($"缓存更新: {_visibleNoteCache.Count}/{ViewModel.Notes.Count} 个可见音符");
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