using System;
using System.Collections.Generic;
using System.Data;
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

namespace WPF_MVVM_SPA_Template.Views
{
    /// <summary>
    /// Lógica de interacción para Form.xaml
    /// </summary>
    public partial class Form : UserControl
    {
        public Form()
        {
            InitializeComponent();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DNI.Reset();
            Email.Reset();
            // El camp Phone és un TextBox estàndard,
            // el seu reset el gestiona el ViewModel via ClearForm()
        }
    }
}
