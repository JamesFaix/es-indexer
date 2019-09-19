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
        const string _outputFile = @"C:\Users\james.faix\Desktop\ES supported file types - sample files-20190917T180139Z-001\results.csv";

        static async Task Main(string[] args)
        {
            var esService = new EsService(_esDomainUrl, _esIndexName, _esPipelineName);
            var fileService = new FileService();

            //Clear out any documents uploaded from the last run of this test app
            await esService.ClearIndex();

            var fileCount = 0;

            Console.WriteLine("Searching target folder for files to upload...");
            var sourceFiles = fileService.EnumerableSampleFiles(_sampleFilesDirectory);

            foreach (var f in sourceFiles)
            {
                Console.WriteLine($"Uploading {f.Name}...");
                await esService.UploadDocument(f);
                Console.WriteLine($"Upload of {f.Name} complete.");
                fileCount++;
            }

            Console.WriteLine("Querying ES for extraction results...");
            var extractedFiles = await esService.GetExtractedFiles(fileCount);

            Console.WriteLine("Writing results to CSV file...");
            fileService.WriteFile(_outputFile, extractedFiles);

            Console.WriteLine("Done");
            Console.Read();
        }
    }
}
