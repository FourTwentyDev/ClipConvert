# Getting Started with ClipConvert

This guide will help you get up and running with ClipConvert, the open-source file converter that works directly with your clipboard.

## Installation

### System Requirements

- Windows 10 or later
- .NET 9.0 or later
- 50MB of free disk space
- 2GB RAM (recommended)

### Download and Install

1. Visit the [Releases page](https://github.com/FourTwentyDev/ClipConvert/releases) on GitHub
2. Download the latest installer package for your operating system
3. Run the installer and follow the on-screen instructions
4. Once installation is complete, ClipConvert will start automatically

### First Launch

When you first launch ClipConvert:

1. The application will run in the background
2. A system tray icon will appear in your taskbar
3. A welcome notification will confirm the application is running
4. The default hotkey (Ctrl+Alt+C) will be active

## Basic Usage

### Converting Your First File

1. **Copy a file to your clipboard**
   - Right-click on any file in File Explorer
   - Select "Copy" from the context menu (or press Ctrl+C)

2. **Activate ClipConvert**
   - Press the default hotkey: Ctrl+Alt+C
   - The conversion dialog will appear

3. **Select your target format**
   - Choose from the available formats in the dropdown menu
   - Available formats will depend on the type of file you copied

4. **Convert the file**
   - Click the "CONVERT" button
   - Wait for the conversion to complete (usually just a few seconds)

5. **Use the converted file**
   - The converted file is now in your clipboard
   - Navigate to where you want to use the file
   - Paste (Ctrl+V) to place the converted file

### Example: Converting a Word Document to PDF

1. Copy a .docx file to your clipboard
2. Press Ctrl+Alt+C to open ClipConvert
3. Select "pdf" from the format dropdown
4. Click "CONVERT"
5. Once conversion is complete, paste the PDF file where needed

## Advanced Features

### Customizing Hotkeys

1. Right-click the ClipConvert icon in the system tray
2. Select "Settings" from the context menu
3. Navigate to the "Hotkeys" tab
4. Click "Change" next to the current hotkey
5. Press your desired key combination
6. Click "Save"

### Startup Configuration

1. Open the Settings dialog
2. Navigate to the "General" tab
3. Toggle "Start with Windows" to control automatic startup
4. Click "Save" to apply changes

### Viewing Conversion Logs

If you encounter any issues or want to review your conversion history:

1. Right-click the ClipConvert icon in the system tray
2. Select "View Log File"
3. The log file will open in your default text editor

## Supported File Formats

### Document Formats

| Source Format | Target Formats |
|---------------|----------------|
| PDF (.pdf) | Text (.txt) |
| Word (.docx) | PDF (.pdf) |
| Markdown (.md) | HTML (.html) |
| Excel (.xlsx) | CSV (.csv) |
| CSV (.csv) | Excel (.xlsx) |

### Image Formats

| Source Format | Target Formats |
|---------------|----------------|
| JPEG (.jpg, .jpeg) | PNG (.png) |
| PNG (.png) | JPEG (.jpg) |

### Audio Formats

| Source Format | Target Formats |
|---------------|----------------|
| MP3 (.mp3) | WAV (.wav) |

## Troubleshooting

### Common Issues

**ClipConvert doesn't detect my file**
- Make sure you've properly copied the file (not just selected it)
- Check if the file format is supported
- Try copying the file again

**Conversion fails**
- Check if the file is not corrupted
- Ensure the file isn't open in another application
- Check the log file for specific error messages

**Hotkey doesn't work**
- Check if another application is using the same hotkey
- Try restarting ClipConvert
- Verify the hotkey is correctly set in Settings

### Getting Help

If you encounter issues not covered in this guide:

1. Check the [GitHub Issues](https://github.com/FourTwentyDev/ClipConvert/issues) for similar problems
2. Review the [FAQ](https://github.com/FourTwentyDev/ClipConvert/wiki/FAQ) on our wiki
3. Open a new issue with detailed information about your problem

## Contributing

We welcome contributions to ClipConvert! If you'd like to help:

1. Fork the repository on GitHub
2. Clone your fork locally
3. Create a new branch for your feature or bugfix
4. Make your changes
5. Submit a pull request

For more details, see our [Contributing Guidelines](../CONTRIBUTING.md).

---

Thank you for using ClipConvert! We hope it makes your file conversion tasks faster and easier.
