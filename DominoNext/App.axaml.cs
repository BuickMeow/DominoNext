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
            System.Diagnostics.Debug.WriteLine("App.Initialize() 完成");
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            System.Diagnostics.Debug.WriteLine("OnFrameworkInitializationCompleted 开始");
            
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                System.Diagnostics.Debug.WriteLine("检测到桌面应用程序生命周期");
                
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();
                System.Diagnostics.Debug.WriteLine("数据验证插件已禁用");

                try
                {
                    // 初始化设置服务
                    _settingsService = new SettingsService();
                    System.Diagnostics.Debug.WriteLine("设置服务已创建");
                    
                    await _settingsService.LoadSettingsAsync();
                    System.Diagnostics.Debug.WriteLine("设置已加载");

                    var viewModel = new MainWindowViewModel(_settingsService);
                    System.Diagnostics.Debug.WriteLine("MainWindowViewModel 已创建");

                    var mainWindow = new MainWindow
                    {
                        DataContext = viewModel,
                    };
                    System.Diagnostics.Debug.WriteLine("MainWindow 已创建");

                    desktop.MainWindow = mainWindow;
                    System.Diagnostics.Debug.WriteLine("MainWindow 已设置为应用程序主窗口");

                    // 显式显示窗口
                    mainWindow.Show();
                    System.Diagnostics.Debug.WriteLine("MainWindow.Show() 已调用");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"创建主窗口时发生错误: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("未检测到桌面应用程序生命周期");
            }

            base.OnFrameworkInitializationCompleted();
            System.Diagnostics.Debug.WriteLine("OnFrameworkInitializationCompleted 完成");
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}