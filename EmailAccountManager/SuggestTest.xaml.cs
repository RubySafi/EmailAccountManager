using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Printing;
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
using System.Windows.Threading;

namespace EmailAccountManager
{
    /// <summary>
    /// SuggestTest.xaml の相互作用ロジック
    /// </summary>
    public partial class SuggestTest : Window
    {

        private const int DisplayMax = 5;
        private string _originalText = string.Empty;
        private bool _isUpdatingTextFromList = false;
        private Dictionary<string, int> RegisteredEmails = new Dictionary<string, int>
        { 
            {"test@example.com", 1},
            {"info@example.com", 2},
            {"info2@example.com", 3},
            {"info3@example.com", 4},
            {"info4@example.com", 5},
            {"info3@example.commmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmdsdsdfsfafdisajfds", 48},
            {"contact@example.org", 6},
            {"support@mydomain.com", 7},
            {"hello@sample.net", 8}
        };


        public SuggestTest()
        {
            InitializeComponent();


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

        int i = 0;
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
            PresentationSource source = PresentationSource.FromVisual(this);
            Matrix logicalToDeviceMatrix = source.CompositionTarget.TransformToDevice;
            Matrix deviceToLogicalMatrix = source.CompositionTarget.TransformFromDevice;

            i++;
            if (sender is ListBoxItem item && item.DataContext is string content)
            {
                var textBlock = FindVisualChild<TextBlock>(item);
                if (textBlock != null && IsTextTrimmed(textBlock))
                {
                 
                    HoverPopupTextBlock.Text = content;

                    var devicePosition = PointToScreen(Mouse.GetPosition(this));
                    var logicalPosition = deviceToLogicalMatrix.Transform(devicePosition);
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

            // 表示できる幅を超えているかどうかで判定
            return formattedText.Width > textBlock.ActualWidth;
        }

    }
}
