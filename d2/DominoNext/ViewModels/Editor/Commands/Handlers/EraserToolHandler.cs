using System.Diagnostics;

namespace DominoNext.ViewModels.Editor.Commands
{
    /// <summary>
    /// 橡皮工具处理器
    /// </summary>
    public class EraserToolHandler
    {
        private PianoRollViewModel? _pianoRollViewModel;

        public void SetPianoRollViewModel(PianoRollViewModel viewModel)
        {
            _pianoRollViewModel = viewModel;
        }

        public void HandlePress(NoteViewModel? clickedNote)
        {
            if (clickedNote != null && _pianoRollViewModel != null)
            {
                Debug.WriteLine("橡皮工具: 删除音符");
                _pianoRollViewModel.Notes.Remove(clickedNote);
            }
        }
    }
}