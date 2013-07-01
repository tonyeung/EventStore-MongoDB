using EventStore;
using EventStore.Dispatcher;
using EventStore.Serialization;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MongoEventStore
{

    public class EventStoreRepository<T> where T : AggregateRoot, new()
    {

        private IStoreEvents WireupEventStore()
        {
            //register the class map
            var types = Assembly.GetAssembly(typeof(IEvent))
                    .GetTypes()
                    .Where(type => type.IsSubclassOf(typeof(IEvent)));

            foreach (var t in types)
                BsonClassMap.LookupClassMap(t); 

            // initialize the eventstore
            return Wireup.Init()
                .LogToOutputWindow()
                .UsingMongoPersistence("EventStoreSample", new DocumentObjectSerializer())
                    .InitializeStorageEngine()
                    .UsingJsonSerialization()
                        .Compress()
                .UsingSynchronousDispatchScheduler()
                    .DispatchTo(new CommitDispatcher())

                .Build();
        }

        public sealed class CommitDispatcher : IDispatchCommits
        {

            public void Dispatch(Commit commit)
            {
                foreach (var item in commit.Events)
                {
                    Console.WriteLine(item.Body);
                }                
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }
        } 

        /// <summary>
        /// Saves a set of events to the event store
        /// </summary>
        /// <param name="AggregateID">The Aggregate ID</param>
        /// <param name="Events">A list of IEvents to be saved</param>
        public void Save(AggregateRoot root)
        {
            // we can call CreateStream(StreamId) if we know there isn't going to be any data.
            // or we can call OpenStream(StreamId, 0, int.MaxValue) to read all commits,
            // if no commits exist then it creates a new stream for us.
            using (var scope = new TransactionScope())
            using (var eventStore = WireupEventStore())
            using (var stream = eventStore.OpenStream(root.Id, 0, int.MaxValue))
            {
                var events = root.GetUncommittedChanges();
                foreach (var e in events)
                {
                    stream.Add(new EventMessage { Body = e });
                }

                var guid = Guid.NewGuid();
                stream.CommitChanges(guid);
                root.MarkChangesAsCommitted();

                scope.Complete();
            }
        }

        public T GetById(Guid ID)
        {
            var obj = new T();

            using (var eventStore = WireupEventStore())
            {
                var snapshot = eventStore.Advanced.GetSnapshot(ID, int.MaxValue);
                if (snapshot == null)
                {
                    using (var stream = eventStore.OpenStream(ID, 0, int.MaxValue))
                    {
                        var events = from s in stream.CommittedEvents
                                     select s.Body as IEvent;

                        obj.LoadsFromHistory(events);
                    }
                }
                else
                {
                    obj = (T)snapshot.Payload;
                    using (var stream = eventStore.OpenStream(snapshot, int.MaxValue))
                    {
                        var events = from s in stream.CommittedEvents
                                     select s.Body as IEvent;

                        obj.LoadsFromHistory(events);
                    }
                }
            }

            return obj;
        }
    }
}
