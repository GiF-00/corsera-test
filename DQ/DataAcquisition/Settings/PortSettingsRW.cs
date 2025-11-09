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
    public static class PortSettingsRW
    {
        private static IniConfigRW? ini = null;

		/*****************
		 * 
		 * Reading PortSettings
		 * 
		 ***/
		public static PortSettings ReadPortSettings(ILogger? logger)
        {
            OpenFileIfNotOpened(logger);

			PortSettings portSettings = new();

			if (ini != null)
			{
				portSettings.ComPort = ini.GetString("COM/ComPort", "");
				portSettings.DeviceAddress = ini.GetInteger("COM/DeviceAddress", 1);

				portSettings.BaudRate = Enum.Parse<BaudRate>($"_{ini.GetString("COM/BaudRate", "115200")}");
				portSettings.DataBits = Enum.Parse<DataBits>($"_{ini.GetString("COM/DataBits", "8")}");
				portSettings.Parity = Enum.Parse<Parity>($"{ini.GetString("COM/Parity", "None")}");
				portSettings.StopBits = Enum.Parse<StopBits>($"{ini.GetString("COM/StopBits", "One")}");

				portSettings.RegistersStartAddress = ini.GetString("REGS/RegistersStartAddress", "0x0020");

				for (int i = 1; i <= portSettings.MultiplicationFactors.Length; i++)
				{
					portSettings.MultiplicationFactors[i - 1] = ini.GetDouble($"REGS/MultiplicationFactorA{i}", 1.00);
				}

				for (int i = 1; i <= portSettings.Offsets.Length; i++)
				{
					portSettings.Offsets[i - 1] = ini.GetDouble($"REGS/OffsetA{i}", 0.0);
				}

				for (int i = 1; i <= portSettings.DisplayUnits.Length; i++)
				{
					portSettings.DisplayUnits[i - 1] = ini.GetString($"REGS/DisplayUnitA{i}", "V");
				}
			}

			return portSettings;
		}

		/*****************
		 * 
		 * Writing PortSettings
		 * 
		 ***/
		public static void WritePortSettings(PortSettings? portSettings, ILogger? logger)
        {
			OpenFileIfNotOpened(logger);

			if (portSettings != null)
			{
				if (ini != null)
				{
					ini.SetString("COM/ComPort", portSettings.ComPort);
					ini.SetInteger("COM/DeviceAddress", portSettings.DeviceAddress);
					ini.SetString("COM/BaudRate", portSettings.BaudRate.ToString().Replace("_", ""));
					ini.SetString("COM/DataBits", portSettings.DataBits.ToString().Replace("_", ""));
					ini.SetString("COM/Parity", portSettings.Parity.ToString());
					ini.SetString("COM/StopBits", portSettings.StopBits.ToString());

					ini.SetString("REGS/RegistersStartAddress", portSettings.RegistersStartAddress);

					for (int i = 1; i <= portSettings.MultiplicationFactors.Length; i++)
					{
						ini.SetDouble($"REGS/MultiplicationFactorA{i}", portSettings.MultiplicationFactors[i - 1]);
					}

					for (int i = 1; i <= portSettings.Offsets.Length; i++)
					{
						ini.SetDouble($"REGS/OffsetA{i}", portSettings.Offsets[i - 1]);
					}

					for (int i = 1; i <= portSettings.DisplayUnits.Length; i++)
					{
						ini.SetString($"REGS/DisplayUnitA{i}", portSettings.DisplayUnits[i - 1]);
					}
				}
			}
			else
			{
				logger?.Warning("While saving Port Settings, null object is provided");
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
				ini = new IniConfigRW(Constants.PortSettingsFile, logger);
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
