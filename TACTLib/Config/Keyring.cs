using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
                string reverseKey = pair.Key.Substring(pair.Key.Length - 16);
                string keyIDString = "";
                for (var i = 0; i < 8; ++i) {
                    keyIDString = reverseKey.Substring(i * 2, 2) + keyIDString;
                }

                var keyID = ulong.Parse(keyIDString, NumberStyles.HexNumber);
                Keys[keyID] = StringToByteArray(pair.Value[0]);
            }
        }

        public void AddKey(ulong keyName, byte[] value) {
            if (keyName == 0) {
                return;
            }

            Keys[keyName] = value;
        }

        public void LoadSupportFile(string path) {
            var debugFileKeyCache = new Dictionary<ulong, byte[]>();
            using (TextReader r = new StreamReader(path)) {
                string? line;
                while ((line = r.ReadLine()) != null) {
                    line = line.Trim().Split(new[] { '#' }, StringSplitOptions.None)[0].Trim();
                    if (string.IsNullOrWhiteSpace(line)) {
                        continue;
                    }

                    string[] c = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

                    if (debugFileKeyCache.ContainsKey(v))
                        Logger.Debug("TACT", $"Duplicate key detected in keyring file. {c[0]}");
                    else
                        debugFileKeyCache.Add(v, keyByte);

                    if (enabled) {
                        if (!Keys.ContainsKey(v)) {
                            Keys.Add(v, keyByte);
                        }
                    } else {
                        if (Keys.ContainsKey(v)) {
                            Keys.Remove(v);
                        }
                    }
                }
            }
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
