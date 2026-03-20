
/* ════════════════════════════════════════════
 * CLIENTS MODEL
 * ════════════════════════════════════════════ */

// Define the name space where this class live
namespace WPF_MVVM_SPA_Template.Models
{
    class Client // Define the Client Class
    {
        public int IdCli { get; set; }

        // DNI can't be NULL, as it is an String we need to 
        // give it an initial value: string.Empty.
        public string DNI { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Lastname { get; set; }
        public string? Mail { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
