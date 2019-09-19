using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Newtonsoft.Json;

namespace PowerDms.EsIndexer
{
    class Program
    {
        static readonly string _EsDomainUrl;
        static readonly string _EsIndexName;
        static readonly string _EsPipelineName;

        static readonly HttpClient _HttpClient;
        static readonly ElasticClient _ElasticClient;

        static Program() {
            _EsDomainUrl = "https://search-james-elastic-test-2-tfm7mjwro5ijmarafnmeqpzhaa.us-east-1.es.amazonaws.com";
            _EsIndexName = "james-elastic-test-2";
            _EsPipelineName = "documentpipeline";

            var esSettings = new ConnectionSettings(new Uri(_EsDomainUrl))
                .DefaultIndex(_EsIndexName);
            _ElasticClient = new ElasticClient(esSettings);

            _HttpClient = new HttpClient();
        }

        static async Task Main(string[] args)
        {
            await SetupIngestPipeline();

            await PutDocumentToExtract();

            Console.Read();
        }

        static async Task SetupIngestPipeline()
        {
            var url = $"{_EsDomainUrl}/_ingest/pipeline/{_EsPipelineName}";

            //NEST may support this in a cleaner way, not sure

            var body = new {
                description = "This is a pipeline!",
                processors = new [] {
                    new {
                        attachment = new {
                            field = "data",
                            properties = new [] {
                                "content",
                                "content_type",
                                "content_length"
                            }
                        }
                    }
                }
            };

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Put,
                Content = ToJsonBody(body)
            };

            var response = await _HttpClient.SendAsync(request);

            Console.WriteLine($"Request responded with status {response.StatusCode}.");
        }

        static async Task PutDocument()
        {
            var body = new
            {
                name = "Henry",
                age = 1,
                isDeveloper = false
            };

            await _ElasticClient.IndexDocumentAsync(body);
            Console.Write("Put document to ES");
        }

        static async Task PutDocumentToExtract()
        {
            var text = "test document contents";

            var body = new
            {
                name = "This is a new test document " + DateTime.UtcNow,
                data = Base64Encode(text),
            };

            await _ElasticClient.IndexAsync(body, x => 
                x.Index(_EsIndexName)
                    .Pipeline(_EsPipelineName));

            Console.Write("Put document to ES for extraction");
        }

        static StringContent ToJsonBody(object obj)
        {
            return new StringContent(
                JsonConvert.SerializeObject(obj),
                Encoding.UTF8,
                "application/json");
        }

        static string Base64Encode(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }
    }
}
