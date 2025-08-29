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
            // ���ڼ���ʱ�Զ���������
            if (DataContext is SettingsWindowViewModel viewModel)
            {
                try
                {
                    // �������ļ���������
                    viewModel.Settings.LoadFromFile();

                    // Ӧ�ü��ص�����
                    ApplyLoadedSettings(viewModel);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"��������ʧ��: {ex.Message}");
                }
            }
        }

        private void ApplyLoadedSettings(SettingsWindowViewModel viewModel)
        {
            // Ӧ����������
            viewModel.ApplyLanguageCommand.Execute(viewModel.Settings.Language);

            // Ӧ����������
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
            // �򵥵ı�����ĶԻ���ʵ��
            // ʵ����Ŀ�п���ʹ�ø����ƵĶԻ������
            return SaveChangesResult.Save; // Ĭ�ϱ���
        }

        private async void SaveSettingsToFile(SettingsWindowViewModel viewModel)
        {
            try
            {
                // �������õ������ļ�
                viewModel.Settings.SaveToFile();
                viewModel.HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"�������õ��ļ�ʧ��: {ex.Message}");
                // ������ʾ������Ϣ���û�
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            // ���ڹر�ʱ����Ƿ���δ����ĸ���
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
                        e.Cancel = true; // ȡ���ر�
                        return;
                }
            }

            base.OnClosing(e);
        }

        // ��Ӵ��ļ��������õİ�ť����¼�
        private async void LoadSettingsFromFile_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsWindowViewModel viewModel)
            {
                try
                {
                    // �����ļ�ѡ��Ի���
                    var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "ѡ�������ļ�",
                        FileTypeFilter = new[] { new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } } },
                        AllowMultiple = false
                    });

                    if (files.Count > 0)
                    {
                        var file = files[0];
                        var filePath = file.TryGetLocalPath();

                        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            // ʹ��SettingsModel���Զ���·�����ط���
                            viewModel.Settings.LoadFromFile(filePath);

                            // Ӧ�ü��ص�����
                            ApplyLoadedSettings(viewModel);

                            // ����и���
                            viewModel.HasUnsavedChanges = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"���ļ���������ʧ��: {ex.Message}");
                    // ������ʾ������Ϣ���û�
                }
            }
        }

        // ��ӱ������õ��ļ��İ�ť����¼�
        private async void SaveSettingsToFile_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsWindowViewModel viewModel)
            {
                try
                {
                    // �����ļ�����Ի���
                    var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                    {
                        Title = "���������ļ�",
                        FileTypeChoices = new[] { new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } } },
                        DefaultExtension = "json",
                        SuggestedFileName = "settings.json"
                    });

                    if (file != null)
                    {
                        var filePath = file.TryGetLocalPath();

                        if (!string.IsNullOrEmpty(filePath))
                        {
                            // ʹ��SettingsModel���Զ���·�����淽��
                            viewModel.Settings.SaveToFile(filePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"�������õ��ļ�ʧ��: {ex.Message}");
                    // ������ʾ������Ϣ���û�
                }
            }
        }
    }

    /// <summary>
    /// ������ĶԻ�����ö��
    /// </summary>
    public enum SaveChangesResult
    {
        Save,
        DontSave,
        Cancel
    }
}