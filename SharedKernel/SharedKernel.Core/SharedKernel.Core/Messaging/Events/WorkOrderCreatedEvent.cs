
namespace SharedKernel.Core.Messaging.Events
{
    public record  WorkOrderCreatedEvent : BaseIntegrationEvent
    {

        public int WorkOrderId { get; init; }
        public string ClientFirstName { get; init; }
        public string ClientLastName { get; init; }
        public string TechnicianFirstName { get; init; }
        public string TechnicianLastName { get; init; }
        public decimal Total { get; init; }
        public DateTime ServiceDate { get; init; }
    }
}
