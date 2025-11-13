
namespace SharedKernel.Core.Messaging.Events
{
    public record  TechnicianCreatedEvent : BaseIntegrationEvent
    {

        public int TechnicianId { get; init; } //the id that belongs to the migration database
        public string FirstName { get; init; }
        public string LastName { get; init; }
    }
}
