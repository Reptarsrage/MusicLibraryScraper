/// <summary>
/// Author: Justin Robb
/// Date: 9/25/2016
/// 
/// Project Description:
/// Adds album art to each file in a library of music using online image sources.
/// 
/// </summary>

namespace MusicLibraryScraper
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Throttles requests so that parrallel API calls do not start getting kicked-back
    /// </summary>
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

        /// <summary>
        /// Gets the amount of time before a task is dequeued. (i.e. Throttle Time)
        /// </summary>
        public static int Timeout
        {
            get
            {
                return timeout;
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="RequestThrottler"/>
        /// </summary>
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

        /// <summary>
        /// Begins a task without throtteling
        /// </summary>
        public void StartTaskImmediately(Task task)
        {
            StartTask(task);
        }

        /// <summary>
        /// Begins a task with throtteling. Task must wait for an open slot before running.
        /// </summary>
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

        /// <summary>
        /// Discontinues all future throtteling
        /// </summary>
        public void Dispose()
        {
            StopInternal();
        }

        /// <summary>
        /// A slot has opened up so lets fill it with a task
        /// </summary>
        private void DequeTask(uint id, uint msg, ref int userCtx, int rsv1, int rsv2)
        {
            Task action;
            if (methodQueue.TryDequeue(out action))
            {
                StartTask(action);
            }
        }

        /// <summary>
        /// Halt
        /// </summary>
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

        /// <summary>
        /// Start a task
        /// </summary>
        private void StartTask(Task action)
        {
            if (action.Status.Equals(TaskStatus.Created))
            {
                action.Start();
            }
        }
    }
}
