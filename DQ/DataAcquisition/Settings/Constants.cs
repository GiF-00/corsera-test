using DataAcquisition.Logger;
using FluentModbus;
using System;
using System.Windows.Media;
using System.IO;

namespace DataAcquisition.Settings
{
	public static class Constants
	{
		// Password
		public static readonly string DefaultUsername = "Admin";
		public static readonly string DefaultPassword = "Admin";

		// Default ID
		public static readonly int UnassignedId = -1;

		// Endianness
		public static readonly ModbusEndianness ModbusEndianness = ModbusEndianness.BigEndian;

		// Time Constants
		public static readonly double PortDiscoveryIntervalMS = 100;
		public static readonly double DataAcquisitionIntervalMS = 250;

		// Log-level
		public static readonly LogLevel LogLevel = LogLevel.Information;

		// Colors
		public static readonly Color TextBoxErrorBgColor = Color.FromArgb(255, 255, 180, 180);

		public static readonly Color StartButtonBgColor = Color.FromArgb(70, 211, 254, 65);
		public static readonly Color StopButtonBgColor = Color.FromArgb(70, 255, 108, 108);

		public static readonly Color ValueInRangeTextColor = Color.FromArgb(200, 72, 249, 13);
		public static readonly Color ValueOutOfRangeTextColor = Color.FromArgb(200, 244, 64, 64);
		public static readonly SolidColorBrush ValueInRangeTextColorBrush = new SolidColorBrush(ValueInRangeTextColor);
		public static readonly SolidColorBrush ValueOutOfRangeTextColorBrush = new SolidColorBrush(ValueOutOfRangeTextColor);

		// Paths
		private static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

		public static readonly string DataFolder = Path.Combine(AppDataPath, "DataAcquisition");

		public static readonly string LogFolder = Path.Combine(DataFolder, "Log");
		public static readonly string ExperimentsDataFolder = Path.Combine(DataFolder, "Experiments");
		public static readonly string ExperimentImagesFolder = Path.Combine(ExperimentsDataFolder, "Images");

		public static readonly string ApplicationSettingsFile = Path.Combine(DataFolder, "ApplicationSettings.ini");
		public static readonly string PortSettingsFile = Path.Combine(DataFolder, "PortSettings.ini");

		public static readonly string DbFilePath = Path.Combine(ExperimentsDataFolder, "Experiments.db");
		public static readonly string DbErrorsFilePath = Path.Combine(ExperimentsDataFolder, "Errors.db");

		public static readonly string DbConnectionString = $"Data Source={DbFilePath};";
		public static readonly string DbErrorsConnectionString = $"Data Source={DbErrorsFilePath};";
	}
}
