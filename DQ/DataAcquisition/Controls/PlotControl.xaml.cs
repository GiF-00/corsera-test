using LiveCharts.Configurations;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace DataAcquisition.Controls
{
	/// <summary>
	/// The data points describer for plotting
	/// </summary>
	public class PlotModel
	{
		public DateTime DateTime { get; set; }
		public double Value { get; set; }
	}

	/// <summary>
	/// Interaction logic for PlotControl.xaml
	/// </summary>
	public partial class PlotControl : UserControl, INotifyPropertyChanged
	{
		private static readonly int MAX_NUMBER_OF_VALUES = 150;

		private double _xAxisMin;
		private double _xAxisMax;
		private double _xAxisStep;

		private double _yAxisMin;
		private double _yAxisMax;
		private double _yAxisStep;

		private DateTime startTime;

		public PlotControl()
		{
			InitializeComponent();

			startTime = DateTime.Now;

			var mapper = Mappers.Xy<PlotModel>()
				.X(model => model.DateTime.Ticks)
				.Y(model => model.Value);

			Charting.For<PlotModel>(mapper);

			PlotValues1 = new ChartValues<PlotModel>();
			PlotValues2 = new ChartValues<PlotModel>();

			for (int i = 0; i < MAX_NUMBER_OF_VALUES; i++)
			{
				PlotValues1.Add(new PlotModel()
				{
					DateTime = startTime - TimeSpan.FromSeconds(MAX_NUMBER_OF_VALUES - i),
					Value = 0
				});

				PlotValues2.Add(new PlotModel()
				{
					DateTime = startTime - TimeSpan.FromSeconds(MAX_NUMBER_OF_VALUES - i),
					Value = 0
				});
			}

			TimeFormatter = value => new DateTime((long)value).ToString("mm:ss");
			ValueFormatter = value => value.ToString("0.000");

			XAxisStep = TimeSpan.FromSeconds(1).Ticks;
			XAxisUnit = TimeSpan.TicksPerSecond;

			SetXAxisLimits(startTime);

			YAxisStep = 1;
			SetYAxisLimits(0, 10);

			DataContext = this;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public ChartValues<PlotModel> PlotValues1 { get; set; }
		public ChartValues<PlotModel> PlotValues2 { get; set; }

		public Func<double, string> TimeFormatter { get; set; }
		public Func<double, string> ValueFormatter { get; set; }

		public double XAxisUnit { get; set; }

		public double XAxisMin
		{
			get { return _xAxisMin; }
			set
			{
				_xAxisMin = value;
				OnPropertyChanged("XAxisMin");
			}
		}

		public double XAxisMax
		{
			get { return _xAxisMax; }
			set
			{
				_xAxisMax = value;
				OnPropertyChanged("XAxisMax");
			}
		}

		public double XAxisStep
		{
			get { return _xAxisStep; }
			set
			{
				_xAxisStep = value;
				OnPropertyChanged("XAxisStep");
			}
		}

		public double YAxisMin
		{
			get { return _yAxisMin; }
			set
			{
				_yAxisMin = value;
				OnPropertyChanged("YAxisMin");
			}
		}

		public double YAxisMax
		{
			get { return _yAxisMax; }
			set
			{
				_yAxisMax = value;
				OnPropertyChanged("YAxisMax");
			}
		}

		public double YAxisStep
		{
			get { return _yAxisStep; }
			set
			{
				_yAxisStep = value;
				OnPropertyChanged("YAxisStep");
			}
		}

		public void Update(double value1, double value2)
		{
			var now = DateTime.Now;

			PlotValues1.Add(new PlotModel
			{
				DateTime = now,
				Value = value1
			});

			PlotValues2.Add(new PlotModel
			{
				DateTime = now,
				Value = value2
			});

			SetXAxisLimits(now);

			if (PlotValues1.Count > MAX_NUMBER_OF_VALUES) PlotValues1.RemoveAt(0);
			if (PlotValues2.Count > MAX_NUMBER_OF_VALUES) PlotValues2.RemoveAt(0);
		}

		private void SetXAxisLimits(DateTime now)
		{
			XAxisMax = now.Ticks + TimeSpan.FromSeconds(0).Ticks;
			XAxisMin = now.Ticks - TimeSpan.FromSeconds(8).Ticks;
		}

		private void SetYAxisLimits(double min, double max)
		{
			YAxisMax = max;
			YAxisMin = min;
		}

		protected virtual void OnPropertyChanged(string propertyName = null!)
		{
			if (PropertyChanged != null)
				PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		internal void ClearSecondChannel()
		{
			for (int i = 0; i < MAX_NUMBER_OF_VALUES; i++)
			{
				PlotValues2.Add(new PlotModel()
				{
					DateTime = startTime - TimeSpan.FromSeconds(MAX_NUMBER_OF_VALUES - i),
					Value = 0
				});
			}
		}

		internal void EnableSecondChannel()
		{
			for (int i = 0; i < MAX_NUMBER_OF_VALUES; i++)
			{
				PlotValues2.Add(new PlotModel()
				{
					DateTime = startTime - TimeSpan.FromSeconds(MAX_NUMBER_OF_VALUES - i),
					Value = 0
				});
			}

			Series2.Visibility = Visibility.Visible;
		}

		internal void DisableSecondChannel()
		{
			Series2.Visibility = Visibility.Hidden;
		}
	}
}
