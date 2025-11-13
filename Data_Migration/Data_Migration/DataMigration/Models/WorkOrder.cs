namespace Data_Migration.DataMigration.Models
{
    public class WorkOrder : Aggregate<int>
    {
        public int ClientId { get; set; }
        public virtual  Client Client { get; set; } = null!;
        public int TechnicianId { get; set; }
        public virtual Technician Technician { get; set; } = null!;
        public DateTime ServiceDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }
        public bool IsPushedToErp { get; set; } = false;
    }
}
