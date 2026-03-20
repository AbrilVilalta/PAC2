using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DLL_AbrilVilalta_MosiesCuartero
{
    public partial class EmailTextBox : UserControl
    {
        public static readonly DependencyProperty EmailProperty =
            DependencyProperty.Register(
                nameof(Email),
                typeof(string),
                typeof(EmailTextBox),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnEmailChanged));

        public static readonly DependencyProperty TooltipMessageProperty =
            DependencyProperty.Register(
            "TooltipMessage",
            typeof(string),
            typeof(EmailTextBox),
            new PropertyMetadata("ex. example@gmail.com"));

        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(
            "BorderColor",
            typeof(Brush),
            typeof(EmailTextBox),
            new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty ErrorIndicatorVisibilityProperty =
            DependencyProperty.Register(
            "ErrorIndicatorVisibility",
            typeof(Visibility),
            typeof(EmailTextBox),
            new PropertyMetadata(Visibility.Collapsed));

        public string Email
        {
            get => (string)GetValue(EmailProperty);
            set => SetValue(EmailProperty, value);
        }

        public string TooltipMessage
        {
            get => (string)GetValue(TooltipMessageProperty);
            set => SetValue(TooltipMessageProperty, value);
        }

        public Brush BorderColor
        {
            get => (Brush)GetValue(BorderColorProperty);
            set => SetValue(BorderColorProperty, value);
        }

        public Visibility ErrorIndicatorVisibility
        {
            get => (Visibility)GetValue(ErrorIndicatorVisibilityProperty);
            set => SetValue(ErrorIndicatorVisibilityProperty, value);
        }

        private bool _hasBeenTouched = false;

        public EmailTextBox()
        {
            InitializeComponent();

            this.IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue == true)
                {
                    _hasBeenTouched = false;
                    ErrorIndicatorVisibility = Visibility.Collapsed;
                    TooltipMessage = "ex. example@gmail.com";
                    try { BorderColor = (Brush)FindResource("InputBorderBrush"); }
                    catch { BorderColor = Brushes.Gray; }
                }
            };

            EmailTextBoxControl.TextChanged += (s, e) =>
            {
                _hasBeenTouched = true;
            };

            EmailTextBoxControl.LostFocus += (s, e) =>
            {
                if (_hasBeenTouched)
                    ValidateEmail();
            };
        }

        private void ValidateEmail()
        {
            string email = EmailTextBoxControl.Text;

            if (string.IsNullOrEmpty(email))
            {
                TooltipMessage = "Email cannot be empty.\nex. example@gmail.com";
                BorderColor = Brushes.LightCoral;
                ErrorIndicatorVisibility = Visibility.Visible;
            }
            else if (!IsValidEmail(email))
            {
                TooltipMessage = "Invalid email address.\nex. example@gmail.com";
                BorderColor = Brushes.LightCoral;
                ErrorIndicatorVisibility = Visibility.Visible;
            }
            else
            {
                TooltipMessage = "ex. example@gmail.com";
                ErrorIndicatorVisibility = Visibility.Collapsed;
                try { BorderColor = (Brush)FindResource("InputBorderBrush"); }
                catch { BorderColor = Brushes.Gray; }
            }
        }

        private static void OnEmailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (EmailTextBox)d;
            control.ValidateEmail();
        }

        private bool IsValidEmail(string email)
        {
            var regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(email);
        }

        private void MyMaskedTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var currentText = textBox?.Text;
            var newText = currentText + e.Text;
            var isTextValid = IsValidEmail(newText);
            e.Handled = !isTextValid;
        }

        public void Reset()
        {
            _hasBeenTouched = false;
            ErrorIndicatorVisibility = Visibility.Collapsed;
            TooltipMessage = "ex. example@gmail.com";
            try { BorderColor = (Brush)FindResource("InputBorderBrush"); }
            catch { BorderColor = Brushes.Gray; }
        }
    }
}