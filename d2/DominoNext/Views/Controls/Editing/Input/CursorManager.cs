using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using DominoNext.Views.Controls.Editing;
using System.Diagnostics;

namespace DominoNext.Views.Controls.Editing.Input
{
    /// <summary>
    /// 光标管理器
    /// </summary>
    public class CursorManager
    {
        private string _currentCursorType = "Default";
        private bool _isHoveringResizeEdge = false;
        private readonly Control _control;

        public CursorManager(Control control)
        {
            _control = control;
        }

        /// <summary>
        /// 根据鼠标位置更新光标
        /// </summary>
        public void UpdateCursorForPosition(Point position, PianoRollViewModel? viewModel)
        {
            if (viewModel == null) return;

            string newCursorType = "Default";
            bool isHoveringResize = false;

            // 如果正在调整大小，始终显示调整光标
            if (viewModel.IsResizing)
            {
                newCursorType = "SizeWE";
                isHoveringResize = true;
            }
            // 如果正在拖拽，始终显示手型光标
            else if (viewModel.IsDragging)
            {
                newCursorType = "Hand";
            }
            // 检查是否悬停在可调整大小的音符边缘上
            else if (viewModel.CurrentTool == EditorTool.Pencil)
            {
                var note = viewModel.GetNoteAtPosition(position);
                if (note != null)
                {
                    var handle = viewModel.GetResizeHandleAtPosition(position, note);
                    if (handle == ResizeHandle.StartEdge || handle == ResizeHandle.EndEdge)
                    {
                        newCursorType = "SizeWE"; // 水平调整光标 ←→
                        isHoveringResize = true;
                    }
                    else
                    {
                        newCursorType = "Hand"; // 移动光标
                    }
                }
                else
                {
                    newCursorType = "Default"; // 默认光标
                }
            }
            else if (viewModel.CurrentTool == EditorTool.Select)
            {
                var note = viewModel.GetNoteAtPosition(position);
                if (note != null)
                {
                    newCursorType = "Hand"; // 选择工具悬停在音符上时显示手型光标
                }
                else
                {
                    newCursorType = "Default"; // 默认光标
                }
            }

            // 更新状态
            bool previousHoveringState = _isHoveringResizeEdge;
            _isHoveringResizeEdge = isHoveringResize;
            
            UpdateCursor(newCursorType);

            // 返回悬停状态是否发生变化
            HoveringStateChanged = previousHoveringState != _isHoveringResizeEdge;
        }

        /// <summary>
        /// 更新控件光标
        /// </summary>
        private void UpdateCursor(string cursorType)
        {
            if (_currentCursorType == cursorType) return;

            _currentCursorType = cursorType;

            // 设置Avalonia光标
            _control.Cursor = cursorType switch
            {
                "SizeWE" => new Cursor(StandardCursorType.SizeWestEast),
                "Hand" => new Cursor(StandardCursorType.Hand),
                _ => new Cursor(StandardCursorType.Arrow) // 默认使用箭头光标
            };

            Debug.WriteLine($"光标已更新为: {cursorType}");
        }

        /// <summary>
        /// 重置光标状态
        /// </summary>
        public void Reset()
        {
            _isHoveringResizeEdge = false;
            UpdateCursor("Default");
        }

        // 属性
        public bool IsHoveringResizeEdge => _isHoveringResizeEdge;
        public bool HoveringStateChanged { get; private set; }
    }
}