/// <summary>
/// Author: Justin Robb
/// Date: 9/25/2016
/// 
/// Project Description:
/// Adds album art to each file in a library of music using online image sources.
/// 
/// </summary>

namespace MusicLibraryScraper.Tasks
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a base class for long running tasks used throughout the program.
    /// </summary>
    public class BaseTask<T> : Task<T>
    {
        public BaseTask(Func<T> a) : base(a, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness)
        {
        }

        public BaseTask(Func<T> a, TaskCreationOptions options) : base(a, options)
        {
        }
    }
}
