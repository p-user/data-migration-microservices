namespace ERP_Service.ERP.Models
{
    public class WorkOrder:Aggregate<int>
    {
        public int ClientId { get; set; }
        public virtual Client? Client { get; set; }

        public int TechnicianId { get; set; }
        public virtual Technician? Technician { get; set; }

        public string Notes { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public DateTime WorkDate { get; set; }

        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        private WorkOrder() { }

        public static WorkOrder Create(int workOrderId,int clientId,int technicianId,string notes,decimal total,DateTime workDate)
        {
            if (workOrderId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(workOrderId), "WorkOrder ID must be positive.");
            }
            if (clientId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(clientId), "Client ID must be positive.");
            }
            if (technicianId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(technicianId), "Technician ID must be positive.");
            }
            if (string.IsNullOrWhiteSpace(notes))
            {
                throw new ArgumentException("Notes cannot be null or empty.", nameof(notes));
            }
            if (total < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(total), "Total cannot be negative.");
            }
            if (workDate == default)
            {
                throw new ArgumentException("Work date cannot be default.", nameof(workDate));
            }
            if (workDate > DateTime.UtcNow.AddYears(1)) 
            {
                throw new ArgumentException("Work date cannot be too far in the future.", nameof(workDate));
            }

            return new WorkOrder
            {
                //Id = workOrderId,
                ClientId = clientId,
                TechnicianId = technicianId,
                Notes = notes,
                Total = total,
                WorkDate = workDate,
                IsValid = true 
            };
        }

        

        public void AssignTechnician(int newTechnicianId)
        {
            if (newTechnicianId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newTechnicianId), "Technician ID must be positive.");
            }
            TechnicianId = newTechnicianId;
        }

        public void AssignClient(int newClientId)
        {
            if (newClientId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newClientId), "Client ID must be positive.");
            }
            ClientId = newClientId;
        }

        public void SetInvalid(string errorMessage)
        {
            IsValid = false;
            ErrorMessage = errorMessage;
        }

        public void SetValid()
        {
            IsValid = true;
            ErrorMessage = null;
        }
    }
}
