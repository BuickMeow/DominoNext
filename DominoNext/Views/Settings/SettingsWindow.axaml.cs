using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DominoNext.ViewModels.Settings;

namespace DominoNext.Views.Settings
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        // 添加从文件加载设置的按钮点击事件
        private async void LoadSettingsFromFile_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsWindowViewModel viewModel)
            {
                try
                {
                    // 弹出文件选择对话框
                    var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "选择配置文件",
                        FileTypeFilter = new[] { new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } } },
                        AllowMultiple = false
                    });

                    if (files.Count > 0)
                    {
                        var file = files[0];
                        var filePath = file.TryGetLocalPath();

                        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            // 使用SettingsModel的自定义路径加载方法
                            viewModel.Settings.LoadFromFile(filePath);

                            // 重新检测当前主题
                            viewModel.UpdateCurrentSelections();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"从文件加载设置失败: {ex.Message}");
                }
            }
        }

        // 添加保存设置到文件的按钮点击事件
        private async void SaveSettingsToFile_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsWindowViewModel viewModel)
            {
                try
                {
                    // 弹出文件保存对话框
                    var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                    {
                        Title = "保存配置文件",
                        FileTypeChoices = new[] { new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } } },
                        DefaultExtension = "json",
                        SuggestedFileName = "settings.json"
                    });

                    if (file != null)
                    {
                        var filePath = file.TryGetLocalPath();

                        if (!string.IsNullOrEmpty(filePath))
                        {
                            // 使用SettingsModel的自定义路径保存方法
                            viewModel.Settings.SaveToFile(filePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"保存设置到文件失败: {ex.Message}");
                }
            }
        }
    }
}