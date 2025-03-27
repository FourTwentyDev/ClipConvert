@echo off
echo Building ClipConvert Installer
echo ============================
echo.

REM Check if WiX Toolset is installed
where candle.exe >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: WiX Toolset not found in PATH.
    echo Please install WiX Toolset from https://wixtoolset.org/releases/
    echo and ensure it's added to your PATH.
    exit /b 1
)

REM Build the main project in Release mode
echo Building main project...
dotnet build ..\FileConvertor.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build main project.
    exit /b 1
)

REM Create Assets directory if it doesn't exist
if not exist "..\Assets" mkdir "..\Assets"

REM Check for required asset files
if not exist "..\Assets\app_icon.ico" (
    echo WARNING: app_icon.ico not found. Using placeholder.
    echo This is a placeholder for the app icon > ..\Assets\app_icon.ico
)

if not exist "..\Assets\banner.bmp" (
    echo WARNING: banner.bmp not found. Using placeholder.
    echo This is a placeholder for the banner image > ..\Assets\banner.bmp
)

if not exist "..\Assets\dialog.bmp" (
    echo WARNING: dialog.bmp not found. Using placeholder.
    echo This is a placeholder for the dialog image > ..\Assets\dialog.bmp
)

if not exist "..\Assets\License.rtf" (
    echo WARNING: License.rtf not found. Using placeholder.
    echo {\rtf1\ansi\deff0{\fonttbl{\f0\fnil\fcharset0 Arial;}}{\colortbl;\red0\green0\blue0;}\f0\fs20 MIT License\par} > ..\Assets\License.rtf
)

REM Build the installer
echo Building installer...
candle.exe ClipConvert.wxs -ext WixUIExtension -dBinDir=..\..\FileConvertor\bin\Release\net9.0-windows -dIconsDir=..\Assets
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to compile WiX source file.
    exit /b 1
)

light.exe ClipConvert.wixobj -ext WixUIExtension -out ClipConvert.msi
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to link WiX object file.
    exit /b 1
)

echo.
echo Build completed successfully!
echo Installer created: %CD%\ClipConvert.msi
echo.

REM Clean up temporary files
del ClipConvert.wixobj
del ClipConvert.wixpdb

exit /b 0
