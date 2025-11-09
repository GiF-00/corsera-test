using DataAcquisition.Logger;
using DataAcquisition.Logger.DefaultLogger;
using DataAcquisition.Settings;
using DataAcquisition.Windows;
using System;
using System.IO;
using System.Windows;

namespace DataAcquisition
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private ILogger? logger = null;

		public App()
		{
			// Creating directories for data, if not existing already.

			Directory.CreateDirectory(Constants.DataFolder);
			Directory.CreateDirectory(Constants.LogFolder);
			Directory.CreateDirectory(Constants.ExperimentsDataFolder);

			// Initializing ILogger
			logger = new FileLogger(
				Path.Combine(Constants.LogFolder, $"{DateTime.Now:yyyyMMdd-hhmmss}.log"),
				Constants.LogLevel,
				timestamp: true);

			// Launching MainWindow
			try
			{
				MainWindow = new MainWindow(logger);
				MainWindow.Show();
			}
			catch (Exception ex)
			{
				logger.Error("Exception thrown by the MainWindow:", ex.Message);
				logger.Dispose();
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			logger?.Dispose();
			base.OnExit(e);
		}
	}
}
