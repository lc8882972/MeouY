using log4net;
using System.Threading;
using System.Threading.Tasks;
using ZooKeeperClient.Client;
using static org.apache.zookeeper.KeeperException;

namespace ZooKeeperClient.Listener
{
    public class ContentWatcher<T>
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ContentWatcher<T>));
        private object _contentLock = new object();
        private static AutoResetEvent resetEvent = new AutoResetEvent(false);
        private Holder<T> _content;
        private string _fileName;
        private ZKClient _zkClient;
        private IZKDataListener dataListener = new ZKDataListener();

        public ContentWatcher(ZKClient zkClient, string fileName)
        {
            _fileName = fileName;
            _zkClient = zkClient;
            dataListener.DataCreatedOrChangeHandler = async (dataPath, data) =>
            {
                await Task.Run(() => SetContent((T)data));
            };
        }

        public void Start()
        {
            _zkClient.SubscribeDataChanges(_fileName, dataListener);
            LOG.Debug("Started ContentWatcher");
        }

        public void Stop()
        {
            _zkClient.UnSubscribeDataChanges(_fileName, dataListener);
        }

        private async Task GetDataAsync()
        {
            try
            {
                SetContent(await _zkClient.GetDataAsync<T>(_fileName));
            }
            catch (NoNodeException)
            {
                // ignore if the node has not yet been created
            }
        }

        private void SetContent(T data)
        {
            lock (_contentLock)
            {
                LOG.Debug($"Received new data: {data}");
                _content = new Holder<T>(data);
                resetEvent.Set();
            }
        }

        public T GetContent()
        {
            while (_content == null)
            {
                resetEvent.WaitOne();
            };
            return _content.value;
        }

    }

    public class Holder<T>
    {
        public Holder()
        {
        }

        public Holder(T value)
        {
            this.value = value;
        }

        public T value { set; get; }
    }
}
