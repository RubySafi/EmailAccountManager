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

namespace EmailAccountManager
{
    /// <summary>
    /// EditAccountWindow.xaml
    /// </summary>
    public partial class EditAccountWindow : Window
    {
        private AppSetting appSetting;
        string accountNameInitial;
        string accountNameCurrent;

        public EditAccountWindow(AppSetting appSetting, string accountNameInitial)
        {
            InitializeComponent();
            this.appSetting = appSetting;
            this.accountNameInitial = accountNameInitial;
            this.accountNameCurrent = accountNameInitial;

            UserNameTextBox.Text = accountNameInitial;
        }
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UserNameTextBox.Text.Trim();

            if (username == accountNameInitial)
            {
                ErrorMessageTextBlock.Text = "Username does not changed.";
                return;
            }

            if (string.IsNullOrEmpty(username))
            {
                ErrorMessageTextBlock.Text = "Username cannot be empty.";
                return;
            }

            if (appSetting.UserNames.Contains(username))
            {
                ErrorMessageTextBlock.Text = "Username already exists.";
                return;
            }

            if (IsInvalidUsername(username))
            {
                ErrorMessageTextBlock.Text = "Username contains invalid characters.";
                return;
            }

            ErrorMessageTextBlock.Text = "";
            this.accountNameCurrent = username;
            DialogResult = true;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
            {
                // Prevent focus movement by arrow keys
                e.Handled = true;
            }
        }
        private bool IsInvalidUsername(string username)
        {

            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                if (username.Contains(c))
                {
                    return true;
                }
            }

            return false;
        }


        public string GetNewAccount()
        {
            return accountNameCurrent;
        }
    }
}
