using DataAcquisition.Entities;
using DataAcquisition.Extensions;
using DataAcquisition.Logger;
using DataAcquisition.Services;
using DataAcquisition.Settings;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DataAcquisition.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ILogger? logger;
		private PortSettings portSettings;
		private ApplicationSettings? applicationSettings;
		private readonly PortsDiscoverer portsDiscoverer;

		private string connectedCom = "";
		private OperationState operationState;

		private readonly string startStopButtonStartTitle = "";
		private readonly string startStopButtonStopTitle = "Stop";

		private readonly Brush deviceAddressInitialBG;
		private readonly Brush valueLabelsInitialFG;

		private readonly List<Border> dataPorts;
		private readonly List<StackPanel> dataPortControls;
		private readonly List<Button> plotButtons;
		private readonly List<Label> unitLabels;
		private readonly List<Label> valueLabels;

		private readonly ExperimentsDb db;

		private Experiment selectedExperiment = null!;

		public MainWindow(ILogger logger)
		{
			this.logger = logger;
			this.logger?.Info("Initializing components");

			InitializeComponent();

			this.logger?.Info("Initializing complete");

			db = new ExperimentsDb(logger);

			dataPorts = new List<Border>()
			{
				DataPortBorderA1,    DataPortBorderA2,    DataPortBorderA3,    DataPortBorderA4,
				DataPortBorderA5,    DataPortBorderA6,    DataPortBorderA7,    DataPortBorderA8,
				DataPortBorderA9,    DataPortBorderA10,   DataPortBorderA11,   DataPortBorderA12,
				DataPortBorderA13,   DataPortBorderA14,   DataPortBorderA15,   DataPortBorderA16
			};

			dataPortControls = new List<StackPanel>()
			{
				DataPortControlsA1,    DataPortControlsA2,    DataPortControlsA3,    DataPortControlsA4,
				DataPortControlsA5,    DataPortControlsA6,    DataPortControlsA7,    DataPortControlsA8,
				DataPortControlsA9,    DataPortControlsA10,   DataPortControlsA11,   DataPortControlsA12,
				DataPortControlsA13,   DataPortControlsA14,   DataPortControlsA15,   DataPortControlsA16
			};

			plotButtons = new List<Button>()
			{
				A1PlotButton,    A2PlotButton,    A3PlotButton,    A4PlotButton,
				A5PlotButton,    A6PlotButton,    A7PlotButton,    A8PlotButton,
				A9PlotButton,    A10PlotButton,   A11PlotButton,   A12PlotButton,
				A13PlotButton,   A14PlotButton,   A15PlotButton,   A16PlotButton
			};

			unitLabels = new List<Label>()
			{
				A1DisplayUnit,    A2DisplayUnit,    A3DisplayUnit,    A4DisplayUnit,
				A5DisplayUnit,    A6DisplayUnit,    A7DisplayUnit,    A8DisplayUnit,
				A9DisplayUnit,    A10DisplayUnit,   A11DisplayUnit,   A12DisplayUnit,
				A13DisplayUnit,   A14DisplayUnit,   A15DisplayUnit,   A16DisplayUnit
			};

			valueLabels = new List<Label>()
			{
				A1DisplayValue,    A2DisplayValue,    A3DisplayValue,    A4DisplayValue,
				A5DisplayValue,    A6DisplayValue,    A7DisplayValue,    A8DisplayValue,
				A9DisplayValue,    A10DisplayValue,   A11DisplayValue,   A12DisplayValue,
				A13DisplayValue,   A14DisplayValue,   A15DisplayValue,   A16DisplayValue
			};

			//-
			//- Initial State of Controls
			//-

			startStopButtonStartTitle = (string)StartStopButton.Content;
			deviceAddressInitialBG = DeviceAddress.Background;
			valueLabelsInitialFG = A1DisplayValue.Foreground;

			//-
			//- Reading stored settings
			//-

			portSettings = PortSettingsRW.ReadPortSettings(logger);
			applicationSettings = ApplicationSettingsRW.ReadApplicationSettings(logger);

			//-
			//- Setting window state as it was the last time upon exit
			//-

			Left = applicationSettings.MainWindowX;
			Top = applicationSettings.MainWindowY;
			Width = applicationSettings.MainWindowWidth;
			Height = applicationSettings.MainWindowHeight;

			if (applicationSettings.MainWindowIsFullScreen)
			{
				WindowState = WindowState.Maximized;
			}

			//-
			//- Starting ports discovery
			//-

			portsDiscoverer = new PortsDiscoverer();
			portsDiscoverer.NewPortFound += OnNewPortFound;
			portsDiscoverer.PortRemoved += OnPortRemoved;
			portsDiscoverer.StartPortsDiscovery();


			//-
			//- Setting UI State
			//-

			SetInitialUiState();
			SetUiOperationState(OperationState.Stopped);
		}

		#region Helpers & Validators
		private void RunOnGUI(Action action)
		{
			if (action != null)
			{
				_ = Dispatcher.InvokeAsync(
					() => { action.Invoke(); },
					System.Windows.Threading.DispatcherPriority.Normal);
			}
		}

		private void ValidateState()
		{
			Dispatcher.Invoke(() =>
			{
				switch (operationState)
				{
					case OperationState.Stopped:
						if (SerialPortComboBox.Items.Count >= 1 && DeviceAddress.Text.IsValidDeviceAddress())
						{
							SetUiOperationState(OperationState.Ready);
							return;
						}
						break;

					case OperationState.Ready:
						if (SerialPortComboBox.Items.Count <= 0 || DeviceAddress.Text.IsValidDeviceAddress() == false)
						{
							SetUiOperationState(OperationState.Stopped);
							return;
						}
						break;

					case OperationState.Started:
						if (SerialPortComboBox.SelectedIndex >= 0 && (string)SerialPortComboBox.Items[SerialPortComboBox.SelectedIndex] != connectedCom)
						{
							if (SerialPortComboBox.Items.Count <= 0)
							{
								SetUiOperationState(OperationState.Stopped);
								return;
							}
							else
							{
								SetUiOperationState(OperationState.Ready);
								return;
							}
						}
						else if (SerialPortComboBox.SelectedIndex < 0)
						{
							SetUiOperationState(OperationState.Stopped);
							return;
						}
						break;
				}
			});
		}
		#endregion
		#region Initial UI State
		private void SetInitialUiState()
		{
			DeviceAddress.Text = portSettings.DeviceAddress.ToString();

			ExperimentsDataUpdated();

			for (int i = 0; i < portSettings.DisplayUnits.Count(); i++)
			{
				unitLabels[i].Content = portSettings.DisplayUnits[i].ToString();
			}

			for (int i = 0; i < valueLabels.Count; i++)
			{
				valueLabels[i].Content = "0.000";
			}

			for (int i = 0; i < plotButtons.Count; i++)
			{
				plotButtons[i].Tag = i + 1;
				plotButtons[i].Click += OnPlotButtonClocked;
			}
		}
		#endregion
		#region UI State During Operation
		private void SetUiOperationState(OperationState newState)
		{
			switch (newState)
			{
				case OperationState.Stopped:
					Dispatcher.Invoke(
						() =>
						{
							StartStopButton.IsEnabled = false;
							StartStopButton.Content = startStopButtonStartTitle;
							StartStopButton.Background = new SolidColorBrush(Constants.StartButtonBgColor);

							SerialPortComboBox.IsEnabled = true;
							DeviceAddress.IsEnabled = true;

							for (int i = 0; i < plotButtons.Count; i++)
							{
								plotButtons[i].IsEnabled = false;
							}
						});
					break;

				case OperationState.Ready:
					Dispatcher.Invoke(
						() =>
						{
							StartStopButton.IsEnabled = true;
							StartStopButton.Content = startStopButtonStartTitle;
							StartStopButton.Background = new SolidColorBrush(Constants.StartButtonBgColor);

							SerialPortComboBox.IsEnabled = true;
							DeviceAddress.IsEnabled = true;

							for (int i = 0; i < plotButtons.Count; i++)
							{
								plotButtons[i].IsEnabled = false;
							}
						});
					break;

				case OperationState.Started:
					Dispatcher.Invoke(
						() =>
						{
							StartStopButton.IsEnabled = true;
							StartStopButton.Content = startStopButtonStopTitle;
							StartStopButton.Background = new SolidColorBrush(Constants.StopButtonBgColor);

							SerialPortComboBox.IsEnabled = false;
							DeviceAddress.IsEnabled = false;

							for (int i = 0; i < plotButtons.Count; i++)
							{
								plotButtons[i].IsEnabled = true;
							}
						});
					break;
			}

			if (newState != operationState)
			{
				if (newState == OperationState.Started)
				{
					StartAcquisition();
				}
				else
				{
					StopAcquisition();
				}
			}

			operationState = newState;
		}

		private void UpdateExperimentView(Experiment experiment)
		{
			if (experiment != null)
			{
				lock (this)
				{
					selectedExperiment = experiment;
				}

				ExperimentImage.Source = new BitmapImage(new Uri($"{selectedExperiment.ImagePath}"));
				ProcedureTextBlock.Text = selectedExperiment.Procedure;

				for (int i = 0; i < selectedExperiment.InputEnabled.Length; i++)
				{
					if (selectedExperiment.InputEnabled[i] == true)
					{
						dataPorts[i].Visibility = Visibility.Visible;
						dataPortControls[i].Visibility = Visibility.Visible;
					}
					else
					{
						dataPorts[i].Visibility = Visibility.Hidden;
						dataPortControls[i].Visibility = Visibility.Hidden;
					}
				}
			}
			else
			{
				lock (this)
				{
					selectedExperiment = null!;
				}

				ExperimentImage.Source = null;
				ProcedureTextBlock.Text = "";

				for (int i = 0; i < dataPorts.Count; i++)
				{
					dataPorts[i].Visibility = Visibility.Visible;
					dataPortControls[i].Visibility = Visibility.Visible;
				}
			}
		}
		#endregion
		#region GUI Events - Data from Other Windows
		public void UpdatePortSettings(PortSettings portSettings)
		{
			this.portSettings = portSettings;

			try
			{
				dataAcquisition.UpdatePortSettings(portSettings);
			}
			catch (Exception) { }

			for (int i = 0; i < portSettings.DisplayUnits.Count(); i++)
			{
				unitLabels[i].Content = portSettings.DisplayUnits[i].ToString();
			}
		}

		public void ExperimentsDataUpdated()
		{
			ExperimentsListView.Items.Clear();

			List<PCB> p = db.ReadAllPCBs();

			foreach (PCB pcb in p)
			{
				TreeViewItem pcbItem = new TreeViewItem()
				{
					Header = pcb.Title,
					Tag = pcb
				};

				foreach (Circuit cct in pcb.Circuits)
				{
					TreeViewItem cctItem = new TreeViewItem()
					{
						Header = cct.Title,
						Tag = cct
					};

					foreach (Experiment exp in cct.Experiments)
					{
						TreeViewItem expItem = new TreeViewItem()
						{
							Header = exp.Title,
							Tag = exp
						};

						cctItem.Items.Add(expItem);
					}

					pcbItem.Items.Add(cctItem);
				}

				ExperimentsListView.Items.Add(pcbItem);
			}
		}
		#endregion
		#region Port Discovery / Removal Callbacks
		private void OnNewPortFound(string port)
		{
			logger?.Info($"Port Added: {port}");

			SerialPortComboBox.Dispatcher.Invoke(
				() =>
				{
					SerialPortComboBox.Items.Add(port);

					if (SerialPortComboBox.Items.Count == 1)
					{
						SerialPortComboBox.SelectedIndex = 0;
						portSettings.ComPort = port;
					}
				});

			ValidateState();
		}

		private void OnPortRemoved(string port)
		{
			logger?.Info($"Port Removed: {port}");

			SerialPortComboBox.Dispatcher.Invoke(
				() =>
				{
					SerialPortComboBox.Items.Remove(port);

					if (SerialPortComboBox.Items.Count == 0)
					{
						SerialPortComboBox.SelectedIndex = -1;
						portSettings.ComPort = "";
					}
				});

			ValidateState();
		}
		#endregion
		#region GUI Events - Clicks
		private void OnPortSettingsClicked(object sender, RoutedEventArgs e)
		{
			try
			{
				Window alreadyOpened = null!;

				foreach (Window window in Application.Current.Windows)
				{
					string ns = window.GetType().ToString();

					if (ns == $"DataAcquisition.Windows.PortSettingsWindow")
					{
						alreadyOpened = window;
						break;
					}
				}

				if (alreadyOpened == null)
				{
					(new PortSettingsWindow(this, operationState, portSettings ?? new PortSettings(), db, logger)).Show();
				}
				else
				{
					alreadyOpened.Focus();
				}
			}
			catch (Exception ex)
			{
				logger?.Error("Error thrown during launching of PortSettingsWindow", ex.Message);
				MessageBox.Show(ex.Message, "Error");
			}
		}

		private void OnManageExperimentsClicked(object sender, RoutedEventArgs e)
		{
			Window alreadyOpened = null!;

			foreach (Window window in Application.Current.Windows)
			{
				string ns = window.GetType().ToString();

				if (ns == $"DataAcquisition.Windows.ExperimentsManagementWindow")
				{
					alreadyOpened = window;
					break;
				}
			}

			if (alreadyOpened == null)
			{
				(new ExperimentsManagementWindow(this, portSettings, db)).Show();
			}
			else
			{
				alreadyOpened.Focus();
			}
		}

		private void OnUpdatePasswordClicked(object sender, RoutedEventArgs e)
		{
			Window alreadyOpened = null!;

			foreach (Window window in Application.Current.Windows)
			{
				string ns = window.GetType().ToString();

				if (ns == $"DataAcquisition.Windows.PasswordUpdateWindow")
				{
					alreadyOpened = window;
					break;
				}
			}

			if (alreadyOpened == null)
			{
				(new PasswordUpdateWindow(db)).Show();
			}
			else
			{
				alreadyOpened.Focus();
			}
		}

		private void About_Click(object sender, RoutedEventArgs e)
		{
			(new AboutWindow()).ShowDialog();
		}

		//- 
		//- Start / Stop Button
		//- 
		private void StartStopButtonClicked(object sender, RoutedEventArgs e)
		{
			if (operationState == OperationState.Ready)
			{
				SetUiOperationState(OperationState.Started);
			}
			else
			{
				SetUiOperationState(OperationState.Ready);

				// foreach (Window window in Application.Current.Windows)
				// {
				// 	string ns = window.GetType().ToString();
				// 
				// 	if (ns == "DataAcquisition.Windows.PlotWindow")
				// 	{
				// 		window.Close();
				// 	}
				// }
			}
		}

		//- 
		//- Plot
		//- 
		private void OnPlotButtonClocked(object sender, RoutedEventArgs e)
		{
			int tag = (int)((Button)sender).Tag;

			if (tag >= 1 && tag <= 16)
			{
				Window alreadyOpened = null!;

				foreach (Window window in Application.Current.Windows)
				{
					string ns = window.GetType().ToString();

					if (window.Tag is int windowTag)
					{
						if (ns == $"DataAcquisition.Windows.PlotWindow" &&
							windowTag == tag)
						{
							alreadyOpened = window;
							break;
						}
					}
				}

				if (alreadyOpened == null)
				{
					PlotWindow newPlot = new PlotWindow(tag);
					newPlot.Tag = tag;
					newPlot.Show();
				}
				else
				{
					alreadyOpened.Focus();
				}
			}
		}
		#endregion
		#region GUI Events - Selections
		private void OnSerialPortSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (SerialPortComboBox.SelectedIndex >= 0)
			{
				portSettings.ComPort = (string)SerialPortComboBox.Items[SerialPortComboBox.SelectedIndex];
			}
			else
			{
				portSettings.ComPort = "";
			}
		}

		private void DeviceAddressTextChanged(object sender, TextChangedEventArgs e)
		{
			if (DeviceAddress.Text.IsValidDeviceAddress() == false)
			{
				DeviceAddress.Background = new SolidColorBrush(Constants.TextBoxErrorBgColor);
			}
			else
			{
				DeviceAddress.Background = deviceAddressInitialBG;
			}

			ValidateState();
		}

		private void OnExperimentsListViewItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			TreeViewItem item = (TreeViewItem)ExperimentsListView.SelectedItem;

			if (item != null)
			{
				if (item.Tag is Experiment ex)
				{
					Experiment experiment = db.ReadExperiment(ex.Id);

					UpdateExperimentView(experiment);
				}
			}
		}

		private void CheckResultsButtonClick(object sender, RoutedEventArgs e)
		{
			string result = "";

			if (operationState != OperationState.Started)
			{
				result = "Acquisition is not started.";
			}
			else if (selectedExperiment == null)
			{
				result = "No experiment is selected yet.";
			}
			else
			{
				result = "";

				for (int i = 0; i < valueLabels.Count; i++)
				{
					if (valueLabels[i].IsVisible && valueLabels[i].IsEnabled)
					{
						if (double.TryParse((string)valueLabels[i].Content, out double val))
						{
							string rangeStr = $"[{selectedExperiment.InputMin[i]}, {selectedExperiment.InputMax[i]}]";

							if (val >= selectedExperiment.InputMin[i] && val <= selectedExperiment.InputMax[i])
							{
								result += $"A{i + 1}: {val:0.000} is within range {rangeStr}\r\n";
							}
							else
							{
								result += $"A{i + 1}: {val:0.000} is out of range {rangeStr}\r\n";

								if (!string.IsNullOrEmpty(selectedExperiment.ErrorMessages[i]))
								{
									foreach (string line in selectedExperiment.ErrorMessages[i].Split("\r\n"))
									{
										result += $"    | {line}\r\n";
									}
								}
							}
						}
					}
				}
			}

			(new ResultsWindow(result)).Show();
		}

		private void SaveCSVButtonClick(object sender, RoutedEventArgs e)
		{
			if (operationState != OperationState.Started)
			{
				MessageBox.Show("Acquisition is not started yet.", "Cannot save CSV");
			}
			else if (selectedExperiment == null)
			{
				MessageBox.Show("No experiment is selected yet.", "Cannot save CSV");
			}
			else
			{
				SaveFileDialog csvDialog = new Microsoft.Win32.SaveFileDialog()
				{
					Title = "Save CSV",
					Filter = "CSV files |*.csv"
				};

				if (csvDialog.ShowDialog() != false)
				{
					string filename = csvDialog.FileName;
					string text = "";

					//- Titles
					for (int i = 0; i < valueLabels.Count; i++)
					{
						if (valueLabels[i].IsVisible && valueLabels[i].IsEnabled)
						{
							string rangeStr = $"[{selectedExperiment.InputMin[i]} - {selectedExperiment.InputMax[i]}]";

							text += $"A{i + 1} {rangeStr}";

							// Not the last one
							if (i != valueLabels.Count - 1)
							{
								text += $", ";
							}
						}
					}

					text += "\r\n";

					for (int i = 0; i < valueLabels.Count; i++)
					{
						if (valueLabels[i].IsVisible && valueLabels[i].IsEnabled)
						{
							if (double.TryParse((string)valueLabels[i].Content, out double val))
							{
								text += $"{val}";
							}

							// Not the last one
							if (i != valueLabels.Count - 1)
							{
								text += $", ";
							}
						}
					}

					try
					{
						File.WriteAllText(filename, text);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Error: {ex.Message}", "Cannot save file");
					}
				}
			}
		}
		#endregion
		#region GUI Events - Window Resizing
		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (applicationSettings != null)
			{
				applicationSettings.MainWindowWidth = Width;
				applicationSettings.MainWindowHeight = Height;
			}
		}

		private void OnStateChanged(object sender, EventArgs e)
		{
			if (applicationSettings != null)
			{
				applicationSettings.MainWindowIsFullScreen = WindowState == WindowState.Maximized;
			}
		}

		private void OnLocationChanged(object sender, EventArgs e)
		{
			if (applicationSettings != null)
			{
				applicationSettings.MainWindowX = Left;
				applicationSettings.MainWindowY = Top;
			}
		}
		#endregion
		#region GUI Events - Window Loading / Closing
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.logger?.Info("Window loaded");
		}

		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			StopAcquisition();

			portsDiscoverer.StopPortsDiscovery();

			if (int.TryParse(DeviceAddress.Text.Trim(), out int v))
			{
				portSettings.DeviceAddress = v;
			}

			ApplicationSettingsRW.WriteApplicationSettings(applicationSettings, logger);
			PortSettingsRW.WritePortSettings(portSettings, logger);

			this.logger?.Info("Window closing");

			Application.Current.Shutdown();
		}
		#endregion
		#region Start / Stop / Acquisition

		private Services.DataAcquisition dataAcquisition = null!;

		private void StartAcquisition()
		{
			if (SerialPortComboBox.SelectedIndex >= 0)
			{
				connectedCom = (string)SerialPortComboBox.Items[SerialPortComboBox.SelectedIndex];
			}

			if (!string.IsNullOrWhiteSpace(connectedCom) && DeviceAddress.Text.IsValidDeviceAddress())
			{
				for (int i = 0; i < valueLabels.Count; i++)
				{
					valueLabels[i].Content = "0.000";
				}

				if (int.TryParse(DeviceAddress.Text.Trim(), out int v))
				{
					portSettings.DeviceAddress = v;
				}

				if (dataAcquisition != null)
				{
					StopAcquisition();
				}

				dataAcquisition = new Services.DataAcquisition(connectedCom, portSettings, logger!);
				dataAcquisition.NewAcquiredValuesProcessed += OnDataAcquiredAfterProcessing;

				try
				{
					dataAcquisition.StartAcquisition();
				}
				catch (Exception ex)
				{
					logger?.Error($"Error occurred while starting acquisition from {connectedCom}", ex.Message);
					MessageBox.Show(ex.Message, "Error");
				}
			}
		}

		private void StopAcquisition()
		{
			if (dataAcquisition != null)
			{
				dataAcquisition.StopAcquisition();
				dataAcquisition.Dispose();
				dataAcquisition = null!;
			}
		}

		private void OnDataAcquiredAfterProcessing(List<double> ain)
		{
			Dispatcher.Invoke(new Action(() =>
			{
				Experiment e;

				lock (this)
				{
					e = selectedExperiment;
				}

				for (int i = 0; i < ain.Count; i++)
				{
					if (valueLabels[i].IsVisible && valueLabels[i].IsEnabled)
					{
						valueLabels[i].Content = ain[i].ToString("0.000");

						if (e != null)
						{
							if (ain[i] >= e.InputMin[i] && ain[i] <= e.InputMax[i])
							{
								valueLabels[i].Foreground = Constants.ValueInRangeTextColorBrush;
							}
							else
							{
								valueLabels[i].Foreground = Constants.ValueOutOfRangeTextColorBrush;
							}
						}
						else
						{
							valueLabels[i].Foreground = valueLabelsInitialFG;
						}
					}
				}

				foreach (Window window in Application.Current.Windows)
				{
					string ns = window.GetType().ToString();

					if (window.Tag is int windowTag)
					{
						if (ns == $"DataAcquisition.Windows.PlotWindow")
						{
							try
							{
								PlotWindow plotWindow = (PlotWindow)window;
								plotWindow.NewDataPointsReceived(ain);
							}
							catch (Exception) { }
						}
					}
				}
			}));
		}
		#endregion
	}
}
