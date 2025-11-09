using DataAcquisition.Entities;
using DataAcquisition.Extensions;
using DataAcquisition.Logger;
using DataAcquisition.Services;
using DataAcquisition.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DataAcquisition.Windows
{
	/// <summary>
	/// Interaction logic for PortSettingsWindow.xaml
	/// </summary>
	public partial class PortSettingsWindow : Window
	{
		private readonly MainWindow mainWindow;
		private readonly OperationState operationState;
		private readonly PortSettings originalPortSettings;
		private readonly ExperimentsDb db;
		private readonly PortSettings updatedPortSettings;
		private readonly ILogger? logger;

		System.Collections.Generic.List<TextBox> mulfactorBoxes;
		System.Collections.Generic.List<TextBox> offsetBoxes;
		System.Collections.Generic.List<TextBox> unitBoxes;

		private readonly Brush registersStartAddressInitialBG;
		private readonly Brush multiplicationFactorsInitialBG;
		private readonly Brush offsetsInitialBG;
		private readonly Brush displayUnitsInitialBG;

		private bool isPasswordVerified = false;
		private DateTime passwordValidationTime = DateTime.MinValue;

		public PortSettingsWindow(
			MainWindow mainWindow,
			OperationState operationState,
			PortSettings portSettings,
			ExperimentsDb db,
			ILogger? logger)
		{
			InitializeComponent();

			this.mainWindow = mainWindow;
			this.operationState = operationState;
			this.originalPortSettings = portSettings;
			this.db = db;
			this.updatedPortSettings = new();
			this.logger = logger;

			mulfactorBoxes = new()
			{
				MultiplicationFactorA1,    MultiplicationFactorA2,    MultiplicationFactorA3,    MultiplicationFactorA4,
				MultiplicationFactorA5,    MultiplicationFactorA6,    MultiplicationFactorA7,    MultiplicationFactorA8,
				MultiplicationFactorA9,    MultiplicationFactorA10,   MultiplicationFactorA11,   MultiplicationFactorA12,
				MultiplicationFactorA13,   MultiplicationFactorA14,   MultiplicationFactorA15,   MultiplicationFactorA16
			};

			offsetBoxes = new()
			{
				OffsetA1,    OffsetA2,    OffsetA3,    OffsetA4,    OffsetA5,    OffsetA6,    OffsetA7,    OffsetA8,
				OffsetA9,    OffsetA10,   OffsetA11,   OffsetA12,   OffsetA13,   OffsetA14,   OffsetA15,   OffsetA16
			};

			unitBoxes = new()
			{
				DisplayUnitA1,    DisplayUnitA2,    DisplayUnitA3,    DisplayUnitA4,
				DisplayUnitA5,    DisplayUnitA6,    DisplayUnitA7,    DisplayUnitA8,
				DisplayUnitA9,    DisplayUnitA10,   DisplayUnitA11,   DisplayUnitA12,
				DisplayUnitA13,   DisplayUnitA14,   DisplayUnitA15,   DisplayUnitA16
			};

			registersStartAddressInitialBG = RegistersStartAddress.Background;
			multiplicationFactorsInitialBG = MultiplicationFactorA1.Background;
			offsetsInitialBG = OffsetA1.Background;
			displayUnitsInitialBG = DisplayUnitA1.Background;

			SetUiBaudRates();
			SetUiDataBits();
			SetUiParity();
			SetUiStopBits();
			SetUiDataValues();

			RegistersStartAddress.TextChanged += RegistersStartAddressTextChanged;

			for (int i = 0; i < mulfactorBoxes.Count; i++)
			{
				mulfactorBoxes[i].TextChanged += MultiplicationFactorTextChanged;
			}

			for (int i = 0; i < offsetBoxes.Count; i++)
			{
				offsetBoxes[i].TextChanged += OffsetTextChanged;
			}

			for (int i = 0; i < unitBoxes.Count; i++)
			{
				unitBoxes[i].TextChanged += DisplayUnitTextChanged;
			}

			TakeMasterPasswordInput();
		}

		#region Password
		private void TakeMasterPasswordInput()
		{
			isPasswordVerified = false;
			PasswordView.Visibility = Visibility.Visible;
			PasswordText.Focus();
			PasswordValidation.Text = string.Empty;
		}

		private void OnPasswordOkButtonClicked(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(PasswordText.Password) &&
				PasswordText.Password.Hash() == db.ReadPassword())
			{
				PasswordValidation.Text = "";
				PasswordView.Visibility = Visibility.Hidden;
				isPasswordVerified = true;
				passwordValidationTime = DateTime.Now;
			}
			else
			{
				PasswordValidation.Text = "Invalid password";
				PasswordText.Password = null;
			}
		}

		private void OnPasswordCancelButtonClicked(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void OnPasswordTextKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				OnPasswordOkButtonClicked(sender, null!);
			}
			else
			{
				PasswordValidation.Text = "";
			}
		}
		#endregion
		#region Initializing the UI
		private void SetUiBaudRates()
		{
			int currentIndex = 0;
			foreach (BaudRate baudRate in Enum.GetValues(typeof(BaudRate)))
			{
				BaudRateComboBox.Items.Add(baudRate.ToString().Replace("_", ""));

				if (baudRate == originalPortSettings.BaudRate)
				{
					BaudRateComboBox.SelectedIndex = currentIndex;
				}

				currentIndex++;
			}

			if (operationState == OperationState.Started)
			{
				BaudRateComboBox.IsEnabled = false;
			}
		}

		private void SetUiDataBits()
		{
			int currentIndex = 0;
			foreach (DataBits dataBits in Enum.GetValues(typeof(DataBits)))
			{
				DataBitsComboBox.Items.Add(dataBits.ToString().Replace("_", ""));

				if (dataBits == originalPortSettings.DataBits)
				{
					DataBitsComboBox.SelectedIndex = currentIndex;
				}

				currentIndex++;
			}

			if (operationState == OperationState.Started)
			{
				DataBitsComboBox.IsEnabled = false;
			}
		}

		private void SetUiParity()
		{
			int currentIndex = 0;
			foreach (Parity parity in Enum.GetValues(typeof(Parity)))
			{
				ParityComboBox.Items.Add(parity.ToString());

				if (parity == originalPortSettings.Parity)
				{
					ParityComboBox.SelectedIndex = currentIndex;
				}

				currentIndex++;
			}

			if (operationState == OperationState.Started)
			{
				ParityComboBox.IsEnabled = false;
			}
		}

		private void SetUiStopBits()
		{
			int currentIndex = 0;
			foreach (StopBits stopBits in Enum.GetValues(typeof(StopBits)))
			{
				switch (stopBits)
				{
					case StopBits.One:
						StopBitsComboBox.Items.Add("1");
						break;

					case StopBits.Two:
						StopBitsComboBox.Items.Add("2");
						break;

					case StopBits.OnePointFive:
						StopBitsComboBox.Items.Add("1.5");
						break;
				}

				if (stopBits == originalPortSettings.StopBits)
				{
					StopBitsComboBox.SelectedIndex = currentIndex;
				}

				currentIndex++;
			}

			if (operationState == OperationState.Started)
			{
				StopBitsComboBox.IsEnabled = false;
			}
		}

		private void SetUiDataValues()
		{
			RegistersStartAddress.Text = originalPortSettings.RegistersStartAddress;

			for (int i = 0; i < mulfactorBoxes.Count; i++)
			{
				mulfactorBoxes[i].Text = originalPortSettings.MultiplicationFactors[i].ToString();
			}

			for (int i = 0; i < offsetBoxes.Count; i++)
			{
				offsetBoxes[i].Text = originalPortSettings.Offsets[i].ToString();
			}

			for (int i = 0; i < unitBoxes.Count; i++)
			{
				unitBoxes[i].Text = originalPortSettings.DisplayUnits[i];
			}
		}
		#endregion
		#region UI Events
		private void RegistersStartAddressTextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(RegistersStartAddress.Text) || !RegistersStartAddress.Text.IsValidHex())
			{
				RegistersStartAddress.Background = new SolidColorBrush(Constants.TextBoxErrorBgColor);
			}
			else
			{
				RegistersStartAddress.Background = registersStartAddressInitialBG;
			}
		}
		private void MultiplicationFactorTextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox t = (TextBox)sender;

			if (t.Text.IsValidMultiplicationFactor() == false)
			{
				t.Background = new SolidColorBrush(Constants.TextBoxErrorBgColor);
			}
			else
			{
				t.Background = multiplicationFactorsInitialBG;
			}
		}
		private void OffsetTextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox t = (TextBox)sender;

			if (t.Text.IsValidOffset() == false)
			{
				t.Background = new SolidColorBrush(Constants.TextBoxErrorBgColor);
			}
			else
			{
				t.Background = offsetsInitialBG;
			}
		}
		private void DisplayUnitTextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox t = (TextBox)sender;

			if (string.IsNullOrWhiteSpace(t.Text))
			{
				t.Background = new SolidColorBrush(Constants.TextBoxErrorBgColor);
			}
			else
			{
				t.Background = displayUnitsInitialBG;
			}
		}

		private void UpdatePortSettingsComm()
		{
			// Baud Rate Selection

			if (BaudRateComboBox.SelectedIndex >= 0)
			{
				string enumStr = $"_{BaudRateComboBox.Items[BaudRateComboBox.SelectedIndex]}";

				if (Enum.IsDefined(typeof(BaudRate), enumStr))
				{
					Enum.TryParse(typeof(BaudRate), enumStr, out object? baudRate);

					if (baudRate != null)
					{
						updatedPortSettings.BaudRate = (BaudRate)baudRate;
					}
				}
			}


			// Data Bits Selection

			if (DataBitsComboBox.SelectedIndex >= 0)
			{
				string enumStr = $"_{DataBitsComboBox.Items[DataBitsComboBox.SelectedIndex]}";

				if (Enum.IsDefined(typeof(DataBits), enumStr))
				{
					Enum.TryParse(typeof(DataBits), enumStr, out object? dataBits);

					if (dataBits != null)
					{
						updatedPortSettings.DataBits = (DataBits)dataBits;
					}
				}
			}


			// Parity Bit Selection

			if (ParityComboBox.SelectedIndex >= 0)
			{
				string enumStr = (string)ParityComboBox.Items[ParityComboBox.SelectedIndex];

				if (Enum.IsDefined(typeof(Parity), enumStr))
				{
					Enum.TryParse(typeof(Parity), enumStr, out object? parity);

					if (parity != null)
					{
						updatedPortSettings.Parity = (Parity)parity;
					}
				}
			}


			// Stop Bits Selection

			if (StopBitsComboBox.SelectedIndex >= 0)
			{
				string enumStr = (string)StopBitsComboBox.Items[StopBitsComboBox.SelectedIndex];

				switch (enumStr)
				{
					case "1":
						enumStr = StopBits.One.ToString();
						break;

					case "2":
						enumStr = StopBits.Two.ToString();
						break;

					case "1.5":
						enumStr = StopBits.OnePointFive.ToString();
						break;
				}

				if (Enum.IsDefined(typeof(StopBits), enumStr))
				{
					Enum.TryParse(typeof(StopBits), enumStr, out object? stopBits);
					if (stopBits != null)
					{
						updatedPortSettings.StopBits = (StopBits)stopBits;
					}
				}
			}
		}
		private bool UpdatePortSettingsDataValues()
		{
			// Registers Start Address

			if (string.IsNullOrWhiteSpace(RegistersStartAddress.Text))
			{
				MessageBox.Show("Empty start address is not allowed", "Error");
				return false;
			}
			else if (!RegistersStartAddress.Text.IsValidHex())
			{
				MessageBox.Show("The start address should be defined in the hexadecimal format like 0x0020", "Error");
				return false;
			}
			else
			{
				updatedPortSettings.RegistersStartAddress = RegistersStartAddress.Text.Trim();
			}


			// Multiplication Factors

			for (int i = 0; i < mulfactorBoxes.Count; i++)
			{
				if (double.TryParse(mulfactorBoxes[i].Text, out double mf))
				{
					updatedPortSettings.MultiplicationFactors[i] = mf;
				}
				else
				{
					MessageBox.Show($"Invalid multiplication factor given for A{i + 1}", "Error");
					return false;
				}
			}


			// Offsets

			for (int i = 0; i < offsetBoxes.Count; i++)
			{
				if (double.TryParse(offsetBoxes[i].Text, out double off))
				{
					updatedPortSettings.Offsets[i] = off;
				}
				else
				{
					MessageBox.Show($"Invalid offset given for A{i + 1}", "Error");
					return false;
				}
			}


			// Display Units

			for (int i = 0; i < unitBoxes.Count; i++)
			{
				if (!string.IsNullOrWhiteSpace(unitBoxes[i].Text))
				{
					updatedPortSettings.DisplayUnits[i] = unitBoxes[i].Text.Trim();
				}
				else
				{
					MessageBox.Show($"Empty display unit given for A{i + 1}", "Error");
					return false;
				}
			}

			return true;
		}

		private void OnOkClick(object sender, RoutedEventArgs e)
		{
			UpdatePortSettingsComm();

			// Inputted values are OK?
			if (UpdatePortSettingsDataValues())
			{
				mainWindow.UpdatePortSettings(updatedPortSettings);
				Close();
			}
		}

		private void OnCancelClick(object sender, RoutedEventArgs e)
		{
			Close();
		}
		#endregion
	}
}
