using Nest;
using System;
using System.Linq;

namespace multiple_indexer
{
    class Program
    {
        const string _EsEndpoint = @"https://search-james-elastic-test-l5km3c47amty77yof7hdwsntu4.us-east-1.es.amazonaws.com";

        static void Main(string[] args)
        {
            //Setup connection

            var settings = new ConnectionSettings(new Uri(_EsEndpoint))
                .DefaultMappingFor<Fish>(_ => _.IndexName("fishes1"))
                .DefaultMappingFor<Bird>(_ => _.IndexName("birds1"));

            var client = new ElasticClient(settings);

            //Insert some documents
            
            //There doesn't seem to be a simple way to index a list of Animals and 
            //have it figure out which index to send Fish and Birds to.
            
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

            //This returns a Pigeon object matching the original object
            var safeBirdsResults = client
                .Search<Bird>(_ => _
                    .Query(_ =>
                        _.Term(animal => animal.IsDangerous, false)
                    )
                )
                .Hits
                .Select(h => h.Source)
                .ToList();

            //This returns two Animal objects matching the original Pigeon and Goldfish, but with only base class properties
            //Making Animal an abstract class will throw an exception here.
            var safeAnimalResults = client
                .Search<Animal>(_ => _
                    .Index(Indices
                        .Index(typeof(Fish))
                        .And(typeof(Bird))
                    )
                    .Query(_ =>
                        _.Term(animal => animal.IsDangerous, false)
                    )
                    .Size(1000)
                )
                .Hits
                .Select(h => h.Source)
                .ToList();

            //This returns a Fish and Bird object matching the original Pigeon and Goldfish
            var safeBirdResults2 = client
                .Search<Bird>(_ => _
                    .Query(_ =>
                        _.Term(bird => bird.IsDangerous, false)
                    )
                    .Size(1000)
                );

            var safeFishResults2 = client
                .Search<Fish>(_ => _
                    .Query(_ =>
                        _.Term(fish => fish.IsDangerous, false)
                    )
                    .Size(1000)
                );

            var allResults = safeBirdResults2.Hits.Cast<IHit<Animal>>()
                .Concat(safeFishResults2.Hits.Cast<IHit<Animal>>())
                .OrderByDescending(h => h.Score)
                .Select(h => h.Source)
                .ToList();

            Console.WriteLine("Hello World!");
        }
    }
}
