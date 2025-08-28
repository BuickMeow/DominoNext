using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using DominoNext.Views.Controls.Editing;
using DominoNext.ViewModels.Editor;
using DominoNext.ViewModels.Editor.State;

namespace DominoNext.Views.Controls.Editing.Input
{
    /// <summary>
    /// 光标管理器 - 优化版本
    /// </summary>
    public class CursorManager
    {
        private readonly Control _control;
        private string _currentCursorType = "Default";
        private bool _isHoveringResizeEdge = false;
        private bool _isHoveringNote = false;

        public CursorManager(Control control)
        {
            _control = control;
        }

        public bool IsHoveringResizeEdge => _isHoveringResizeEdge;
        public bool IsHoveringNote => _isHoveringNote; // 新增：是否悬停在音符上
        public bool HoveringStateChanged { get; private set; }

        public void UpdateCursorForPosition(Point position, PianoRollViewModel? viewModel)
        {
            HoveringStateChanged = false;
            if (viewModel == null) return;

            string newCursorType = "Default";
            bool isHoveringResize = false;
            bool isHoveringNote = false;

            if (viewModel.ResizeState.IsResizing)
            {
                newCursorType = "SizeWE";
                isHoveringResize = true;
            }
            else if (viewModel.DragState.IsDragging)
            {
                newCursorType = "SizeAll"; // 拖拽时显示四向箭头
            }
            else if (viewModel.CurrentTool == EditorTool.Pencil)
            {
                var note = viewModel.GetNoteAtPosition(position);
                if (note != null)
                {
                    isHoveringNote = true;
                    var handle = viewModel.GetResizeHandleAtPosition(position, note);
                    if (handle == ResizeHandle.StartEdge || handle == ResizeHandle.EndEdge)
                    {
                        newCursorType = "SizeWE"; // 调整大小时显示左右箭头
                        isHoveringResize = true;
                    }
                    else
                    {
                        newCursorType = "SizeAll"; // 悬停在音符上时显示四向箭头（拖拽模式）
                    }
                }
                else
                {
                    newCursorType = "Default"; // 空白区域默认光标
                }
            }
            else if (viewModel.CurrentTool == EditorTool.Select)
            {
                var note = viewModel.GetNoteAtPosition(position);
                if (note != null)
                {
                    isHoveringNote = true;
                    newCursorType = "SizeAll"; // 选择工具悬停在音符上时也显示四向箭头
                }
                else
                {
                    newCursorType = "Default"; // 空白区域默认光标
                }
            }

            // 检测悬停状态变化
            bool previousHoveringResizeState = _isHoveringResizeEdge;
            bool previousHoveringNoteState = _isHoveringNote;
            
            _isHoveringResizeEdge = isHoveringResize;
            _isHoveringNote = isHoveringNote;
            
            if (previousHoveringResizeState != _isHoveringResizeEdge || 
                previousHoveringNoteState != _isHoveringNote)
            {
                HoveringStateChanged = true;
            }
            
            UpdateCursor(newCursorType);
        }

        public void Reset()
        {
            _isHoveringResizeEdge = false;
            _isHoveringNote = false;
            UpdateCursor("Default");
        }

        private void UpdateCursor(string cursorType)
        {
            if (_currentCursorType == cursorType) return;

            _currentCursorType = cursorType;

            _control.Cursor = cursorType switch
            {
                "SizeWE" => new Cursor(StandardCursorType.SizeWestEast),     // 左右箭头
                "SizeAll" => new Cursor(StandardCursorType.SizeAll),        // 四向箭头
                "Hand" => new Cursor(StandardCursorType.Hand),              // 手形
                _ => new Cursor(StandardCursorType.Arrow)                   // 默认箭头
            };
        }
    }
}