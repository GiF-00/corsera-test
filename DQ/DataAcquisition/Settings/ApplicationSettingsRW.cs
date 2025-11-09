using DataAcquisition.Entities;
using DataAcquisition.Logger;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAcquisition.Settings
{
    public static class ApplicationSettingsRW
	{
        private static IniConfigRW? ini = null;

		/*****************
		 * 
		 * Reading ApplicationSettings
		 * 
		 ***/
		public static ApplicationSettings ReadApplicationSettings(ILogger? logger)
        {
            OpenFileIfNotOpened(logger);

			ApplicationSettings applicationSettings = new()
			{
				MainWindowIsFullScreen = false,
				MainWindowX = 50,
				MainWindowY = 50,
				MainWindowWidth = 500,
				MainWindowHeight = 500,
			};

			if (ini != null)
			{
				applicationSettings.MainWindowIsFullScreen = ini.GetInteger("MainWindow/IsFullScreen", 1) != 0;
				applicationSettings.MainWindowX = ini.GetDouble("MainWindow/X", 50);
				applicationSettings.MainWindowY = ini.GetDouble("MainWindow/Y", 50);
				applicationSettings.MainWindowWidth = ini.GetDouble("MainWindow/Width", 500);
				applicationSettings.MainWindowHeight = ini.GetDouble("MainWindow/Height", 400);
			}

			return applicationSettings;
		}

		/*****************
		 * 
		 * Writing ApplicationSettings
		 * 
		 ***/
		public static void WriteApplicationSettings(ApplicationSettings? applicationSettings, ILogger? logger)
        {
			OpenFileIfNotOpened(logger);

			if (applicationSettings != null)
			{
				if (ini != null)
				{
					ini.SetDouble("MainWindow/IsFullScreen", applicationSettings.MainWindowIsFullScreen ? 1 : 0);
					ini.SetDouble("MainWindow/X", applicationSettings.MainWindowX);
					ini.SetDouble("MainWindow/Y", applicationSettings.MainWindowY);
					ini.SetDouble("MainWindow/Width", applicationSettings.MainWindowWidth);
					ini.SetDouble("MainWindow/Height", applicationSettings.MainWindowHeight);
				}
			}
			else
			{
				logger?.Warning("While saving Application Settings, null object is provided");
			}

			SaveFileIfOpened();
		}


		/*****************
		 * 
		 * Internal
		 * 
		 ***/
		private static void OpenFileIfNotOpened(ILogger? logger)
		{
			if (ini == null)
			{
				ini = new IniConfigRW(Constants.ApplicationSettingsFile, logger);
			}
		}

		private static void SaveFileIfOpened()
		{
			if (ini != null)
			{
				ini.SaveFile();
			}
		}
	}
}
