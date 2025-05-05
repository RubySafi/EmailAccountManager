using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// AddWindow.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        // To store the email addresses
        public ObservableCollection<MailElm> EmailList { get; set; }

        private SiteInfo siteInfo;

        public SecurityLevel selectedLevel { get; set; }

        public AddWindow()
        {
            InitializeComponent();

            EmailList = new ObservableCollection<MailElm>();
            EmailDataGrid.ItemsSource = EmailList;

            siteInfo = new SiteInfo();
        }

        // Add email to the list
        private void AddEmailButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailInputTextBox.Text.Trim();
            string comment = EmailCommentTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Email address cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EmailList.Any(e => string.Equals(e.Address, email, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("This email address has already been added.", "Duplicate Entry", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var emailElm = new MailElm() { Address = email, Comment = comment, Timestamp = DateTime.Now };
            EmailList.Add(emailElm);
            EmailInputTextBox.Clear();
            EmailCommentTextBox.Clear();

        }

        // Remove selected email from the list
        private void RemoveEmailButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmailDataGrid.SelectedItem != null)
            {
                EmailList.Remove((MailElm)EmailDataGrid.SelectedItem);
            }
        }

        // Handle OK button click (to save the site info)
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            string siteName = SiteNameTextBox.Text.Trim();
            var selectedLevel = SecurityLevelComboBox.SelectedItem;

            if (string.IsNullOrWhiteSpace(siteName))
            {
                MessageBox.Show("Site name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedLevel == null)
            {
                MessageBox.Show("Please select a security level.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EmailList == null || EmailList.Count == 0)
            {
                MessageBox.Show("At least one email address must be added.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var SelectedSecurityLevel = (SecurityLevel)selectedLevel;
            siteInfo.Timestamp = DateTime.Now;
            siteInfo.SiteName = siteName.Trim();
            siteInfo.SecurityLevel = SelectedSecurityLevel;
            siteInfo.Comment = SiteCommentTextBox.Text.Trim();
            siteInfo.EmailList = EmailList.ToList();

            this.DialogResult = true; // Close the window and indicate success
            this.Close();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
            {
                // Prevent focus movement by arrow keys
                e.Handled = true;
            }
        }

        // Handle Cancel button click
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Close the window and indicate cancellation
            this.Close();
        }

        public SiteInfo GetNewSite()
        {
            return siteInfo;
        }
        private void SecurityLevelComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            SecurityLevelComboBox.ItemsSource = Enum.GetValues(typeof(SecurityLevel)).Cast<SecurityLevel>();

            SecurityLevelComboBox.SelectedIndex = 0;
        }


    }
}
