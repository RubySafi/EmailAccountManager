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
    /// EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        // To store the email addresses
        public ObservableCollection<MailElm> EmailList { get; set; }

        private SiteInfo siteInfo;

        public SecurityLevel SelectedSecurityLevel { get; set; }

        public EditWindow(SiteInfo info)
        {
            InitializeComponent();

            this.siteInfo = info;
            EmailList = new ObservableCollection<MailElm>(info.EmailList);
            EmailDataGrid.ItemsSource = EmailList;

            SiteNameTextBox.Text = info.SiteName;
            SiteCommentTextBox.Text = info.Comment;
            

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

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
            {
                // Prevent focus movement by arrow keys
                e.Handled = true;
            }
        }

        // Remove selected email from the list
        private void RemoveEmailButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmailDataGrid.SelectedItem != null)
            {
                EmailList.Remove((MailElm)EmailDataGrid.SelectedItem);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
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


            SelectedSecurityLevel = (SecurityLevel)SecurityLevelComboBox.SelectedItem;
            siteInfo.Timestamp = DateTime.Now;
            siteInfo.SiteName = siteName.Trim();
            siteInfo.SecurityLevel = SelectedSecurityLevel;
            siteInfo.EmailList = EmailList.ToList();
            siteInfo.Comment = SiteCommentTextBox.Text.Trim();

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public SiteInfo GetNewSite()
        {
            return siteInfo;
        }
        private void SecurityLevelComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            SecurityLevelComboBox.ItemsSource = Enum.GetValues(typeof(SecurityLevel)).Cast<SecurityLevel>();

            SecurityLevelComboBox.SelectedIndex = siteInfo.SecurityLevel switch
            {
                SecurityLevel.None => 0,
                SecurityLevel.General => 1,
                SecurityLevel.Finance => 2,
                _ => 0,
            };
        }
    }
}
