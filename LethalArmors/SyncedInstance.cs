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

        [NonSerialized]
        static readonly DataContractSerializer serializer = new(typeof(T));
        
        public static T Default { get; private set; }
        public static T Instance { get; private set; }

        internal static bool Synced;

        protected void InitInstance(T instance)
        {
            Default = instance;
            Instance = instance;

            // Ensure the size of an integer is correct for the current system.
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

        public static byte[] SerializeToBytes(T val)
        {
            using MemoryStream stream = new();

            try {
                serializer.WriteObject(stream, val);
                return stream.ToArray();
            }
            catch (Exception e) {
                LethalArmorsPlugin.Log.LogError($"Error serializing instance: {e}");
                return null;
            }

        }

        public static T DeserializeFromBytes(byte[] data) 
        {
            using MemoryStream stream = new(data);

            try {
                return (T) serializer.ReadObject(stream);
            } catch (Exception e)
            {
                LethalArmorsPlugin.Log.LogError($"Failed to deserialize instance: {e}");
                return default;
            }
        }

        internal static void SendMessage(string label, ulong clientId, FastBufferWriter stream)
        {
            bool fragment = stream.Capacity > stream.MaxCapacity;
            NetworkDelivery delivery = fragment ? NetworkDelivery.ReliableFragmentedSequenced : NetworkDelivery.Reliable;

            if(fragment)
            {
                LethalArmorsPlugin.Log.LogDebug(
                    $"Size of stream ({stream.Capacity}) was past the max buffer size.\n" +
                    "Config instance will be sent in framents to avoid overflowing the buffer."
                );
            }

            MessageManager.SendNamedMessage(label, clientId, stream, delivery);
        }

    }
}
