using System.IO;
using ProtoBuf;

namespace InceptionCache.Core.Serialization
{
    public class ProtoBufSerializer : ISerializer
    {
        public byte[] Serialize<T>(T o)
        {
            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream, o);
                var objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        public T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            using (var memoryStream = new MemoryStream(stream))
            {
                var result = Serializer.Deserialize<T>(memoryStream);
                return result;
            }
        }
    }
}
