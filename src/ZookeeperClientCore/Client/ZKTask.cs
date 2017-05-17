using log4net;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ZooKeeperClient.Client
{
    public class ZKTask
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ZKTask));

        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        public Task _zkTask;

        private BlockingCollection<ZKEvent> _events = new BlockingCollection<ZKEvent>(new ConcurrentQueue<ZKEvent>());

        public class ZKEvent
        {
            public Func<Task>  Run { set; get; }
        }

        public ZKTask(string name)
        {
            tokenSource = new CancellationTokenSource();
            _zkTask = new Task(Run, tokenSource.Token);
        }
  
        public void Start()
        {
            _zkTask.Start();
        }

        public  void Run()
        {
            while (!tokenSource.IsCancellationRequested)
            {
                ZKEvent zkEvent = _events.Take();
                try
                {
                    zkEvent.Run();
                }
                catch (Exception e)
                {
                    LOG.Error($"Error handling event {zkEvent.ToString()}", e);
                }
            }
        }

        public void Send(ZKEvent @event)
        {
            if (_zkTask != null && _zkTask.Status != TaskStatus.Canceled)
            {
                _events.Add(@event);
            }
        }

        public void Cancel()
        {
            tokenSource.Cancel();
        }

    }
}

