using Nest;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PowerDms.EsIndexer
{
    public class EsService
    {
        private readonly string _domainUrl;
        private readonly string _indexName;
        private readonly string _pipelineName;

        private readonly ElasticClient _esClient;
        private readonly HttpClient _httpClient;

        public EsService(
            string domainUrl,
            string indexName,
            string pipelineName)
        {
            _domainUrl = domainUrl;
            _indexName = indexName;
            _pipelineName = pipelineName;

            var esSettings = new ConnectionSettings(new Uri(_domainUrl))
                .DefaultIndex(_indexName);
            _esClient = new ElasticClient(esSettings);

            _httpClient = new HttpClient();
        }

        public async Task UploadDocument(SampleFile file)
        {
            await _esClient.IndexAsync(file, x =>
                x.Index(_indexName)
                    .Pipeline(_pipelineName));
        }

        /// <summary>
        /// Create a ingest attachment pipeline. You should only do this once after 
        /// you setup the index, not every time you run this test app.
        /// </summary>
        public async Task CreatePipeline()
        {
            var url = $"{_domainUrl}/_ingest/pipeline/{_pipelineName}";

            //NEST may support this in a cleaner way, not sure

            var body = new
            {
                description = "This is a pipeline!",
                processors = new[] {
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

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to create pipeline");
            }
        }

        private static StringContent ToJsonBody(object obj)
        {
            return new StringContent(
                JsonConvert.SerializeObject(obj),
                Encoding.UTF8,
                "application/json");
        }

        /// <summary>
        /// Deletes all documents in the index
        /// </summary>
        public async Task ClearIndex()
        {
            var req = new DeleteByQueryRequest(_indexName)
            {
                Query = new QueryStringQuery
                {
                    Query = "*"
                }
            };

            await _esClient.DeleteByQueryAsync(req);
        }
    }
}
