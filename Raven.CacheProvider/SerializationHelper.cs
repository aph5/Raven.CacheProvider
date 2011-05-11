using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Raven.CacheProvider
{
    public static class SerializationHelper
    {
        public static byte[] Serialize(object entry)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, entry);
                return ms.ToArray();
            }
        }

        public static object Deserialize(byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                var formatter = new BinaryFormatter();
                return formatter.Deserialize(ms);
            }
        }
    }
}