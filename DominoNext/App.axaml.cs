using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DominoNext.Services.Implementation;
using DominoNext.Services.Interfaces;
using DominoNext.ViewModels;
using DominoNext.Views;

namespace DominoNext
{
    public partial class App : Application
    {
        private ISettingsService? _settingsService;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            System.Diagnostics.Debug.WriteLine("App.Initialize() ");
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            System.Diagnostics.Debug.WriteLine("OnFrameworkInitializationCompleted ʼ");
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                System.Diagnostics.Debug.WriteLine("⵽Ӧó");
                
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                System.Diagnostics.Debug.WriteLine("֤ѽ");

                try
                {
                    // ʼ÷
                    _settingsService = new SettingsService();
                    System.Diagnostics.Debug.WriteLine("÷Ѵ");
                    
                    await _settingsService.LoadSettingsAsync();
                    System.Diagnostics.Debug.WriteLine("Ѽ");

                    var viewModel = new MainWindowViewModel(_settingsService, new ProjectStorageService(), new PlaybackService());
                    System.Diagnostics.Debug.WriteLine("MainWindowViewModel Ѵ");

                    var mainWindow = new MainWindow
                    {
                        DataContext = viewModel,
                    };
                    System.Diagnostics.Debug.WriteLine("MainWindow Ѵ");

                    desktop.MainWindow = mainWindow;
                    System.Diagnostics.Debug.WriteLine("MainWindow ΪӦó");

                    // ʽʾ
                    mainWindow.Show();
                    System.Diagnostics.Debug.WriteLine("MainWindow.Show() ѵ");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ʱ: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"ջ: {ex.StackTrace}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("δ⵽Ӧó");
            }

            base.OnFrameworkInitializationCompleted();
            System.Diagnostics.Debug.WriteLine("OnFrameworkInitializationCompleted ");
        }
        
        private void DisableAvaloniaDataAnnotationValidation()
        {
            // 避免 Avalonia 和 CommunityToolkit 的重复验证
            // 更多信息：https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                foreach (var plugin in BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToList())
                {
                    BindingPlugins.DataValidators.Remove(plugin);
                }
            }
        }

    }
}