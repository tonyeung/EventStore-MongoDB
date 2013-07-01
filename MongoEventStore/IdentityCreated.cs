using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoEventStore
{
    class IdentityCreated : IEvent
    {
        public IdentityCreated()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public Guid NewGuid { get; set; }
    }
}
