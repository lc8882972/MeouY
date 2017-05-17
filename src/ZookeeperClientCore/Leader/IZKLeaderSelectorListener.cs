using ZooKeeperClient.Client;

namespace ZooKeeperClient.Leader
{
    public interface IZKLeaderSelectorListener
    {
        void TakeLeadership(ZKClient zkClient, ILeaderSelector selector);
    }
}
