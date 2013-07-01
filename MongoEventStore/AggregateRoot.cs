using ReflectionMagic;
using System;
using System.Collections.Generic;

namespace MongoEventStore
{
    public abstract class AggregateRoot
    {
        private readonly List<IEvent> _changes = new List<IEvent>();

        public abstract Guid Id { get; }
        public int Version { get; internal set; }

        public IEnumerable<IEvent> GetUncommittedChanges()
        {
            return _changes;
        }

        public void MarkChangesAsCommitted()
        {
            _changes.Clear();
        }

        public void LoadsFromHistory(IEnumerable<IEvent> history)
        {
            foreach (var e in history) ApplyChange(e, false);
        }

        protected void ApplyChange(IEvent e)
        {
            ApplyChange(e, true);
        }

        private void ApplyChange(IEvent e, bool isNew)
        {
            this.AsDynamic().Apply(e);
            if (isNew) _changes.Add(e);
        }
    }

    public static class PrivateReflectionDynamicObjectExtensions
    {
        public static dynamic AsDynamic(this object o)
        {
            return new PrivateReflectionDynamicObjectInstance(o);
        }
    }
}
