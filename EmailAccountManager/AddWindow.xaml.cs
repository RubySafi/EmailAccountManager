using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        private int TagNumber;

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
            siteInfo.Tag = TagNumber;

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

        private void IncrementButton_Click(object sender, RoutedEventArgs e)
        {
            TagNumber = GetValidNumber(TagNumber + 1);
            TagNumberTextBox.Text = TagNumber.ToString();
        }

        private void DecrementButton_Click(object sender, RoutedEventArgs e)
        {
            TagNumber = GetValidNumber(TagNumber - 1);
            TagNumberTextBox.Text = TagNumber.ToString();
        }

        private int GetValidNumber(int number)
        {
            if (number < 0)
            {
                return TagNumber;
            }
            if (number > 1000)
            {
                return TagNumber;
            }
            return number;
        }

        private void TagNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(TagNumberTextBox.Text, out int value))
            {
                var caretIndex = TagNumberTextBox.CaretIndex;
                TagNumber = GetValidNumber(value);
                TagNumberTextBox.Text = GetValidNumber(value).ToString();
                TagNumberTextBox.CaretIndex = Math.Min(caretIndex, TagNumberTextBox.Text.Length);
            }
            else
            {
                var caretIndex = TagNumberTextBox.CaretIndex;
                TagNumberTextBox.Text = TagNumber.ToString();
                TagNumberTextBox.CaretIndex = Math.Min(caretIndex, TagNumberTextBox.Text.Length);
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
