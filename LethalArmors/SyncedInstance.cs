using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

using Unity.Netcode;

namespace LethalArmors
{
    [Serializable]
    public class SyncedInstance<T>
    {
        internal static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;
        internal static bool IsClient => NetworkManager.Singleton.IsClient;
        internal static bool IsHost => NetworkManager.Singleton.IsHost;

        [NonSerialized]
        protected static int IntSize = 4;
        
        public static T Default { get; private set; }
        public static T Instance { get; private set; }
        
        public static bool Synced { get; internal set; }

        protected void InitInstance(T instance)
        {
            Default = instance;
            Instance = instance;

            // Assume default int size of 4 but verify with system
            // Most systems will use 4 byte ints.
            IntSize = sizeof(int);
        }

        internal static void SyncInstance(byte[] data)
        {
            Instance = DeserializeFromBytes(data);
            Synced = true;
        }

        internal static void RevertSync()
        {
            Instance = Default;
            Synced = false;
        }

        [Obsolete]
        public static byte[] SerializeToBytes(T val)
        {
            BinaryFormatter bf = new();
            using MemoryStream stream = new();

            try {
                bf.Serialize(stream, val);
                return stream.ToArray();
            }
            catch (Exception e) {
                LethalArmorsPlugin.Log.LogError($"Failed to serialize instance: {e}");
                return default;
            }

        }

        [Obsolete]
        public static T DeserializeFromBytes(byte[] data) 
        {

            BinaryFormatter bf = new();
            using MemoryStream stream = new(data);

            try {
                return (T) bf.Deserialize(stream);
            } catch (Exception e)
            {
                LethalArmorsPlugin.Log.LogError($"Failed to deserialize instance: {e}");
                return default;
            }
        }

    }
}
