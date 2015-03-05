namespace InceptionCache.Core.Serialization
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T o);
        T Deserialize<T>(byte[] stream);
    }
}
