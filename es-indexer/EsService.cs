using Polly;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Policy = Polly.Policy;

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

        public async Task TearDownPipeline()
        {
            var url = $"{_domainUrl}/_ingest/pipeline/{_pipelineName}";

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Delete
            };

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to delete pipeline");
            }
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
                processors = new object [] {
                    new {
                        attachment = new {
                            field = "data",
                            properties = new [] {
                                "content",
                            }
                        },
                    },
                    new {
                        gsub = new {
                            field = "attachment.content",
                            pattern = @"\s+",
                            replacement = " "
                        }
                    }
                },
                on_failure = new object[] {
                    new {
                        set = new {
                            field = "error",
                            value = "Text extraction failed."
                        }
                    },
                    new {
                        set = new {
                            field = "attachment.content",
                            value = ""
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
                Query = EverythingQuery()
            };

            await _esClient.DeleteByQueryAsync(req);
        }

        public async Task<IList<ExtractedFile>> GetExtractedFiles(int expectedDocumentCount)
        {
            var req = new SearchRequest<ExtractedFile>(_indexName)
            {
                Query = EverythingQuery(),
                Size = 1000 //Limit of max results returned at a time
            };

            return await Policy
                .HandleResult<IList<ExtractedFile>>(
                    /* 
                     * This will retry when the condition is FALSE, 
                     * but it won't throw an exception if it stays FALSE after all retries.
                     * This is what we want here, because we want to retry in the case that only 
                     * 20/30 docs have been processed by the ingest pipeline so far.
                     * But some docs (those with passwords) will fail in the pipeline and will never be returned.
                     * So we may never get 30/30 back, but we should pause for a few seconds and give ES a chance to catch up.
                     */
                    files => files.Count != expectedDocumentCount
                )
                .WaitAndRetryAsync(
                    new[] { 1, 3, 5 }
                        .Select(n => TimeSpan.FromSeconds(n))
                )
                .ExecuteAsync(async () =>
                {
                    var resp = await _esClient.SearchAsync<ExtractedFile>(req);
                    var docs = resp.Documents.ToList();
                    return docs;
                });
        }

        private QueryContainer EverythingQuery()
        {
            return new QueryStringQuery
            {
                Query = "*"
            };
        }
    }
}
