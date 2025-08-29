using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DominoNext.ViewModels.Editor.State
{
    /// <summary>
    /// 力度编辑状态管理
    /// </summary>
    public partial class VelocityEditingState : ObservableObject
    {
        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private Point _startPosition;

        [ObservableProperty]
        private Point? _currentPosition;

        [ObservableProperty]
        private List<NoteViewModel>? _editingNotes;

        [ObservableProperty]
        private List<Point> _editingPath = new();

        // 保存原始力度值用于撤销
        public Dictionary<NoteViewModel, int> OriginalVelocities { get; private set; } = new();

        /// <summary>
        /// 开始编辑
        /// </summary>
        public void StartEditing(Point position)
        {
            IsEditing = true;
            StartPosition = position;
            CurrentPosition = position;
            EditingPath.Clear();
            EditingPath.Add(position);
            OriginalVelocities.Clear();
        }

        /// <summary>
        /// 更新当前位置
        /// </summary>
        public void UpdatePosition(Point position)
        {
            CurrentPosition = position;
        }

        /// <summary>
        /// 添加到编辑路径
        /// </summary>
        public void AddToPath(Point position)
        {
            EditingPath.Add(position);
            CurrentPosition = position;
        }

        /// <summary>
        /// 设置编辑中的音符
        /// </summary>
        public void SetEditingNotes(List<NoteViewModel> notes)
        {
            EditingNotes = notes.ToList();
        }

        /// <summary>
        /// 添加单个编辑音符
        /// </summary>
        public void AddEditingNote(NoteViewModel note)
        {
            EditingNotes ??= new List<NoteViewModel>();
            if (!EditingNotes.Contains(note))
            {
                EditingNotes.Add(note);
            }
        }

        /// <summary>
        /// 保存原始力度值
        /// </summary>
        public void SaveOriginalVelocities(IEnumerable<NoteViewModel> notes)
        {
            foreach (var note in notes)
            {
                SaveOriginalVelocity(note);
            }
        }

        /// <summary>
        /// 保存单个音符的原始力度值
        /// </summary>
        public void SaveOriginalVelocity(NoteViewModel note)
        {
            if (!OriginalVelocities.ContainsKey(note))
            {
                OriginalVelocities[note] = note.Velocity;
            }
        }

        /// <summary>
        /// 结束编辑
        /// </summary>
        public void EndEditing()
        {
            IsEditing = false;
            CurrentPosition = null;
            EditingNotes?.Clear();
            EditingPath.Clear();
            OriginalVelocities.Clear();
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            EndEditing();
        }
    }
}