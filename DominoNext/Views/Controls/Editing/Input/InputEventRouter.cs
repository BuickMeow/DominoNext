using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using DominoNext.ViewModels.Editor;
using DominoNext.Views.Controls.Editing;

namespace DominoNext.Views.Controls.Editing.Input
{
    /// <summary>
    /// �����¼�·���� - �򻯰汾
    /// </summary>
    public class InputEventRouter
    {
        public void HandlePointerPressed(PointerPressedEventArgs e, PianoRollViewModel? viewModel, Control control)
        {
            Debug.WriteLine("=== OnPointerPressed (MVVM) ===");

            if (viewModel?.EditorCommands == null)
            {
                Debug.WriteLine("����: ViewModel��EditorCommandsΪ��");
                return;
            }

            var position = e.GetPosition(control);
            var properties = e.GetCurrentPoint(control).Properties;

            Debug.WriteLine($"ָ��λ��: {position}, ����: {viewModel.CurrentTool}");

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

            Debug.WriteLine("=== OnPointerPressed End ===");
        }

        public void HandlePointerMoved(PointerEventArgs e, PianoRollViewModel? viewModel)
        {
            if (viewModel?.EditorCommands == null) return;

            var position = e.GetPosition((Visual)e.Source!);

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

        public void HandlePointerReleased(PointerReleasedEventArgs e, PianoRollViewModel? viewModel)
        {
            if (viewModel?.EditorCommands == null) return;

            var position = e.GetPosition((Visual)e.Source!);

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