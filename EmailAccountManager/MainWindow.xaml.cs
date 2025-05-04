using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace EmailAccountManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<SiteInfo> SiteList { get; set; } = new ObservableCollection<SiteInfo>();
        public ObservableCollection<SiteInfo> FilteredSiteList { get; set; } = new ObservableCollection<SiteInfo>();


        public MainWindow()
        {
            InitializeComponent();
            DatabaseHelper.SetDatabasePath("administrator.db");
            DatabaseHelper.InitializeDatabase();

            SiteList = DatabaseHelper.LoadSites();
            FilteredSiteList = new ObservableCollection<SiteInfo>(SiteList);

            this.DataContext = this;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            DatabaseHelper.SaveSites(SiteList);
        }

        private void SetDebugData()
        {
            //SiteList = new List<SiteInfo>
            SiteList = new ObservableCollection<SiteInfo>()
            {
                new SiteInfo
                {
                    SiteName = "ExampleSite",
                    Comment = "A sample website",
                    Timestamp = DateTime.Now.AddDays(-2),
                    SecurityLevel = SecurityLevel.General,
                    EmailList = new List<MailElm>
                    {
                        new MailElm { Address = "user1@example.com", Comment = "Main account", Timestamp = DateTime.Now.AddDays(-2) },
                        new MailElm { Address = "user2@example.com", Comment = "Backup", Timestamp = DateTime.Now.AddDays(-1) },
                        new MailElm { Address = "user3@example.com", Comment = "Other", Timestamp = DateTime.Now },
                        new MailElm { Address = "user4@example.com", Comment = "Other", Timestamp = DateTime.Now },
                        new MailElm { Address = "user5@example.com", Comment = "Other", Timestamp = DateTime.Now }
                    }
                },
                new SiteInfo
                {
                    SiteName = "FinanceApp",
                    Comment = "Banking portal",
                    Timestamp = DateTime.Now.AddMonths(-1),
                    SecurityLevel = SecurityLevel.Finance,
                    EmailList = new List<MailElm>
                    {
                        new MailElm { Address = "finance@example.com", Comment = "Primary", Timestamp = DateTime.Now.AddMonths(-1) }
                    }
                }
            };
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Enter email address to filter...")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = Brushes.Black;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Enter email address to filter...";
                SearchTextBox.Foreground = Brushes.Gray;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = SearchTextBox.Text.Trim();

            FilteredSiteList.Clear();

            foreach (var site in SiteList)
            {
                if (string.IsNullOrWhiteSpace(searchString) || site.IsDisplay(searchString))
                {
                    FilteredSiteList.Add(site);
                }
            }
        }


        private void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();
            bool? result = addWindow.ShowDialog(); 

            if (result == true)
            {
                var newSiteInfo = addWindow.GetNewSite();
                SiteList.Add(newSiteInfo);
                DatabaseHelper.SaveSites(SiteList);
            }
        }
        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SiteDataGrid.SelectedItem != null)
            {
                var item = (SiteInfo)SiteDataGrid.SelectedItem;
                var sb = new StringBuilder();
                sb.AppendLine($"Are you sure you want to delete the following entry?");
                sb.AppendLine($"Site: {item.SiteName}");
                sb.AppendLine($"Emails: {item.AllEmails}");
                sb.AppendLine($"Comment: {item.Comment}");

                MessageBoxResult result = MessageBox.Show(
                    sb.ToString(),
                    "Delete Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    SiteList.Remove((SiteInfo)SiteDataGrid.SelectedItem);
                    SiteDataGrid.SelectedItem = null;
                    DatabaseHelper.SaveSites(SiteList);
                }
            }
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {

            if (SiteDataGrid.SelectedItem != null)
            {
                var item = (SiteInfo)SiteDataGrid.SelectedItem;
                var itemCopy = new SiteInfo(item);
                EditWindow editWindow = new EditWindow(itemCopy);
                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    var newSiteInfo = editWindow.GetNewSite();
                    SiteList[SiteList.IndexOf(item)] = newSiteInfo;
                    DatabaseHelper.SaveSites(SiteList);
                }
            }
        }

    }
}