# Data Acquisition System

A comprehensive WPF-based data acquisition and control system for interfacing with microcontroller units via UART communication.

## Features

### User Interface
- **Logic Switches**: 8 digital output controls (OUT0-OUT7) with visual toggle buttons
- **Power Supply Control**: Adjustable 0-5V power supply with slider control
- **Logic Indicators**: 8 LED indicators for digital inputs (IN0-IN7)
- **Digital Meters**: DC/AC Voltmeter and Ammeter displays
- **Function Generator**: Module for signal generation
- **Oscilloscope**: Module for signal visualization
- **Experiments Panel**: Pre-configured experiments with procedures

### Communication
- **UART/Serial Communication**: Configurable COM port and baud rate
- **Real-time Data Acquisition**: Continuous polling of analog and digital inputs
- **Command Interface**: Send commands to control outputs and read inputs

### Data Management
- **CSV Export**: Save acquired data to CSV files
- **Results Checking**: Validate experimental results
- **Configuration Files**: Customizable settings

## Requirements

- .NET 6.0 or higher
- Windows OS (WPF application)
- Serial port hardware (COM port)

## Building the Project

### Using .NET CLI

```bash
dotnet restore
dotnet build
dotnet run
```

### Using Visual Studio

1. Open `DataAcquisition.csproj` in Visual Studio
2. Build the solution (Ctrl+Shift+B)
3. Run the application (F5)

## Usage

### Starting Data Acquisition

1. **Select COM Port**: Choose the appropriate COM port from the dropdown
2. **Set Device Address**: Enter the device address (default: 1)
3. **Click Start**: Begin data acquisition

### Controlling Outputs

- **Digital Outputs**: Toggle OUT0-OUT7 switches to control digital pins
- **Power Supply**: Adjust the slider to set voltage (0-5V)

### Monitoring Inputs

- **Digital Inputs**: Watch IN0-IN7 LED indicators (Red = ON, Gray = OFF)
- **Analog Readings**: View real-time measurements in digital meters

### Running Experiments

1. Select an experiment from the Experiments tree
2. Read the procedure in the Procedure panel
3. Follow the steps to complete the experiment
4. Use "Check Results" to validate
5. Use "Save CSV" to export data

## UART Protocol

### Commands Sent to Device

- `GET ADS` - Request 16 analog channel readings
- `GET PINS` - Request 8 digital input states
- `SET PIN{n} {0|1}` - Set digital output pin (n=0-7)
- `SET VOLTAGE {value}` - Set power supply voltage (0.00-5.00)

### Expected Responses

- **Analog Data**: `value1,value2,value3,...,value16` (comma-separated)
- **Digital Inputs**: `01010101` (8-bit string, 0=LOW, 1=HIGH)

## Project Structure

```
DataAcquisition/
├── DataAcquisition.csproj    # Project configuration
├── App.xaml                   # Application resources and styles
├── App.xaml.cs                # Application entry point
├── MainWindow.xaml            # Main UI layout
├── MainWindow.xaml.cs         # UI logic and event handlers
├── DataAcquisitionUART.cs     # UART communication class
├── Assets/                    # Icons and resources
│   ├── manual.png
│   ├── config.png
│   ├── app.ico
│   ├── manual.pdf
│   └── config.txt
└── README.md                  # This file
```

## Configuration

Edit `Assets/config.txt` to customize:
- Serial port settings (baud rate, parity, etc.)
- Polling interval
- Display settings
- Logging options

## Troubleshooting

### COM Port Not Found
- Ensure the device is connected
- Check Device Manager for available COM ports
- Verify driver installation

### Connection Failed
- Check baud rate matches device settings
- Verify COM port is not in use by another application
- Ensure proper cable connections

### No Data Received
- Verify device is responding to commands
- Check UART protocol implementation on device
- Review command format and responses

## License

© 2025 All Rights Reserved

## Support

For issues, questions, or contributions, please refer to the project documentation.
