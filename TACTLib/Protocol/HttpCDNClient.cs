using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using TACTLib.Client;

namespace TACTLib.Protocol {
    public class HttpCDNClient : ICDNClient
    {
        private static readonly HttpClientHandler s_httpClientHandler = new HttpClientHandler
        {
            // unlikely to be supported by cdn but...
            AutomaticDecompression = DecompressionMethods.All
        };

        private readonly HttpClient m_httpClient;
        private ClientHandler m_client;

        public HttpCDNClient(HttpClient? httpClient)
        {
            m_httpClient = httpClient ?? new HttpClient(s_httpClientHandler);
        }

        public virtual void SetClientHandler(ClientHandler handler)
        {
            m_client = handler;
        }

        public virtual byte[]? Fetch(string type, string key, Range? range=null, string? suffix=null)
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
                    requestMessage.Headers.Range = new RangeHeaderValue(range.Value.Start.Value, range.Value.End.Value);
                }
                
                try 
                {
                    using var response = m_httpClient.Send(requestMessage, HttpCompletionOption.ResponseHeadersRead);
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
