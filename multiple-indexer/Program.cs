using System;
using System.Linq;
using Nest;

namespace multiple_indexer
{
    class Program
    {
        const string _EsEndpoint = @"https://search-james-elastic-test-l5km3c47amty77yof7hdwsntu4.us-east-1.es.amazonaws.com";
        
        static void Main(string[] args)
        {
            //Setup connection

            var settings = new ConnectionSettings(new Uri(_EsEndpoint))
                .DefaultMappingFor<Fish>(_ => _.IndexName("fishes"))
                .DefaultMappingFor<Bird>(_ => _.IndexName("birds"));

            var client = new ElasticClient(settings);

            //Insert some documents

            client.IndexMany(new[]
            {
                new Fish
                {
                    Name = "Shark",
                    IsDangerous = true,
                    Salinity = 0.03
                },
                new Fish
                {
                    Name = "Goldfish",
                    IsDangerous = false,
                    Salinity = 0.00
                }
            });

            client.IndexMany(new[]
            {
                new Bird
                {
                    Name = "Pigeon",
                    IsDangerous = false,
                    CanFly = true,
                    Wingspan = 6
                },
                new Bird
                {
                    Name = "Cassowary",
                    IsDangerous = true,
                    CanFly = false,
                    Wingspan = 30
                }
            });

            //Search for documents

            var safeBirdsResults = client
                .Search<Bird>(_ => _
                    .Query(_ =>
                        _.Term(animal => animal.IsDangerous, false)
                    )
                )
                .Hits
                .Select(h => h.Source)
                .ToList();
            //This returns a Pigeon object matching the original object

            var safeAnimalResults = client
                .Search<Animal>(_ => _
                    .Index(Indices
                        .Index(typeof(Fish))
                        .And(typeof(Bird))
                    )
                    .Query(_ => 
                        _.Term(animal => animal.IsDangerous, false) 
                    )
                )
                .Hits
                .Select(h => h.Source)
                .ToList();
            //This returns two Animal objects matching the original Pigeon and Goldfish, but with only base class properties
            //Making Animal an abstract class will throw an exception here.

            Console.WriteLine("Hello World!");
        }
    }
}
