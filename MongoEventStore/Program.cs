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
            var quit = true;
            do
            {
                CRUD();

                Console.WriteLine("press enter to continue, or q to quit");
                quit = Console.ReadKey().KeyChar.ToString() != "q";
                Console.WriteLine();
            } while (quit);
        }

        static void CRUD()
        {

            var repo = new EventStoreRepository<Identities>();

            Console.WriteLine("C to create new object, R to rebuild object");
            var Key = Console.ReadKey().KeyChar.ToString();
            Console.WriteLine();

            if (Key == "c")
            {
                var identity = new Identities.Builder().WithNewID().Build();
                repo.Save(identity);
                Console.WriteLine(identity.Id);
            }
            else
            {
                var identity = repo.GetById(Guid.Parse("dcdf08ba-913b-4395-bfd8-ce6d2b0da718"));
                Console.WriteLine(identity.Id);

            }
        }
    }

}
