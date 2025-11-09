using DataAcquisition.Entities;
using DataAcquisition.Extensions;
using DataAcquisition.Services;
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
	/// <summary>
	/// Interaction logic for PasswordUpdateWindow.xaml
	/// </summary>
	public partial class PasswordUpdateWindow : Window
	{
		private readonly ExperimentsDb db;

		private bool isPasswordVerified = false;
		private DateTime passwordValidationTime = DateTime.MinValue;

		public PasswordUpdateWindow(ExperimentsDb db)
		{
			InitializeComponent();

			this.db = db;

			TakeMasterPasswordInput();
		}

		private void OnOkButtonClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void OnCloseButtonClick(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void TakeMasterPasswordInput()
		{
			isPasswordVerified = false;
			PasswordView.Visibility = Visibility.Visible;
			PasswordText.Focus();
			PasswordText.Password = "";
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

		private void OnPasswordChangeProceedClicked(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(CurrentPasswordText.Password))
			{
				PasswordChangeValidation.Visibility = Visibility.Visible;
				PasswordChangeValidation.Text = "Invalid current password";
			}
			else if (string.IsNullOrWhiteSpace(NewPasswordText.Password))
			{
				PasswordChangeValidation.Visibility = Visibility.Visible;
				PasswordChangeValidation.Text = "Invalid new password";
			}
			else if (NewPasswordText.Password != NewPasswordVerifiedText.Password)
			{
				PasswordChangeValidation.Visibility = Visibility.Visible;
				PasswordChangeValidation.Text = "New password texts do not match";
			}
			else
			{
				if (CurrentPasswordText.Password.Hash() == db.ReadPassword())
				{
					db.UpdatePassword(NewPasswordText.Password.Hash());

					MessageBox.Show("Password updated", "Password Update");

					CurrentPasswordText.Password = null;
					NewPasswordText.Password = null;
					NewPasswordVerifiedText.Password = null;
					PasswordChangeValidation.Visibility = Visibility.Collapsed;

					TakeMasterPasswordInput();
				}
				else
				{
					PasswordChangeValidation.Visibility = Visibility.Visible;
					PasswordChangeValidation.Text = "Invalid current password";
				}
			}
		}

		private void OnPasswordChangeBoxesKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				OnPasswordChangeProceedClicked(sender, null!);
			}
			else
			{
				PasswordChangeValidation.Visibility = Visibility.Collapsed;
				PasswordChangeValidation.Text = "";
			}
		}
	}
}
