using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using DominoNext.Views.Controls.Editing;
using DominoNext.ViewModels.Editor;
using DominoNext.ViewModels.Editor.State;

namespace DominoNext.Views.Controls.Editing.Input
{
    /// <summary>
    /// �������� - �Ż��汾
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
        public bool IsHoveringNote => _isHoveringNote; // �������Ƿ���ͣ��������
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
                newCursorType = "SizeAll"; // ��קʱ��ʾ�����ͷ
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
                        newCursorType = "SizeWE"; // ������Сʱ��ʾ���Ҽ�ͷ
                        isHoveringResize = true;
                    }
                    else
                    {
                        newCursorType = "SizeAll"; // ��ͣ��������ʱ��ʾ�����ͷ����קģʽ��
                    }
                }
                else
                {
                    newCursorType = "Default"; // �հ�����Ĭ�Ϲ��
                }
            }
            else if (viewModel.CurrentTool == EditorTool.Select)
            {
                var note = viewModel.GetNoteAtPosition(position);
                if (note != null)
                {
                    isHoveringNote = true;
                    newCursorType = "SizeAll"; // ѡ�񹤾���ͣ��������ʱҲ��ʾ�����ͷ
                }
                else
                {
                    newCursorType = "Default"; // �հ�����Ĭ�Ϲ��
                }
            }

            // �����ͣ״̬�仯
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
                "SizeWE" => new Cursor(StandardCursorType.SizeWestEast),     // ���Ҽ�ͷ
                "SizeAll" => new Cursor(StandardCursorType.SizeAll),        // �����ͷ
                "Hand" => new Cursor(StandardCursorType.Hand),              // ����
                _ => new Cursor(StandardCursorType.Arrow)                   // Ĭ�ϼ�ͷ
            };
        }
    }
}