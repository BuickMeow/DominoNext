using System.Diagnostics;

namespace DominoNext.ViewModels.Editor.Commands
{
    /// <summary>
    /// ��Ƥ���ߴ�����
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
                Debug.WriteLine("��Ƥ����: ɾ������");
                _pianoRollViewModel.Notes.Remove(clickedNote);
            }
        }
    }
}