using DominoNext.Models.Settings;
using DominoNext.Services.Interfaces;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Threading;

namespace DominoNext.Services.Implementation
{
    /// <summary>
    /// 设置服务实现
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private const string SettingsFileName = "settings.json";
        private readonly string _settingsFilePath;

        public SettingsModel Settings { get; private set; }
        public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

        public SettingsService()
        {
            // 设置文件保存在用户数据目录
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "DominoNext");
            Directory.CreateDirectory(appFolder);
            _settingsFilePath = Path.Combine(appFolder, SettingsFileName);

            Settings = new SettingsModel();
            Settings.PropertyChanged += OnSettingsPropertyChanged;
        }

        private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != null)
            {
                SettingsChanged?.Invoke(this, new SettingsChangedEventArgs
                {
                    PropertyName = e.PropertyName
                });

                // 如果颜色相关属性变更，立即应用主题设置
                if (IsColorProperty(e.PropertyName))
                {
                    ApplyThemeSettings();
                }

                // 自动保存设置
                _ = Task.Run(SaveSettingsAsync);
            }
        }

        private bool IsColorProperty(string propertyName)
        {
            return propertyName.EndsWith("Color") || propertyName == nameof(Settings.Theme);
        }

        public async Task LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    // 首次运行，使用默认设置
                    await SaveSettingsAsync();
                    return;
                }

                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var loadedSettings = JsonSerializer.Deserialize<SettingsModel>(json);
                
                if (loadedSettings != null)
                {
                    // 复制属性值
                    Settings.Language = loadedSettings.Language;
                    Settings.Theme = loadedSettings.Theme;
                    Settings.AutoSave = loadedSettings.AutoSave;
                    Settings.AutoSaveInterval = loadedSettings.AutoSaveInterval;
                    Settings.ShowGridLines = loadedSettings.ShowGridLines;
                    Settings.SnapToGrid = loadedSettings.SnapToGrid;
                    Settings.DefaultZoom = loadedSettings.DefaultZoom;
                    Settings.UseNativeMenuBar = loadedSettings.UseNativeMenuBar;
                    Settings.MaxUndoSteps = loadedSettings.MaxUndoSteps;
                    Settings.ConfirmBeforeDelete = loadedSettings.ConfirmBeforeDelete;
                    Settings.ShowVelocityBars = loadedSettings.ShowVelocityBars;
                    Settings.PianoKeyWidth = loadedSettings.PianoKeyWidth;
                    Settings.EnableKeyboardShortcuts = loadedSettings.EnableKeyboardShortcuts;
                    Settings.CustomShortcutsJson = loadedSettings.CustomShortcutsJson;

                    // 基础主题颜色
                    Settings.BackgroundColor = loadedSettings.BackgroundColor;
                    Settings.NoteColor = loadedSettings.NoteColor;
                    Settings.GridLineColor = loadedSettings.GridLineColor;
                    Settings.KeyWhiteColor = loadedSettings.KeyWhiteColor;
                    Settings.KeyBlackColor = loadedSettings.KeyBlackColor;
                    Settings.SelectionColor = loadedSettings.SelectionColor;

                    // 扩展界面元素颜色
                    Settings.NoteSelectedColor = loadedSettings.NoteSelectedColor;
                    Settings.NoteDraggingColor = loadedSettings.NoteDraggingColor;
                    Settings.NotePreviewColor = loadedSettings.NotePreviewColor;
                    Settings.VelocityIndicatorColor = loadedSettings.VelocityIndicatorColor;
                    Settings.MeasureHeaderBackgroundColor = loadedSettings.MeasureHeaderBackgroundColor;
                    Settings.MeasureLineColor = loadedSettings.MeasureLineColor;
                    Settings.MeasureTextColor = loadedSettings.MeasureTextColor;
                    Settings.SeparatorLineColor = loadedSettings.SeparatorLineColor;
                    Settings.KeyBorderColor = loadedSettings.KeyBorderColor;
                    Settings.KeyTextWhiteColor = loadedSettings.KeyTextWhiteColor;
                    Settings.KeyTextBlackColor = loadedSettings.KeyTextBlackColor;
                }

                // 应用设置
                ApplyLanguageSettings();
                ApplyThemeSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
                // 如果加载失败，使用默认设置
                Settings.ResetToDefaults();
            }
        }

        public async Task SaveSettingsAsync()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(Settings, options);
                await File.WriteAllTextAsync(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
            }
        }

        public async Task ResetToDefaultsAsync()
        {
            Settings.ResetToDefaults();
            await SaveSettingsAsync();
            
            ApplyLanguageSettings();
            ApplyThemeSettings();
        }

        public void ApplyLanguageSettings()
        {
            try
            {
                var culture = new CultureInfo(Settings.Language);
                CultureInfo.CurrentCulture = culture;
                CultureInfo.CurrentUICulture = culture;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"应用语言设置失败: {ex.Message}");
            }
        }

        public void ApplyThemeSettings()
        {
            try
            {
                if (Application.Current != null)
                {
                    // 在UI线程上执行主题更新
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Application.Current.RequestedThemeVariant = Settings.Theme;

                        // 将设置中的所有颜色注入到 Application.Current.Resources，供控件使用
                        var resources = Application.Current.Resources;
                        
                        // 基础颜色
                        SetBrushResource(resources, "AppBackgroundBrush", Settings.BackgroundColor, "#FFFAFAFA");
                        SetBrushResource(resources, "NoteBrush", Settings.NoteColor, "#FF4CAF50");
                        SetBrushResource(resources, "GridLineBrush", Settings.GridLineColor, "#1F000000");
                        SetBrushResource(resources, "KeyWhiteBrush", Settings.KeyWhiteColor, "#FFFFFFFF");
                        SetBrushResource(resources, "KeyBlackBrush", Settings.KeyBlackColor, "#FF1F1F1F");
                        SetBrushResource(resources, "SelectionBrush", Settings.SelectionColor, "#800099FF");

                        // 音符状态颜色
                        SetBrushResource(resources, "NoteSelectedBrush", Settings.NoteSelectedColor, "#FFFF9800");
                        SetBrushResource(resources, "NoteDraggingBrush", Settings.NoteDraggingColor, "#FF2196F3");
                        SetBrushResource(resources, "NotePreviewBrush", Settings.NotePreviewColor, "#804CAF50");

                        // UI元素颜色
                        SetBrushResource(resources, "VelocityIndicatorBrush", Settings.VelocityIndicatorColor, "#FFFFC107");
                        SetBrushResource(resources, "MeasureHeaderBackgroundBrush", Settings.MeasureHeaderBackgroundColor, "#FFF5F5F5");
                        SetBrushResource(resources, "MeasureLineBrush", Settings.MeasureLineColor, "#FF000080");
                        SetBrushResource(resources, "MeasureTextBrush", Settings.MeasureTextColor, "#FF000000");
                        SetBrushResource(resources, "SeparatorLineBrush", Settings.SeparatorLineColor, "#FFCCCCCC");
                        SetBrushResource(resources, "KeyBorderBrush", Settings.KeyBorderColor, "#FF1F1F1F");
                        SetBrushResource(resources, "KeyTextWhiteBrush", Settings.KeyTextWhiteColor, "#FF000000");
                        SetBrushResource(resources, "KeyTextBlackBrush", Settings.KeyTextBlackColor, "#FFFFFFFF");

                        // 为音符渲染器提供边框笔刷（基于主颜色生成更深的边框色）
                        SetPenBrushResource(resources, "NotePenBrush", Settings.NoteColor, "#FF2E7D32");
                        SetPenBrushResource(resources, "NoteSelectedPenBrush", Settings.NoteSelectedColor, "#FFF57C00");
                        SetPenBrushResource(resources, "NoteDraggingPenBrush", Settings.NoteDraggingColor, "#FF1976D2");
                        SetPenBrushResource(resources, "NotePreviewPenBrush", Settings.NotePreviewColor, "#FF2E7D32");

                        // 新增：UI界面元素颜色资源
                        // 工具栏相关
                        SetBrushResource(resources, "ToolbarBackgroundBrush", Settings.MeasureHeaderBackgroundColor, "#FFF0F0F0");
                        SetBrushResource(resources, "ToolbarBorderBrush", Settings.SeparatorLineColor, "#FFD0D0D0");
                        SetBrushResource(resources, "ButtonBorderBrush", Settings.SeparatorLineColor, "#FFD0D0D0");
                        SetBrushResource(resources, "ButtonHoverBrush", Settings.SelectionColor, "#FFE8F4FD");
                        SetBrushResource(resources, "ButtonPressedBrush", Settings.SelectionColor, "#FFD0E8FF");
                        SetBrushResource(resources, "ButtonActiveBrush", Settings.NoteSelectedColor, "#FF3d80df");
                        
                        // 滑块和滚动条颜色
                        SetBrushResource(resources, "SliderTrackBrush", Settings.SeparatorLineColor, "#FFE0E0E0");
                        SetBrushResource(resources, "SliderThumbBrush", Settings.NoteSelectedColor, "#FF3d80df");
                        SetBrushResource(resources, "SliderThumbHoverBrush", Settings.NoteDraggingColor, "#FF5a9cff");
                        SetBrushResource(resources, "SliderThumbPressedBrush", Settings.NoteColor, "#FF2d6bbf");
                        
                        // 主区域背景
                        SetBrushResource(resources, "PianoKeysBackgroundBrush", Settings.KeyBlackColor, "#FF2F2F2F");
                        SetBrushResource(resources, "MainCanvasBackgroundBrush", Settings.BackgroundColor, "#FFFFFFFF");
                        SetBrushResource(resources, "PopupBackgroundBrush", Settings.BackgroundColor, "#FFFFFFFF");
                        
                        // 文字颜色
                        SetBrushResource(resources, "StatusTextBrush", Settings.MeasureTextColor, "#FF666666");
                        SetBrushResource(resources, "BorderLineBlackBrush", Settings.KeyBlackColor, "#FF000000");

                        // 强制刷新所有UI元素
                        ForceRefreshAllControls();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"应用主题设置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 强制刷新所有UI控件
        /// </summary>
        private void ForceRefreshAllControls()
        {
            try
            {
                if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    foreach (var window in desktop.Windows)
                    {
                        RefreshControlAndChildren(window);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"强制刷新UI控件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 递归刷新控件及其子控件
        /// </summary>
        private void RefreshControlAndChildren(Control control)
        {
            try
            {
                // 强制重新渲染
                control.InvalidateVisual();
                
                // 强制重新测量和排列
                control.InvalidateMeasure();
                control.InvalidateArrange();

                // 递归处理子控件
                if (control is Panel panel)
                {
                    foreach (Control child in panel.Children)
                    {
                        RefreshControlAndChildren(child);
                    }
                }
                else if (control is ContentControl contentControl && contentControl.Content is Control childControl)
                {
                    RefreshControlAndChildren(childControl);
                }
                else if (control is ItemsControl itemsControl)
                {
                    // 对于 ItemsControl，强制重新生成项目
                    itemsControl.InvalidateVisual();
                    
                    // 如果有子项，也递归刷新
                    foreach (var item in itemsControl.GetRealizedContainers())
                    {
                        if (item is Control itemControl)
                        {
                            RefreshControlAndChildren(itemControl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"刷新控件失败: {ex.Message}");
            }
        }

        private void SetBrushResource(IResourceDictionary resources, string key, string colorHex, string fallbackHex)
        {
            try
            {
                var hex = string.IsNullOrEmpty(colorHex) ? fallbackHex : colorHex;
                var color = Avalonia.Media.Color.Parse(hex);
                var brush = new SolidColorBrush(color);

                if (resources.ContainsKey(key))
                {
                    resources[key] = brush;
                }
                else
                {
                    resources.Add(key, brush);
                }
            }
            catch
            {
                // 忽略解析错误，使用回退颜色
                try
                {
                    var color = Avalonia.Media.Color.Parse(fallbackHex);
                    var brush = new SolidColorBrush(color);
                    if (resources.ContainsKey(key))
                    {
                        resources[key] = brush;
                    }
                    else
                    {
                        resources.Add(key, brush);
                    }
                }
                catch { }
            }
        }

        private void SetPenBrushResource(IResourceDictionary resources, string key, string baseColorHex, string fallbackHex)
        {
            try
            {
                var hex = string.IsNullOrEmpty(baseColorHex) ? fallbackHex : baseColorHex;
                var color = Avalonia.Media.Color.Parse(hex);
                
                // 生成更深的边框颜色
                var darkerColor = Color.FromArgb(
                    color.A,
                    (byte)(color.R * 0.7),
                    (byte)(color.G * 0.7),
                    (byte)(color.B * 0.7)
                );
                
                var brush = new SolidColorBrush(darkerColor);

                if (resources.ContainsKey(key))
                {
                    resources[key] = brush;
                }
                else
                {
                    resources.Add(key, brush);
                }
            }
            catch
            {
                // 忽略解析错误，使用回退颜色
                try
                {
                    var color = Avalonia.Media.Color.Parse(fallbackHex);
                    var brush = new SolidColorBrush(color);
                    if (resources.ContainsKey(key))
                    {
                        resources[key] = brush;
                    }
                    else
                    {
                        resources.Add(key, brush);
                    }
                }
                catch { }
            }
        }
    }
}