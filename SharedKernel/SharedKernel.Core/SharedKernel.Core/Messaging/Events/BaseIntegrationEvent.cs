using SharedKernel.Core.DDD;

namespace SharedKernel.Core.Messaging.Events
{
    public record BaseIntegrationEvent : IDomainEvent
    {
        public Guid Id => Guid.NewGuid();
        public DateTime OccurredOn => DateTime.Now;
        public string EventType => GetType().AssemblyQualifiedName;
    }
}
