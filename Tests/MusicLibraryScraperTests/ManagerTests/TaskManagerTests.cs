namespace MusicLibraryScraperTests.AmazonAlbumArtLookupTests
{
    using NUnit.Framework;
    using MusicLibraryScraper.Managers;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using MusicLibraryScraper;
    using System.Threading;

    /// <summary>
    /// Summary description for ImageManagerTests
    /// </summary>
    [TestFixture]
    public class TaskManagerTests
    {
        ConcurrentQueue<long> _queue;
        RequestThrottler _throttler;

        [SetUp]
        public void TaskManagerTestsSetUp()
        {
            _queue = new ConcurrentQueue<long>();
            _throttler = new RequestThrottler();
        }

        [TearDown]
        public void TaskManagerTestsTearDown()
        {
            _throttler.Dispose();
            _throttler = null;
            _queue = null;
        }

        [Test]
        [Category("TaskManagerTests")]
        public void TaskManagerThrottleTest()
        {
            int numTasks = 100;
            var tasks = new List<Task>();
            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < numTasks; i++)
            {
                tasks.Add(new Task(() => _queue.Enqueue(watch.ElapsedMilliseconds), TaskCreationOptions.LongRunning));
            }

            Parallel.ForEach(tasks, new ParallelOptions { MaxDegreeOfParallelism = 16 },
                task =>
                {
                    var manager = new TaskManager();
                    Assert.IsTrue(manager.RunTask(task, "TEST", true, _throttler, false), "Failed to run task.");
                });

            watch.Stop();
            Assert.IsTrue(_queue.Count == numTasks);
            long ticks = 0;
            int count = 1;
            while (_queue.Count > 0)
            {
                _queue.TryDequeue(out ticks);
                Assert.GreaterOrEqual(ticks + 5L, RequestThrottler.Timeout * count); // always greater or equal to the throtteling (within 5ms)
                count++;
            }
        }

        [Test]
        [Category("TaskManagerTests")]
        public void TaskManagerThrottleSleepyTest()
        {
            int numTasks = 100;
            var tasks = new List<Task>();
            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < numTasks; i++)
            {
                tasks.Add(new Task(() => {
                    _queue.Enqueue(watch.ElapsedMilliseconds);
                    Thread.Sleep(555);
                }, TaskCreationOptions.LongRunning));
            }

            Parallel.ForEach(tasks,
                task =>
                {
                    var manager = new TaskManager();
                    Assert.IsTrue(manager.RunTask(task, "TEST", true, _throttler, false), "Failed to run task.");
                    Thread.Sleep(55);
                });

            watch.Stop();
            Assert.IsTrue(_queue.Count == numTasks);
            long ticks = 0;
            int count = 1;
            while (_queue.Count > 0)
            {
                _queue.TryDequeue(out ticks);
                Assert.GreaterOrEqual(ticks + 5L, RequestThrottler.Timeout * count); // always greater or equal to the throtteling (within 5ms)
                count++;
            }
        }

        [Test]
        [Category("TaskManagerTests")]
        public void TaskManagerNoThrottleTest()
        {
            int numTasks = 20;
            var tasks = new List<Task>();
            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < numTasks; i++)
            {
                tasks.Add(new Task(() => _queue.Enqueue(watch.ElapsedMilliseconds), TaskCreationOptions.LongRunning));
            }

            Parallel.ForEach(tasks,
                task =>
                {
                    var manager = new TaskManager();
                    Assert.IsTrue(manager.RunTask(task, "TEST", false, _throttler, false), "Failed to run task.");
                });

            watch.Stop();
            Assert.IsTrue(_queue.Count == numTasks);
            long ticks = 0;
            while (_queue.Count > 0)
            {
                _queue.TryDequeue(out ticks);
                Assert.Less(ticks, 30000, "Thread Timed out"); // timeout
            }
        }
    }
}
