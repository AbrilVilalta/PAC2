using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WPF_MVVM_SPA_Template.ViewModels;

namespace WPF_MVVM_SPA_Template
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        internal void CanviarTema(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            // El Tag contiene el tema A CARGAR
            string nomTema = btn.Tag?.ToString() ?? "DarkTheme";

            AplicarTema(nomTema);

            // El próximo clic cargará el opuesto
            btn.Tag = nomTema == "DarkTheme" ? "LightTheme" : "DarkTheme";

            // Actualiza el icono via FindName para evitar problemas de contexto
            string iconKey = nomTema == "DarkTheme" ? "MoonIcon" : "SunIcon";
            if (FindName("ThemeIconPath") is Path iconPath &&
                Application.Current.Resources[iconKey] is Geometry geo)
            {
                iconPath.Data = geo;
            }
        }

        internal void AplicarTema(string themeName)
        {
            Application.Current.Resources.MergedDictionaries.Clear();

            string[] diccionaris =
            {
                $"/Assets/Themes/{themeName}.xaml",
                "/Assets/Themes/GlobalStyles.xaml",
                "/Assets/Themes/FontSize.xaml",
                "/Assets/Resources/ThemeIcon.xaml"
            };

            foreach (var path in diccionaris)
            {
                Application.Current.Resources.MergedDictionaries.Add(
                    new ResourceDictionary
                    {
                        Source = new Uri(path, UriKind.Relative)
                    });
            }
        }
    }
}