using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DominoNext.ViewModels.Editor
{
    public partial class TrackViewModel : ObservableObject
    {
        public string Name { get; set; } = "轨道";
        public ObservableCollection<NoteViewModel> Notes { get; } = new();
        
        // 洋葱皮功能相关属性
        [ObservableProperty]
        private bool _isOnionSkinEnabled = true; // 默认启用洋葱皮功能
        
        [ObservableProperty]
        private int _onionSkinPreviousFrames = 1;
        
        [ObservableProperty]
        private int _onionSkinNextFrames = 1;
        
        [ObservableProperty]
        private double _onionSkinOpacity = 0.3;
        
        // 添加用于标识轨道是否被选中用于洋葱皮显示
        [ObservableProperty]
        private bool _isOnionSkinSelected = true; // 默认选中所有轨道
    }
}