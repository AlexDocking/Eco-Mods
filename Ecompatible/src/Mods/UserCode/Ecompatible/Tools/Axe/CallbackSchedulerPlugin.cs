namespace Eco.Mods.Organisms
{
    using System;
    using System.Collections.Generic;
    using Eco.Core.Utils.Threading;
    using Eco.Core.Plugins.Interfaces;
    using System.Threading.Tasks;

    /// <summary>
    /// Fire a callback to items in the queue that have waited a set duration.
    /// </summary>
    class CallbackSchedulerPlugin : IThreadedPlugin, IModKitPlugin
    {
        public CallbackSchedulerPlugin()
        {
            timer = PeriodicWorkerFactory.CreateWithInterval(TimeSpan.FromMilliseconds(1000), ProcessCollisionQueue);
        }
        readonly IntervalActionWorker timer;
        private static PriorityQueue<Action, long> Callbacks { get; } = new PriorityQueue<Action, long>();
        private static readonly object queueLock = new object();
        internal static void QueueCallback(Action callback, int millisecondsToWait)
        {
            long ticks = DateTime.Now.AddMilliseconds(millisecondsToWait).Ticks;
            lock (queueLock)
            {
                Callbacks.Enqueue(() => callback.Invoke(), ticks);
            }
        }
        
        internal static void ProcessCollisionQueue()
        {
            lock (queueLock)
            {
                long now = DateTime.Now.Ticks;
            
                bool finished = false;
                do
                {
                    if (Callbacks.TryPeek(out Action callback, out long timeToCall) && timeToCall < now)
                    {
                        callback.Invoke();
                        Callbacks.TryDequeue(out _, out _);
                    }
                    else
                    {
                        finished = true;
                    }
                }
                while (!finished);
            }
        }

        public void Run() => timer.Start();

        public Task ShutdownAsync() => timer.ShutdownAsync();

        public string GetStatus() => "";
        public string GetCategory() => "";
    }
}
