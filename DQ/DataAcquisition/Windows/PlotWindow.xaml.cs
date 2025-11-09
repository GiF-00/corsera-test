using DataAcquisition.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace DataAcquisition.Windows
{
	/// <summary>
	/// Interaction logic for PlotWindow.xaml
	/// </summary>
	public partial class PlotWindow : Window
	{
		private readonly int channel;
		private int channel2 = -1;

		private bool isDumping = false;
		private string dumpFilename = "";
		private string dumpPath = "";
		private StreamWriter dumpFile = null!;

		private Brush initialYAxisMinBrush;
		private Brush initialYAxisMaxBrush;

		private readonly string dumpButtonInitialText;

		public PlotWindow(int channel)
		{
			InitializeComponent();

			Title = Title + $" Channel {channel}";

			dumpButtonInitialText = (string)DumpButton.Content;

			dumpPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

			//- 
			//- Initial colors
			//- 
			initialYAxisMinBrush = YAxisMin.Background;
			initialYAxisMaxBrush = YAxisMax.Background;

			//- 
			//- Graph settings
			//- 

			XAxisStep.Items.Add((double)0.5);
			XAxisStep.Items.Add((double)1);
			XAxisStep.Items.Add((double)2.5);
			XAxisStep.Items.Add((double)5);
			XAxisStep.SelectedIndex = 1;

			YAxisStep.Items.Add((double)1);
			YAxisStep.Items.Add((double)2.5);
			YAxisStep.Items.Add((double)5);
			YAxisStep.Items.Add((double)10);
			YAxisStep.SelectedIndex = 1;

			YAxisMin.Text = $"{0}";
			YAxisMax.Text = $"{10}";

			Plot.YAxisMin = 0;
			Plot.YAxisMax = 10;

			//- 
			//- Second channel
			//- 

			SecondChannel.Items.Add($"None");

			for (int i = 0; i < 16; i++)
			{
				SecondChannel.Items.Add($"A{i + 1}");
			}

			SecondChannel.SelectedIndex = 0;

			this.channel = channel;
		}

		public void NewDataPointsReceived(List<double> values)
		{
			if (channel2 >= 1 && channel2 <= 16)
			{
				Plot.Update(values[channel - 1], values[channel2 - 1]);
			}
			else
			{
				Plot.Update(values[channel - 1], 0);
			}

			try
			{
				if (dumpFile != null)
				{
					if (channel2 >= 1 && channel2 <= 16)
					{
						dumpFile.WriteLine($"{DateTime.Now:HH:mm:ss.fff},{values[channel - 1]},{values[channel2 - 1]}");
					}
					else
					{
						dumpFile.WriteLine($"{DateTime.Now:HH:mm:ss.fff},{values[channel - 1]}");
					}

					dumpFile.Flush();
				}
			}
			catch (Exception) { }
		}

		private void DumpButton_Click(object sender, RoutedEventArgs e)
		{
			if (isDumping)
			{
				//- Already started

				isDumping = false;

				try
				{
					if (dumpFile != null)
					{
						dumpFile.Flush();
						dumpFile.Close();
						dumpFile.Dispose();
						dumpFile = null!;
					}
				}
				catch (Exception) { }

				if (dumpFilename != "")
				{
					DumpMessage.Content = $"Written to file: {dumpFilename}";
				}

				DumpButton.Content = dumpButtonInitialText;
			}
			else
			{
				//- Starting dumping

				isDumping = true;

				dumpFilename = GetDumpFilename();

				try
				{
					if (dumpFile != null)
					{
						dumpFile.Flush();
						dumpFile.Close();
						dumpFile.Dispose();
						dumpFile = null!;
					}

					dumpFile = new StreamWriter(new FileStream(dumpFilename, FileMode.OpenOrCreate));
					dumpFile.WriteLine("Time,Value");

					DumpMessage.Content = $"Writing to file: {dumpFilename}";
				}
				catch (Exception) { }

				DumpButton.Content = "Stop";
			}
		}

		private string GetDumpFilename()
		{
			DateTime now = DateTime.Now;
			dumpFilename = Path.Combine(dumpPath, $"Channel {channel} - {now:yyyyMMdd HHmmss}.csv");

			return dumpFilename;
		}

		private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				if (dumpFile != null)
				{
					dumpFile.Flush();
					dumpFile.Close();
					dumpFile.Dispose();
					dumpFile = null!;
				}
			}
			catch (Exception) { }
		}

		private void XAxisStep_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			try
			{
				double x = (double)XAxisStep.SelectedItem;
				Plot.XAxisStep = TimeSpan.FromSeconds(x).Ticks;
			}
			catch { }
		}

		private void YAxisStep_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			try
			{
				double y = (double)YAxisStep.SelectedItem;
				Plot.YAxisStep = y;
			}
			catch { }
		}

		private void OnYAxisMinTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			if (double.TryParse(YAxisMin.Text, out double val))
			{
				YAxisMin.Background = initialYAxisMinBrush;

				Plot.YAxisMin = val;
			}
			else
			{
				YAxisMin.Background = new SolidColorBrush(Constants.TextBoxErrorBgColor);
			}
		}

		private void OnYAxisMaxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			if (double.TryParse(YAxisMax.Text, out double val))
			{
				YAxisMax.Background = initialYAxisMaxBrush;

				Plot.YAxisMax = val;
			}
			else
			{
				YAxisMax.Background = new SolidColorBrush(Constants.TextBoxErrorBgColor);
			}
		}

		private void SecondChannel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			string sel = (string)SecondChannel.SelectedItem;

			if (!string.IsNullOrEmpty(sel))
			{
				if (sel == "None")
				{
					channel2 = -1;
					Plot.DisableSecondChannel();
				}
				else
				{
					sel = sel.Replace("A", "").Trim();

					if (int.TryParse(sel, out int val))
					{
						if (val != channel2)
						{
							channel2 = val;
							Plot.ClearSecondChannel();
							Plot.EnableSecondChannel();
						}
					}
				}
			}
		}
	}
}
