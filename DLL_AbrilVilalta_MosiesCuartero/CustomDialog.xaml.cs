using System.Windows;

namespace WPF_MVVM_SPA_Template.Views
{
    public partial class CustomDialog : Window
    {
        public CustomDialog(string message, string title = "Error")
        {
            InitializeComponent();
            TitleText.Text = title;
            MessageText.Text = message;
        }

        // ── Static helper — use this instead of MessageBox.Show ──────
        public static void Show(string message, string title = "Error")
        {
            var dialog = new CustomDialog(message, title);

            // Try to attach to the active window so it centers correctly
            if (Application.Current?.MainWindow != null)
                dialog.Owner = Application.Current.MainWindow;

            dialog.ShowDialog();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}