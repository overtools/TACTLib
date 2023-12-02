using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TACTLib.Config;

namespace TACTLib.Protocol.NGDP
{
    public abstract class NGDPClientBase
    {
        public abstract string Get(string query);
        public abstract Task<string> GetAsync(string query, CancellationToken cancellationToken=default);
        
        public List<Dictionary<string, string>> GetKV(string query)
        {
            var responseString = Get(query);
            using var streamReader = new StringReader(responseString);
            return InstallationInfo.ParseToDict(streamReader);
        }
        
        public async Task<List<Dictionary<string, string>>> GetKVAsync(string query, CancellationToken cancellationToken=default)
        {
            var responseString = await GetAsync(query, cancellationToken);
            using var streamReader = new StringReader(responseString);
            return InstallationInfo.ParseToDict(streamReader);
        }
        
        public abstract string GetVersionsQuery(string product);
        public abstract string GetCDNsQuery(string product);
        public abstract string GetBGDLsQuery(string product);
        
        public List<Dictionary<string, string>> GetVersions(string product) => GetKV(GetVersionsQuery(product));
        public List<Dictionary<string, string>> GetCDNs(string product) => GetKV(GetCDNsQuery(product));
        public List<Dictionary<string, string>> GetBGDLs(string product) => GetKV(GetBGDLsQuery(product));
        
        public Dictionary<string, string>? GetVersion(string product, string region) => GetVersions(product).FirstOrDefault(x => x["Region"] == region);
        public Dictionary<string, string>? GetCDN(string product, string region) => GetCDNs(product).FirstOrDefault(x => x["Name"] == region);
        public Dictionary<string, string>? GetBGDL(string product, string region) => GetBGDLs(product).FirstOrDefault(x => x["Region"] == region);
        
        public Task<List<Dictionary<string, string>>> GetVersionsAsync(string product, CancellationToken cancellationToken=default) => GetKVAsync(GetVersionsQuery(product), cancellationToken);
        public Task<List<Dictionary<string, string>>> GetCDNsAsync(string product, CancellationToken cancellationToken=default) => GetKVAsync(GetCDNsQuery(product), cancellationToken);
        public Task<List<Dictionary<string, string>>> GetBGDLsAsync(string product, CancellationToken cancellationToken=default) => GetKVAsync(GetBGDLsQuery(product), cancellationToken);
        
        public async Task<Dictionary<string, string>?> GetVersionAsync(string product, string region, CancellationToken cancellationToken=default) => 
            (await GetVersionsAsync(product, cancellationToken)).FirstOrDefault(x => x["Region"] == region);
        public async Task<Dictionary<string, string>?> GetCDNAsync(string product, string region, CancellationToken cancellationToken=default) => 
            (await GetCDNsAsync(product, cancellationToken)).FirstOrDefault(x => x["Name"] == region);
        public async Task<Dictionary<string, string>?> GetBGDLAsync(string product, string region, CancellationToken cancellationToken=default) => 
            (await GetBGDLsAsync(product, cancellationToken)).FirstOrDefault(x => x["Region"] == region);
        
        private static readonly Dictionary<string, string> RenameMapVersions = new Dictionary<string, string>
        {
            {"Region", "Branch"},
            {"BuildConfig", "BuildKey"},
            {"CDNConfig", "CDNKey"},
            {"KeyRing", "Keyring"},
            {"VersionsName", "Version"}
        };

        private static readonly Dictionary<string, string> RenameMapCDNs = new Dictionary<string, string>
        {
            {"Path", "CDNPath"},
            {"Hosts", "CDNHosts"},
            {"Servers", "CDNServers"}
        };
        
        public Dictionary<string, string> CreateInstallationInfo(string product, string region)
        {
            var cdns = GetCDN(product, region);
            var version = GetVersion(product, region);
            
            var map = new Dictionary<string, string> {
                {"Active", "1"},
                {"InstallKey", ""},
                {"IMSize", ""},
                {"Armadillo", ""},
                {"LastActivated", "0"},
                {"BuildComplete", "1"}
            };

            if (version != null) {
                foreach (var remap in RenameMapVersions) {
                    map[remap.Value] = version[remap.Key];
                }
            }

            if (cdns != null) {
                foreach (var remap in RenameMapCDNs) {
                    map[remap.Value] = cdns[remap.Key];
                }
            }

            return map;
        }
    }
}