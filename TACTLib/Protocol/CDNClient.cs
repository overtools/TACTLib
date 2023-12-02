using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using TACTLib.Client;

namespace TACTLib.Protocol {
    public class CDNClient
    {
        private static readonly HttpClientHandler s_httpClientHandler = new HttpClientHandler
        {
            // unlikely to be supported by cdn but...
            AutomaticDecompression = DecompressionMethods.All
        };
        public static readonly HttpClient s_httpClient = new HttpClient(s_httpClientHandler);
        
        private readonly ClientHandler m_client;

        public CDNClient(ClientHandler handler)
        {
            m_client = handler;
        }

        public byte[]? OpenData(CKey key)
        {
            return FetchCDN("data", key.ToHexString());
        }

        public Stream? OpenConfig(string key)
        {
            var data = FetchCDN("config", key);
            if (data == null) return null;
            return new MemoryStream(data);
        }

        public byte[]? FetchCDN(string type, string key, (int start, int end)? range=null, string? suffix=null)
        {
            key = key.ToLowerInvariant();
            
            var hosts = m_client.InstallationInfo.Values["CDNHosts"].Split(' ');
            foreach (var host in hosts)
            {
                if (host.EndsWith("cdn.blizzard.com"))
                {
                    continue; // these still dont work very well..
                }
                
                var url = $"http://{host}/{m_client.InstallationInfo.Values["CDNPath"]}/{type}/{key.AsSpan(0, 2)}/{key.AsSpan(2, 2)}/{key}{suffix}";
                Logger.Info("CDN", $"Fetching file {url}");

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                if (range != null)
                {
                    requestMessage.Headers.Range = new RangeHeaderValue(range.Value.start, range.Value.end);
                }
                
                try 
                {
                    using var response = s_httpClient.Send(requestMessage, HttpCompletionOption.ResponseHeadersRead);
                    if (!response.IsSuccessStatusCode)
                    {
                        continue;
                    }
                    
                    var result = response.Content.ReadAsByteArrayAsync().Result; // todo: async over sync
                    return result;
                } catch (Exception e) {
                    // ignored
                    Logger.Debug("CDN", $"Error fetching {url}: {e}");
                }
            }

            return null;
        }
    }
}
