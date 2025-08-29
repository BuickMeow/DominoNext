using Avalonia;
using DominoNext.Services.Interfaces;
using DominoNext.ViewModels.Editor.State;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoNext.ViewModels.Editor.Modules
{
    /// <summary>
    /// ���ȱ༭ģ�� - �����������ȵĽ����༭
    /// </summary>
    public class VelocityEditingModule
    {
        private readonly ICoordinateService _coordinateService;
        private readonly VelocityEditingState _state;
        private PianoRollViewModel? _pianoRollViewModel;

        // ����Ǧ��ģʽ�����������¼
        private HashSet<NoteViewModel> _processedNotes = new();

        private double _canvasHeight = 100.0; // ���滭���߶�

        public event Action? OnVelocityUpdated;

        public VelocityEditingModule(ICoordinateService coordinateService)
        {
            _coordinateService = coordinateService;
            _state = new VelocityEditingState();
        }

        public void SetPianoRollViewModel(PianoRollViewModel viewModel)
        {
            _pianoRollViewModel = viewModel;
        }

        /// <summary>
        /// ���û����߶� - ��VelocityViewCanvas����
        /// </summary>
        public void SetCanvasHeight(double height)
        {
            _canvasHeight = height;
            System.Diagnostics.Debug.WriteLine($"Canvas height set to: {height}");
        }

        #region ��������

        public bool IsEditingVelocity => _state.IsEditing;
        public List<NoteViewModel>? EditingNotes => _state.EditingNotes;
        public List<Point> EditingPath => _state.EditingPath;
        public Point? CurrentEditPosition => _state.CurrentPosition;

        #endregion

        #region ���ȱ༭����

        /// <summary>
        /// ��ʼ���ȱ༭
        /// </summary>
        public void StartEditing(Point position)
        {
            if (_pianoRollViewModel == null) return;

            System.Diagnostics.Debug.WriteLine($"StartEditing called with position: {position}, tool: {_pianoRollViewModel.CurrentTool}");

            _state.StartEditing(position);
            _processedNotes.Clear(); // ����Ѵ���������¼

            // ���ݵ�ǰ����ģʽȷ���༭Ŀ��
            switch (_pianoRollViewModel.CurrentTool)
            {
                case EditorTool.Select:
                    System.Diagnostics.Debug.WriteLine("Using Select tool mode");
                    // ѡ�񹤾ߣ��༭ѡ�е�����
                    StartSelectModeEditing(position);
                    break;
                    
                case EditorTool.Pencil:
                    System.Diagnostics.Debug.WriteLine("Using Pencil tool mode");
                    // Ǧ�ʹ��ߣ��ֻ�ģʽ�༭
                    StartPencilModeEditing(position);
                    break;
                    
                default:
                    System.Diagnostics.Debug.WriteLine($"Unsupported tool: {_pianoRollViewModel.CurrentTool}");
                    break;
            }

            OnVelocityUpdated?.Invoke();
        }

        /// <summary>
        /// �������ȱ༭
        /// </summary>
        public void UpdateEditing(Point position)
        {
            if (!_state.IsEditing || _pianoRollViewModel == null) return;

            _state.UpdatePosition(position);

            switch (_pianoRollViewModel.CurrentTool)
            {
                case EditorTool.Select:
                    UpdateSelectModeEditing(position);
                    break;
                    
                case EditorTool.Pencil:
                    UpdatePencilModeEditing(position);
                    break;
            }

            OnVelocityUpdated?.Invoke();
        }

        /// <summary>
        /// �������ȱ༭
        /// </summary>
        public void EndEditing()
        {
            if (!_state.IsEditing) return;

            // Ӧ�����յ����ȸ���
            ApplyVelocityChanges();
            
            _state.EndEditing();
            _processedNotes.Clear(); // ����Ѵ���������¼
            OnVelocityUpdated?.Invoke();
        }

        /// <summary>
        /// ȡ�����ȱ༭
        /// </summary>
        public void CancelEditing()
        {
            if (!_state.IsEditing) return;

            // �ָ�ԭʼ����ֵ
            RestoreOriginalVelocities();
            
            _state.EndEditing();
            _processedNotes.Clear(); // ����Ѵ���������¼
            OnVelocityUpdated?.Invoke();
        }

        #endregion

        #region ѡ�񹤾�ģʽ

        private void StartSelectModeEditing(Point position)
        {
            if (_pianoRollViewModel == null) return;

            // ��ȡѡ�е�����
            var selectedNotes = _pianoRollViewModel.Notes.Where(n => n.IsSelected).ToList();
            
            if (!selectedNotes.Any())
            {
                // ���û��ѡ������������ѡ����λ�õ�����
                var clickedNote = FindNoteAtPosition(position);
                if (clickedNote != null)
                {
                    selectedNotes.Add(clickedNote);
                    clickedNote.IsSelected = true;
                }
            }

            if (selectedNotes.Any())
            {
                _state.SetEditingNotes(selectedNotes);
                _state.SaveOriginalVelocities(selectedNotes);
            }
        }

        private void UpdateSelectModeEditing(Point position)
        {
            if (_state.EditingNotes?.Any() != true) return;

            // �������ȱ仯��
            var deltaY = position.Y - _state.StartPosition.Y;
            var velocityChange = CalculateVelocityChange(deltaY);

            // Ӧ�����ȱ仯�����б༭�е�����
            foreach (var note in _state.EditingNotes)
            {
                if (_state.OriginalVelocities.TryGetValue(note, out var originalVelocity))
                {
                    var newVelocity = Math.Max(1, Math.Min(127, originalVelocity + velocityChange));
                    note.Velocity = newVelocity;
                }
            }
        }

        #endregion

        #region Ǧ�ʹ���ģʽ - ����ֵģʽ���򻯰棩

        private void StartPencilModeEditing(Point position)
        {
            if (_pianoRollViewModel == null) return;

            System.Diagnostics.Debug.WriteLine($"Starting pencil mode editing at {position}");
            
            // ����Ѵ���������¼
            _processedNotes.Clear();
            
            // ����ǰλ�õ�����
            ProcessNotesAtPositionSimple(position);
            _state.AddToPath(position);
        }

        private void UpdatePencilModeEditing(Point position)
        {
            if (_pianoRollViewModel == null) return;

            _state.AddToPath(position);

            // Ǧ��ģʽ������ǰλ�õ�����
            ProcessNotesAtPositionSimple(position);
        }

        /// <summary>
        /// �򻯰��������� - ֱ�ӻ���ʱ��λ��
        /// </summary>
        private void ProcessNotesAtPositionSimple(Point position)
        {
            if (_pianoRollViewModel == null) return;

            // ���㵱ǰλ�ö�Ӧ�ľ�������ֵ
            var velocity = CalculateVelocityFromY(position.Y);
            var timeInTicks = _pianoRollViewModel.GetTimeFromX(position.X);
            
            System.Diagnostics.Debug.WriteLine($"Processing position {position}, velocity: {velocity}, time: {timeInTicks}");

            // �����ڵ�ǰʱ��λ�ø�������������
            foreach (var note in _pianoRollViewModel.Notes)
            {
                var noteStartTime = note.StartPosition.ToTicks(_pianoRollViewModel.TicksPerBeat);
                var noteEndTime = noteStartTime + note.Duration.ToTicks(_pianoRollViewModel.TicksPerBeat);
                
                // ���ʱ���Ƿ���������Χ��
                if (timeInTicks >= noteStartTime && timeInTicks <= noteEndTime)
                {
                    // ����Ƿ���������ͷ������ǰ25%��ʱ�䷶Χ�ڣ�
                    var noteDuration = noteEndTime - noteStartTime;
                    var startThreshold = noteDuration * 0.25; // ������ͷ25%��ʱ�䷶Χ
                    
                    if (timeInTicks <= noteStartTime + startThreshold)
                    {
                        // ����Ƿ��Ѿ�������������
                        if (!_processedNotes.Contains(note))
                        {
                            System.Diagnostics.Debug.WriteLine($"Updating note velocity from {note.Velocity} to {velocity}");
                            
                            // ����ԭʼ���ȣ������û����Ļ���
                            if (_state.EditingNotes?.Contains(note) != true)
                            {
                                _state.AddEditingNote(note);
                                _state.SaveOriginalVelocity(note);
                            }
                            
                            // ֱ����������ֵ
                            note.Velocity = velocity;
                            _processedNotes.Add(note);
                        }
                    }
                }
            }
        }

        #endregion

        #region ��������

        private NoteViewModel? FindNoteAtPosition(Point position)
        {
            if (_pianoRollViewModel == null) return null;

            return _pianoRollViewModel.GetNoteAtPosition(position);
        }

        private int CalculateVelocityChange(double deltaY)
        {
            // �����ر仯ת��Ϊ���ȱ仯
            // ����100���ض�Ӧ127������ֵ
            return (int)Math.Round(-deltaY * 127.0 / 100.0);
        }

        private int CalculateVelocityFromY(double y)
        {
            // ʹ��VelocityBarRenderer�Ĺ�����������������ֵ
            // ʹ��ʵ�ʵĻ����߶�
            return DominoNext.Renderers.VelocityBarRenderer.CalculateVelocityFromY(y, _canvasHeight);
        }

        private void ApplyVelocityChanges()
        {
            // �����������ӳ���/����֧��
            // ��ǰʵ��ֱ��Ӧ�ø���
        }

        private void RestoreOriginalVelocities()
        {
            if (_state.EditingNotes == null) return;

            foreach (var note in _state.EditingNotes)
            {
                if (_state.OriginalVelocities.TryGetValue(note, out var originalVelocity))
                {
                    note.Velocity = originalVelocity;
                }
            }
        }

        #endregion
    }
}