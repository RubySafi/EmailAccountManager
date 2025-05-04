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
            EmailCommentTextBox.Text = info.Comment;
            

            siteInfo = new SiteInfo();
        }

        // Add email to the list
        private void AddEmailButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailInputTextBox.Text;
            string comment = EmailCommentTextBox.Text;


            if (!string.IsNullOrEmpty(email))
            {
                var emailElm = new MailElm() { Address = email, Comment = comment, Timestamp = DateTime.Now };
                EmailList.Add(emailElm);
                EmailInputTextBox.Clear();
                EmailCommentTextBox.Clear();
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
            string siteName = SiteNameTextBox.Text;
            SelectedSecurityLevel = (SecurityLevel)SecurityLevelComboBox.SelectedItem;

            siteInfo.Timestamp = DateTime.Now;
            siteInfo.SiteName = siteName;
            siteInfo.SecurityLevel = SelectedSecurityLevel;

            siteInfo.EmailList = EmailList.ToList();

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
