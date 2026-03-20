using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPF_MVVM_SPA_Template.Views;

namespace DLL_AbrilVilalta_MosiesCuartero
{
    public partial class DNITextBox : UserControl
    {
        private bool _hasBeenTouched = false;

        public DNITextBox()
        {
            InitializeComponent();

            this.IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue == true)
                {
                    _hasBeenTouched = false;
                    ValidateDNI();
                    ErrorIndicatorVisibility = Visibility.Collapsed;
                    TooltipMessage = "ex. 12345678Z";
                    try { BorderColor = (Brush)FindResource("InputBorderBrush"); }
                    catch { BorderColor = Brushes.Gray; }
                }
            };

            DNITextBoxControl.TextChanged += (_, __) =>
            {
                _hasBeenTouched = true;
                ValidateDNI();
            };

            DNITextBoxControl.LostFocus += (s, e) =>
            {
                if (_hasBeenTouched)
                    ValidateDNI();
            };
        }

        // ── DependencyProperties ────────────────────────────────────────────

        public static readonly DependencyProperty DNIProperty =
            DependencyProperty.Register("DNI", typeof(string), typeof(DNITextBox),
                new FrameworkPropertyMetadata(string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.Register("Mask", typeof(string), typeof(DNITextBox),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TooltipMessageProperty =
            DependencyProperty.Register("TooltipMessage", typeof(string), typeof(DNITextBox),
                new PropertyMetadata("ex. 12345678Z"));

        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register("BorderColor", typeof(Brush), typeof(DNITextBox),
                new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty ErrorIndicatorVisibilityProperty =
            DependencyProperty.Register("ErrorIndicatorVisibility", typeof(Visibility), typeof(DNITextBox),
                new PropertyMetadata(Visibility.Collapsed));

        // ── NOU: IsValid ────────────────────────────────────────────────────
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register("IsValid", typeof(bool), typeof(DNITextBox),
                new PropertyMetadata(false));

        // ── Propietats CLR ──────────────────────────────────────────────────

        public string DNI
        {
            get => (string)GetValue(DNIProperty);
            set => SetValue(DNIProperty, value);
        }

        public string Mask
        {
            get => (string)GetValue(MaskProperty);
            set => SetValue(MaskProperty, value);
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

        // NOU
        public bool IsValid
        {
            get => (bool)GetValue(IsValidProperty);
            set => SetValue(IsValidProperty, value);
        }

        // ── Validació ───────────────────────────────────────────────────────

        private void SetError(string message)
        {
            TooltipMessage = message;
            BorderColor = Brushes.LightCoral;
            ErrorIndicatorVisibility = Visibility.Visible;
            IsValid = false;
        }

        private void SetValid()
        {
            TooltipMessage = "ex. 12345678Z";
            ErrorIndicatorVisibility = Visibility.Collapsed;
            IsValid = true;
            try { BorderColor = (Brush)FindResource("InputBorderBrush"); }
            catch { BorderColor = Brushes.Gray; }
        }

        private void ValidateDNI()
        {
            // 1. Buit
            if (string.IsNullOrEmpty(DNI))
            {
                SetError("DNI cannot be empty.\nex. 12345678Z");
                return;
            }

            // 2. Longitud (màscara)
            if (DNI.Length != Mask.Length)
            {
                SetError("Invalid DNI.\nex. 12345678Z");
                return;
            }

            // 3. Format (màscara regex)
            if (!IsDNIValid(DNI))
            {
                SetError("Invalid DNI.\nex. 12345678Z");
                return;
            }

            // 4. Els 8 primers han de ser dígits
            if (!int.TryParse(DNI.Substring(0, 8), out int numDNI))
            {
                SetError("Invalid DNI. The first 8 characters must be digits.\nex. 12345678Z");
                return;
            }

            // 5. Lletra de control
            var lletresDNI = new List<string>
            {
                "T","R","W","A","G","M","Y","F","P","D","X","B","N","J","Z","S","Q","V","H","L","C","K","E"
            };

            string lletraIntroduida = DNI[8].ToString().ToUpper();
            string lletraCorrecta = lletresDNI[numDNI % 23];

            if (lletraIntroduida != lletraCorrecta)
            {
                SetError($"Invalid DNI. With {DNI.Substring(0, 8)} the correct letter is '{lletraCorrecta}', not '{lletraIntroduida}'.\nex. 12345678Z");
                return;
            }

            // Tot correcte
            SetValid();
        }

        // ── Esdeveniments del TextBox ────────────────────────────────────────

        private void MyMaskedTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var newText = textBox?.Text + e.Text;
            e.Handled = !IsDNIValid(newText);
        }

        private void MyMaskedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateDNI();
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private bool IsDNIValid(string dni)
        {
            if (string.IsNullOrEmpty(Mask)) return true;
            if (dni.Length > Mask.Length) return false;
            var subMask = Mask.Substring(0, dni.Length);
            return Regex.IsMatch(dni, ConvertMaskToRegex(subMask));
        }

        private string ConvertMaskToRegex(string mask)
        {
            string pattern = "^";
            foreach (var c in mask)
            {
                switch (c)
                {
                    case '0': pattern += "[0-9]"; break;
                    case 'A': pattern += "[A-Z]"; break;
                    case 'a': pattern += "[a-z]"; break;
                    case 'Z':
                    case 'z': pattern += "[a-zA-Z]"; break;
                    case '9': pattern += "[0-9a-zA-Z]"; break;
                    default: pattern += Regex.Escape(c.ToString()); break;
                }
            }
            return pattern + "$";
        }

        public void Reset()
        {
            BorderColor = Brushes.Gray;
            TooltipMessage = "ex. 12345678Z";
            IsValid = false;
        }
    }
}