using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ImageResizer.Storage;
using Newtonsoft.Json;

namespace RestAPIImageRetriever
{
    public class RestAPIImageRetriever : BlobProviderBase
    {
        static readonly HttpClient client = new HttpClient();

        static readonly string API_BASE = "rest/V2.0/list/MediaAssetFile/byIdentifiers?identifiers=";
        static readonly string LAST_MODIFIED = "&fields=MediaAssetFileAttribute.LastModified";
        static readonly string FILE_URL = "&fields=MediaAssetFileAttribute.HTTPPath";


        public RestAPIImageRetriever()
        {
            try
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["RestAPIBaseURI"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var authString = $"{ConfigurationManager.AppSettings["RestAPIUser"]}:{ConfigurationManager.AppSettings["RestAPIPassword"]}";
                var byteArray = Encoding.ASCII.GetBytes(authString);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                LogFileAppender("Plugin has started!");
            }
            catch (Exception e)
            {
                LogFileAppender($"Error thrown during startup, {e}");
            }  
        }

        public override Task<IBlobMetadata> FetchMetadataAsync(string virtualPath, NameValueCollection queryString)
        {
            LogFileAppender($"DEBUG :: Trying to get meta data for virtualPath {virtualPath}");
            try
            {
                LogFileAppender($"DEBUG :: Trying to get meta data for virtualPath {virtualPath}");
                var data = GetDataFromAPI(virtualPath, true);
                var result = new BlobMetadata { Exists = data.RowCount == "1" };
                if (result.Exists.Value)
                    result.LastModifiedDateUtc = DateTime.Parse(data.Rows.Row.Values.Value.Text);

                return Task.FromResult<IBlobMetadata>(result);
            }
            catch (Exception e)
            {
                LogFileAppender($"ERROR :: Something went wrong when trying to fetch metadata, {e}");
                return Task.FromResult<IBlobMetadata>(new BlobMetadata { Exists = false });
            }   
        }

        public override Task<Stream> OpenAsync(string virtualPath, NameValueCollection queryString)
        {
            LogFileAppender($"DEBUG :: Trying to get stream for virtualPath {virtualPath}");
            try
            {
                LogFileAppender($"DEBUG :: Trying to get stream for virtualPath {virtualPath}");
                var data = GetDataFromAPI(virtualPath, false);
                if (data == null)
                    throw new NullReferenceException($"ERROR :: No data found for virtual path {virtualPath}");
                if (data.RowCount != "1")
                    throw new ArgumentException($"ERROR :: The path {virtualPath} did not return 1 row!");

                var stream = GetStreamFromUrl(data.Rows.Row.Values.Value.Text);
                stream.Seek(0, SeekOrigin.Begin);
                return Task.FromResult<Stream>(stream);
            }
            catch (Exception e)
            {
                LogFileAppender($"ERROR :: Something went wrong when trying to open stream for virtual path {virtualPath}, {e}");
                return null;
            }
        }

        private EntityItemTable GetDataFromAPI(string virtualPath, bool metaDataCall)
        {
            var response = client.GetAsync(GetPath(virtualPath, metaDataCall)).Result;
            LogFileAppender($"DEBUG :: Location: {response.Headers.Location}");
            return JsonConvert.DeserializeObject<EntityItemTable>(response.Content.ToString());
        }

        private static string GetPath(string virtualPath, bool metaDataCall)
        {
            var id = virtualPath.Split('/').ToList().Last();
            var field = metaDataCall ? LAST_MODIFIED : FILE_URL;
            return API_BASE + id + field;
        }

        private static Stream GetStreamFromUrl(string url)
        {
            byte[] imageData = null;

            using (var wc = new System.Net.WebClient())
                imageData = wc.DownloadData(url);

            return new MemoryStream(imageData);
        }

        private void LogFileAppender(string log)
        {
            var logFileName = "C:\\temp\\RestAPIImageRetriever.txt";
            File.AppendAllLines(logFileName, new List<string> { $"{DateTime.UtcNow} :: {log}" });
        }
    }
}
