using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DLL_AbrilVilalta_MosiesCuartero
{
    /// <summary>
    /// Lógica de interacción para DeleteDialog.xaml
    /// </summary>
    public partial class DeleteDialog : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.No;

        public DeleteDialog(string message, string title = "Advertència")
        {
            InitializeComponent();
            MessageText.Text = message;
            TitleText.Text = title;
        }

        public static MessageBoxResult Show(string message, string title = "Advertència", Window? owner = null)
        {
            var dialog = new DeleteDialog(message, title)
            {
                Owner = owner ?? Application.Current.MainWindow
            };
            dialog.ShowDialog();
            return dialog.Result;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }
    }
}
