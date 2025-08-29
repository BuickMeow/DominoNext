using System;
using System.IO;
using System.Threading.Tasks;
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
            Loaded += SettingsWindow_Loaded;
        }

        private void SettingsWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            // 窗口加载时自动加载设置
            if (DataContext is SettingsWindowViewModel viewModel)
            {
                try
                {
                    // 从配置文件加载设置
                    viewModel.Settings.LoadFromFile();

                    // 应用加载的设置
                    ApplyLoadedSettings(viewModel);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
                }
            }
        }

        private void ApplyLoadedSettings(SettingsWindowViewModel viewModel)
        {
            // 应用语言设置
            viewModel.ApplyLanguageCommand.Execute(viewModel.Settings.Language);

            // 应用主题设置
            var themeKey = viewModel.Settings.Theme.Key switch
            {
                "Default" => "Default",
                "Light" => "Light",
                "Dark" => "Dark",
                _ => "Default"
            };
            viewModel.ApplyThemeCommand.Execute(themeKey);
        }

        private SaveChangesResult ShowSaveChangesDialog()
        {
            // 简单的保存更改对话框实现
            // 实际项目中可以使用更完善的对话框服务
            return SaveChangesResult.Save; // 默认保存
        }

        private async void SaveSettingsToFile(SettingsWindowViewModel viewModel)
        {
            try
            {
                // 保存设置到配置文件
                viewModel.Settings.SaveToFile();
                viewModel.HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置到文件失败: {ex.Message}");
                // 可以显示错误消息给用户
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            // 窗口关闭时检查是否有未保存的更改
            if (DataContext is SettingsWindowViewModel viewModel && viewModel.HasUnsavedChanges)
            {
                var result = ShowSaveChangesDialog();
                switch (result)
                {
                    case SaveChangesResult.Save:
                        SaveSettingsToFile(viewModel);
                        break;
                    case SaveChangesResult.DontSave:
                        break;
                    case SaveChangesResult.Cancel:
                        e.Cancel = true; // 取消关闭
                        return;
                }
            }

            base.OnClosing(e);
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

                            // 应用加载的设置
                            ApplyLoadedSettings(viewModel);

                            // 标记有更改
                            viewModel.HasUnsavedChanges = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"从文件加载设置失败: {ex.Message}");
                    // 可以显示错误消息给用户
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
                    // 可以显示错误消息给用户
                }
            }
        }
    }

    /// <summary>
    /// 保存更改对话框结果枚举
    /// </summary>
    public enum SaveChangesResult
    {
        Save,
        DontSave,
        Cancel
    }
}