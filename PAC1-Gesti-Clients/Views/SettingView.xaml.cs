
/* ════════════════════════════════════════════
 * SETTINGVIEW (code-behind)
 * ════════════════════════════════════════════ */

using Newtonsoft.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPF_MVVM_SPA_Template.ViewModels;

namespace WPF_MVVM_SPA_Template.Views
{
    public partial class SettingView : UserControl
    {
        private string _activeFont = "Segoe UI"; // Currently applied font family.
        private string _activeFontSz = "Medium"; // Currently applied font size.
        private bool _isDark = true;             // Tracks whether dark theme is active.

        // Checks the live ResourceDictionary list to detect the active theme.
        private bool IsDarkTheme =>
            Application.Current.Resources.MergedDictionaries
                .Any(d => d.Source != null &&
                          d.Source.OriginalString.Contains("DarkTheme"));

        public SettingView()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                _isDark = IsDarkTheme;
                RefreshThemeButton();
            };
        }

        // ── THEME ────────────────────────────────────────────────────────────

        // Toggles between dark and light theme when the user clicks the theme button.
        private void ThemeToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            _isDark = !_isDark;
            string themeName = _isDark ? "DarkTheme" : "LightTheme";
            LoadTheme(themeName);
            RefreshThemeButton();
        }

        /* ══════════════════════════════════════════════════════════════════════════════════
         * Replaces all ResourceDictionaries with the ones for the selected theme,
         * then re-applies font size and family so they are not lost on theme switch.
         * ══════════════════════════════════════════════════════════════════════════════════ */
        private void LoadTheme(string themeName)
        {
            var dicts = Application.Current.Resources.MergedDictionaries;
            dicts.Clear();

            // Reload all required dictionaries in the correct order.
            string[] paths =
            {
                $"/Assets/Themes/{themeName}.xaml",
                "/Assets/Themes/GlobalStyles.xaml",
                "/Assets/Themes/FontSize.xaml",
                "/Assets/Resources/ThemeIcon.xaml"
            };

            foreach (var path in paths)
            {
                dicts.Add(new ResourceDictionary
                {
                    Source = new System.Uri(path, System.UriKind.Relative)
                });
            }

            // Re-apply font size tokens since dicts.Clear() wiped them.
            ApplyCurrentFontSize();

            // Re-apply custom font family if the user had changed it.
            if (Application.Current.MainWindow is Window win && _activeFont != "Segoe UI")
                win.FontFamily = new FontFamily(_activeFont);

            // Keep the theme toggle button in the MainWindow toolbar in sync.
            SyncMainWindowThemeButton(themeName);

            // Refresh OxyPlot charts so they repaint with the new theme colors
            if (Application.Current.MainWindow?.DataContext is MainViewModel mainVm)
            {
                // Reload HomeView stats cards and bar chart.
                mainVm.HomeVM.loadResources();

                // Force GraphicView to re-run UpdatePlot() by briefly clearing
                // the selected client and restoring it — the PropertyChanged
                // handler inside GraphicViewModel will fire and repaint.
                if (mainVm.GraphicVM != null)
                {
                    var current = mainVm.CrudVM.SelectedClient;
                    mainVm.CrudVM.SelectedClient = null;
                    mainVm.CrudVM.SelectedClient = current;
                }
            }
        }

        /* ══════════════════════════════════════════════════════════════════════
         * Updates the theme toggle button in the MainWindow toolbar to reflect
         * the newly loaded theme
         * ══════════════════════════════════════════════════════════════════════ */
        private void SyncMainWindowThemeButton(string loadedTheme)
        {
            if (Application.Current.MainWindow is not MainWindow mw) return;

            // Tag stores the theme that will be loaded on the NEXT click.
            string nextTheme = loadedTheme == "DarkTheme" ? "LightTheme" : "DarkTheme";

            if (mw.FindName("ThemeBtn") is Button themeBtn)
                themeBtn.Tag = nextTheme;

            // Swap the icon geometry between moon (dark) and sun (light).
            if (mw.FindName("ThemeIconPath") is System.Windows.Shapes.Path iconPath)
            {
                string iconKey = loadedTheme == "DarkTheme" ? "MoonIcon" : "SunIcon";
                if (Application.Current.Resources[iconKey] is Geometry geo)
                    iconPath.Data = geo;
            }
        }

        /* ══════════════════════════════════════════════════════════════════════
         * Updates the theme button label, subtitle, and icon to match the 
         * current state.
         * ══════════════════════════════════════════════════════════════════════ */
        private void RefreshThemeButton()
        {
            ThemeToggleBtn.Content = _isDark ? "Dark mode" : "Light mode";
            ThemeSubLabel.Text = $"Currently: {(_isDark ? "Dark" : "Light")}";

            // FindName requires ApplyTemplate to be called first if the template
            // has not yet been applied
            ThemeToggleBtn.ApplyTemplate();
            if (ThemeToggleBtn.Template.FindName("Icon", ThemeToggleBtn) is TextBlock iconBlock)
                iconBlock.Text = _isDark ? "☾" : "☀";
        }

        // ── FONT FAMILY ───────────────────────────────────────────────────────

        /* ══════════════════════════════════════════════════════════════
         * Applies the font family stored in the clicked chip's Tag
         * and highlights the active chip.
         * ══════════════════════════════════════════════════════════════ */
        private void FontChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            _activeFont = btn.Tag?.ToString() ?? "Segoe UI";

            if (Application.Current.MainWindow is Window win)
                win.FontFamily = new FontFamily(_activeFont);

            RefreshFontChips();
        }

        /* ══════════════════════════════════════════════════════════════
         * Applies the active/inactive style to each font chip based 
         * on _activeFont.
         * ══════════════════════════════════════════════════════════════ */
        private void RefreshFontChips()
        {
            foreach (Button chip in FontChipsPanel.Children.OfType<Button>())
            {
                chip.Style = chip.Tag?.ToString() == _activeFont
                    ? (Style)FindResource("FontChipActiveStyle")
                    : (Style)FindResource("FontChipStyle");
            }
        }

        // ── FONT SIZE ─────────────────────────────────────────────────────────

        /* ══════════════════════════════════════════════════════════════
         * Reads the size preset from the clicked chip's Tag, applies it,
         * updates the subtitle label, and highlights the active chip.
         * ══════════════════════════════════════════════════════════════ */
        private void FontSizeChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            _activeFontSz = btn.Tag?.ToString() ?? "Medium";
            ApplyCurrentFontSize();

            FontSizeSubLabel.Text = _activeFontSz switch
            {
                "Small" => "Currently: Small (12 px)",
                "Large" => "Currently: Large (16 px)",
                _ => "Currently: Medium (14 px)"
            };

            if (btn.Parent is StackPanel panel)
            {
                foreach (Button chip in panel.Children.OfType<Button>())
                {
                    chip.Style = chip.Tag?.ToString() == _activeFontSz
                        ? (Style)FindResource("FontChipActiveStyle")
                        : (Style)FindResource("FontChipStyle");
                }
            }
        }

        /* ══════════════════════════════════════════════════════════════
         * Writes the font-size token values into Application.Current.Resources
         * so every style that references them updates automatically.
         * ══════════════════════════════════════════════════════════════ */
        private void ApplyCurrentFontSize()
        {
            switch (_activeFontSz)
            {
                case "Small":
                    Application.Current.Resources["font-size-xs"] = 11d;
                    Application.Current.Resources["font-size-sm"] = 12d;
                    Application.Current.Resources["font-size-base"] = 13d;
                    Application.Current.Resources["font-size-lg"] = 16d;
                    break;
                case "Large":
                    Application.Current.Resources["font-size-xs"] = 14d;
                    Application.Current.Resources["font-size-sm"] = 16d;
                    Application.Current.Resources["font-size-base"] = 18d;
                    Application.Current.Resources["font-size-lg"] = 22d;
                    break;
                default:
                    Application.Current.Resources["font-size-xs"] = 12d;
                    Application.Current.Resources["font-size-sm"] = 13.5;
                    Application.Current.Resources["font-size-base"] = 15d;
                    Application.Current.Resources["font-size-lg"] = 18d;
                    break;
            }
        }
    }
}