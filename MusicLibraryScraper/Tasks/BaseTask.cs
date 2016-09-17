namespace MusicLibraryScraper.Tasks
{
    using System;
    using System.Threading.Tasks;

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
