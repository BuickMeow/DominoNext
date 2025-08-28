using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using DominoNext.Views.Controls.Editing;
using System.Diagnostics;

namespace DominoNext.Views.Controls.Editing.Input
{
    /// <summary>
    /// ��������
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
        /// �������λ�ø��¹��
        /// </summary>
        public void UpdateCursorForPosition(Point position, PianoRollViewModel? viewModel)
        {
            if (viewModel == null) return;

            string newCursorType = "Default";
            bool isHoveringResize = false;

            // ������ڵ�����С��ʼ����ʾ�������
            if (viewModel.IsResizing)
            {
                newCursorType = "SizeWE";
                isHoveringResize = true;
            }
            // ���������ק��ʼ����ʾ���͹��
            else if (viewModel.IsDragging)
            {
                newCursorType = "Hand";
            }
            // ����Ƿ���ͣ�ڿɵ�����С��������Ե��
            else if (viewModel.CurrentTool == EditorTool.Pencil)
            {
                var note = viewModel.GetNoteAtPosition(position);
                if (note != null)
                {
                    var handle = viewModel.GetResizeHandleAtPosition(position, note);
                    if (handle == ResizeHandle.StartEdge || handle == ResizeHandle.EndEdge)
                    {
                        newCursorType = "SizeWE"; // ˮƽ������� ����
                        isHoveringResize = true;
                    }
                    else
                    {
                        newCursorType = "Hand"; // �ƶ����
                    }
                }
                else
                {
                    newCursorType = "Default"; // Ĭ�Ϲ��
                }
            }
            else if (viewModel.CurrentTool == EditorTool.Select)
            {
                var note = viewModel.GetNoteAtPosition(position);
                if (note != null)
                {
                    newCursorType = "Hand"; // ѡ�񹤾���ͣ��������ʱ��ʾ���͹��
                }
                else
                {
                    newCursorType = "Default"; // Ĭ�Ϲ��
                }
            }

            // ����״̬
            bool previousHoveringState = _isHoveringResizeEdge;
            _isHoveringResizeEdge = isHoveringResize;
            
            UpdateCursor(newCursorType);

            // ������ͣ״̬�Ƿ����仯
            HoveringStateChanged = previousHoveringState != _isHoveringResizeEdge;
        }

        /// <summary>
        /// ���¿ؼ����
        /// </summary>
        private void UpdateCursor(string cursorType)
        {
            if (_currentCursorType == cursorType) return;

            _currentCursorType = cursorType;

            // ����Avalonia���
            _control.Cursor = cursorType switch
            {
                "SizeWE" => new Cursor(StandardCursorType.SizeWestEast),
                "Hand" => new Cursor(StandardCursorType.Hand),
                _ => new Cursor(StandardCursorType.Arrow) // Ĭ��ʹ�ü�ͷ���
            };

            Debug.WriteLine($"����Ѹ���Ϊ: {cursorType}");
        }

        /// <summary>
        /// ���ù��״̬
        /// </summary>
        public void Reset()
        {
            _isHoveringResizeEdge = false;
            UpdateCursor("Default");
        }

        // ����
        public bool IsHoveringResizeEdge => _isHoveringResizeEdge;
        public bool HoveringStateChanged { get; private set; }
    }
}