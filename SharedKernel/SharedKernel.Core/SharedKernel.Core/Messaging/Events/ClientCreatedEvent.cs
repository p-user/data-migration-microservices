using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Core.Messaging.Events
{
    public  record ClientCreatedEvent : BaseIntegrationEvent
    {

        public int ClientId { get; init; }
        public string FirstName { get; init; } 
        public string LastName { get; init; } 
    }
}
