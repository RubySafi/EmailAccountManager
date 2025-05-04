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
    /// RegisterAccountWindow.xaml
    /// </summary>
    public partial class RegisterAccountWindow : Window
    {
        private AppSetting appSetting;

        public RegisterAccountWindow(AppSetting appSetting)
        {
            InitializeComponent();
            this.appSetting = appSetting;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UserNameTextBox.Text.Trim();

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

            appSetting.UserNames.Add(username);
            appSetting.DefaultUser = username;

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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

    }
}
