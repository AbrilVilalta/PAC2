using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DLL_AbrilVilalta_MosiesCuartero
{
    public partial class PhoneTextBox : UserControl
    {
        public static readonly DependencyProperty PhoneProperty =
            DependencyProperty.Register(
            "Phone",
            typeof(string),
            typeof(PhoneTextBox),
            new FrameworkPropertyMetadata(string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnPhoneChanged));

        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.Register("Mask",
            typeof(string),
            typeof(PhoneTextBox),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TooltipMessageProperty =
            DependencyProperty.Register(
            "TooltipMessage",
            typeof(string),
            typeof(PhoneTextBox),
            new PropertyMetadata("ex. 123456789"));

        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(
            "BorderColor",
            typeof(Brush),
            typeof(PhoneTextBox),
            new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty ErrorIndicatorVisibilityProperty =
            DependencyProperty.Register(
            "ErrorIndicatorVisibility",
            typeof(Visibility),
            typeof(PhoneTextBox),
            new PropertyMetadata(Visibility.Collapsed));

        public string Phone
        {
            get => (string)GetValue(PhoneProperty);
            set => SetValue(PhoneProperty, value);
        }

        public string Mask
        {
            get { return (string)GetValue(MaskProperty); }
            set { SetValue(MaskProperty, value); }
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

        private bool _isFormatting;

        public PhoneTextBox()
        {
            InitializeComponent();

            this.IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue == true)
                {
                    _hasBeenTouched = false;
                    ErrorIndicatorVisibility = Visibility.Collapsed;
                    TooltipMessage = "ex. 123456789";
                    try { BorderColor = (Brush)FindResource("InputBorderBrush"); }
                    catch { BorderColor = Brushes.Gray; }
                }
            };

            PhoneTextBoxControl.TextChanged += PhoneTextBoxControl_TextChanged;

            PhoneTextBoxControl.LostFocus += (s, e) =>
            {
                if (_hasBeenTouched)
                    ValidatePhone();
            };
        }

        private void ValidatePhone()
        {
            if (string.IsNullOrEmpty(Phone))
            {
                TooltipMessage = "Phone cannot be empty.\nex. 123456789";
                BorderColor = (Brush)FindResource("InputBorderBrush");
                ErrorIndicatorVisibility = Visibility.Collapsed;
            }
            else if (!IsValidPhone(Phone))
            {
                TooltipMessage = "Invalid phone number.\nIt must have 9 digits.\nex. 123456789";
                BorderColor = Brushes.LightCoral;
                ErrorIndicatorVisibility = Visibility.Visible;
            }
            else
            {
                TooltipMessage = "ex. 123456789";
                ErrorIndicatorVisibility = Visibility.Collapsed;
                try { BorderColor = (Brush)FindResource("InputBorderBrush"); }
                catch { BorderColor = Brushes.Gray; }
            }
        }

        private static void OnPhoneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PhoneTextBox)d;
            control.ValidatePhone();
        }

        private void PhoneTextBoxControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormatting) return; // evita recursió

            _hasBeenTouched = true;
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string digits = new string(textBox.Text.Where(char.IsDigit).ToArray());
            string formatted = "";
            if (digits.Length >= 1) formatted = digits.Substring(0, Math.Min(3, digits.Length));
            if (digits.Length > 3) formatted += "-" + digits.Substring(3, Math.Min(3, digits.Length - 3));
            if (digits.Length > 6) formatted += "-" + digits.Substring(6, Math.Min(3, digits.Length - 6));

            _isFormatting = true;
            try
            {
                if (textBox.Text != formatted)
                {
                    int caretPos = textBox.CaretIndex;
                    int digitsBeforeCaret = textBox.Text
                        .Substring(0, caretPos)
                        .Count(char.IsDigit);

                    textBox.Text = formatted;

                    int newCaret = 0;
                    int digitCount = 0;
                    while (newCaret < formatted.Length && digitCount < digitsBeforeCaret)
                    {
                        if (char.IsDigit(formatted[newCaret])) digitCount++;
                        newCaret++;
                    }
                    textBox.CaretIndex = newCaret;
                }

                Phone = formatted; // ← sempre actualitza, sense retard
            }
            finally
            {
                _isFormatting = false;
            }
        }

        private void MyMaskedTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var currentText = textBox?.Text;
            var newText = currentText + e.Text;
            e.Handled = !IsValidPhone(newText);
        }

        private bool IsValidPhone(string phone)
        {
            if (string.IsNullOrEmpty(Mask)) return true;
            if (phone.Length > Mask.Length) return false;
            if (phone.Length < Mask.Length) return false;
            var subMask = Mask.Substring(0, phone.Length);
            var regexPattern = ConvertMaskToRegex(subMask);
            return Regex.IsMatch(phone, regexPattern);
        }

        private string ConvertMaskToRegex(string mask)
        {
            string pattern = "^";
            foreach (var character in mask)
            {
                switch (character)
                {
                    case '0': pattern += "[0-9]"; break;
                    case 'A': pattern += "[A-Z]"; break;
                    case 'a': pattern += "[a-z]"; break;
                    case 'Z': pattern += "[a-zA-Z]"; break;
                    case 'z': pattern += "[a-zA-Z]"; break;
                    case '9': pattern += "[0-9a-zA-Z]"; break;
                    default: pattern += Regex.Escape(character.ToString()); break;
                }
            }
            pattern += "$";
            return pattern;
        }

        public void Reset()
        {
            _hasBeenTouched = false;
            ErrorIndicatorVisibility = Visibility.Collapsed;
            TooltipMessage = "ex. 123456789";
            try { BorderColor = (Brush)FindResource("InputBorderBrush"); }
            catch { BorderColor = Brushes.Gray; }
        }
    }
}