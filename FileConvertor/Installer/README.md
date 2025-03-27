# ClipConvert Installer

This directory contains the WiX Toolset project files needed to build an installer for ClipConvert.

## Prerequisites

To build the installer, you need:

1. [WiX Toolset](https://wixtoolset.org/releases/) (v3.11 or later)
2. [WiX Toolset Visual Studio Extension](https://marketplace.visualstudio.com/items?itemName=WixToolset.WixToolsetVisualStudio2019Extension) (if using Visual Studio)
3. .NET SDK 9.0 or later

## Building the Installer

### Using Visual Studio

1. Open the solution in Visual Studio
2. Build the FileConvertor project in Release mode
3. Right-click on the ClipConvert installer project and select "Build"
4. The installer will be generated in the `FileConvertor/Installer/bin/Release` directory

### Using Command Line

1. Build the main project:
   ```
   dotnet build FileConvertor/FileConvertor.csproj -c Release
   ```

2. Build the installer using the WiX tools:
   ```
   cd FileConvertor/Installer
   candle ClipConvert.wxs -ext WixUIExtension -dBinDir=..\..\FileConvertor\bin\Release\net9.0-windows -dIconsDir=..\Assets
   light ClipConvert.wixobj -ext WixUIExtension -out ClipConvert.msi
   ```

## Installer Features

- Installs ClipConvert to Program Files
- Creates Start Menu and Desktop shortcuts
- Configures auto-start with Windows
- Provides uninstall functionality
- Launches the application after installation

## Customization

- Update product information in `ClipConvert.wxs`
- Replace assets in the `FileConvertor/Assets` directory:
  - `app_icon.ico`: Application icon
  - `banner.bmp`: Installer banner (493×58 pixels)
  - `dialog.bmp`: Installer background (493×312 pixels)
  - `License.rtf`: License text in RTF format
