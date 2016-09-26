/// <summary>
/// Author: Justin Robb
/// Date: 9/25/2016
/// 
/// Project Description:
/// Adds album art to each file in a library of music using online image sources.
/// 
/// </summary>

namespace MusicLibraryScraper.Managers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Runs tasks and waits for tasks to complete.
    /// Throttles tasks when specified.
    /// </summary>
    public class TaskManager
    {
        /// <summary>
        /// Runs the given task and blocks current thread until task completes.
        /// </summary>
        /// <param name="task">Task to run</param>
        /// <param name="description">Description of task</param>
        /// <param name="throttle">Flag to throttle the task.</param>
        /// <param name="RequestThrottler">Object to use when throtteling tasks.</param>
        /// <param name="verbose">Flag for output.</param>
        /// <returns>A flag indicating success or failure.</returns>
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
