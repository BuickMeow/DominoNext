using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;

namespace DominoNext.Behaviors
{
    public static class NoteEditingBehavior
    {
        public static readonly AttachedProperty<IRelayCommand<Point>?> CreateNoteCommandProperty =
            AvaloniaProperty.RegisterAttached<Control, IRelayCommand<Point>?>("CreateNoteCommand", typeof(NoteEditingBehavior));

        public static readonly AttachedProperty<IRelayCommand<Point>?> UpdateDragCommandProperty =
            AvaloniaProperty.RegisterAttached<Control, IRelayCommand<Point>?>("UpdateDragCommand", typeof(NoteEditingBehavior));

        public static readonly AttachedProperty<IRelayCommand?> EndDragCommandProperty =
            AvaloniaProperty.RegisterAttached<Control, IRelayCommand?>("EndDragCommand", typeof(NoteEditingBehavior));

        public static IRelayCommand<Point>? GetCreateNoteCommand(Control obj)
        {
            return obj.GetValue(CreateNoteCommandProperty);
        }

        public static void SetCreateNoteCommand(Control obj, IRelayCommand<Point>? value)
        {
            obj.SetValue(CreateNoteCommandProperty, value);
        }

        public static IRelayCommand<Point>? GetUpdateDragCommand(Control obj)
        {
            return obj.GetValue(UpdateDragCommandProperty);
        }

        public static void SetUpdateDragCommand(Control obj, IRelayCommand<Point>? value)
        {
            obj.SetValue(UpdateDragCommandProperty, value);
        }

        public static IRelayCommand? GetEndDragCommand(Control obj)
        {
            return obj.GetValue(EndDragCommandProperty);
        }

        public static void SetEndDragCommand(Control obj, IRelayCommand? value)
        {
            obj.SetValue(EndDragCommandProperty, value);
        }

        static NoteEditingBehavior()
        {
            // 修复：使用正确的 Avalonia 属性变更事件订阅方式
            CreateNoteCommandProperty.Changed.AddClassHandler<Control>(OnCreateNoteCommandChanged);
        }

        private static void OnCreateNoteCommandChanged(Control sender, AvaloniaPropertyChangedEventArgs e)
        {
            // 移除旧的事件处理器
            if (e.OldValue != null)
            {
                sender.PointerPressed -= OnPointerPressed;
                sender.PointerMoved -= OnPointerMoved;
                sender.PointerReleased -= OnPointerReleased;
            }

            // 添加新的事件处理器
            if (e.NewValue != null)
            {
                sender.PointerPressed += OnPointerPressed;
                sender.PointerMoved += OnPointerMoved;
                sender.PointerReleased += OnPointerReleased;
            }
        }

        private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is Control control && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                var command = GetCreateNoteCommand(control);
                var position = e.GetPosition(control);
                command?.Execute(position);
            }
        }

        private static void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (sender is Control control)
            {
                var command = GetUpdateDragCommand(control);
                var position = e.GetPosition(control);
                command?.Execute(position);
            }
        }

        private static void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (sender is Control control)
            {
                var command = GetEndDragCommand(control);
                command?.Execute(null);
            }
        }
    }
}