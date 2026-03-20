using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DLL_AbrilVilalta_MosiesCuartero
{
    public partial class MinLengthTextBoxName : UserControl
    {
        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: Text
        // The actual text value entered by the user, supports binding.
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(MinLengthTextBoxName),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnTextChanged));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: MinLength
        // Minimum number of characters required. Default is 3.
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty MinLengthProperty =
            DependencyProperty.Register(
                "MinLength",
                typeof(int),
                typeof(MinLengthTextBoxName),
                new PropertyMetadata(3));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: TooltipMessage
        // Message shown in the tooltip of the error circle indicator.
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty TooltipMessageProperty =
            DependencyProperty.Register(
                "TooltipMessage",
                typeof(string),
                typeof(MinLengthTextBoxName),
                new PropertyMetadata(string.Empty));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: BorderColor
        // Controls the border color of the TextBox (red when invalid).
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(
                "BorderColor",
                typeof(Brush),
                typeof(MinLengthTextBoxName),
                new PropertyMetadata(Brushes.Gray));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: IsValid
        // Public boolean that exposes whether the current value is valid.
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty IsValidProperty =
            DependencyProperty.Register(
                "IsValid",
                typeof(bool),
                typeof(MinLengthTextBoxName),
                new PropertyMetadata(true));

        // ══════════════════════════════════════════════════════════════
        // DependencyProperty: ErrorIndicatorVisibility
        // Controls whether the red error circle is shown or hidden.
        // ══════════════════════════════════════════════════════════════
        public static readonly DependencyProperty ErrorIndicatorVisibilityProperty =
            DependencyProperty.Register(
                "ErrorIndicatorVisibility",
                typeof(Visibility),
                typeof(MinLengthTextBoxName),
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
        // Flag: tracks whether the user has interacted with the field.
        // The error circle only appears after the first interaction.
        // ══════════════════════════════════════════════════════════════
        private bool _hasBeenTouched = false;

        // ══════════════════════════════════════
        // Constructor
        // ══════════════════════════════════════

        public MinLengthTextBoxName()
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

                    try
                    {
                        BorderColor = (Brush)FindResource("InputBorderBrush");

                    }
                    catch
                    {
                        BorderColor = Brushes.Gray;
                    }
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
        // Callback: called every time the Text DependencyProperty changes
        // (e.g. when the ViewModel pushes a value via binding on ClearForm)
        // ══════════════════════════════════════════════════════════════
        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MinLengthTextBoxName)d;
            var newValue = (string)e.NewValue;

            // Keep internal TextBox in sync if the value came from outside (ViewModel)
            if (control.MinLengthTextBoxControl != null &&
                control.MinLengthTextBoxControl.Text != newValue)
            {
                control.MinLengthTextBoxControl.Text = newValue;
            }

            // If the ViewModel clears the field (ClearForm), reset the visual state
            if (string.IsNullOrEmpty(newValue))
            {
                control._hasBeenTouched = false;
                control.ErrorIndicatorVisibility = Visibility.Collapsed;
                control.TooltipMessage = string.Empty;

                try
                {
                    control.BorderColor = (Brush)control.FindResource("InputBorderBrush");
                }
                catch
                {
                    control.BorderColor = Brushes.Gray;
                }
            }

            control.Validate();
        }

        // ══════════════════════════════════════════════════════════════
        // Validate: checks the text against the MinLength requirement.
        // Updates BorderColor, TooltipMessage, IsValid and the
        // visibility of the error indicator circle.
        // ══════════════════════════════════════════════════════════════
        private void Validate()
        {
            if (string.IsNullOrEmpty(Text))
            {
                TooltipMessage = $"Name cannot be empty.\n A minimum of {MinLength} characters is required.";
                BorderColor = Brushes.LightCoral;
                IsValid = false;
                ErrorIndicatorVisibility = Visibility.Visible;
            }
            else if (Text.Length < MinLength)
            {
                TooltipMessage = $"The text is too short.\nYou entered {Text.Length} character(s), but a minimum of {MinLength} is required.";
                BorderColor = Brushes.LightCoral;
                IsValid = false;
                ErrorIndicatorVisibility = Visibility.Visible;
            }
            else
            {
                TooltipMessage = string.Empty;
                IsValid = true;
                ErrorIndicatorVisibility = Visibility.Collapsed;

                try
                {
                    BorderColor = (Brush)FindResource("InputBorderBrush");
                }
                catch
                {
                    BorderColor = Brushes.Gray;
                }
            }
        }

        private void MyMaskedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }

    }
}