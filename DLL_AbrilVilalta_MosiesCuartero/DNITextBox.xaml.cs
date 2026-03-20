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
        // Flag per saber si l'usuari ha tocat el camp
        private bool _hasBeenTouched = false;

        public DNITextBox()
        {
            InitializeComponent();

            // Reseteja l'estat cada vegada que el formulari es torna visible
            this.IsVisibleChanged += (s, e) =>
            {
                if ((bool)e.NewValue == true)
                {
                    _hasBeenTouched          = false;
                    ValidateDNI();
                    ErrorIndicatorVisibility = Visibility.Collapsed;
                    TooltipMessage           = "ex. 12345678Z";
                    try   { BorderColor = (Brush)FindResource("InputBorderBrush"); }
                    catch { BorderColor = Brushes.Gray; }
                }
            };

            DNITextBoxControl.TextChanged += (_, __) =>
            {
                _hasBeenTouched = true;
                ValidateDNI();
            };

            // Marca el camp com a tocat quan l'usuari escriu
            DNITextBoxControl.TextChanged += (s, e) =>
            {
                _hasBeenTouched = true;
            };

            // Només valida en perdre el focus si l'usuari ha interactuat
            DNITextBoxControl.LostFocus += (s, e) =>
            {
                if (_hasBeenTouched)
                    ValidateDNI();
            };
        }

        // DependencyProperty per al Text (Propietat per a WPF)
        public static readonly DependencyProperty DNIProperty =
            DependencyProperty.Register("DNI",
            typeof(string),
            typeof(DNITextBox),
            new FrameworkPropertyMetadata(string.Empty,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // DependencyProperty per a la màscara (Propietat per a WPF)
        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.Register("Mask",
            typeof(string),
            typeof(DNITextBox),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TooltipMessageProperty =
            DependencyProperty.Register(
            "TooltipMessage",
            typeof(string),
            typeof(DNITextBox),
            new PropertyMetadata("ex. 12345678Z"));

        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(
            "BorderColor",
            typeof(Brush),
            typeof(DNITextBox),
            new PropertyMetadata(Brushes.Gray));

        // Controla la visibilitat del cercle d'error
        public static readonly DependencyProperty ErrorIndicatorVisibilityProperty =
            DependencyProperty.Register(
            "ErrorIndicatorVisibility",
            typeof(Visibility),
            typeof(DNITextBox),
            new PropertyMetadata(Visibility.Collapsed));

        public string DNI
        {
            get { return (string)GetValue(DNIProperty); }
            set { SetValue(DNIProperty, value); }
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

        private void ValidateDNI()
        {

            if (string.IsNullOrEmpty(DNI))
            {
                TooltipMessage = "DNI cannot be empty. \nex. 12345678Z";
                BorderColor = Brushes.LightCoral;
                ErrorIndicatorVisibility = Visibility.Visible;
            }
            else if (DNI.Length != Mask.Length)
            {
                TooltipMessage = "Invalid DNI. \nex. 12345678Z";
                BorderColor = Brushes.LightCoral;
                ErrorIndicatorVisibility = Visibility.Visible;
            }
            else if (!IsDNIValid(DNI))
            {
                TooltipMessage = "Invalid DNI. \nex. 12345678Z";
                BorderColor = Brushes.LightCoral;
                ErrorIndicatorVisibility = Visibility.Visible;
            }
            else
            {
                TooltipMessage = "ex. 12345678Z";
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

            try
            {
                if (!int.TryParse(DNI.Substring(0, 8), out int numDNI))
                {
                    TooltipMessage = "Invalid DNI. The first 8 characters must be digits. \nex. 12345678Z";
                    BorderColor = Brushes.LightCoral;
                    ErrorIndicatorVisibility = Visibility.Visible;
                }
                else
                {
                    List<string> t_LletraDni = new List<string>
                            {
                                "T", "R", "W", "A", "G", "M", "Y", "F", "P", "D", "X", "B", "N", "J", "Z", "S", "Q", "V", "H", "L", "C", "K", "E"
                            };
                    string lletraDni = DNI[8].ToString().ToUpper();
                    int calcul = numDNI % 23;

                    if (lletraDni != t_LletraDni[calcul])
                    {
                        TooltipMessage = "Invalid DNI. With the number {DNI.Substring(0, 8)} the correct letter is '{t_LletraDni[calcul]}', not '{lletraDni}'. \nex. 12345678Z";
                        BorderColor = Brushes.LightCoral;
                        ErrorIndicatorVisibility = Visibility.Visible;
                    }
                }
            }
            catch
            {
                TooltipMessage = "Invalid DNI format. It must be 8 digits followed by a letter. \nex. 12345678Z";
                BorderColor = Brushes.LightCoral;
                ErrorIndicatorVisibility = Visibility.Visible;
            }

        }

        // Mètode que valida l'entrada d'acord amb la màscara (s'executa quan es prem una tecla)
        private void MyMaskedTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox     = sender as TextBox;
            var currentText = textBox?.Text;
            var newText     = currentText + e.Text;
            var isTextValid = IsDNIValid(newText);
            e.Handled       = !isTextValid;
            ValidateDNI();
        }

        private void MyMaskedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateDNI();
        }

        private bool IsDNIValid(string dni)
        {
            if (string.IsNullOrEmpty(Mask)) return true;
            if (dni.Length > Mask.Length) return false;
            var subMask = Mask.Substring(0, dni.Length);
            var regexPattern = ConvertMaskToRegex(subMask);
            return Regex.IsMatch(dni, regexPattern);
        }

        // Funció per convertir la màscara a una expressió regular
        private string ConvertMaskToRegex(string mask)
        {
            string pattern = "^";
            foreach (var character in mask)
            {
                switch (character)
                {
                    case '0': pattern += "[0-9]";       break;
                    case 'A': pattern += "[A-Z]";       break;
                    case 'a': pattern += "[a-z]";       break;
                    case 'Z': pattern += "[a-zA-Z]";    break;
                    case 'z': pattern += "[a-zA-Z]";    break;
                    case '9': pattern += "[0-9a-zA-Z]"; break;
                    default:  pattern += Regex.Escape(character.ToString()); break;
                }
            }
            pattern += "$";
            return pattern;
        }
        public void Reset()
        {
            BorderColor = Brushes.Gray;
            TooltipMessage = "ex. 12345678Z";
        }

    }
}