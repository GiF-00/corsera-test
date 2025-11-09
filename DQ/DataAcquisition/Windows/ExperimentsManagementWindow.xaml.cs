using DataAcquisition.Entities;
using DataAcquisition.Extensions;
using DataAcquisition.Services;
using DataAcquisition.Settings;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace DataAcquisition.Windows
{
	public enum Operation
	{
		None,
		Add,
		Edit
	}

	/// <summary>
	/// Interaction logic for ExperimentsManagementWindow.xaml
	/// </summary>
	public partial class ExperimentsManagementWindow : Window
	{
		private readonly MainWindow mainWindow;
		private readonly PortSettings portSettings;
		private readonly ExperimentsDb db;

		private bool dataUpdated = false;

		private bool isPasswordVerified = false;
		private DateTime passwordValidationTime = DateTime.MinValue;

		private Operation operation = Operation.None;

		PCB lastPCB = null!;
		Circuit lastCircuit = null!;
		Experiment lastExperiment = null!;

		public ExperimentsManagementWindow(MainWindow mainWindow, PortSettings portSettings, ExperimentsDb db)
		{
			InitializeComponent();

			this.mainWindow = mainWindow;
			this.portSettings = portSettings;
			this.db = db;

			TakeMasterPasswordInput();
		}

		public void ExperimentUpdated(Experiment experiment)
		{
			if (experiment.Id == Constants.UnassignedId)
			{
				db.CreateExperiment(experiment);
			}
			else
			{
				db.UpdateExperiment(experiment);
			}

			dataUpdated = true;
			RefreshDataDisplayed();
		}

		private void RefreshDataDisplayed()
		{
			List<PCB> allPCBs = db.ReadAllPCBs();

			lastPCB = (PCB)PcbSelectionListView.SelectedItem;
			lastCircuit = (Circuit)CircuitSelectionListView.SelectedItem;
			lastExperiment = (Experiment)ExperimentSelectionListView.SelectedItem;

			PcbSelectionListView.Items.Clear();
			CircuitSelectionListView.Items.Clear();
			ExperimentSelectionListView.Items.Clear();

			int index = 0;

			foreach (PCB pcb in allPCBs)
			{
				PcbSelectionListView.Items.Add(pcb);

				if (lastPCB != null && pcb.Id == lastPCB.Id)
				{
					PcbSelectionListView.SelectedIndex = index;
				}

				index++;
			}

			lastPCB = null!;

			DecideExperimentsDataButtonsState();
		}

		#region Master Ok / Close buttons
		private void OnOkButtonClick(object sender, RoutedEventArgs e)
		{
			if (dataUpdated)
			{
				mainWindow.ExperimentsDataUpdated();
				Close();
			}

			Close();
		}

		private void OnCloseButtonClick(object sender, RoutedEventArgs e)
		{
			if (dataUpdated)
			{
				mainWindow.ExperimentsDataUpdated();
				Close();
			}

			Close();
		}

		private void OnWindowClosed(object sender, EventArgs e)
		{
			if (dataUpdated)
			{
				mainWindow.ExperimentsDataUpdated();
				Close();
			}

			Close();
		}
		#endregion
		#region Overlay views display (hide / show)
		private void TakeMasterPasswordInput()
		{
			isPasswordVerified = false;
			PasswordView.Visibility = Visibility.Visible;
			PasswordText.Focus();
			PasswordValidation.Text = string.Empty;
		}

		private void TakePCBInput(string initialText = "")
		{
			PCBView.Visibility = Visibility.Visible;
			PCBText.Focus();
			PCBText.Text = initialText;
			PCBValidation.Text = "";
		}
		private void CancelPCBInput()
		{
			PCBView.Visibility = Visibility.Hidden;
		}

		private void TakeCircuitInput(string initialText = "")
		{
			CircuitView.Visibility = Visibility.Visible;
			CircuitText.Focus();
			CircuitText.Text = initialText;
			CircuitValidation.Text = "";
		}
		private void CancelCircuitInput()
		{
			CircuitView.Visibility = Visibility.Visible;
		}
		#endregion
		#region Password-related events
		//- 
		//- Password Buttons
		//- 

		private void OnPasswordOkButtonClicked(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(PasswordText.Password) &&
				PasswordText.Password.Hash() == db.ReadPassword())
			{
				PasswordValidation.Text = "";
				PasswordView.Visibility = Visibility.Hidden;
				isPasswordVerified = true;
				passwordValidationTime = DateTime.Now;

				RefreshDataDisplayed();
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
		#region PCB view events
		private void OnPCBOkButtonClicked(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(PCBText.Text))
			{
				PCBValidation.Text = "";
				PCBView.Visibility = Visibility.Hidden;

				if (operation == Operation.Add)
				{
					db.CreatePCB(new PCB()
					{
						Title = PCBText.Text.Trim()
					});
				}
				else if (operation == Operation.Edit)
				{
					PCB pcb = (PCB)PcbSelectionListView.SelectedItem;

					if (pcb != null)
					{
						pcb.Title = PCBText.Text.Trim();

						db.UpdatePCB(pcb);
					}
				}

				dataUpdated = true;
				RefreshDataDisplayed();
			}
			else
			{
				PCBValidation.Text = "Invalid name provided.";
				PCBText.Text = null;
			}
		}

		private void OnPCBCancelButtonClicked(object sender, RoutedEventArgs e)
		{
			CancelPCBInput();
		}

		private void OnPCBTextKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				OnPCBOkButtonClicked(sender, null!);
			}
			else
			{
				PCBValidation.Text = "";
			}
		}
		#endregion
		#region Circuit view events
		private void OnCircuitOkButtonClicked(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(CircuitText.Text))
			{
				CircuitValidation.Text = "";
				CircuitView.Visibility = Visibility.Hidden;

				PCB pcb = (PCB)PcbSelectionListView.SelectedItem;

				if (pcb != null)
				{
					if (operation == Operation.Add)
					{
						db.CreateCircuit(new Circuit()
						{
							Title = CircuitText.Text.Trim(),
							PcbId = pcb.Id
						});
					}
					else if (operation == Operation.Edit)
					{
						Circuit cct = (Circuit)CircuitSelectionListView.SelectedItem;

						if (cct != null)
						{
							cct.Title = CircuitText.Text.Trim();
							cct.PcbId = pcb.Id;

							db.UpdateCircuit(cct);
						}
					}
				}

				dataUpdated = true;
				RefreshDataDisplayed();
			}
			else
			{
				CircuitValidation.Text = "Invalid name provided.";
				CircuitText.Text = null;
			}
		}

		private void OnCircuitCancelButtonClicked(object sender, RoutedEventArgs e)
		{
			CancelCircuitInput();
		}

		private void OnCircuitTextKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				OnCircuitOkButtonClicked(sender, null!);
			}
			else
			{
				CircuitValidation.Text = "";
			}
		}
		#endregion
		#region PCBs, Circuits and Experiments List
		private void PcbSelectionListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			PCB pcb = (PCB)PcbSelectionListView.SelectedItem;

			if (pcb != null)
			{
				CircuitSelectionListView.Items.Clear();
				ExperimentSelectionListView.Items.Clear();

				int index = 0;

				foreach (Circuit cct in pcb.Circuits)
				{
					CircuitSelectionListView.Items.Add(cct);

					if (lastCircuit != null && cct.Id == lastCircuit.Id)
					{
						CircuitSelectionListView.SelectedIndex = index;
					}

					index++;
				}

				lastCircuit = null!;
			}

			DecideExperimentsDataButtonsState();
		}
		
		private void CircuitSelectionListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Circuit cct = (Circuit)CircuitSelectionListView.SelectedItem;

			if (cct != null)
			{
				ExperimentSelectionListView.Items.Clear();

				int index = 0;

				foreach (Experiment exp in cct.Experiments)
				{
					ExperimentSelectionListView.Items.Add(exp);

					if (lastExperiment != null && lastExperiment.Id == exp.Id)
					{
						ExperimentSelectionListView.SelectedIndex = index;
					}

					index++;
				}

				lastExperiment = null!;
			}

			DecideExperimentsDataButtonsState();
		}
		
		private void ExperimentSelectionListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			DecideExperimentsDataButtonsState();
		}

		private void DecideExperimentsDataButtonsState()
		{
			PcbEditButton.IsEnabled = false;
			PcbRemoveButton.IsEnabled = false;

			CircuitAddButton.IsEnabled = false;
			CircuitEditButton.IsEnabled = false;
			CircuitRemoveButton.IsEnabled = false;

			ExperimentAddButton.IsEnabled = false;
			ExperimentEditButton.IsEnabled = false;
			ExperimentRemoveButton.IsEnabled = false;

			if (PcbSelectionListView.SelectedItem != null)
			{
				PcbEditButton.IsEnabled = true;
				PcbRemoveButton.IsEnabled = true;
				CircuitAddButton.IsEnabled = true;
			}

			if (CircuitSelectionListView.SelectedItem != null)
			{
				CircuitEditButton.IsEnabled = true;
				CircuitRemoveButton.IsEnabled = true;
				ExperimentAddButton.IsEnabled = true;
			}

			if (ExperimentSelectionListView.SelectedItem != null)
			{
				ExperimentEditButton.IsEnabled = true;
				ExperimentRemoveButton.IsEnabled = true;
			}
		}

		private void PcbAddButton_Click(object sender, RoutedEventArgs e)
		{
			operation = Operation.Add;

			TakePCBInput();
		}

		private void PcbEditButton_Click(object sender, RoutedEventArgs e)
		{
			operation = Operation.Edit;

			PCB pcb = (PCB)PcbSelectionListView.SelectedItem;

			if (pcb != null)
			{
				TakePCBInput(pcb.Title);
			}
		}

		private void PcbRemoveButton_Click(object sender, RoutedEventArgs e)
		{
			operation = Operation.None;

			PCB pcb = (PCB)PcbSelectionListView.SelectedItem;

			if (pcb != null)
			{
				if (MessageBox.Show(
					$"Do you want to delete the PCB \"{pcb.Title}\"?", "Confirm",
					MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
				{
					db.DeletePCB(pcb.Id);

					dataUpdated = true;
					RefreshDataDisplayed();
				}
			}
		}

		private void CircuitAddButton_Click(object sender, RoutedEventArgs e)
		{
			operation = Operation.Add;

			TakeCircuitInput();
		}

		private void CircuitEditButton_Click(object sender, RoutedEventArgs e)
		{
			operation = Operation.Edit;

			Circuit cct = (Circuit)CircuitSelectionListView.SelectedItem;

			if (cct != null)
			{
				TakeCircuitInput(cct.Title);
			}
		}

		private void CircuitRemoveButton_Click(object sender, RoutedEventArgs e)
		{
			operation = Operation.None;

			Circuit cct = (Circuit)CircuitSelectionListView.SelectedItem;

			if (cct != null)
			{
				if (MessageBox.Show(
					$"Do you want to delete the Circuit \"{cct.Title}\"?", "Confirm",
					MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
				{
					db.DeleteCircuit(cct.Id);

					dataUpdated = true;
					RefreshDataDisplayed();
				}
			}
		}

		private void ExperimentAddButton_Click(object sender, RoutedEventArgs e)
		{
			operation = Operation.Add;

			Circuit cct = (Circuit)CircuitSelectionListView.SelectedItem;

			if (cct != null)
			{
				Experiment ex = new Experiment()
				{
					Id = Constants.UnassignedId,
					CircuitId = cct.Id
				};

				(new ExperimentWindow(this, portSettings, ex)).ShowDialog();
			}
		}

		private void ExperimentEditButton_Click(object sender, RoutedEventArgs e)
		{
			operation = Operation.Edit;

			Circuit cct = (Circuit)CircuitSelectionListView.SelectedItem;
			Experiment ex = (Experiment)ExperimentSelectionListView.SelectedItem;

			if (ex != null)
			{
				Experiment experiment = db.ReadExperiment(ex.Id);
				(new ExperimentWindow(this, portSettings, experiment)).ShowDialog();
			}
		}

		private void ExperimentRemoveButton_Click(object sender, RoutedEventArgs e)
		{
			operation = Operation.None;

			Experiment ex = (Experiment)ExperimentSelectionListView.SelectedItem;

			if (ex != null)
			{
				if (MessageBox.Show(
					$"Do you want to delete the Experiment \"{ex.Title}\"?", "Confirm",
					MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
				{
					db.DeleteExperiment(ex.Id);

					dataUpdated = true;
					RefreshDataDisplayed();
				}
			}
		}
		#endregion
	}
}
