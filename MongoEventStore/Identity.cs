using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MongoEventStore
{
    public class Identities : AggregateRoot
    {

        #region members
        private Guid id;
        public override Guid Id
        {
            get { return id; }
        }
        #endregion

        #region ctor
        public Identities()
        {
        }
        #endregion

        #region behavior

        private void CreateNewId()
        {
            id = Guid.Parse("dcdf08ba-913b-4395-bfd8-ce6d2b0da718");
            ApplyChange(new IdentityCreated() { NewGuid = id });
        }

        #endregion

        #region apply
        private void Apply(IdentityCreated e)
        {
            id = e.NewGuid;
        }
        #endregion

        #region builder
        public class Builder
        {
            private Identities identity = new Identities();

            public Builder WithNewID()
            {
                if (identity.id == Guid.Empty)
                {
                    identity.CreateNewId();
                }
                return this;
            }

            public Identities Build()
            {
                if (identity.Id == null)
                {
                    throw new Exception("Identity.Id Required");
                }

                var result = identity;
                identity = null;

                return result;
            }
        }
        #endregion
    }
}
