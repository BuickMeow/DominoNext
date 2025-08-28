using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Input;

namespace DominoNext.ViewModels.Editor.Commands
{
    /// <summary>
    /// ѡ�񹤾ߴ�����
    /// </summary>
    public class SelectToolHandler
    {
        private PianoRollViewModel? _pianoRollViewModel;

        public void SetPianoRollViewModel(PianoRollViewModel viewModel)
        {
            _pianoRollViewModel = viewModel;
        }

        public void HandlePress(Point position, NoteViewModel? clickedNote, KeyModifiers modifiers)
        {
            if (_pianoRollViewModel == null) return;

            if (clickedNote != null)
            {
                // ѡ�񹤾�֧�ֶ�������ק
                Debug.WriteLine("ѡ�񹤾�: ѡ��������׼����ק");
                
                // �����ѡ�߼�
                if (modifiers.HasFlag(KeyModifiers.Control))
                {
                    // Ctrl+������л�ѡ��״̬
                    clickedNote.IsSelected = !clickedNote.IsSelected;
                }
                else
                {
                    // �������������Ѿ���ѡ�У����ж��������ѡ�У�����ѡ��״̬׼����ק
                    bool wasAlreadySelected = clickedNote.IsSelected;
                    bool hasMultipleSelected = _pianoRollViewModel.Notes.Count(n => n.IsSelected) > 1;
                    
                    if (!wasAlreadySelected || !hasMultipleSelected)
                    {
                        // �������ѡ��ֻѡ��ǰ����
                        _pianoRollViewModel.SelectionModule.ClearSelection(_pianoRollViewModel.Notes);
                        clickedNote.IsSelected = true;
                    }
                    // ���������ѡ�����ж�ѡ����������ѡ��������ק
                }
                
                // ��ʼ��ק����ѡ�е�����
                _pianoRollViewModel.DragModule.StartDrag(clickedNote, position);
            }
            else
            {
                // ��ʼ��ѡ
                Debug.WriteLine("ѡ�񹤾�: ��ʼ��ѡ");
                if (!modifiers.HasFlag(KeyModifiers.Control))
                {
                    _pianoRollViewModel.SelectionModule.ClearSelection(_pianoRollViewModel.Notes);
                }
                _pianoRollViewModel.SelectionModule.StartSelection(position);
            }
        }
    }
}