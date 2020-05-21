using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using TACTLib.Client;
using TACTLib.Core.Product.Tank;
using static TACTLib.Utils;

namespace TACTLib.Core {
    public class Keyring : Config.Config {
        /// <summary>
        /// Keyring keys
        /// </summary>
        public readonly Dictionary<ulong, byte[]> Keys;

        public Keyring(ClientHandler client, Stream stream) : base(client, stream) {
            Keys = new Dictionary<ulong, byte[]>();
            foreach (KeyValuePair<string,List<string>> pair in Values) {
                string reverseKey = pair.Key.Substring(pair.Key.Length - 16);
                string keyIDString = "";
                for (int i = 0; i < 8; ++i) {
                    keyIDString = reverseKey.Substring(i * 2, 2) + keyIDString;
                }
                
                ulong keyID = ulong.Parse(keyIDString, NumberStyles.HexNumber);
                Keys[keyID] = StringToByteArray(pair.Value[0]);
                
                //Console.Out.WriteLine(pair.Value[0]);
            }


            if (client.CreateArgs.LoadSupportKeyring) {
                string keyFile = client.CreateArgs.SupportKeyring ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @$"\{client.Product:G}.keyring";
                if (File.Exists(keyFile)) {
                    LoadSupportFile(keyFile);
                }
            }
        }

        public void AddKey(ulong keyName, byte[] value) {
            Keys[keyName] = value;
        }

        public void LoadSupportFile(string path) {
            using (TextReader r = new StreamReader(path)) {
                string line;
                while ((line = r.ReadLine()) != null) {
                    line = line.Trim().Split(new[] {'#'}, StringSplitOptions.None)[0].Trim();
                    if (string.IsNullOrWhiteSpace(line)) {
                        continue;
                    }
                    string[] c = line.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    if (c.Length < 2) {
                        continue;
                    }
                    bool enabled = true;
                    if (c.Length >= 3) {
                        enabled = c[2] == "1";
                    }

                    ulong v;
                    try {
                        v = ulong.Parse(c[0], NumberStyles.HexNumber);
                    } catch {
                        continue;
                    }

                    if (enabled) {
                        if (!Keys.ContainsKey(v)) {
                            Keys.Add(v, StringToByteArray(c[1]));
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
        public byte[] GetKey(ulong keyID) {
            Keys.TryGetValue(keyID, out byte[] key);
            return key;
        }

    }
}