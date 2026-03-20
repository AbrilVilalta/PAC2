using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DLL_AbrilVilalta_MosiesCuartero
{
    public partial class MinLengthTextBoxSurname : UserControl
    {
        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: Text
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(MinLengthTextBoxSurname),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextChanged));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: MinLength (kept for API compatibility)
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty MinLengthProperty =
            DependencyProperty.Register(
                "MinLength",
                typeof(int),
                typeof(MinLengthTextBoxSurname),
                new PropertyMetadata(0));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: TooltipMessage
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty TooltipMessageProperty =
            DependencyProperty.Register(
                "TooltipMessage",
                typeof(string),
                typeof(MinLengthTextBoxSurname),
                new PropertyMetadata(string.Empty));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: BorderColor
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(
                "BorderColor",
                typeof(Brush),
                typeof(MinLengthTextBoxSurname),
                new PropertyMetadata(Brushes.Gray));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: IsValid
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(
                "IsValid",
                typeof(bool),
                typeof(MinLengthTextBoxSurname),
                new PropertyMetadata(true));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: ErrorIndicatorVisibility
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty ErrorIndicatorVisibilityProperty =
            DependencyProperty.Register(
                "ErrorIndicatorVisibility",
                typeof(Visibility),
                typeof(MinLengthTextBoxSurname),
                new PropertyMetadata(Visibility.Collapsed));

        // ══════════════════════════════════════
        // CLR Property Wrappers
        // ══════════════════════════════════════

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public int MinLength
        {
            get => (int)GetValue(MinLengthProperty);
            set => SetValue(MinLengthProperty, value);
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

        // ══════════════════════════════════════════════════════════════
        // Flag: only show error after the user has interacted.
        // ══════════════════════════════════════════════════════════════
        private bool _hasBeenTouched = false;

        // ══════════════════════════════════════
        // Constructor
        // ══════════════════════════════════════

        public MinLengthTextBoxSurname()
        {
            InitializeComponent();

            // Reset state every time the form becomes visible again
            this.IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue == true)
                {
                    _hasBeenTouched = false;
                    ErrorIndicatorVisibility = Visibility.Collapsed;
                    TooltipMessage = string.Empty;

                    try { BorderColor = (Brush)FindResource("InputBorderBrush"); }
                    catch { BorderColor = Brushes.Gray; }
                }
            };

            // Mark as touched and sync DP when the user types
            MinLengthTextBoxControl.TextChanged += (s, e) =>
            {
                if (MinLengthTextBoxControl.Text != Text)
                    Text = MinLengthTextBoxControl.Text;

                _hasBeenTouched = true;
            };

            // Only validate on lost focus if the user has typed something
            MinLengthTextBoxControl.LostFocus += (s, e) =>
            {
                if (_hasBeenTouched)
                    Validate();
            };
        }

        // ══════════════════════════════════════════════════════════════
        // Callback: syncs the internal TextBox when the ViewModel
        // pushes a value (e.g. LoadClientForEdit or ClearForm).
        // ══════════════════════════════════════════════════════════════
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MinLengthTextBoxSurname)d;
            var newValue = (string)e.NewValue;

            // Keep internal TextBox in sync
            if (control.MinLengthTextBoxControl != null &&
                control.MinLengthTextBoxControl.Text != newValue)
            {
                control.MinLengthTextBoxControl.Text = newValue;
            }

            // ClearForm sends empty string — reset visual state
            if (string.IsNullOrEmpty(newValue))
            {
                control._hasBeenTouched = false;
                control.ErrorIndicatorVisibility = Visibility.Collapsed;
                control.TooltipMessage = string.Empty;

                try { control.BorderColor = (Brush)control.FindResource("InputBorderBrush"); }
                catch { control.BorderColor = Brushes.Gray; }
            }

            control.Validate();
        }

        // ══════════════════════════════════════════════════════════════
        // Validate: only fails if the text contains special characters.
        // Letters (including accented), spaces, hyphens and apostrophes
        // are all accepted as valid surname characters.
        // ══════════════════════════════════════════════════════════════
        private void Validate()
        {
            // Empty field is allowed — no error
            if (string.IsNullOrEmpty(Text))
            {
                TooltipMessage = string.Empty;
                IsValid = true;
                ErrorIndicatorVisibility = Visibility.Collapsed;

                try { BorderColor = (Brush)FindResource("InputBorderBrush"); }
                catch { BorderColor = Brushes.Gray; }
                return;
            }

            // Allow letters (including accented), spaces, hyphens and apostrophes
            var regex = new Regex(@"^[\p{L}\s\-']+$");

            if (!regex.IsMatch(Text))
            {
                TooltipMessage = "The last name cannot contain special characters.\nOnly letters, spaces, hyphens, and apostrophes are allowed.";
                BorderColor = Brushes.LightCoral;
                IsValid = false;
                ErrorIndicatorVisibility = Visibility.Visible;
            }
            else
            {
                TooltipMessage = string.Empty;
                IsValid = true;
                ErrorIndicatorVisibility = Visibility.Collapsed;

                try { BorderColor = (Brush)FindResource("InputBorderBrush"); }
                catch { BorderColor = Brushes.Gray; }
            }
        }

        private void MyMaskedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }

    }
}