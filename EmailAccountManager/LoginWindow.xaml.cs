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
    /// LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private AppSetting appSetting;
        public string CurrentUser { get; private set; }

        public LoginWindow(AppSetting appSetting)
        {
            InitializeComponent();

            this.appSetting = appSetting;
            if (appSetting.IsAutoLogin)
            {
                string defaultUser = appSetting.DefaultUser;
                if (appSetting.UserNames.Contains(defaultUser))
                {
                    CurrentUser = defaultUser;
                    this.DialogResult = true;
                    return;
                }
                else
                {
                    Logger.LogError("The default user cannot be found.");
                    appSetting.DefaultUser = "";
                    MessageBox.Show("The default user could not be found. Please select a valid user.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }




            LoadAccountList();
            SetLastLoginUser();
        }

        private void LoadAccountList()
        {
            AccountListBox.ItemsSource = appSetting.UserNames;
        }

        private void SetLastLoginUser()
        {
            if (!string.IsNullOrEmpty(appSetting.DefaultUser) && appSetting.UserNames.Contains(appSetting.DefaultUser))
            {
                AccountListBox.SelectedItem = appSetting.DefaultUser;
            }
        }

        private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {

            RegisterAccountWindow registerAccountWindow = new RegisterAccountWindow(appSetting);
            bool? result = registerAccountWindow.ShowDialog();

            if (result == true)
            {
                SetLastLoginUser();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListBox.SelectedItem == null)
            {
                MessageBox.Show("You need to select a login account", "Notify", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            CurrentUser = AccountListBox.SelectedItem.ToString();
            appSetting.DefaultUser = CurrentUser;

            this.DialogResult = true;
        }

        private void CancelButton_Click(Object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
