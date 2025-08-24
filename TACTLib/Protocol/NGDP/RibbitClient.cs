using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using MimeKit.Text;

namespace TACTLib.Protocol.NGDP
{
    public class RibbitClient : NGDPClientBase
    {
        public readonly Uri m_uri;
        
        public RibbitClient(string host)
        {
            if (!Uri.TryCreate(host, UriKind.RelativeOrAbsolute, out var uri)) {
                throw new Exception($"unable to create uri from ribbit host: \"{host}\"");
            }
            m_uri = uri;
        }
        
        public override string Get(string query)
        {
            Logger.Info(nameof(RibbitClient), $"Fetching Ribbit {query}");
            
            using var client = new TcpClient();
            client.Connect(m_uri.Host, m_uri.Port);
                
            using var stream = client.GetStream();
            using (var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true))
            {
                writer.WriteLine(query);
                writer.Flush();
            }
                
            var message = MimeMessage.Load(stream);
            return GetTextBody(message);
        }
        
        public override async Task<string> GetAsync(string query, CancellationToken cancellationToken=default)
        {
            Logger.Info(nameof(RibbitClient), $"Fetching Ribbit {query}");
            
            using var client = new TcpClient();
            await client.ConnectAsync(m_uri.Host, m_uri.Port, cancellationToken);

            await using var stream = client.GetStream();
            await using (var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true))
            {
                await writer.WriteLineAsync(query.AsMemory(), cancellationToken); // todo: why can't pass string + cancellationtoken? 
                await writer.FlushAsync(cancellationToken);
            }
                
            var message = await MimeMessage.LoadAsync(stream, cancellationToken);
            return GetTextBody(message);
        }
        
        private static string GetTextBody(MimeMessage message)
        {
            // return message.GetTextBody(TextFormat.Text);
            // todo: using this (^) pulls in HtmlEntityDecoder which is about 500kb in size
            // mimekit needs an overload where the encoding can be specified instead of auto-detecting
            
            var body = (MultipartAlternative)message.Body;
            if (!body.TryGetValue(TextFormat.Plain, out var textPart))
            {
                throw new InvalidDataException("ribbit message had no text part");
            }
            return textPart.GetText(Encoding.UTF8);
        }
        
        public override string GetVersionsQuery(string product) => $"v1/products/{product}/versions";
        public override string GetCDNsQuery(string product) => $"v1/products/{product}/cdns";
        public override string GetBGDLsQuery(string product) => $"v1/products/{product}/bgdl";
        
        public List<Dictionary<string, string>> GetSummary() => GetKV("v1/summary");
        public Task<List<Dictionary<string, string>>> GetSummaryAsync(CancellationToken cancellationToken=default) => GetKVAsync("v1/summary", cancellationToken);
    }
}
