using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Input;

namespace DominoNext.ViewModels.Editor.Commands
{
    /// <summary>
    /// 选择工具处理器
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
                // 选择工具支持多音符拖拽
                Debug.WriteLine("选择工具: 选择音符并准备拖拽");
                
                // 处理多选逻辑
                if (modifiers.HasFlag(KeyModifiers.Control))
                {
                    // Ctrl+点击：切换选择状态
                    clickedNote.IsSelected = !clickedNote.IsSelected;
                }
                else
                {
                    // 如果点击的音符已经被选中，且有多个音符被选中，保持选择状态准备拖拽
                    bool wasAlreadySelected = clickedNote.IsSelected;
                    bool hasMultipleSelected = _pianoRollViewModel.Notes.Count(n => n.IsSelected) > 1;
                    
                    if (!wasAlreadySelected || !hasMultipleSelected)
                    {
                        // 清除其他选择，只选择当前音符
                        _pianoRollViewModel.SelectionModule.ClearSelection(_pianoRollViewModel.Notes);
                        clickedNote.IsSelected = true;
                    }
                    // 如果音符已选中且有多选，保持所有选择用于拖拽
                }
                
                // 开始拖拽所有选中的音符
                _pianoRollViewModel.DragModule.StartDrag(clickedNote, position);
            }
            else
            {
                // 开始框选
                Debug.WriteLine("选择工具: 开始框选");
                if (!modifiers.HasFlag(KeyModifiers.Control))
                {
                    _pianoRollViewModel.SelectionModule.ClearSelection(_pianoRollViewModel.Notes);
                }
                _pianoRollViewModel.SelectionModule.StartSelection(position);
            }
        }
    }
}