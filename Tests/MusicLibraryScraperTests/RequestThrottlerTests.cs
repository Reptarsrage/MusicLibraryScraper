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
        ConcurrentStack<long> _stack;
        RequestThrottler throttler;

        [TestFixtureSetUp]
        public void TaskManagerTestsSetUp()
        {
            _stack = new ConcurrentStack<long>();
             throttler = new RequestThrottler();
        }

        [TestFixtureTearDown]
        public void TaskManagerTestsTearDown()
        {
            _stack.Clear();
            _stack = null;
            throttler.Dispose();
            throttler = null;
        }

        [Test]
        [Category("RequestThrottlerTests")]
        public void TaskManagerThrottleTest()
        {
            int numTasks = 100;
            var tasks = new List<Task>();
            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < numTasks; i++)
            {
                tasks.Add(new Task(() =>
                {
                    _stack.Push(watch.ElapsedMilliseconds);
                }));
            }

            Parallel.ForEach(tasks,
                task => {
                    throttler.ThrottleTask(task);
                });

            while (!SpinWait.SpinUntil(() => {return _stack.Count == numTasks;}, 100)) {
                Assert.LessOrEqual(watch.ElapsedMilliseconds, 30000, "Timed out.");
            }

            watch.Stop();
            Assert.IsTrue(_stack.Count == numTasks);
            long ticks = 0;
            long ticks2;
            _stack.TryPop(out ticks);
            while (_stack.Count > 0)
            {
                _stack.TryPop(out ticks2);
                Assert.GreaterOrEqual(ticks - ticks2, RequestThrottler.Timeout * 0.95d); // within 5% error margin
                ticks = ticks2;
            }
        }
    }
}
