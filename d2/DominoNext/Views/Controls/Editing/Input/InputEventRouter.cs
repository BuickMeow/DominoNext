using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using DominoNext.Views.Controls.Editing;

namespace DominoNext.Views.Controls.Editing.Input
{
    /// <summary>
    /// 输入事件路由器
    /// </summary>
    public class InputEventRouter
    {
        /// <summary>
        /// 处理指针按下事件
        /// </summary>
        public void HandlePointerPressed(PointerPressedEventArgs e, PianoRollViewModel? viewModel, Control control)
        {
            if (viewModel?.EditorCommands == null) return;

            var position = e.GetPosition(control);
            var properties = e.GetCurrentPoint(control).Properties;

            Debug.WriteLine($"指针位置: {position}, 工具: {viewModel.CurrentTool}");

            if (properties.IsLeftButtonPressed)
            {
                var commandParameter = new EditorInteractionArgs
                {
                    Position = position,
                    Tool = viewModel.CurrentTool,
                    Modifiers = e.KeyModifiers,
                    InteractionType = EditorInteractionType.Press
                };

                if (viewModel.EditorCommands.HandleInteractionCommand.CanExecute(commandParameter))
                {
                    viewModel.EditorCommands.HandleInteractionCommand.Execute(commandParameter);
                    control.Focus();
                    e.Pointer.Capture(control);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// 处理指针移动事件
        /// </summary>
        public void HandlePointerMoved(PointerEventArgs e, PianoRollViewModel? viewModel)
        {
            if (viewModel?.EditorCommands == null) return;

            var position = e.GetPosition((Control)e.Source!);
            var commandParameter = new EditorInteractionArgs
            {
                Position = position,
                Tool = viewModel.CurrentTool,
                InteractionType = EditorInteractionType.Move
            };

            if (viewModel.EditorCommands.HandleInteractionCommand.CanExecute(commandParameter))
            {
                viewModel.EditorCommands.HandleInteractionCommand.Execute(commandParameter);
            }
        }

        /// <summary>
        /// 处理指针释放事件
        /// </summary>
        public void HandlePointerReleased(PointerReleasedEventArgs e, PianoRollViewModel? viewModel)
        {
            if (viewModel?.EditorCommands == null) return;

            var position = e.GetPosition((Control)e.Source!);
            var commandParameter = new EditorInteractionArgs
            {
                Position = position,
                Tool = viewModel.CurrentTool,
                InteractionType = EditorInteractionType.Release
            };

            if (viewModel.EditorCommands.HandleInteractionCommand.CanExecute(commandParameter))
            {
                viewModel.EditorCommands.HandleInteractionCommand.Execute(commandParameter);
            }

            e.Pointer.Capture(null);
            e.Handled = true;
        }

        /// <summary>
        /// 处理键盘按下事件
        /// </summary>
        public void HandleKeyDown(KeyEventArgs e, PianoRollViewModel? viewModel)
        {
            if (viewModel?.EditorCommands == null) return;

            var keyCommandParameter = new KeyCommandArgs
            {
                Key = e.Key,
                Modifiers = e.KeyModifiers
            };

            if (viewModel.EditorCommands.HandleKeyCommand.CanExecute(keyCommandParameter))
            {
                viewModel.EditorCommands.HandleKeyCommand.Execute(keyCommandParameter);
                e.Handled = true;
            }
        }
    }
}