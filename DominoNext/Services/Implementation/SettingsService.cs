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
    /// ���÷���ʵ��
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private const string SettingsFileName = "settings.json";
        private readonly string _settingsFilePath;

        public SettingsModel Settings { get; private set; }
        public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;

        public SettingsService()
        {
            // �����ļ��������û�����Ŀ¼
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

                // �����ɫ������Ա��������Ӧ����������
                if (IsColorProperty(e.PropertyName))
                {
                    ApplyThemeSettings();
                }

                // �Զ���������
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
                    // �״����У�ʹ��Ĭ������
                    await SaveSettingsAsync();
                    return;
                }

                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var loadedSettings = JsonSerializer.Deserialize<SettingsModel>(json);
                
                if (loadedSettings != null)
                {
                    // ��������ֵ
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

                    // ����������ɫ
                    Settings.BackgroundColor = loadedSettings.BackgroundColor;
                    Settings.NoteColor = loadedSettings.NoteColor;
                    Settings.GridLineColor = loadedSettings.GridLineColor;
                    Settings.KeyWhiteColor = loadedSettings.KeyWhiteColor;
                    Settings.KeyBlackColor = loadedSettings.KeyBlackColor;
                    Settings.SelectionColor = loadedSettings.SelectionColor;

                    // ��չ����Ԫ����ɫ
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

                // Ӧ������
                ApplyLanguageSettings();
                ApplyThemeSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"��������ʧ��: {ex.Message}");
                // �������ʧ�ܣ�ʹ��Ĭ������
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
                System.Diagnostics.Debug.WriteLine($"��������ʧ��: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"Ӧ����������ʧ��: {ex.Message}");
            }
        }

        public void ApplyThemeSettings()
        {
            try
            {
                if (Application.Current != null)
                {
                    // ��UI�߳���ִ���������
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Application.Current.RequestedThemeVariant = Settings.Theme;

                        // �������е�������ɫע�뵽 Application.Current.Resources�����ؼ�ʹ��
                        var resources = Application.Current.Resources;
                        
                        // ������ɫ
                        SetBrushResource(resources, "AppBackgroundBrush", Settings.BackgroundColor, "#FFFAFAFA");
                        SetBrushResource(resources, "NoteBrush", Settings.NoteColor, "#FF4CAF50");
                        SetBrushResource(resources, "GridLineBrush", Settings.GridLineColor, "#1F000000");
                        SetBrushResource(resources, "KeyWhiteBrush", Settings.KeyWhiteColor, "#FFFFFFFF");
                        SetBrushResource(resources, "KeyBlackBrush", Settings.KeyBlackColor, "#FF1F1F1F");
                        SetBrushResource(resources, "SelectionBrush", Settings.SelectionColor, "#800099FF");

                        // ����״̬��ɫ
                        SetBrushResource(resources, "NoteSelectedBrush", Settings.NoteSelectedColor, "#FFFF9800");
                        SetBrushResource(resources, "NoteDraggingBrush", Settings.NoteDraggingColor, "#FF2196F3");
                        SetBrushResource(resources, "NotePreviewBrush", Settings.NotePreviewColor, "#804CAF50");

                        // UIԪ����ɫ
                        SetBrushResource(resources, "VelocityIndicatorBrush", Settings.VelocityIndicatorColor, "#FFFFC107");
                        SetBrushResource(resources, "MeasureHeaderBackgroundBrush", Settings.MeasureHeaderBackgroundColor, "#FFF5F5F5");
                        SetBrushResource(resources, "MeasureLineBrush", Settings.MeasureLineColor, "#FF000080");
                        SetBrushResource(resources, "MeasureTextBrush", Settings.MeasureTextColor, "#FF000000");
                        SetBrushResource(resources, "SeparatorLineBrush", Settings.SeparatorLineColor, "#FFCCCCCC");
                        SetBrushResource(resources, "KeyBorderBrush", Settings.KeyBorderColor, "#FF1F1F1F");
                        SetBrushResource(resources, "KeyTextWhiteBrush", Settings.KeyTextWhiteColor, "#FF000000");
                        SetBrushResource(resources, "KeyTextBlackBrush", Settings.KeyTextBlackColor, "#FFFFFFFF");

                        // Ϊ������Ⱦ���ṩ�߿��ˢ����������ɫ���ɸ���ı߿�ɫ��
                        SetPenBrushResource(resources, "NotePenBrush", Settings.NoteColor, "#FF2E7D32");
                        SetPenBrushResource(resources, "NoteSelectedPenBrush", Settings.NoteSelectedColor, "#FFF57C00");
                        SetPenBrushResource(resources, "NoteDraggingPenBrush", Settings.NoteDraggingColor, "#FF1976D2");
                        SetPenBrushResource(resources, "NotePreviewPenBrush", Settings.NotePreviewColor, "#FF2E7D32");

                        // ������UI����Ԫ����ɫ��Դ
                        // ���������
                        SetBrushResource(resources, "ToolbarBackgroundBrush", Settings.MeasureHeaderBackgroundColor, "#FFF0F0F0");
                        SetBrushResource(resources, "ToolbarBorderBrush", Settings.SeparatorLineColor, "#FFD0D0D0");
                        SetBrushResource(resources, "ButtonBorderBrush", Settings.SeparatorLineColor, "#FFD0D0D0");
                        SetBrushResource(resources, "ButtonHoverBrush", Settings.SelectionColor, "#FFE8F4FD");
                        SetBrushResource(resources, "ButtonPressedBrush", Settings.SelectionColor, "#FFD0E8FF");
                        SetBrushResource(resources, "ButtonActiveBrush", Settings.NoteSelectedColor, "#FF3d80df");
                        
                        // ����͹�������ɫ
                        SetBrushResource(resources, "SliderTrackBrush", Settings.SeparatorLineColor, "#FFE0E0E0");
                        SetBrushResource(resources, "SliderThumbBrush", Settings.NoteSelectedColor, "#FF3d80df");
                        SetBrushResource(resources, "SliderThumbHoverBrush", Settings.NoteDraggingColor, "#FF5a9cff");
                        SetBrushResource(resources, "SliderThumbPressedBrush", Settings.NoteColor, "#FF2d6bbf");
                        
                        // �����򱳾�
                        SetBrushResource(resources, "PianoKeysBackgroundBrush", Settings.KeyBlackColor, "#FF2F2F2F");
                        SetBrushResource(resources, "MainCanvasBackgroundBrush", Settings.BackgroundColor, "#FFFFFFFF");
                        SetBrushResource(resources, "PopupBackgroundBrush", Settings.BackgroundColor, "#FFFFFFFF");
                        
                        // ������ɫ
                        SetBrushResource(resources, "StatusTextBrush", Settings.MeasureTextColor, "#FF666666");
                        SetBrushResource(resources, "BorderLineBlackBrush", Settings.KeyBlackColor, "#FF000000");

                        // ǿ��ˢ������UIԪ��
                        ForceRefreshAllControls();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ӧ����������ʧ��: {ex.Message}");
            }
        }

        /// <summary>
        /// ǿ��ˢ������UI�ؼ�
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
                System.Diagnostics.Debug.WriteLine($"ǿ��ˢ��UI�ؼ�ʧ��: {ex.Message}");
            }
        }

        /// <summary>
        /// �ݹ�ˢ�¿ؼ������ӿؼ�
        /// </summary>
        private void RefreshControlAndChildren(Control control)
        {
            try
            {
                // ǿ��������Ⱦ
                control.InvalidateVisual();
                
                // ǿ�����²���������
                control.InvalidateMeasure();
                control.InvalidateArrange();

                // �ݹ鴦���ӿؼ�
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
                    // ���� ItemsControl��ǿ������������Ŀ
                    itemsControl.InvalidateVisual();
                    
                    // ��������Ҳ�ݹ�ˢ��
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
                System.Diagnostics.Debug.WriteLine($"ˢ�¿ؼ�ʧ��: {ex.Message}");
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
                // ���Խ�������ʹ�û�����ɫ
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
                
                // ���ɸ���ı߿���ɫ
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
                // ���Խ�������ʹ�û�����ɫ
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