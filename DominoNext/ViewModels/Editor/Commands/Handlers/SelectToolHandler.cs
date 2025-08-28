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
                // 选择工具支持多选和拖拽
                Debug.WriteLine("选择工具: 选择音符准备拖拽");
                
                // 处理多选逻辑
                if (modifiers.HasFlag(KeyModifiers.Control))
                {
                    // Ctrl+点击切换选择状态
                    clickedNote.IsSelected = !clickedNote.IsSelected;
                }
                else
                {
                    // 如果音符还没有被选中，或者有多个音符选中，则清除选择只选当前音符
                    bool wasAlreadySelected = clickedNote.IsSelected;
                    bool hasMultipleSelected = _pianoRollViewModel.Notes.Count(n => n.IsSelected) > 1;
                    
                    if (!wasAlreadySelected || !hasMultipleSelected)
                    {
                        // 清除其他选择只选当前音符
                        _pianoRollViewModel.SelectionModule.ClearSelection(_pianoRollViewModel.Notes);
                        clickedNote.IsSelected = true;
                    }
                    // 如果音符已经选中且有多个选中音符，保持选择状态准备拖拽
                }
                
                // 开始拖拽所有选中的音符
                _pianoRollViewModel.DragModule.StartDrag(clickedNote, position);
            }
            else
            {
                // 点击空白区域
                if (modifiers.HasFlag(KeyModifiers.Control))
                {
                    // Ctrl+点击空白区域开始框选（追加选择）
                    Debug.WriteLine("选择工具: 开始追加框选");
                    _pianoRollViewModel.SelectionModule.StartSelection(position);
                }
                else
                {
                    // 普通点击空白区域：清除所有选择并开始框选
                    Debug.WriteLine("选择工具: 清除所有选择并开始新框选");
                    _pianoRollViewModel.SelectionModule.ClearSelection(_pianoRollViewModel.Notes);
                    _pianoRollViewModel.SelectionModule.StartSelection(position);
                }
            }
        }
    }
}