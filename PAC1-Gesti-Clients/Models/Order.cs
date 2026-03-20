
/* ════════════════════════════════════════════
 * ORDERS MODEL
 * ════════════════════════════════════════════ */

namespace WPF_MVVM_SPA_Template.Models
{
    internal class Order
    {
        public int IdOrd { get; set; }
        public int IdPro { get; set; }
        public int IdCli { get; set; }
        public int Units { get; set; }
        public double UnitPrice { get; set; }
        public double Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
