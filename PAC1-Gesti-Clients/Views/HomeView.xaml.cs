using FastReport;
using FastReport.Data;
using FastReport.Export.PdfSimple;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WPF_MVVM_SPA_Template.Models;
using WPF_MVVM_SPA_Template.Services;

namespace WPF_MVVM_SPA_Template.Views
{
    /// <summary>
    /// Lógica de interacción para HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {

        private readonly ClientService _clientService;
        public HomeView()
        {
            InitializeComponent();
            _clientService = new ClientService();
        }

        private void BtnExportarPDF_Click(object sender, RoutedEventArgs e)
        {
            string rutaInforme = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Report_clients.frx");

            Report report = new Report();
            report.Load(rutaInforme);

            var rawData = _clientService.LoadClientsData();
            var jObject = Newtonsoft.Json.Linq.JObject.Parse(rawData.ToString());
            List<Client> clients = jObject["clients"].ToObject<List<Client>>();

            report.RegisterData(clients, "Clients");

            DataSourceBase dataSource = report.GetDataSource("Clients");
            dataSource.Enabled = true;

            DataBand? dataBand = report.FindObject("Data1") as DataBand;
            if (dataBand != null)
            {
                dataBand.DataSource = dataSource;
            }

            report.Prepare();
            string rutaPDF = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Clients.pdf");

            using (MemoryStream ms = new MemoryStream())
            {
                PDFSimpleExport pdfExport = new PDFSimpleExport();
                report.Export(pdfExport, ms);
                File.WriteAllBytes(rutaPDF, ms.ToArray());
            }

            Process.Start(new ProcessStartInfo(rutaPDF) { UseShellExecute = true });
        }

    }
}
