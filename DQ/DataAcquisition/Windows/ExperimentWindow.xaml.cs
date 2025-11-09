using DataAcquisition.Entities;
using DataAcquisition.Extensions;
using DataAcquisition.Settings;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DataAcquisition.Windows
{
	/// <summary>
	/// Interaction logic for ExperimentWindow.xaml
	/// </summary>
	public partial class ExperimentWindow : Window
	{
		private List<CheckBox> selCheckBoxes;
		private List<TextBox> minValues;
		private List<TextBox> maxValues;
		private List<Label> unitLabels;
		private List<TextBox> errorMessages;

		private readonly Brush experimentNameInitialBG;
		private readonly Brush minValueInitialBG;
		private readonly Brush maxValueInitialBG;

		private readonly ExperimentsManagementWindow managementWindow;

		private Experiment currentExperiment;

		public ExperimentWindow(ExperimentsManagementWindow managementWindow, PortSettings portSettings, Experiment experiment = null!)
		{
			InitializeComponent();

			if (experiment != null)
			{
				currentExperiment = experiment;

				if (experiment.Id != Constants.UnassignedId)
				{
					Title = $"Edit {experiment.Title}";
				}
			}
			else
			{
				currentExperiment = new Experiment()
				{
					Id = Constants.UnassignedId,
					CircuitId = 1
				};
			}

			//- 
			//- Initial colors
			//- 
			experimentNameInitialBG = ExperimentName.Background;
			minValueInitialBG = MinValueA1.Background;
			maxValueInitialBG = MaxValueA1.Background;

			//- 
			//- Controls' listing
			//- 
			selCheckBoxes = new List<CheckBox>()
			{
				A1SelectionCheckBox,    A2SelectionCheckBox,    A3SelectionCheckBox,    A4SelectionCheckBox,
				A5SelectionCheckBox,    A6SelectionCheckBox,    A7SelectionCheckBox,    A8SelectionCheckBox,
				A9SelectionCheckBox,    A10SelectionCheckBox,   A11SelectionCheckBox,   A12SelectionCheckBox,
				A13SelectionCheckBox,   A14SelectionCheckBox,   A15SelectionCheckBox,   A16SelectionCheckBox
			};
			minValues = new List<TextBox>()
			{
				MinValueA1,    MinValueA2,    MinValueA3,    MinValueA4,
				MinValueA5,    MinValueA6,    MinValueA7,    MinValueA8,
				MinValueA9,    MinValueA10,   MinValueA11,   MinValueA12,
				MinValueA13,   MinValueA14,   MinValueA15,   MinValueA16
			};
			maxValues = new List<TextBox>()
			{
				MaxValueA1,    MaxValueA2,    MaxValueA3,    MaxValueA4,
				MaxValueA5,    MaxValueA6,    MaxValueA7,    MaxValueA8,
				MaxValueA9,    MaxValueA10,   MaxValueA11,   MaxValueA12,
				MaxValueA13,   MaxValueA14,   MaxValueA15,   MaxValueA16
			};
			unitLabels = new List<Label>()
			{
				DisplayUnitA1,    DisplayUnitA2,    DisplayUnitA3,    DisplayUnitA4,
				DisplayUnitA5,    DisplayUnitA6,    DisplayUnitA7,    DisplayUnitA8,
				DisplayUnitA9,    DisplayUnitA10,   DisplayUnitA11,   DisplayUnitA12,
				DisplayUnitA13,   DisplayUnitA14,   DisplayUnitA15,   DisplayUnitA16
			};

			errorMessages = new List<TextBox>()
			{
				A1ErrorMessage,    A2ErrorMessage,    A3ErrorMessage,    A4ErrorMessage,
				A5ErrorMessage,    A6ErrorMessage,    A7ErrorMessage,    A8ErrorMessage,
				A9ErrorMessage,    A10ErrorMessage,   A11ErrorMessage,   A12ErrorMessage,
				A13ErrorMessage,   A14ErrorMessage,   A15ErrorMessage,   A16ErrorMessage,
			};


			//- 
			//- Setting defaults / initials
			//- 
			ExperimentName.Text = currentExperiment.Title;
			ImageFile.Text = currentExperiment.ImagePath;

			if (!string.IsNullOrWhiteSpace(ImageFile.Text))
			{
				BitmapImage bitmap = new BitmapImage(new Uri($"{ImageFile.Text}"));
				Image.Source = bitmap;
			}

			ProcedureTextBox.Text = currentExperiment.Procedure;

			for (int i = 0; i < minValues.Count(); i++)
			{
				// Enabled / Disabled
				selCheckBoxes[i].IsChecked = currentExperiment.InputEnabled[i];

				// Min Value
				minValues[i].Text = currentExperiment.InputMin[i].ToString();
				minValues[i].TextChanged += OnMinValueTextChanged;

				// Max Value
				maxValues[i].Text = currentExperiment.InputMax[i].ToString();
				maxValues[i].TextChanged += OnMaxValueTextChanged;

				// Units
				unitLabels[i].Content = portSettings.DisplayUnits[i];

				// Error Messages
				errorMessages[i].Text = currentExperiment.ErrorMessages[i];
			}

			this.managementWindow = managementWindow;
		}

		private void ExperimentNameTextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(ExperimentName.Text))
			{
				ExperimentName.Background = new SolidColorBrush(Constants.TextBoxErrorBgColor);
			}
			else
			{
				ExperimentName.Background = experimentNameInitialBG;
			}
		}

		private void OnMinValueTextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox t = (TextBox)sender;

			if (t.Text.IsValidMinValue() == false)
			{
				t.Background = new SolidColorBrush(Constants.TextBoxErrorBgColor);
			}
			else
			{
				t.Background = minValueInitialBG;
			}
		}

		private void OnMaxValueTextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox t = (TextBox)sender;

			if (t.Text.IsValidMaxValue() == false)
			{
				t.Background = new SolidColorBrush(Constants.TextBoxErrorBgColor);
			}
			else
			{
				t.Background = minValueInitialBG;
			}
		}

		private void OnImageButtonClicked(object sender, RoutedEventArgs e)
		{
			OpenFileDialog imageDialog = new Microsoft.Win32.OpenFileDialog()
			{
				Filter = "Image files |*.jpeg;*.jpg;*.png;*.gif;*.bmp"
			};

			if (imageDialog.ShowDialog() != false)
			{
				ImageFile.Text = imageDialog.FileName;

				BitmapImage bitmap = new BitmapImage(new Uri($"{ImageFile.Text}"));
				Image.Source = bitmap;
			}
		}

		private bool UpdateExperimentValues()
		{
			//- 
			//- Validation checks
			//- 

			if (string.IsNullOrWhiteSpace(ExperimentName.Text))
			{
				MessageBox.Show("Experiment's name is not provided", "Error");
				return false;
			}

			if (string.IsNullOrWhiteSpace(ImageFile.Text))
			{
				MessageBox.Show("Image file is not provided", "Error");
				return false;
			}

			for (int i = 0; i < minValues.Count; i++)
			{
				if (minValues[i].IsEnabled && minValues[i].Text.IsValidMinValue() == false)
				{
					MessageBox.Show($"Invalid minimum value given for A{i + 1}", "Error");
					return false;
				}

				if (maxValues[i].IsEnabled && maxValues[i].Text.IsValidMaxValue() == false)
				{
					MessageBox.Show($"Invalid maximum value given for A{i + 1}", "Error");
					return false;
				}
			}


			//- 
			//- Filling-in the values for experiment
			//- 
			currentExperiment.Title = ExperimentName.Text;
			currentExperiment.ImagePath = ImageFile.Text;
			currentExperiment.Procedure = ProcedureTextBox.Text;

			for (int i = 0; i < minValues.Count; i++)
			{
				if (minValues[i].IsEnabled)
				{
					currentExperiment.InputEnabled[i] = true;
					currentExperiment.InputMin[i] = double.Parse(minValues[i].Text);
					currentExperiment.InputMax[i] = double.Parse(maxValues[i].Text);
					currentExperiment.ErrorMessages[i] = errorMessages[i].Text;
				}
				else
				{
					currentExperiment.InputEnabled[i] = false;
				}
			}

			return true;
		}

		private void OnOkClick(object sender, RoutedEventArgs e)
		{
			if (UpdateExperimentValues())
			{
				managementWindow.ExperimentUpdated(currentExperiment);
				Close();
			}
		}

		private void OnCancelClick(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
