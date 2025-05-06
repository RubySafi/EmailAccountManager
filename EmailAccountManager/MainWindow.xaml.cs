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
using System.IO;
using System.Windows.Controls.Primitives;
using System.Diagnostics.Eventing.Reader;

namespace EmailAccountManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<SiteInfo> SiteList { get; set; } = new ObservableCollection<SiteInfo>();
        public ObservableCollection<SiteInfo> FilteredSiteList { get; set; } = new ObservableCollection<SiteInfo>();

        AppSetting appSetting;

        public string CurrentUserName;

        bool skipDataBase = false;

        public MainWindow()
        {
            InitializeComponent();

            appSetting = AppSetting.Load();
            AutoLoginCheckBox.IsChecked = appSetting.IsAutoLogin;


            var suggestForm = new SuggestTest();
            bool? result = suggestForm.ShowDialog();  // または Show()
            Application.Current.Shutdown();

            if (!TryLogin())
            {
                skipDataBase = true;
                this.Close();
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentUserName))
            {
                Logger.LogError($"Login user is not set.");
                MessageBox.Show("Login user is not set. The application will now close. Please restart it later.", "Application Exit");
                Application.Current.Shutdown();
            }



            InitializeDatabaseForUser();


            this.PreviewKeyDown += MainWindow_PreviewKeyDown;

            UpdateStatusBar();

            this.DataContext = this;

            this.Title = $"Account Manager {AsmUtility.GetAssemblyVersion()}";
        }

        public void InitializeDatabaseForUser()
        {

            string userDatabaseFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db");
            if (!Directory.Exists(userDatabaseFolder))
            {
                Directory.CreateDirectory(userDatabaseFolder);
            }

            string databasePath = System.IO.Path.Combine(userDatabaseFolder, $"{CurrentUserName}.db");

            DatabaseHelper.SetDatabasePath(databasePath);
            DatabaseHelper.InitializeDatabase();

            SiteList = DatabaseHelper.LoadSites();
            FilteredSiteList = new ObservableCollection<SiteInfo>(SiteList);
        }

        public bool TryLogin()
        {

            if (appSetting.CanAutoLogin)
            {
                CurrentUserName = appSetting.DefaultUser;
                return true;
            }

            if (appSetting.IsAutoLogin)
            {
                string defaultUser = appSetting.DefaultUser;
                if (!appSetting.UserNames.Contains(defaultUser))
                {
                    Logger.LogError($"The default user cannot be found. {defaultUser}");
                    appSetting.DefaultUser = "";
                    MessageBox.Show($"The default user [{defaultUser}] could not be found. Please select a valid user.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            LoginWindow loginWindow = new LoginWindow(appSetting);
            bool? result = loginWindow.ShowDialog();
            if (result == true)
            {
                CurrentUserName = loginWindow.CurrentUser;
                return true;
            }

            return false;
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
            {
                e.Handled = true;
                AddItem();
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D)
            {
                e.Handled = true;
                DeleteItem();
            }
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.E)
            {
                e.Handled = true;
                EditItem();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
            {
                // Prevent focus movement by arrow keys
                e.Handled = true;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {

            base.OnClosing(e);

            appSetting.IsAutoLogin = AutoLoginCheckBox.IsChecked ?? false;
            AppSetting.Save(appSetting);

            if (!skipDataBase)
            {
                DatabaseHelper.SaveSites(SiteList);
            }
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

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            PlaceholderLabel.Visibility = string.IsNullOrWhiteSpace(SearchTextBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (UseFilterCheckBox.IsChecked == true)
            {
                ApplyFilter();
            }
        }

        private void UseFilterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void UseFilterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FilteredSiteList.Clear();
            foreach (var site in SiteList)
            {
                FilteredSiteList.Add(site);
            }
        }

        private void ApplyFilter()
        {
            string searchString = SearchTextBox.Text?.Trim() ?? "";

            FilteredSiteList.Clear();

            var filtered = SiteList
                    .Where(site => string.IsNullOrWhiteSpace(searchString) || site.IsDisplay(searchString))
                    .OrderByDescending(site => site.Tag)
                    .ToList();

            foreach (var site in filtered)
            {
                FilteredSiteList.Add(site);
            }
            UpdateStatusBar();
        }

        private void AddItem()
        {
            AddWindow addWindow = new AddWindow();
            bool? result = addWindow.ShowDialog();

            if (result == true)
            {
                var newSiteInfo = addWindow.GetNewSite();

                bool isDuplicate = SiteList.Any(s => string.Equals(s.SiteName, newSiteInfo.SiteName, StringComparison.OrdinalIgnoreCase));
                if (isDuplicate)
                {
                    MessageBox.Show("A site with the same name already exists.",
                        "Duplicate Site", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SiteList.Add(newSiteInfo);
                DatabaseHelper.SaveSites(SiteList);


                ApplyFilter();
                UpdateStatusBar();
            }
        }

        private void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddItem();
        }

        private void DeleteItem()
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

                    ApplyFilter();
                    UpdateStatusBar();
                }
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteItem();
        }

        private void EditItem()
        {
            if (SiteDataGrid.SelectedItem != null)
            {
                var originalItem = (SiteInfo)SiteDataGrid.SelectedItem;
                var itemCopy = new SiteInfo(originalItem);
                EditWindow editWindow = new EditWindow(itemCopy);
                bool? result = editWindow.ShowDialog();

                if (result == true)
                {
                    var newSiteInfo = editWindow.GetNewSite();

                    bool isDuplicate = SiteList.Where(s => s != originalItem)
                                                .Any(s => string.Equals(s.SiteName, newSiteInfo.SiteName, StringComparison.OrdinalIgnoreCase));

                    if (isDuplicate)
                    {
                        MessageBox.Show("A site with the same name already exists.",
                            "Duplicate Site", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    SiteList[SiteList.IndexOf(originalItem)] = newSiteInfo;
                    DatabaseHelper.SaveSites(SiteList);

                    ApplyFilter();
                    UpdateStatusBar();
                }
            }
        }

        private void SiteDataGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;

            Point relativePosition = e.GetPosition(dataGrid);

            // Calculate the ideal position to show the context menu (the default position is strange)
            if (this.FindResource("SiteContextMenu") is ContextMenu contextMenu)
            {
                contextMenu.PlacementTarget = dataGrid;
                contextMenu.Placement = PlacementMode.Relative;

                // Open the context menu and measure its size
                contextMenu.IsOpen = true;
                contextMenu.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                contextMenu.Arrange(new Rect(0, 0, contextMenu.DesiredSize.Width, contextMenu.DesiredSize.Height));

                // Set the position based on the measured size
                contextMenu.HorizontalOffset = relativePosition.X + contextMenu.DesiredSize.Width;
                contextMenu.VerticalOffset = relativePosition.Y;

                contextMenu.IsOpen = true;
                e.Handled = true;
            }

        }


        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                row.IsSelected = true;
            }
        }



        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            EditItem();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddItem();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditItem();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteItem();
        }


        private void UpdateStatusBar()
        {
            StatusUserTextBlock.Text = $"User: {CurrentUserName}";
            StatusTotalCountTextBlock.Text = $"Total: {SiteList.Count}";
            StatusFilteredCountTextBlock.Text = $"Filtered: {FilteredSiteList.Count}";
        }


    }
}