using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace ZooKeeperClient.Serialize
{
    public class SerializableSerializer : IZKSerializer
    {
        public byte[] Serialize<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms, Encoding.UTF8))
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        new JsonSerializer().Serialize(writer, obj, typeof(T));
                        sw.Flush();
                        return ms.ToArray();
                    }
                }
            }
        }

        public T Deserialize<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                using (var sr = new StreamReader(ms, Encoding.UTF8))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        return (T)new JsonSerializer().Deserialize(reader, typeof(T));
                    }
                }
            }
        }
    }
}

