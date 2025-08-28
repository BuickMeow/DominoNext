using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Input;
using DominoNext.ViewModels.Editor.State;

namespace DominoNext.ViewModels.Editor.Commands
{
    /// <summary>
    /// 铅笔工具处理器
    /// </summary>
    public class PencilToolHandler
    {
        private PianoRollViewModel? _pianoRollViewModel;

        public void SetPianoRollViewModel(PianoRollViewModel viewModel)
        {
            _pianoRollViewModel = viewModel;
        }

        public void HandlePress(Point position, NoteViewModel? clickedNote, KeyModifiers modifiers)
        {
            if (_pianoRollViewModel == null) return;

            if (clickedNote == null)
            {
                // 开始拖拽创建新音符
                Debug.WriteLine("铅笔工具: 开始拖拽创建新音符");
                _pianoRollViewModel.CreationModule.StartCreating(position);
            }
            else
            {
                // 检查是否点击在音符边缘以调整长度
                var resizeHandle = _pianoRollViewModel.ResizeModule.GetResizeHandleAtPosition(position, clickedNote);

                if (resizeHandle != ResizeHandle.None)
                {
                    // 开始音符长度调整
                    Debug.WriteLine($"铅笔工具: 开始调整音符长度 - {resizeHandle}");

                    // 处理多选调整大小
                    if (modifiers.HasFlag(KeyModifiers.Control))
                    {
                        clickedNote.IsSelected = !clickedNote.IsSelected;
                    }
                    else
                    {
                        if (!clickedNote.IsSelected)
                        {
                            _pianoRollViewModel.SelectionModule.ClearSelection(_pianoRollViewModel.Notes);
                            clickedNote.IsSelected = true;
                        }
                    }

                    _pianoRollViewModel.ResizeModule.StartResize(position, clickedNote, resizeHandle);
                }
                else
                {
                    // 选择并开始拖拽现有音符
                    Debug.WriteLine("铅笔工具: 开始拖拽现有音符");
                    
                    // 处理多选逻辑
                    if (modifiers.HasFlag(KeyModifiers.Control))
                    {
                        clickedNote.IsSelected = !clickedNote.IsSelected;
                    }
                    else
                    {
                        bool wasAlreadySelected = clickedNote.IsSelected;
                        bool hasMultipleSelected = _pianoRollViewModel.Notes.Count(n => n.IsSelected) > 1;
                        
                        if (!wasAlreadySelected || !hasMultipleSelected)
                        {
                            _pianoRollViewModel.SelectionModule.ClearSelection(_pianoRollViewModel.Notes);
                            clickedNote.IsSelected = true;
                        }
                    }
                    
                    if (clickedNote.IsSelected)
                    {
                        _pianoRollViewModel.DragModule.StartDrag(clickedNote, position);
                    }
                }
            }
        }
    }
}