using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DLL_AbrilVilalta_MosiesCuartero
{
    public partial class PhoneTextBox : UserControl
    {
        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: Phone
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty PhoneProperty =
            DependencyProperty.Register(
            "Phone",
            typeof(string),
            typeof(PhoneTextBox),
            new FrameworkPropertyMetadata(string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: Mask
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.Register("Mask",
            typeof(string),
            typeof(PhoneTextBox),
            new PropertyMetadata(string.Empty));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: TooltipMessage
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty TooltipMessageProperty =
            DependencyProperty.Register(
            "TooltipMessage",
            typeof(string),
            typeof(PhoneTextBox),
            new PropertyMetadata("ex. 123456789"));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: BorderColor
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(
            "BorderColor",
            typeof(Brush),
            typeof(PhoneTextBox),
            new PropertyMetadata(Brushes.Gray));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: IsValid
        // Public boolean that exposes whether the current value is valid.
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(
                "IsValid",
                typeof(bool),
                typeof(PhoneTextBox),
                new PropertyMetadata(true));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: ErrorIndicatorVisibility
        // Controls whether the red error circle is shown or hidden.
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty ErrorIndicatorVisibilityProperty =
            DependencyProperty.Register(
                "ErrorIndicatorVisibility",
                typeof(Visibility),
                typeof(PhoneTextBox),
                new PropertyMetadata(Visibility.Collapsed));

        // ══════════════════════════════════════
        // CLR Property Wrappers
        // ══════════════════════════════════════

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

        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }

        public Visibility ErrorIndicatorVisibility
        {
            get => (Visibility)GetValue(ErrorIndicatorVisibilityProperty);
            set => SetValue(ErrorIndicatorVisibilityProperty, value);
        }

        // ══════════════════════════════════════
        // Constructor
        // ══════════════════════════════════════

        public PhoneTextBox()
        {
            InitializeComponent();
            PhoneTextBoxControl.LostFocus += (s, e) => ValidatePhone();
            PhoneTextBoxControl.TextChanged += PhoneTextBoxControl_TextChanged;
        }

        // ══════════════════════════════════════════════════════════════
        // ValidatePhone: checks the phone against the Mask requirement.
        // Updates BorderColor, TooltipMessage, IsValid and the
        // visibility of the error indicator circle.
        // ══════════════════════════════════════════════════════════════
        private void ValidatePhone()
        {
            if (string.IsNullOrEmpty(Phone))
            {
                TooltipMessage = "Phone cannot be empty.\nex. 123456789";
                BorderColor = (Brush)FindResource("InputBorderBrush");
                IsValid = false;
                ErrorIndicatorVisibility = Visibility.Visible;
            }
            else if (!IsValidPhone(Phone))
            {
                TooltipMessage = "Invalid phone.\nex. 123456789";
                BorderColor = Brushes.LightCoral;
                IsValid = false;
                ErrorIndicatorVisibility = Visibility.Visible;
            }
            else
            {
                TooltipMessage = "ex. 123456789";
                BorderColor = (Brush)FindResource("InputBorderBrush");
                IsValid = true;
                ErrorIndicatorVisibility = Visibility.Collapsed;
            }
        }

        private void PhoneTextBoxControl_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string digits = new string(textBox.Text.Where(char.IsDigit).ToArray());

            string formatted = "";
            if (digits.Length >= 1) formatted = digits.Substring(0, Math.Min(3, digits.Length));
            if (digits.Length > 3) formatted += "-" + digits.Substring(3, Math.Min(3, digits.Length - 3));
            if (digits.Length > 6) formatted += "-" + digits.Substring(6, Math.Min(3, digits.Length - 6));

            if (textBox.Text != formatted)
            {
                Phone = formatted;
            }
        }

        private void MyMaskedTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var currentText = textBox?.Text;
            var newText = currentText + e.Text;
            var isTextValid = IsValidPhone(newText);
            ValidatePhone();
            e.Handled = !isTextValid;
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

        // ══════════════════════════════════════════════════════════════
        // Reset: restores the control to its initial clean state.
        // ══════════════════════════════════════════════════════════════
        public void Reset()
        {
            BorderColor = Brushes.Gray;
            TooltipMessage = "ex. 123456789";
            IsValid = true;
            ErrorIndicatorVisibility = Visibility.Collapsed;
        }
    }
}