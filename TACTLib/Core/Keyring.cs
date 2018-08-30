using System.Collections.Generic;
using System.Globalization;
using TACTLib.Client;

namespace TACTLib.Core {
    public class Keyring : Config.Config {
        /// <summary>
        /// Keyring keys
        /// </summary>
        public readonly Dictionary<ulong, byte[]> Keys;

        public Keyring(ClientHandler client, string key) : base(client, key) {
            Keys = new Dictionary<ulong, byte[]>();
            foreach (KeyValuePair<string,List<string>> pair in Values) {
                string reverseKey = pair.Key.Substring(pair.Key.Length - 16);
                string keyIDString = "";
                for (int i = 0; i < 8; ++i) {
                    keyIDString = reverseKey.Substring(i * 2, 2) + keyIDString;
                }
                
                ulong keyID = ulong.Parse(keyIDString, NumberStyles.HexNumber);
                Keys[keyID] = Utils.StringToByteArray(pair.Value[0]);
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