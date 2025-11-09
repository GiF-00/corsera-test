# Build Instructions for Data Acquisition System

## Prerequisites

This is a **C# WPF (Windows Presentation Foundation)** application that requires:

1. **Windows Operating System** (WPF is Windows-only)
2. **.NET 6.0 SDK or higher**
3. **Visual Studio 2022** (recommended) or **Visual Studio Code** with C# extension

## Installation Steps

### Option 1: Using Visual Studio 2022 (Recommended)

1. **Install Visual Studio 2022**
   - Download from: https://visualstudio.microsoft.com/downloads/
   - During installation, select the ".NET desktop development" workload

2. **Open the Project**
   - Launch Visual Studio 2022
   - Click "Open a project or solution"
   - Navigate to the project folder and select `DataAcquisition.csproj`

3. **Restore NuGet Packages**
   - Visual Studio will automatically restore packages
   - Or manually: Right-click solution → "Restore NuGet Packages"

4. **Build the Project**
   - Press `Ctrl+Shift+B` or
   - Menu: Build → Build Solution

5. **Run the Application**
   - Press `F5` (Debug mode) or `Ctrl+F5` (Release mode)

### Option 2: Using .NET CLI

1. **Install .NET 6.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/6.0
   - Install the SDK (not just the runtime)

2. **Open Command Prompt/PowerShell**
   - Navigate to the project directory:
   ```bash
   cd path\to\DataAcquisition
   ```

3. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the Project**
   ```bash
   dotnet build --configuration Release
   ```

5. **Run the Application**
   ```bash
   dotnet run
   ```

### Option 3: Using Visual Studio Code

1. **Install Prerequisites**
   - Install .NET 6.0 SDK
   - Install Visual Studio Code
   - Install C# extension (ms-dotnettools.csharp)

2. **Open Project**
   - Open the project folder in VS Code
   - VS Code will prompt to add required assets (click "Yes")

3. **Build and Run**
   - Press `F5` to build and debug
   - Or use terminal commands:
   ```bash
   dotnet build
   dotnet run
   ```

## Project Structure

```
DataAcquisition/
├── DataAcquisition.csproj    # Project file
├── App.xaml                   # Application definition
├── App.xaml.cs                # Application code-behind
├── MainWindow.xaml            # Main window UI
├── MainWindow.xaml.cs         # Main window logic
├── DataAcquisitionUART.cs     # UART communication
└── Assets/                    # Resources
    ├── config.txt
    └── README.txt
```

## Dependencies

The project uses the following NuGet packages:
- **System.IO.Ports** (v7.0.0) - For serial port communication

These are automatically restored during build.

## Building for Distribution

### Create a Self-Contained Executable

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

This creates a single executable file in:
```
bin\Release\net6.0-windows\win-x64\publish\
```

### Create Framework-Dependent Build

```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

Requires .NET 6.0 Runtime to be installed on target machine.

## Troubleshooting

### Error: "The type or namespace name 'Windows' does not exist"
- **Solution**: Ensure you're building on Windows with WPF support
- Check that `<UseWPF>true</UseWPF>` is in the .csproj file

### Error: "SDK 'Microsoft.NET.Sdk' not found"
- **Solution**: Install .NET 6.0 SDK or higher

### Error: "Package 'System.IO.Ports' not found"
- **Solution**: Run `dotnet restore` to download NuGet packages

### Build succeeds but application doesn't run
- **Solution**: Ensure you're on Windows (WPF is Windows-only)
- Check that all required DLLs are in the output directory

## Running on Linux/Mac

**Note**: This is a WPF application which is **Windows-only**. It cannot run natively on Linux or Mac.

Alternative options:
1. Use Windows VM or Wine (limited WPF support)
2. Port the application to Avalonia UI (cross-platform XAML framework)
3. Rewrite using .NET MAUI for cross-platform support

## Hardware Requirements

- **OS**: Windows 10 or higher
- **RAM**: 4GB minimum, 8GB recommended
- **.NET**: .NET 6.0 Runtime or SDK
- **Serial Port**: Physical COM port or USB-to-Serial adapter

## Additional Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [WPF Documentation](https://docs.microsoft.com/dotnet/desktop/wpf/)
- [Serial Port Programming](https://docs.microsoft.com/dotnet/api/system.io.ports.serialport)

## Support

For build issues or questions, refer to:
- Project README.md
- .NET SDK documentation
- Visual Studio documentation
