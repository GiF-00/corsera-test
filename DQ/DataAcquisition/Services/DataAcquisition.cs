using DataAcquisition.Entities;
using DataAcquisition.Logger;
using DataAcquisition.Settings;
using FluentModbus;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Security.Policy;
using System.Threading;

namespace DataAcquisition.Services
{
	public class DataAcquisition : IDisposable
	{
		public event Action<List<double>>? NewAcquiredValuesProcessed;

		private bool _isRunning = false;
		private Timer _timer;

		private readonly string comPort;
		private readonly ILogger logger;

		private  PortSettings portSettings;

		private ModbusRtuClient client;
		private int deviceAddress;
		private int startAddress;
		private const int regsCount = 16;
		private List<double> valuesReceived = new List<double>() {
			0.0, 0.0, 0.0, 0.0,
			0.0, 0.0, 0.0, 0.0,
			0.0, 0.0, 0.0, 0.0,
			0.0, 0.0, 0.0, 0.0,
		};

		public DataAcquisition(string comPort, PortSettings portSettings, ILogger logger)
		{
			_isRunning = false;
			_timer = new Timer(AcquireData, null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

			this.comPort = comPort;
			this.portSettings = portSettings;
			this.logger = logger;

			System.IO.Ports.Parity parity = System.IO.Ports.Parity.None;
			System.IO.Ports.StopBits stopBits = System.IO.Ports.StopBits.None;

			switch (portSettings.Parity)
			{
				case Entities.Parity.None:
					parity = System.IO.Ports.Parity.None;
					break;

				case Entities.Parity.Odd:
					parity = System.IO.Ports.Parity.Odd;
					break;

				case Entities.Parity.Even:
					parity = System.IO.Ports.Parity.Even;
					break;
			}

			switch (portSettings.StopBits)
			{
				case Entities.StopBits.One:
					stopBits = System.IO.Ports.StopBits.One;
					break;

				case Entities.StopBits.Two:
					stopBits = System.IO.Ports.StopBits.Two;
					break;

				case Entities.StopBits.OnePointFive:
					stopBits = System.IO.Ports.StopBits.OnePointFive;
					break;
			}

			client = new ModbusRtuClient()
			{
				BaudRate = (int)portSettings.BaudRate,
				Parity = parity,
				StopBits = stopBits
			};

			deviceAddress = portSettings.DeviceAddress;
			startAddress = Convert.ToInt32(portSettings.RegistersStartAddress, 16);
		}

		public void UpdatePortSettings(PortSettings portSettings)
		{
			lock (this)
			{
				this.portSettings = portSettings;
			}
		}

		#region Start / Stop Acquisition
		public void StartAcquisition()
		{
			logger.Info($"Starting data acquisition from {comPort} @{Constants.DataAcquisitionIntervalMS}ms interval");

			client.Connect(comPort, Constants.ModbusEndianness);

			_isRunning = true;
			_timer.Change(
				TimeSpan.FromMilliseconds(Constants.DataAcquisitionIntervalMS),
				TimeSpan.FromMilliseconds(Constants.DataAcquisitionIntervalMS));
		}

		public void StopAcquisition()
		{
			logger.Info($"Stopping data acquisition from {comPort}");

			_isRunning = false;
			_timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
		}
		#endregion

		#region Acquire Data
		private void AcquireData(object? obj)
		{
			_timer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));

			short[] shortData = null!;

			try
			{
				shortData = client.ReadHoldingRegisters<short>(deviceAddress, startAddress, regsCount).ToArray();
			}
			catch (Exception ex)
			{
				logger.Error($"Reading error at {comPort}", ex.Message);
			}

			if (shortData != null && shortData.Length <= 16 && shortData.Length >= 1)
			{
				PortSettings ps;

				lock (this)
				{
					ps = portSettings;
				}

				for (int i = 0; i < shortData.Length; i++)
				{
					valuesReceived[i] = ((double)shortData[i]) / 1000.0 * ps.MultiplicationFactors[i] + ps.Offsets[i];
				}

				NewAcquiredValuesProcessed?.Invoke(valuesReceived);
			}

			if (_isRunning)
			{
				_timer.Change(
					TimeSpan.FromMilliseconds(Constants.DataAcquisitionIntervalMS),
					TimeSpan.FromMilliseconds(Constants.DataAcquisitionIntervalMS));
			}
		}

		public void Dispose()
		{
			client.Dispose();
		}
		#endregion
	}
}
