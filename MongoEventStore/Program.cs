using EventStore;
using EventStore.Dispatcher;
using EventStore.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoEventStore
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = Wireup.Init()
                .UsingMongoPersistence("EventStore", new DocumentObjectSerializer())
                    .InitializeStorageEngine()
                    .UsingJsonSerialization()
                        .Compress()
                .UsingSynchronousDispatchScheduler()
                    .DispatchTo(new CommitDispatcher())

                .Build();

            var myMessage = new MyMessage() { CustomerId = Guid.NewGuid(), MessageId = Guid.NewGuid() };


            using (store)
            {
                using (var stream = store.CreateStream(myMessage.CustomerId))
                {
                    stream.Add(new EventMessage { Body = myMessage });
                    stream.CommitChanges(myMessage.MessageId);
                }
            }
        } 
    }

    public class MyMessage
    {
        public Guid CustomerId { get; set; }
        public Guid MessageId { get; set; }
    }

    public sealed class CommitDispatcher : IDispatchCommits
    {
        
        public void Dispatch(Commit commit)
        {
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    } 
}
