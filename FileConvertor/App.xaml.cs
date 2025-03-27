﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms; // For NotifyIcon
using System.Drawing; // For Icon
using FileConvertor.Core;
using FileConvertor.Core.Converters;
using FileConvertor.Core.Helpers;
using FileConvertor.Core.Interfaces;
using FileConvertor.Core.Logging;
using FileConvertor.Core.Services;
using FileConvertor.UI.ViewModels;
using FileConvertor.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FileConvertor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : WpfApplication
    {
        private readonly ServiceProvider _serviceProvider;
        private IHotkeyService? _hotkeyService;
        private IClipboardService? _clipboardService;
        private ISettingsService? _settingsService;
        private NotifyIcon? _notifyIcon;
        private ConversionDialog? _mainWindow;
        private SettingsDialog? _settingsDialog;

        /// <summary>
        /// Initializes a new instance of the App class
        /// </summary>
        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
            
            // Initialize system tray icon
            InitializeNotifyIcon();
        }

        /// <summary>
        /// Configures the services for dependency injection
        /// </summary>
        /// <param name="services">Service collection</param>
        private void ConfigureServices(IServiceCollection services)
        {
            // Register services
            services.AddSingleton<IClipboardService, ClipboardService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            
            // Create and configure the converter factory with lazy loading
            services.AddSingleton<ConverterFactory>(provider =>
            {
                var factory = new ConverterFactory(provider);
                
                // Register converter types instead of instances
                factory.RegisterConverterType(typeof(TextToPdfConverter));
                factory.RegisterConverterType(typeof(PngToJpgConverter));
                factory.RegisterConverterType(typeof(ExcelToCsvConverter));
                factory.RegisterConverterType(typeof(MarkdownToHtmlConverter));
                factory.RegisterConverterType(typeof(WordToPdfConverter));
                factory.RegisterConverterType(typeof(JpgToPngConverter));
                factory.RegisterConverterType(typeof(HtmlToPdfConverter));
                factory.RegisterConverterType(typeof(CsvToExcelConverter));
                factory.RegisterConverterType(typeof(PdfToTextConverter));
                factory.RegisterConverterType(typeof(Mp3ToWavConverter));
                
                Logger.Log(LogLevel.Info, "App", "Registered converter types for lazy loading");
                return factory;
            });
            
            services.AddSingleton<IFileTypeDetector, FileTypeDetector>();

            // Register view models
            services.AddTransient<ConversionViewModel>();
            services.AddTransient<SettingsViewModel>();

            // Register views with lazy loading
            services.AddSingleton<Lazy<ConversionDialog>>(provider => 
                new Lazy<ConversionDialog>(() => {
                    Logger.Log(LogLevel.Debug, "App", "Lazy-loading ConversionDialog");
                    var viewModel = provider.GetRequiredService<ConversionViewModel>();
                    return new ConversionDialog(viewModel);
                }));
                
            services.AddSingleton<Lazy<SettingsDialog>>(provider => 
                new Lazy<SettingsDialog>(() => {
                    Logger.Log(LogLevel.Debug, "App", "Lazy-loading SettingsDialog");
                    var viewModel = provider.GetRequiredService<SettingsViewModel>();
                    return new SettingsDialog(viewModel);
                }));
                
            // Also register the actual dialog types for direct access if needed
            services.AddTransient<ConversionDialog>();
            services.AddTransient<SettingsDialog>();
        }

        /// <summary>
        /// Initializes the system tray icon
        /// </summary>
        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application, // Use a default icon
                Visible = true,
                Text = "File Converter"
            };

            // Create context menu
            var contextMenu = new ContextMenuStrip();
            
            var openItem = new ToolStripMenuItem("Open");
            openItem.Click += (s, e) => ShowMainWindow();
            contextMenu.Items.Add(openItem);
            
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => WpfApplication.Current.Shutdown();
            contextMenu.Items.Add(exitItem);
            
            // Add a separator
            contextMenu.Items.Add(new ToolStripSeparator());
            
            // Add a convert item
            var convertItem = new ToolStripMenuItem("Convert File");
            convertItem.Click += (s, e) => CheckClipboardAndShowWindow();
            contextMenu.Items.Add(convertItem);
            
            // Add a separator
            contextMenu.Items.Add(new ToolStripSeparator());
            
            // Add a settings item
            var settingsItem = new ToolStripMenuItem("Settings");
            settingsItem.Click += (s, e) => ShowSettingsDialog();
            contextMenu.Items.Add(settingsItem);
            
            // Add a view log item
            var viewLogItem = new ToolStripMenuItem("View Log File");
            viewLogItem.Click += (s, e) => OpenLogFile();
            contextMenu.Items.Add(viewLogItem);
            
            _notifyIcon.ContextMenuStrip = contextMenu;
            
            // Double-click to open
            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
        }

        /// <summary>
        /// Shows the main window and checks for clipboard content
        /// </summary>
        private void ShowMainWindow()
        {
            // Check if there's a file in the clipboard
            _clipboardService = _serviceProvider.GetRequiredService<IClipboardService>();
            if (!_clipboardService.ContainsFile())
            {
                // No file in clipboard, show a notification and don't open the window
                _notifyIcon?.ShowBalloonTip(
                    3000, 
                    "File Converter", 
                    "No file detected in clipboard. Copy a file and try again.", 
                    ToolTipIcon.Info);
                return;
            }

            // Get or create the main window
            if (_mainWindow == null)
            {
                _mainWindow = _serviceProvider.GetRequiredService<ConversionDialog>();
                
            // Initialize the hotkey service if not already done
            if (_hotkeyService == null)
            {
                // Show and hide the window to ensure it has a valid handle
                _mainWindow.Show();
                _mainWindow.Hide();
                
                // Get the settings service
                _settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
                var settings = _settingsService.CurrentSettings;
                
                // Create the hotkey service with the current settings
                _hotkeyService = new HotkeyService(new WindowInteropHelper(_mainWindow).Handle);
                _hotkeyService.RegisterHotkey(
                    settings.HotkeyModifiers,
                    settings.HotkeyKey,
                    () => CheckClipboardAndShowWindow());
                _hotkeyService.StartListening();
            }
            }

            // Refresh the ViewModel to check for new clipboard content
            var viewModel = (ConversionViewModel)_mainWindow.DataContext;
            viewModel.RefreshFromClipboard();

            // Show the window
            if (_mainWindow.WindowState == WindowState.Minimized)
            {
                _mainWindow.WindowState = WindowState.Normal;
            }
            _mainWindow.Show();
            _mainWindow.Activate();
        }

        /// <summary>
        /// Checks if there's a file in the clipboard and shows the window if there is
        /// </summary>
        private void CheckClipboardAndShowWindow()
        {
            _clipboardService = _serviceProvider.GetRequiredService<IClipboardService>();
            if (_clipboardService.ContainsFile())
            {
                ShowMainWindow();
            }
            else
            {
                // No file in clipboard, show a notification
                _notifyIcon?.ShowBalloonTip(
                    3000, 
                    "File Converter", 
                    "No file detected in clipboard. Copy a file and try again.", 
                    ToolTipIcon.Info);
            }
        }

        /// <summary>
        /// Handles the application startup event
        /// </summary>
        /// <param name="e">Startup event args</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize logger
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logDirectory = Path.Combine(appDataPath, "FileConvertor", "Logs");
            var logFilePath = Path.Combine(logDirectory, $"FileConvertor_{DateTime.Now:yyyyMMdd}.log");
            Logger.Initialize(logFilePath);
            Logger.Log(LogLevel.Info, "App", "Application starting up");

            // Initialize services but don't show the window
            _mainWindow = _serviceProvider.GetRequiredService<ConversionDialog>();
            _clipboardService = _serviceProvider.GetRequiredService<IClipboardService>();
            
            // Initialize the main window first
            _mainWindow.Show();
            _mainWindow.Hide();
            
            // Get the settings service
            _settingsService = _serviceProvider.GetRequiredService<ISettingsService>();
            var settings = _settingsService.CurrentSettings;
            
            // Now the window has a handle, so we can register the hotkey service
            _hotkeyService = new HotkeyService(new WindowInteropHelper(_mainWindow).Handle);
            _hotkeyService.RegisterHotkey(
                settings.HotkeyModifiers,
                settings.HotkeyKey,
                () => CheckClipboardAndShowWindow());
            _hotkeyService.StartListening();
            
            // Show a notification that the app is running
            _notifyIcon?.ShowBalloonTip(
                3000, 
                "File Converter", 
                $"Running in the background. Press {settings.HotkeyDisplayText} to convert files.", 
                ToolTipIcon.Info);
        }

        /// <summary>
        /// Shows the settings dialog
        /// </summary>
        private void ShowSettingsDialog()
        {
            try
            {
                // Get or create the settings dialog
                if (_settingsDialog == null)
                {
                    var viewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
                    _settingsDialog = _serviceProvider.GetRequiredService<SettingsDialog>();
                }
                
                // Show the dialog
                _settingsDialog.Show();
                _settingsDialog.Activate();
                
                Logger.Log(LogLevel.Info, "App", "Settings dialog shown");
            }
            catch (Exception ex)
            {
                _notifyIcon?.ShowBalloonTip(
                    3000, 
                    "File Converter", 
                    $"Error showing settings dialog: {ex.Message}", 
                    ToolTipIcon.Error);
                
                Logger.LogException(LogLevel.Error, "App", "Error showing settings dialog", ex);
            }
        }
        
        /// <summary>
        /// Opens the log file in the default text editor
        /// </summary>
        private void OpenLogFile()
        {
            try
            {
                var logFilePath = Logger.GetLogFilePath();
                if (!string.IsNullOrEmpty(logFilePath) && File.Exists(logFilePath))
                {
                    // Open the log file with the default application
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = logFilePath,
                        UseShellExecute = true
                    });
                    
                    Logger.Log(LogLevel.Info, "App", $"Opening log file: {logFilePath}");
                }
                else
                {
                    _notifyIcon?.ShowBalloonTip(
                        3000, 
                        "File Converter", 
                        "Log file not found.", 
                        ToolTipIcon.Warning);
                    
                    Logger.Log(LogLevel.Warning, "App", $"Log file not found: {logFilePath}");
                }
            }
            catch (Exception ex)
            {
                _notifyIcon?.ShowBalloonTip(
                    3000, 
                    "File Converter", 
                    $"Error opening log file: {ex.Message}", 
                    ToolTipIcon.Error);
                
                Logger.LogException(LogLevel.Error, "App", "Error opening log file", ex);
            }
        }

        /// <summary>
        /// Handles the application exit event
        /// </summary>
        /// <param name="e">Exit event args</param>
        protected override void OnExit(ExitEventArgs e)
        {
            // Dispose the hotkey service
            _hotkeyService?.Dispose();

            // Dispose the notify icon
            _notifyIcon?.Dispose();

            // Dispose the service provider
            _serviceProvider.Dispose();

            base.OnExit(e);
        }
    }
}
