// Copyright (c) Alex Docking
//
// This file is part of Ecompatible.
//
// Ecompatible is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//
// Ecompatible is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License along with Ecompatible. If not, see <https://www.gnu.org/licenses/>.

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
