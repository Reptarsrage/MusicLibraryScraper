namespace MusicLibraryScraper
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.Runtime.InteropServices;

    public class RequestThrottler : IDisposable
    {
        [DllImport("WinMM.dll", SetLastError = true)]
        private static extern uint timeSetEvent(int msDelay, int msResolution,
        TimerEventHandler handler, ref int userCtx, int eventType);

        [DllImport("WinMM.dll", SetLastError = true)]
        static extern uint timeKillEvent(uint timerEventId);

        private delegate void TimerEventHandler(uint id, uint msg,
               ref int userCtx, int rsv1, int rsv2);

        private ConcurrentQueue<Task> methodQueue;
        private object slock = new object();
        private uint timerId = 0;
        private TimerEventHandler timerRef;
        private const int timeout = 100;

        public static int Timeout
        {
            get
            {
                return timeout;
            }
        }

        public RequestThrottler()
        {
            lock (slock)
            {
                if (timerId == 0)
                {
                    int userCtx = 0;
                    timerRef = new TimerEventHandler(DequeTask);
                    timerId = timeSetEvent(timeout, 10, timerRef, ref userCtx, 1);
                }

                if (methodQueue == null)
                {
                    methodQueue = new ConcurrentQueue<Task>();
                }
            }
        }

        public void StartTaskImmediately(Task task)
        {
            StartTask(task);
        }

        public void ThrottleTask(Task task)
        {
            lock (slock)
            {
                if (timerId == 0)
                {
                    int userCtx = 0;
                    timerRef = new TimerEventHandler(DequeTask);
                    timerId = timeSetEvent(timeout, 10, timerRef, ref userCtx, 1);
                }
            }

            methodQueue.Enqueue(task);
        }

        public void Dispose()
        {
            StopInternal();
        }

        private void DequeTask(uint id, uint msg, ref int userCtx, int rsv1, int rsv2)
        {
            Task action;
            if (methodQueue.TryDequeue(out action))
            {
                StartTask(action);
            }
        }

        private void StopInternal()
        {
            lock (slock)
            {
                if (timerId != 0)
                {
                    timeKillEvent(timerId);
                    timerId = 0;
                }

                if (methodQueue != null)
                {
                    Task o = null;
                    while (methodQueue.Count > 0 && methodQueue.TryDequeue(out o)) {
                        o.Dispose();
                    }
                    methodQueue = null;
                }
            }
        }

        private void StartTask(Task action)
        {
            if (action.Status.Equals(TaskStatus.Created))
            {
                action.Start();
            }
        }
    }
}
