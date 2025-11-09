using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace DataAcquisition
{
    public partial class MainWindow : Window
    {
        private DataAcquisitionUART? dataAcquisition;
        private List<ToggleButton> outputPins = new List<ToggleButton>();
        private List<Ellipse> inputLeds = new List<Ellipse>();
        private bool isAcquiring = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeControls();
            LoadAvailablePorts();
        }

        private void InitializeControls()
        {
            // Initialize output pin toggle buttons
            outputPins.Add(OUT0);
            outputPins.Add(OUT1);
            outputPins.Add(OUT2);
            outputPins.Add(OUT3);
            outputPins.Add(OUT4);
            outputPins.Add(OUT5);
            outputPins.Add(OUT6);
            outputPins.Add(OUT7);

            // Initialize input LED indicators
            inputLeds.Add(IN0);
            inputLeds.Add(IN1);
            inputLeds.Add(IN2);
            inputLeds.Add(IN3);
            inputLeds.Add(IN4);
            inputLeds.Add(IN5);
            inputLeds.Add(IN6);
            inputLeds.Add(IN7);

            // Disable controls until connected
            SetControlsEnabled(false);
        }

        private void LoadAvailablePorts()
        {
            ComPortComboBox.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            
            foreach (string port in ports)
            {
                ComPortComboBox.Items.Add(port);
            }

            if (ComPortComboBox.Items.Count > 0)
            {
                ComPortComboBox.SelectedIndex = 0;
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            foreach (var pin in outputPins)
            {
                pin.IsEnabled = enabled;
            }
            PowerSupplySlider.IsEnabled = enabled;
        }

        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (!isAcquiring)
            {
                // Start acquisition
                if (ComPortComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string selectedPort = ComPortComboBox.SelectedItem.ToString()!;
                
                if (!int.TryParse(DeviceAddressTextBox.Text, out int deviceAddress))
                {
                    MessageBox.Show("Invalid device address.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    dataAcquisition = new DataAcquisitionUART(selectedPort, 9600);
                    dataAcquisition.DataReceived += DataAcquisition_DataReceived;
                    dataAcquisition.Start();

                    isAcquiring = true;
                    StartStopButton.Content = "Stop";
                    StartStopButton.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                    ComPortComboBox.IsEnabled = false;
                    DeviceAddressTextBox.IsEnabled = false;
                    SetControlsEnabled(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to start acquisition: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // Stop acquisition
                StopAcquisition();
            }
        }

        private void StopAcquisition()
        {
            if (dataAcquisition != null)
            {
                dataAcquisition.Stop();
                dataAcquisition.DataReceived -= DataAcquisition_DataReceived;
                dataAcquisition.Dispose();
                dataAcquisition = null;
            }

            isAcquiring = false;
            StartStopButton.Content = "Start";
            StartStopButton.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
            ComPortComboBox.IsEnabled = true;
            DeviceAddressTextBox.IsEnabled = true;
            SetControlsEnabled(false);

            // Reset all indicators
            foreach (var led in inputLeds)
            {
                led.Fill = new SolidColorBrush(Colors.Gray);
            }
        }

        private void DataAcquisition_DataReceived(object? sender, DataReceivedEventArgs e)
        {
            // Update UI on the UI thread
            Dispatcher.Invoke(() =>
            {
                // Update digital input LEDs
                for (int i = 0; i < Math.Min(e.DigitalInputs.Count, inputLeds.Count); i++)
                {
                    inputLeds[i].Fill = e.DigitalInputs[i] 
                        ? new SolidColorBrush(Colors.Red) 
                        : new SolidColorBrush(Colors.Gray);
                }

                // Update analog values (meters)
                if (e.AnalogValues.Count >= 4)
                {
                    DCVoltmeter.Text = $"{e.AnalogValues[0]:F2} V";
                    ACVoltmeter.Text = $"{e.AnalogValues[1]:F2} V";
                    DCAmmeter.Text = $"{e.AnalogValues[2]:F2} mA";
                    ACAmmeter.Text = $"{e.AnalogValues[3]:F2} mA";
                }
            });
        }

        private void PinToggle_Click(object sender, RoutedEventArgs e)
        {
            if (dataAcquisition == null || !isAcquiring)
                return;

            ToggleButton? button = sender as ToggleButton;
            if (button == null)
                return;

            int pinIndex = outputPins.IndexOf(button);
            if (pinIndex >= 0)
            {
                bool value = button.IsChecked ?? false;
                dataAcquisition.SetDigitalOutput(pinIndex, value);
            }
        }

        private void PowerSupplySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VoltageDisplay != null)
            {
                VoltageDisplay.Text = $"{e.NewValue:F1} V";
            }

            if (dataAcquisition != null && isAcquiring)
            {
                dataAcquisition.SetVoltage(e.NewValue);
            }
        }

        private void FunctionGenerator_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Function Generator module will be loaded here.", "Function Generator", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Oscilloscope_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Oscilloscope module will be loaded here.", "Oscilloscope", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PortSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Port Settings dialog will be shown here.", "Port Settings", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenManual_Click(object sender, RoutedEventArgs e)
        {
            string manualPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "manual.pdf");
            
            if (File.Exists(manualPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = manualPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open manual: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Manual file not found.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OpenConfig_Click(object sender, RoutedEventArgs e)
        {
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "config.txt");
            
            if (File.Exists(configPath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = configPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open config: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Config file not found.", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Data Acquisition System v1.0\n\n" +
                          "A comprehensive data acquisition and control system.\n\n" +
                          "© 2025 All Rights Reserved", 
                          "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CheckResults_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Results checking functionality will be implemented here.", 
                "Check Results", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveCSV_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = "csv",
                FileName = $"DataAcquisition_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        writer.WriteLine("Timestamp,DC Voltage,AC Voltage,DC Current,AC Current");
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}," +
                                       $"{DCVoltmeter.Text},{ACVoltmeter.Text}," +
                                       $"{DCAmmeter.Text},{ACAmmeter.Text}");
                    }

                    MessageBox.Show("Data saved successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save CSV: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExperimentsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item)
            {
                string? header = item.Header?.ToString();
                
                if (header != null)
                {
                    ProcedureTextBlock.Text = GetProcedureText(header);
                }
            }
        }

        private string GetProcedureText(string experimentName)
        {
            return experimentName switch
            {
                "LED Blinking" => "1. Connect LED to OUT0 pin\n" +
                                 "2. Toggle OUT0 switch to turn LED on/off\n" +
                                 "3. Observe the LED state change\n" +
                                 "4. Repeat with other output pins",
                
                "Switch Interface" => "1. Connect switch to IN0 pin\n" +
                                     "2. Press the switch\n" +
                                     "3. Observe IN0 LED indicator\n" +
                                     "4. Test with multiple switches",
                
                "Seven Segment Display" => "1. Connect seven segment display to OUT0-OUT6\n" +
                                          "2. Toggle switches to display numbers\n" +
                                          "3. Create patterns for 0-9 digits",
                
                "ADC Reading" => "1. Connect analog sensor to ADC input\n" +
                                "2. Start data acquisition\n" +
                                "3. Observe voltage readings in meters\n" +
                                "4. Save data using Save CSV button",
                
                "DAC Output" => "1. Use Power Supply slider\n" +
                               "2. Adjust voltage from 0-5V\n" +
                               "3. Measure output with multimeter\n" +
                               "4. Verify voltage accuracy",
                
                "Temperature Sensor" => "1. Connect temperature sensor to analog input\n" +
                                       "2. Start acquisition\n" +
                                       "3. Monitor temperature readings\n" +
                                       "4. Record data over time",
                
                "UART Communication" => "1. Configure COM port settings\n" +
                                       "2. Connect device via UART\n" +
                                       "3. Start communication\n" +
                                       "4. Send and receive data",
                
                "I2C Interface" => "1. Connect I2C device\n" +
                                  "2. Configure device address\n" +
                                  "3. Read/Write data\n" +
                                  "4. Verify communication",
                
                "SPI Interface" => "1. Connect SPI device\n" +
                                  "2. Configure SPI settings\n" +
                                  "3. Transfer data\n" +
                                  "4. Verify data integrity",
                
                _ => "Select an experiment to view detailed procedure."
            };
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopAcquisition();
        }
    }
}
