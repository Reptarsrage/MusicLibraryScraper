namespace MusicLibraryScraperTests.AmazonAlbumArtLookupTests
{
    using NUnit.Framework;
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
    public class RequestThrottlerTests
    {
        ConcurrentQueue<long> _queue;
        RequestThrottler throttler;

        [SetUp]
        public void TaskManagerTestsSetUp()
        {
            _queue = new ConcurrentQueue<long>();
             throttler = new RequestThrottler();
        }

        [TearDown]
        public void TaskManagerTestsTearDown()
        {
            _queue = null;
            throttler.Dispose();
            throttler = null;
        }

        [Test]
        [Category("RequestThrottlerTests")]
        public void RequestThrottlerTest()
        {
            int numTasks = 100;
            var tasks = new List<Task>();
            var watch = new Stopwatch();
            watch.Start();

            for (int i = 0; i < numTasks; i++)
            {
                tasks.Add(new Task(() => _queue.Enqueue(watch.ElapsedMilliseconds), TaskCreationOptions.LongRunning));
            }

            Parallel.ForEach(tasks, task => throttler.ThrottleTask(task));

            while (!SpinWait.SpinUntil(() => ( _queue.Count == numTasks), 100)) {
                Assert.LessOrEqual(watch.ElapsedMilliseconds, 30000, "Timed out.");
            }

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
        [Category("RequestThrottlerTests")]
        public void RequestThrottlerSleepyTest()
        {
            int numTasks = 100;
            var tasks = new List<Task>();
            var watch = new Stopwatch();
            watch.Start();

            for (int i = 0; i < numTasks; i++)
            {
                tasks.Add(new Task(() => {
                    
                    _queue.Enqueue(watch.ElapsedMilliseconds);
                    Thread.Sleep(1001);
                }, TaskCreationOptions.LongRunning));
            }

            Parallel.ForEach(tasks, task => {
                throttler.ThrottleTask(task);
                Thread.Sleep(55);
            });

            while (!SpinWait.SpinUntil(() => (_queue.Count == numTasks), 100))
            {
                Assert.LessOrEqual(watch.ElapsedMilliseconds, 60000, "Timed out.");
            }

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
        [Category("RequestThrottlerTests")]
        public void StartImmediatelyTest()
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
                task => throttler.StartTaskImmediately(task));

            while (!SpinWait.SpinUntil(() => (_queue.Count == numTasks), 100))
            {
                Assert.LessOrEqual(watch.ElapsedMilliseconds, 30000, "Timed out.");
            }

            watch.Stop();
            Assert.IsTrue(_queue.Count == numTasks);
            long ticks = 0;
            while (_queue.Count > 0)
            {
                _queue.TryDequeue(out ticks);
                Assert.Less(ticks, 30000, "Thread timed out"); // always less than throtteling
            }
        }
    }
}
