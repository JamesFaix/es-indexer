using System;
using System.Threading.Tasks;

namespace PowerDms.EsIndexer
{
    class Program
    {
        const string _esDomainUrl = "https://search-james-elastic-test-2-tfm7mjwro5ijmarafnmeqpzhaa.us-east-1.es.amazonaws.com";
        const string _esIndexName = "james-elastic-test-2";
        const string _esPipelineName = "documentpipeline";
        const string _sampleFilesDirectory = @"C:\Users\james.faix\Desktop\ES supported file types - sample files-20190917T180139Z-001\ES supported file types - sample files";

        static async Task Main(string[] args)
        {
            var esService = new EsService(_esDomainUrl, _esIndexName, _esPipelineName);
            var fileService = new FileService();

            //Clear out any documents uploaded from the last run of this test app
            await esService.ClearIndex();

            var files = fileService.EnumerableSampleFiles(_sampleFilesDirectory);

            foreach (var f in files)
            {
                Console.WriteLine($"Uploading {f.Name}...");
                await esService.UploadDocument(f);
                Console.WriteLine($"Upload of {f.Name} complete.");
            }

            Console.Read();
        }
    }
}
