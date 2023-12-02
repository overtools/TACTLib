using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TACTLib.Protocol.NGDP {
    public class NGDPClient : NGDPClientBase
    {
        private readonly string m_host;
        
        public NGDPClient(string host)
        {
            m_host = host;
        }

        public override string Get(string query)
        {
            Logger.Info("CDN", $"Fetching NGDP {query}");
            using var response = CDNClient.s_httpClient.Send(new HttpRequestMessage(HttpMethod.Get, $"{m_host}/{query}"));
            return response.Content.ReadAsStringAsync().Result; // todo: async over sync
        }

        public override async Task<string> GetAsync(string query, CancellationToken cancellationToken = default)
        {
            Logger.Info("CDN", $"Fetching NGDP {query}");
            using var response = await CDNClient.s_httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"{m_host}/{query}"), cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        
        public override string GetVersionsQuery(string product) => $"{product}/versions";
        public override string GetCDNsQuery(string product) => $"{product}/cdns";
        public override string GetBGDLsQuery(string product) => $"{product}/bgdl";
    }
}
