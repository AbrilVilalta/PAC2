
/* ════════════════════════════════════════════
 * CLIENTSERVICE
 * ════════════════════════════════════════════ */

using Newtonsoft.Json;
using System.IO;

namespace WPF_MVVM_SPA_Template.Services
{
    internal class ClientService
    {

        public event Action? DataChanged;
        // JSON Doc Path
        private const string FilePath = @"..\..\..\Assets\Client.json";

        // Load Client Data
        public dynamic LoadClientsData()
        {
            // Open the JSON file, read its content and deserialize it.
            // If the file is empty or null, return an empty clients object.
            using (StreamReader file = File.OpenText(FilePath))
            {
                string json = file.ReadToEnd();
                return JsonConvert.DeserializeObject(json) ?? new { clients = new object[] { } };
            }
        }

        // Serialize and save the client data to the JSON file,
        // then notify all subscribers that the data has changed.
        public void SaveToFile(dynamic clientData)
        {
            string json = JsonConvert.SerializeObject(clientData, Formatting.Indented);
            File.WriteAllText(FilePath, json);

            DataChanged?.Invoke();
        }
    }
}
