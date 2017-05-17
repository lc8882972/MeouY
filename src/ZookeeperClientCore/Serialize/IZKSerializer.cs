namespace ZooKeeperClient.Serialize
{
    public interface IZKSerializer
    {
        byte[] Serialize<T>(T obj);

        T Deserialize<T>(byte[] bytes);
    }
}
