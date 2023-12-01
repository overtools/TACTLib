using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using TACTLib.Client;
using static TACTLib.Utils;

namespace TACTLib.Config {
    public class Keyring : Config {
        /// <summary>
        /// Keyring keys
        /// </summary>
        public readonly Dictionary<ulong, byte[]> Keys;

        public Keyring(Stream? stream) : base(stream) {
            Keys = new Dictionary<ulong, byte[]>();
            foreach (var pair in Values) {
                var reverseKey = pair.Key.AsSpan(pair.Key.Length - 16);
                var keyID = ulong.Parse(reverseKey, NumberStyles.HexNumber);
                keyID = BinaryPrimitives.ReverseEndianness(keyID); // unconditional be -> le
                
                Keys[keyID] = StringToByteArray(pair.Value[0]);
            }
        }

        public void AddKey(ulong keyName, byte[] value) {
            if (keyName == 0) {
                return;
            }

            Keys[keyName] = value;
        }

        private static Dictionary<ulong, byte[]?> ParseSupportFile(Stream stream) {
            var keys = new Dictionary<ulong, byte[]?>();

            using TextReader r = new StreamReader(stream);
            string? line;
            while ((line = r.ReadLine()) != null) {
                line = line.Trim().Split('#')[0].Trim();
                if (string.IsNullOrWhiteSpace(line)) {
                    continue;
                }

                string[] c = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (c.Length < 2) {
                    continue;
                }

                var enabled = true;
                if (c.Length >= 3) {
                    enabled = c[2] == "1";
                }

                ulong v;
                try {
                    v = ulong.Parse(c[0], NumberStyles.HexNumber);
                } catch {
                    continue;
                }

                var keyByte = StringToByteArray(c[1]);

                if (keys.ContainsKey(v))
                    Logger.Debug("TACT", $"Duplicate key detected in keyring file. {c[0]}");

                if (enabled) {
                    keys[v] = keyByte;
                } else {
                    keys[v] = null;
                }
            }

            return keys;
        }

        private static Dictionary<ulong, byte[]?>? LoadSupportFileFromDisk(string filePath) {
            if (!File.Exists(filePath)) {
                Logger.Warn("TACT", $"Keyring file {filePath} not found");
                return null;
            }

            try {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return ParseSupportFile(fileStream);
            } catch (Exception ex) {
                Logger.Warn("TACT", $"Failed to loading keyring from disk: {ex.Message}");
            }
            return null;
        }

        private static Dictionary<ulong, byte[]?>? LoadSupportFileFromRemote(string url) {
            try {
                Logger.Debug("TACT", $"Loading keyring from remote: {url}");
                var response = new HttpClient().Send(new HttpRequestMessage(HttpMethod.Get, url));
                response.EnsureSuccessStatusCode();

                using var stream = response.Content.ReadAsStream();
                return ParseSupportFile(stream);
            } catch (Exception ex) {
                Logger.Warn("TACT", $"Failed to load keyring from remote: {ex.Message}");
                Logger.Warn("TACT", "Some content may be unavailable");
            }
            return null;
        }

        private void AddLocalKeyringKeys(Dictionary<ulong, byte[]?>? keys) {
            if (keys == null) return;

            foreach (var keyPair in keys) {
                if (keyPair.Value != null) {
                    if (!Keys.ContainsKey(keyPair.Key)) {
                        AddKey(keyPair.Key, keyPair.Value);
                    }
                } else {
                    if (Keys.ContainsKey(keyPair.Key)) {
                        Keys.Remove(keyPair.Key);
                    }
                }
            }
        }

        private void AddRemoteKeyringKeys(Dictionary<ulong, byte[]?>? remoteKeys, Dictionary<ulong, byte[]?>? localKeys) {
            if (remoteKeys == null) return;

            var remoteAddedCount = 0;
            foreach (var remoteKeyPair in remoteKeys) {
                // ignore whatever remote has if local file has it
                if (localKeys != null && localKeys.ContainsKey(remoteKeyPair.Key)) continue;

                // disabled in remote lol...
                if (remoteKeyPair.Value == null) continue;

                AddKey(remoteKeyPair.Key, remoteKeyPair.Value);
                remoteAddedCount++;
            }

            if (remoteAddedCount > 0) {
                Logger.Info("TACT", $"Downloaded {remoteAddedCount} key(s) from remote keyring");
            }
        }

        public void LoadSupportKeyrings(ClientHandler client) {
            Dictionary<ulong, byte[]?>? localKeys = null;
            Dictionary<ulong, byte[]?>? remoteKeys = null;

            if (client.CreateArgs.LoadSupportKeyring) {
                var keyFileName = client.CreateArgs.SupportKeyring ?? $@"{client.Product:G}.keyring";
                var keyFile = Path.Combine(AppContext.BaseDirectory, keyFileName);

                localKeys = LoadSupportFileFromDisk(keyFile);
            }

            if (!string.IsNullOrEmpty(client.CreateArgs.RemoteKeyringUrl)) {
                remoteKeys = LoadSupportFileFromRemote(client.CreateArgs.RemoteKeyringUrl);
            }

            AddLocalKeyringKeys(localKeys);
            AddRemoteKeyringKeys(remoteKeys, localKeys);
        }

        /// <summary>
        /// Get encryption key value
        /// </summary>
        /// <param name="keyID">Target key id</param>
        /// <returns>Key value</returns>
        public byte[]? GetKey(ulong keyID) {
            Keys.TryGetValue(keyID, out var key);
            return key;
        }
    }
}