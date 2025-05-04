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
        //public List<SiteInfo> SiteList { get; set; }
        public ObservableCollection<SiteInfo> SiteList { get; set; } = new ObservableCollection<SiteInfo>();


    public MainWindow()
        {
            InitializeComponent();

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

            this.DataContext = this;


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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();
            bool? result = addWindow.ShowDialog(); 

            if (result == true)
            {
                var newSiteInfo = addWindow.GetNewSite();
                SiteList.Add(newSiteInfo);
            }
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
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
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
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
                }
            }
        }

    }
}