using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FunctionApp2
{
    public static class Test1FnApp
    {
        [FunctionName("Test1FnApp")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string Website = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "url", true) == 0)
                .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            var result = Website == null ? req.CreateResponse(HttpStatusCode.OK,"Add a 'url' to the query string") : GetContent(Website,req);


            return result;
        }

        public static HttpResponseMessage GetContent(string url, HttpRequestMessage req)
        {
            HttpResponseMessage parseResult;

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(url);
            var node = htmlDoc.DocumentNode.SelectSingleNode("//head/title");
            var blobId = SaveContent(node.Name);
            parseResult = req.CreateResponse(HttpStatusCode.OK, "Node Name: " + node.Name + "\n" + node.OuterHtml + "Blob Id: " + blobId);
            
            return parseResult;
        }

        public static async Task<string> SaveContent(string data)
        {
            var _baseUrl = new Uri("https://weatherappstorage2018.blob.core.windows.net/");
            var _storageCreds = new StorageCredentials("weatherappstorage2018", System.Environment.GetEnvironmentVariable("BlobStorageKey") );
            var _client = new CloudBlobClient(_baseUrl, _storageCreds);
            var id = Guid.NewGuid().ToString();
            var container = _client.GetContainerReference("test-blob");
            var blob = container.GetBlockBlobReference(id);
            await blob.UploadTextAsync(data);
            return id;

        }
    }
}
