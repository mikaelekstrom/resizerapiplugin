using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using ImageResizer.Configuration;
using ImageResizer.Plugins;
using Newtonsoft.Json;

namespace RestAPIImageRetriever
{
    public class RestAPIImageRetriever : IPlugin, IVirtualImageProvider
    {
        public IPlugin Install(Config c)
        {
            c.Plugins.add_plugin(this);
            return this;
        }

        public bool Uninstall(Config c)
        {
            c.Plugins.remove_plugin(this);
            return true;
        }

        private void LogFileAppender(string log)
        {
            var logFileName = "C:\\temp\\RestAPIImageRetriever.txt";
            File.AppendAllLines(logFileName, new List<string> { $"{DateTime.UtcNow} :: {log}" });
        }


        static readonly HttpClient client = new HttpClient();

        static readonly string API_BASE = "rest/V1.0/list/MediaAsset/bySearch?query=MediaAssetDocument.Identifier(eng,originalimage) EQUALS ";

        public RestAPIImageRetriever()
        {
            try
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["RestAPIBaseURI"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                LogFileAppender("Plugin has started!");
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

        public bool FileExists(string virtualPath, NameValueCollection queryString)
        {
            LogFileAppender($"trying to see if file {virtualPath} exists");
            var data = GetDataFromAPI(virtualPath);
            LogFileAppender($"{virtualPath} file exists: {data.RowCount == "1"}");
            return data.RowCount == "1";
        }

        private EntityItemTable GetDataFromAPI(string virtualPath)
        {
            var response = client.GetAsync(API_BASE + virtualPath.Split('/').ToList().Last()).Result;
            LogFileAppender($"{response.Headers.Location}");
            return JsonConvert.DeserializeObject<EntityItemTable>(response.Content.ToString());
        }

        public IVirtualFile GetFile(string virtualPath, NameValueCollection queryString)
        {
            var data = GetDataFromAPI(virtualPath);
            return new BasicVirtualFile(virtualPath, data.Rows.Row.Values.Value.Text);
        }
    }

    public class BasicVirtualFile : IVirtualFile
    {
        private readonly string url;

        public string VirtualPath { get; }

        public BasicVirtualFile(string virtualPath, string url)
        {
            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                throw new ArgumentException("message", nameof(virtualPath));
            }
            VirtualPath = virtualPath;
            this.url = url;
        }

        public Stream Open()
        {
            byte[] fileData = null;
            using (var wc = new System.Net.WebClient())
                fileData = wc.DownloadData(url);

            return new MemoryStream(fileData);
        }
    }
}
