using Avalonia;
using Avalonia.Input;

namespace DominoNext.ViewModels.Editor.Commands.Handlers
{
    /// <summary>
    /// 力度编辑工具处理器
    /// </summary>
    public class VelocityToolHandler
    {
        private PianoRollViewModel? _pianoRollViewModel;

        public void SetPianoRollViewModel(PianoRollViewModel viewModel)
        {
            _pianoRollViewModel = viewModel;
        }

        public void HandlePress(Point position, NoteViewModel? clickedNote, KeyModifiers modifiers)
        {
            if (_pianoRollViewModel?.VelocityEditingModule == null) return;

            // 开始力度编辑
            _pianoRollViewModel.VelocityEditingModule.StartEditing(position);
        }

        public void HandleMove(Point position)
        {
            if (_pianoRollViewModel?.VelocityEditingModule == null) return;

            // 更新力度编辑
            _pianoRollViewModel.VelocityEditingModule.UpdateEditing(position);
        }

        public void HandleRelease(Point position)
        {
            if (_pianoRollViewModel?.VelocityEditingModule == null) return;

            // 结束力度编辑
            _pianoRollViewModel.VelocityEditingModule.EndEditing();
        }
    }
}