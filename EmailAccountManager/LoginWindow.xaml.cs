using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;

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


            this.PreviewKeyDown += LoginWindow_PreviewKeyDown;


            LoadAccountList();
            SetLastLoginUser();
        }

        private void LoginWindow_PreviewKeyDown(object sender, KeyEventArgs e)
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

        private void AccountListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;

            Point relativePosition = e.GetPosition(listBox);
            var element = listBox.InputHitTest(relativePosition) as DependencyObject;

            while (element != null && !(element is ListBoxItem))
            {
                element = VisualTreeHelper.GetParent(element);
            }

            if (element is ListBoxItem item)
            {
                item.IsSelected = true;
            }

            // Calculate the ideal position to show the context menu (the default position is strange)
            if (this.FindResource("SiteContextMenu") is ContextMenu contextMenu)
            {
                contextMenu.PlacementTarget = listBox;
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

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
            {
                // Prevent focus movement by arrow keys
                e.Handled = true;
            }
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
            AddItem();
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



        private void AddItem()
        {
            RegisterAccountWindow registerAccountWindow = new RegisterAccountWindow(appSetting);
            bool? result = registerAccountWindow.ShowDialog();

            if (result == true)
            {
                SetLastLoginUser();
            }
        }

        private void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddItem();
        }

        private void DeleteItem()
        {
            if (AccountListBox.SelectedItem != null)
            {
                var item = (string)AccountListBox.SelectedItem;
                var sb = new StringBuilder();
                sb.AppendLine($"Are you sure you want to delete the following entry?");
                sb.AppendLine($"Account: {item}");


                MessageBoxResult result = MessageBox.Show(
                    sb.ToString(),
                    "Delete Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    appSetting.UserNames.Remove(item);
                    AccountListBox.SelectedItem = null;
                    AppSetting.Save(appSetting);

                    string dbPath = $"db/{item}.db";
                    string backupDir = "db/backup";
                    string backupPath = $"{backupDir}/{item}_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                    try
                    {
                        if (File.Exists(dbPath))
                        {
                            Directory.CreateDirectory(backupDir);
                            File.Move(dbPath, backupPath);

                            Logger.LogInfo($"Database for user \"{item}\" moved to backup: {backupPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to move database to backup:\n{ex.Message}",
                            "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Logger.LogError($"Failed to move {dbPath} to backup.", ex);
                    }


                }
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteItem();
        }

        private void EditItem()
        {
            if (AccountListBox.SelectedItem != null)
            {
                var oldUserName = (string)AccountListBox.SelectedItem;
                string itemCopy = oldUserName;//prepare a future update to change a class;
                EditAccountWindow editAccountWindow = new EditAccountWindow(appSetting, itemCopy);
                bool? result = editAccountWindow.ShowDialog();

                if (result == true)
                {
                    var newUserName = editAccountWindow.GetNewAccount();

                    string oldDbPath = $"db/{oldUserName}.db";
                    string newDbPath = $"db/{newUserName}.db";

                    int index = appSetting.UserNames.IndexOf(oldUserName);
                    if (index >= 0)
                    {
                        try
                        {
                            if (File.Exists(oldDbPath))
                            {
                                if (File.Exists(newDbPath))
                                {
                                    MessageBox.Show($"A database already exists for user \"{newUserName}\". Please choose a different name.",
                                        "Rename Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }

                                File.Move(oldDbPath, newDbPath);

                                appSetting.UserNames[index] = newUserName;
                                AppSetting.Save(appSetting);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to rename database file:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            Logger.LogError($"Failed to rename {oldDbPath} to {newDbPath}", ex);
                        }
                    }
                    else {
                        MessageBox.Show(
                                $"Failed to update the account name from \"{oldUserName}\" to \"{newUserName}\".",
                                "Update Failed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                        Logger.LogError($"Failed to update account name: \"{oldUserName}\" -> \"{newUserName}\".");
                    }
                }
            }
        }
        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            EditItem();
        }

    }
}
