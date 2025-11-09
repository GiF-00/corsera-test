using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace DataAcquisition
{
    public class DataReceivedEventArgs : EventArgs
    {
        public List<double> AnalogValues { get; set; } = new List<double>();
        public List<bool> DigitalInputs { get; set; } = new List<bool>();
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class DataAcquisitionUART : IDisposable
    {
        private SerialPort? serialPort;
        private Timer? pollingTimer;
        private bool isRunning = false;
        private readonly object lockObject = new object();

        public event EventHandler<DataReceivedEventArgs>? DataReceived;

        // Configuration
        private readonly string portName;
        private readonly int baudRate;
        private readonly int pollingIntervalMs = 500;

        // Data storage
        private List<double> analogValues = new List<double>(new double[16]);
        private List<bool> digitalInputs = new List<bool>(new bool[8]);

        public DataAcquisitionUART(string portName, int baudRate = 9600)
        {
            this.portName = portName;
            this.baudRate = baudRate;
        }

        public void Start()
        {
            lock (lockObject)
            {
                if (isRunning)
                    return;

                try
                {
                    // Configure and open serial port
                    serialPort = new SerialPort(portName, baudRate)
                    {
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        DataBits = 8,
                        Handshake = Handshake.None,
                        ReadTimeout = 1000,
                        WriteTimeout = 1000
                    };

                    serialPort.Open();
                    isRunning = true;

                    // Start polling timer
                    pollingTimer = new Timer(PollingCallback, null, 0, pollingIntervalMs);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to start UART communication: {ex.Message}", ex);
                }
            }
        }

        public void Stop()
        {
            lock (lockObject)
            {
                if (!isRunning)
                    return;

                isRunning = false;

                // Stop timer
                pollingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                pollingTimer?.Dispose();
                pollingTimer = null;

                // Close serial port
                if (serialPort != null && serialPort.IsOpen)
                {
                    try
                    {
                        serialPort.Close();
                    }
                    catch { }
                }
            }
        }

        private void PollingCallback(object? state)
        {
            if (!isRunning || serialPort == null || !serialPort.IsOpen)
                return;

            try
            {
                // Request analog data
                RequestAnalogData();

                // Request digital inputs
                RequestDigitalInputs();

                // Raise event with updated data
                OnDataReceived();
            }
            catch (Exception ex)
            {
                // Log error but continue polling
                System.Diagnostics.Debug.WriteLine($"Polling error: {ex.Message}");
            }
        }

        private void RequestAnalogData()
        {
            try
            {
                lock (lockObject)
                {
                    if (serialPort == null || !serialPort.IsOpen)
                        return;

                    // Send command to get analog data
                    serialPort.WriteLine("GET ADS");

                    // Wait for response
                    Thread.Sleep(50);

                    if (serialPort.BytesToRead > 0)
                    {
                        string response = serialPort.ReadLine().Trim();
                        ParseAnalogData(response);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Analog data request error: {ex.Message}");
            }
        }

        private void RequestDigitalInputs()
        {
            try
            {
                lock (lockObject)
                {
                    if (serialPort == null || !serialPort.IsOpen)
                        return;

                    // Send command to get digital inputs
                    serialPort.WriteLine("GET PINS");

                    // Wait for response
                    Thread.Sleep(50);

                    if (serialPort.BytesToRead > 0)
                    {
                        string response = serialPort.ReadLine().Trim();
                        ParseDigitalInputs(response);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Digital inputs request error: {ex.Message}");
            }
        }

        private void ParseAnalogData(string data)
        {
            try
            {
                // Expected format: "value1,value2,value3,...,value16"
                string[] values = data.Split(',');

                for (int i = 0; i < Math.Min(values.Length, analogValues.Count); i++)
                {
                    if (double.TryParse(values[i].Trim(), out double value))
                    {
                        analogValues[i] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Parse analog data error: {ex.Message}");
            }
        }

        private void ParseDigitalInputs(string data)
        {
            try
            {
                // Expected format: "01010101" (8 bits)
                for (int i = 0; i < Math.Min(data.Length, digitalInputs.Count); i++)
                {
                    digitalInputs[i] = data[i] == '1';
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Parse digital inputs error: {ex.Message}");
            }
        }

        public void SetDigitalOutput(int pinIndex, bool value)
        {
            if (pinIndex < 0 || pinIndex > 7)
                return;

            try
            {
                lock (lockObject)
                {
                    if (serialPort == null || !serialPort.IsOpen)
                        return;

                    // Send command to set digital output
                    string command = $"SET PIN{pinIndex} {(value ? "1" : "0")}";
                    serialPort.WriteLine(command);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Set digital output error: {ex.Message}");
            }
        }

        public void SetVoltage(double voltage)
        {
            if (voltage < 0 || voltage > 5)
                return;

            try
            {
                lock (lockObject)
                {
                    if (serialPort == null || !serialPort.IsOpen)
                        return;

                    // Send command to set voltage
                    string command = $"SET VOLTAGE {voltage:F2}";
                    serialPort.WriteLine(command);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Set voltage error: {ex.Message}");
            }
        }

        private void OnDataReceived()
        {
            DataReceived?.Invoke(this, new DataReceivedEventArgs
            {
                AnalogValues = new List<double>(analogValues),
                DigitalInputs = new List<bool>(digitalInputs),
                Timestamp = DateTime.Now
            });
        }

        public void Dispose()
        {
            Stop();
            serialPort?.Dispose();
        }
    }
}
