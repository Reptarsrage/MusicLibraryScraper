using System;
using System.Collections.Concurrent;
using System.Timers;
using System.Threading.Tasks;

namespace MusicLibraryScraper
{
    static class RequestThrottler
    {
        private static ConcurrentQueue<Task> methodQueue = new ConcurrentQueue<Task>();
        private static Timer timer;
        private static object slock;

        private static void tick(object sender, ElapsedEventArgs e)
        {
            
            if (methodQueue.Count > 0)
            {
                Task action;
                if (methodQueue.TryDequeue(out action))
                {
                lock (slock) // lock just for starting tasks in case there is ever any overlap
                {
                    if (action.Status.Equals(TaskStatus.Created))
                    {
                            Logger.WriteLine("Starting Task.");
                            action.Start();
                    }
                }
                }
            }
        }

        public static void AddTask(Task task)
        {
            if (timer == null)
            {
                timer = new Timer();
                timer.Interval = 100;
                timer.Elapsed += tick;
                timer.Start();
            }

            if (slock == null)
            {
                slock = new object();
            }

            methodQueue.Enqueue(task);
        }
    }
}
