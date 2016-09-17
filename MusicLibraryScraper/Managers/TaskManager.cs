namespace MusicLibraryScraper.Managers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class TaskManager
    {
        public TaskManager()
        {
        }

        public bool RunTask(Task task, string description, bool throttle, RequestThrottler RequestThrottler, bool verbose = true) {
            try
            {
                if (throttle)
                {
                    RequestThrottler.ThrottleTask(task);
                }
                else
                {
                    RequestThrottler.StartTaskImmediately(task);
                }

                bool finished = task.IsCompleted;
                while (!SpinWait.SpinUntil(() => finished, 100))
                {
                    /* Spin spin spin */
                    finished = task.IsCompleted;
                }

                if (task.IsFaulted)
                {
                    if (verbose)
                    {
                        var ex = task.Exception.InnerExceptions?[0] ?? task.Exception;
                        Logger.WriteError($"Error: {description}.\n{ex.Message}\n{ex.StackTrace}");

                    }
                    return false;
                }
                else if (task.IsCanceled)
                {
                    if (verbose)
                    {
                        Logger.WriteWarning($"Error: {description}. Task cancelled.");
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                var ex = e.InnerException ?? e;
                Logger.WriteError($"An error occured while: {description}.\n{ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
    }
}
