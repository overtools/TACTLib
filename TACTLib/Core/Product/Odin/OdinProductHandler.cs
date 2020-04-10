using System.IO;
using TACTLib.Client;

namespace TACTLib.Core.Product.Odin {
	[ProductHandler(TACTProduct.ModernWarfare)]
	public class OdinProductHandler : IProductHandler {
		private ClientHandler _client;

		// ReSharper disable once UnusedParameter.Local
		public OdinProductHandler(ClientHandler client, Stream ignored) {
			_client = client;
		}

		/// <inheritdoc />
		public Stream OpenFile(object key) {
			if (key is string fileKey) {
				return _client.VFS.Open(fileKey);
			}

			return null;
		}
	}
}
