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
using System.Windows.Interop;
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

        private int TagNumber;

        public SecurityLevel SelectedSecurityLevel { get; set; }

        private DpiManager dpiManager;

        private const int DisplayMax = 5;
        private string _originalText = string.Empty;
        private bool _isUpdatingTextFromList = false;

        private Dictionary<string, int> RegisteredEmails;

        public EditWindow(SiteInfo info, Dictionary<string, int> RegisteredEmails)
        {
            InitializeComponent();

            this.siteInfo = info;
            EmailList = new ObservableCollection<MailElm>(info.EmailList);
            EmailDataGrid.ItemsSource = EmailList;

            SiteNameTextBox.Text = info.SiteName;
            SiteCommentTextBox.Text = info.Comment;
            TagNumber = info.Tag;
            TagNumberTextBox.Text = TagNumber.ToString();

            this.RegisteredEmails = RegisteredEmails;

            this.SourceInitialized += EditWindow_SourceInitialized;
            this.Loaded += (s, e) => dpiManager = new DpiManager(this);
        }

        //DPI Changed Event handling
        private void EditWindow_SourceInitialized(object sender, EventArgs e)
        {
            var source = (HwndSource)PresentationSource.FromVisual(this);
            source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_DPICHANGED = 0x02E0;

            if (msg == WM_DPICHANGED)
            {
                dpiManager?.Update(this);
                handled = false;
            }

            return IntPtr.Zero;
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
            siteInfo.Tag = TagNumber;

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

        private void EmailInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingTextFromList) return;

            _originalText = EmailInputTextBox.Text;

            if (string.IsNullOrEmpty(_originalText))
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            var input = _originalText.ToLower();
            var suggestions = RegisteredEmails
                .Where(kv => kv.Key.ToLower().StartsWith(input))
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .Take(DisplayMax)
                .ToList();

            if (suggestions.Count == 1 && suggestions[0].Equals(input, StringComparison.OrdinalIgnoreCase))
            {
                SuggestionPopup.IsOpen = false;
                return;
            }

            SuggestionListBox.ItemsSource = suggestions;

            if (suggestions.Any())
            {
                SuggestionListBox.SelectedIndex = -1;
                ShowSuggestionPopup();
            }
            else
            {
                SuggestionPopup.IsOpen = false;
            }
        }



        private void SuggestionListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SuggestionListBox.SelectedItem != null)
            {
                EmailInputTextBox.Text = SuggestionListBox.SelectedItem.ToString();
                SuggestionPopup.IsOpen = false;
                EmailInputTextBox.CaretIndex = EmailInputTextBox.Text.Length;
                EmailInputTextBox.Focus();
            }
        }

        private void SuggestionListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            int current = SuggestionListBox.SelectedIndex;
            int max = SuggestionListBox.Items.Count;

            if (e.Key == Key.Enter && SuggestionListBox.SelectedItem != null)
            {
                ConfirmSuggestion();
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                if (current == 0)
                {
                    SuggestionListBox.SelectedIndex = -1;
                    EmailInputTextBox.Text = _originalText;
                    EmailInputTextBox.CaretIndex = _originalText.Length;
                    EmailInputTextBox.Focus();
                    SuggestionPopup.IsOpen = false;
                    e.Handled = true;
                }
                else
                {
                    SuggestionListBox.SelectedIndex = current - 1;
                    UpdatePreviewText();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Down)
            {
                if (current == max - 1)
                {
                    SuggestionListBox.SelectedIndex = -1;
                    EmailInputTextBox.Text = _originalText;
                    EmailInputTextBox.CaretIndex = _originalText.Length;
                    EmailInputTextBox.Focus();
                    SuggestionPopup.IsOpen = false;
                    e.Handled = true;
                }
                else
                {
                    SuggestionListBox.SelectedIndex = current + 1;
                    UpdatePreviewText();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                SuggestionPopup.IsOpen = false;
                EmailInputTextBox.Focus();
                e.Handled = true;
            }
        }



        private void EmailInputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && SuggestionPopup.IsOpen)
            {
                SuggestionPopup.IsOpen = false;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                if (!SuggestionPopup.IsOpen)
                {
                    string input = EmailInputTextBox.Text;
                    IEnumerable<string> suggestions;

                    if (string.IsNullOrEmpty(input))
                    {
                        suggestions = RegisteredEmails
                            .OrderByDescending(kv => kv.Value)
                            .Select(kv => kv.Key)
                            .Take(DisplayMax);
                    }
                    else
                    {
                        suggestions = RegisteredEmails
                            .Where(kv => kv.Key.ToLower().StartsWith(input.ToLower()))
                            .OrderByDescending(kv => kv.Value)
                            .Select(kv => kv.Key)
                            .Take(DisplayMax);
                    }

                    SuggestionListBox.ItemsSource = suggestions.ToList();
                    SuggestionListBox.SelectedIndex = -1;

                    if (SuggestionListBox.HasItems)
                        ShowSuggestionPopup();
                }

                if (SuggestionListBox.Items.Count == 0)
                    return;

                int index = (e.Key == Key.Up) ? SuggestionListBox.Items.Count - 1 : 0;
                SuggestionListBox.SelectedIndex = index;
                var item = (ListBoxItem)SuggestionListBox.ItemContainerGenerator.ContainerFromIndex(index);
                item?.Focus();
                e.Handled = true;
            }
        }

        private void ShowSuggestionPopup()
        {

            if (EmailInputTextBox == null || SuggestionPopup == null)
                return;

            SuggestionPopup.IsOpen = true;
        }


        private void ConfirmSuggestion()
        {
            if (SuggestionListBox.SelectedItem != null)
            {
                _isUpdatingTextFromList = true;
                EmailInputTextBox.Text = SuggestionListBox.SelectedItem.ToString();
                _originalText = EmailInputTextBox.Text;
                EmailInputTextBox.CaretIndex = EmailInputTextBox.Text.Length;
                SuggestionPopup.IsOpen = false;
                EmailInputTextBox.Focus();
                _isUpdatingTextFromList = false;
            }
        }

        private void UpdatePreviewText()
        {
            if (SuggestionListBox.SelectedItem != null)
            {
                _isUpdatingTextFromList = true;
                EmailInputTextBox.Text = SuggestionListBox.SelectedItem.ToString();
                EmailInputTextBox.CaretIndex = EmailInputTextBox.Text.Length;
                _isUpdatingTextFromList = false;
            }
        }

        private void SuggestionListBox_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in SuggestionListBox.Items)
            {
                if (SuggestionListBox.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem listBoxItem)
                {
                    listBoxItem.MouseEnter += ListBoxItem_MouseEnter;
                    listBoxItem.MouseLeave += ListBoxItem_MouseLeave;
                }
            }
        }
        private void ListBoxItem_MouseEnter(object sender, MouseEventArgs e)
        {

            if (sender is ListBoxItem item && item.DataContext is string content)
            {
                var textBlock = FindVisualChild<TextBlock>(item);
                if (textBlock != null && IsTextTrimmed(textBlock))
                {

                    HoverPopupTextBlock.Text = content;

                    var devicePosition = PointToScreen(Mouse.GetPosition(this));
                    var logicalPosition = dpiManager.DeviceToLogicalMatrix.Transform(devicePosition);
                    HoverPopup.HorizontalOffset = logicalPosition.X + 0;
                    HoverPopup.VerticalOffset = logicalPosition.Y + 20;

                    HoverPopup.IsOpen = true;
                }
            }
        }
        private T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }


        private void ListBoxItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (HoverPopup.IsOpen)
            {
                HoverPopup.IsOpen = false;
            }
        }

        private bool IsTextTrimmed(TextBlock textBlock)
        {
            var typeface = new Typeface(
                textBlock.FontFamily,
                textBlock.FontStyle,
                textBlock.FontWeight,
                textBlock.FontStretch);

            var formattedText = new FormattedText(
                textBlock.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                textBlock.FlowDirection,
                typeface,
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1);

            return formattedText.Width > textBlock.ActualWidth;
        }

    }
}
