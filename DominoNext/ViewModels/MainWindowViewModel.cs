using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using DominoNext.Services.Interfaces;
using DominoNext.ViewModels.Editor;
using DominoNext.ViewModels.Settings;
using DominoNext.Views.Settings;

namespace DominoNext.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;

        public string Greeting { get; } = "Welcome to Avalonia!";

        public PianoRollViewModel PianoRoll { get; } = new();

        public MainWindowViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        // 无参构造函数用于设计时
        public MainWindowViewModel() : this(new DominoNext.Services.Implementation.SettingsService())
        {
        }

        [RelayCommand]
        private async Task OpenSettingsAsync()
        {
            var settingsViewModel = new SettingsWindowViewModel(_settingsService);
            var settingsWindow = new SettingsWindow
            {
                DataContext = settingsViewModel
            };

            // 安全地获取主窗口
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow != null)
            {
                await settingsWindow.ShowDialog(desktop.MainWindow);
            }
            else
            {
                // 如果没有主窗口，就作为独立窗口显示
                settingsWindow.Show();
            }
        }

        [RelayCommand]
        private void NewFile()
        {
            // TODO: 实现新建文件功能
        }

        [RelayCommand]
        private void OpenFile()
        {
            // TODO: 实现打开文件功能
        }

        [RelayCommand]
        private void SaveFile()
        {
            // TODO: 实现保存文件功能
        }

        [RelayCommand]
        private void ExitApplication()
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}