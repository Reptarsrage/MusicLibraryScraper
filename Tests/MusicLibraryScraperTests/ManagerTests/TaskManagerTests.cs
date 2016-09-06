namespace MusicLibraryScraperTests.AmazonAlbumArtLookupTests
{
    using NUnit.Framework;
    using MusicLibraryScraper.Managers;
    using System.Collections.Generic;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Drawing;
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
        [Category("TaskManagerTests")]
        public void TaskManagerThrottleTest()
        {
            int numTasks = 32;
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

            while (!SpinWait.SpinUntil(() => { return _stack.Count == numTasks; }, 100)) { }

            watch.Stop();
            Assert.IsTrue(_stack.Count == numTasks);
            long ticks = 0;
            long ticks2;
            _stack.TryPop(out ticks);
            while (_stack.Count > 0)
            {
                _stack.TryPop(out ticks2);
                Assert.GreaterOrEqual(ticks - ticks2, 95);
                ticks = ticks2;
            }
        }
    }
}
