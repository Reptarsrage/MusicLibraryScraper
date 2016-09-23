namespace MusicLibraryScraperTests.AmazonAlbumArtLookupTests
{
    using NUnit.Framework;
    using MusicLibraryScraper.Managers;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Drawing;
    using System.Threading;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using MusicLibraryScraper;

    /// <summary>
    /// Summary description for ImageManagerTests
    /// </summary>
    [TestFixture]
    public class CacheManagerTests
    {
        private CacheManager _cache;
        private FileInfo _imageFile;
        private DirectoryInfo _outDir;
        private string _imageUrl = @"https://images-na.ssl-images-amazon.com/images/I/513FU4FC9lL.jpg";

        [SetUp]
        public void CacheManagerTestsSetUp()
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            var path = new FileInfo(ass.Location).Directory;
            var outDirLoc = Path.Combine(path.FullName, @"Test\");

            _cache = new CacheManager();
            _imageFile = new FileInfo(Path.Combine(path.FullName, @"Resources\test_jpg.jpg"));

            if (Directory.Exists(outDirLoc)) {
                Directory.Delete(outDirLoc, true);
            }
            Directory.CreateDirectory(outDirLoc);
            _outDir = new DirectoryInfo(outDirLoc);

            Logger.ResetAll();
        }

        [TearDown]
        public void CacheManagerTestsTesrDown()
        {
            _cache.Dispose();
            _imageFile = null;
            if (_outDir?.FullName != null && Directory.Exists(_outDir.FullName))
            {
                Directory.Delete(_outDir.FullName, true);
            }
            _outDir = null;
        }

        #region GetOptimizedImage

        [Test]
        [Category("CacheManagerTests")]
        public void GetOptimizedImageWithoutFileTest()
        {
            var watch = new Stopwatch();
            using (Image image = new Bitmap(_imageFile.FullName))
            using (var task = _cache.GetOptimizedImage("SOME DISTINCT URL KEY", image))
            {
                Assert.IsNotNull(task);

                watch.Start();
                task.Start();
                while (!SpinWait.SpinUntil(() => task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 1000, "Task Timed out");
                }
                watch.Stop();

                using (Image optImage = task.Result)
                using (var largerStream = new MemoryStream())
                using (var smallerStream = new MemoryStream())
                {
                    Assert.NotNull(optImage);
                    image.Save(largerStream, image.RawFormat);
                    optImage.Save(smallerStream, optImage.RawFormat);
                    Assert.AreEqual(ImageManager.MinSize, Math.Min(optImage.Size.Height, optImage.Size.Width));
                    Assert.LessOrEqual(smallerStream.Length, largerStream.Length);
                }
            }
        }

        [Test]
        [Category("CacheManagerTests")]
        public void GetOptimizedImageWithFileTest()
        {
            using (var task = _cache.GetOptimizedImage(_imageFile, _outDir)) {
                Assert.IsNotNull(task);

                var watch = new Stopwatch();
                watch.Start();

                task.Start();
                while (!SpinWait.SpinUntil(() => task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 1000, "Task Timed out");
                }
                watch.Stop();

                Assert.NotNull(File.Exists(task?.Result?.FullName), "File Exists");
                var optiFile = new FileInfo(task.Result.FullName);

                using (Image image = new Bitmap(optiFile.FullName))
                {
                    Assert.AreEqual(ImageManager.MinSize, Math.Min(image.Size.Height, image.Size.Width));
                    Assert.LessOrEqual(optiFile.Length, _imageFile.Length);
                }
            }
        }
        #endregion
        #region GetAlbumImage
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageTest()
        {
            using (var task = _cache.GetAlbumImage(_imageUrl))
            {
                Assert.IsNotNull(task);

                var watch = new Stopwatch();
                watch.Start();

                task.Start();
                while (!SpinWait.SpinUntil(() => task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 5000, "Task Timed out");
                }
                watch.Stop();

                using (var image = task.Result) {
                    Assert.NotNull(image, "Image downloaded");
                }
            }
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageBadInputTest()
        {
            using (var task = _cache.GetAlbumImage("DAT URL DOE"))
            {
                Assert.IsNotNull(task);

                var watch = new Stopwatch();
                watch.Start();

                task.Start();
                while (!SpinWait.SpinUntil(() => task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 5000, "Task Timed out");
                }
                watch.Stop();

                using (var image = task.Result)
                {
                    Assert.Null(image, "Image not downloaded");
                }
            }
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => _cache.GetAlbumImage(null)); 
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageCachedTest()
        {
            var watch = new Stopwatch();
            watch.Start();
            Parallel.For(0, 100, i =>
            {
                var itask = _cache.GetAlbumImage(_imageUrl);
                try
                {
                    itask.Start();
                }
                catch (InvalidOperationException)
                {
                    /* Already started */
                }
            });

            using (var task = _cache.GetAlbumImage(_imageUrl))
            {
                Assert.NotNull(task, "Task created");
                Assert.IsFalse(task.Status.Equals(TaskStatus.Created), "Task started");

                while (!SpinWait.SpinUntil(() => !task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 1000, "Tasks took too long. Probably not cached correctly.");
                }

                watch.Stop();


                using (Image image = task.Result)
                {
                    Assert.NotNull(image, "Image not downlaoded");
                }
            }

            Assert.AreEqual(1, Logger.ImageDownloadCount, "Only downlaoded one image.");
        }

        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageFileTest()
        {
            using (var task = _cache.GetAlbumImageFile(_imageUrl, "jpg", _outDir))
            {
                Assert.IsNotNull(task);

                var watch = new Stopwatch();
                watch.Start();

                task.Start();
                while (!SpinWait.SpinUntil(() => task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 5000, "Task Timed out");
                }
                watch.Stop();

                Assert.NotNull(task.Result, "Image downloaded");
                Assert.Greater(task.Result.Length, 0, "Image downloaded");
                using (Image image = new Bitmap(task.Result.FullName))
                {
                    Assert.NotNull(image, "Image loaded");
                }
            }
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageBadInputFileTest()
        {
            using (var task = _cache.GetAlbumImageFile("DAT URL DOE", "jpg", _outDir))
            {
                Assert.IsNotNull(task);

                var watch = new Stopwatch();
                watch.Start();

                task.Start();
                while (!SpinWait.SpinUntil(() => task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 5000, "Task Timed out");
                }
                watch.Stop();

                Assert.Null(task.Result, "Image not downloaded");
            }
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageNullFileTest()
        {
            Assert.Throws<ArgumentNullException>(() => _cache.GetAlbumImageFile(null, "jpg", _outDir));
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageBadOutDirTest()
        {
            Assert.Throws<ArgumentNullException>(() => _cache.GetAlbumImageFile(_imageUrl, "jpg", null));
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageCachedFileTest()
        {
            var watch = new Stopwatch();
            watch.Start();
            Parallel.For(0, 100, i =>
            {
                var itask = _cache.GetAlbumImageFile(_imageUrl, "jpg", _outDir);
                try
                {
                    itask.Start();
                }
                catch (InvalidOperationException)
                {
                    /* Already started */
                }
            });

            using (var task = _cache.GetAlbumImageFile(_imageUrl, "jpg", _outDir))
            {
                Assert.NotNull(task, "Task created");
                Assert.IsFalse(task.Status.Equals(TaskStatus.Created), "Task started");

                while (!SpinWait.SpinUntil(() => !task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 1000, "Tasks took too long. Probably not cached correctly.");
                }

                watch.Stop();

                Assert.NotNull(task.Result, "Image not downlaoded");
            }
            Assert.AreEqual(1, Logger.ImageDownloadCount, "Only downlaoded one image.");
        }
        #endregion
        #region GetAlbumImageUrl
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageUrlTest()
        {
            using (var task = _cache.GetAlbumImageURL("Modest Mouse", "Moon Antarctica"))
            {
                Assert.IsNotNull(task);

                var watch = new Stopwatch();
                watch.Start();

                task.Start();
                while (!SpinWait.SpinUntil(() => task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 5000, "Task Timed out");
                }
                watch.Stop();

                Assert.NotNull(task.Result, "Urls fetched");
                Assert.Greater(task.Result.Results.Count, 0, "Urls fetched");
                foreach (var result in task.Result.Results)
                {
                    Assert.NotNull(result.Url, "Url fetched");
                }
            }
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageUrlBadInputTest()
        {
            using (var task = _cache.GetAlbumImageURL("@#$SDF", "SPOOOOOOOOOOOOONS"))
            {
                Assert.IsNotNull(task);

                var watch = new Stopwatch();
                watch.Start();

                task.Start();
                while (!SpinWait.SpinUntil(() => task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 5000, "Task Timed out");
                }
                watch.Stop();

                Assert.Null(task.Result, "Urls not fetched");
            }
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageUrlNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => _cache.GetAlbumImageURL(null, null));
            Assert.Throws<ArgumentNullException>(() => _cache.GetAlbumImageURL("", null));
            Assert.Throws<ArgumentNullException>(() => _cache.GetAlbumImageURL(null, ""));
            Assert.Throws<ArgumentNullException>(() => _cache.GetAlbumImageURL("", ""));
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageUrlCachedTest()
        {
            var watch = new Stopwatch();
            watch.Start();
            Parallel.For(0, 100, i =>
            {
                var itask = _cache.GetAlbumImageURL("Modest Mouse", "Moon Antarctica");
                try
                {
                    itask.Start();
                }
                catch (InvalidOperationException)
                {
                    /* Already started */
                }
            });

            using (var task = _cache.GetAlbumImageURL("Modest Mouse", "Moon Antarctica"))
            {
                Assert.NotNull(task, "Task created");
                Assert.IsFalse(task.Status.Equals(TaskStatus.Created), "Task started");

                while (!SpinWait.SpinUntil(() => !task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 1000, "Tasks took too long. Probably not cached correctly.");
                }

                watch.Stop();

                Assert.NotNull(task.Result);
                Assert.Greater(task.Result.Results.Count, 0, "Urls fetched");
            }

            Assert.AreEqual(1, Logger.AmazonRequestCounter, "Only requested one image from Google.");
        }
        #endregion
        #region GetAlbumImageURLUsingGoogle
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageURLUsingGoogleTest()
        {
            using (var task = _cache.GetAlbumImageURLUsingGoogle("Modest Mouse Moon Antarctica"))
            {
                Assert.IsNotNull(task);

                var watch = new Stopwatch();
                watch.Start();

                task.Start();
                while (!SpinWait.SpinUntil(() => task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 5000, "Task Timed out");
                }
                watch.Stop();

                Assert.NotNull(task.Result, "Urls fetched");
                Assert.Greater(task.Result.Results.Count, 0, "Urls fetched");
                foreach (var result in task.Result.Results)
                {
                    Assert.NotNull(result.Url, "Url fetched");
                }
            }
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageURLUsingGoogleBadInputTest()
        {
            using (var task = _cache.GetAlbumImageURLUsingGoogle("thissuperlongstringshouldneverexistongoogle"))
            {
                Assert.IsNotNull(task);

                var watch = new Stopwatch();
                watch.Start();

                task.Start();
                while (!SpinWait.SpinUntil(() => task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 5000, "Task Timed out");
                }
                watch.Stop();

                Assert.NotNull(task.Exception, "Error thrown");
            }
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageURLUsingGoogleNullTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var task = _cache.GetAlbumImageURLUsingGoogle(null);
                task.Dispose();
            });
        }
        [Test]
        [Category("CacheManagerTests")]
        public void GetAlbumImageURLUsingGoogleCachedTest()
        {
            var watch = new Stopwatch();
            watch.Start();
            Parallel.For(0, 100, i =>
            {
                var itask = _cache.GetAlbumImageURLUsingGoogle("Modest Mouse Moon Antarctica");
                try
                {
                    itask.Start();
                }
                catch (InvalidOperationException)
                {
                    /* Already started */
                }
            });

            using (var task = _cache.GetAlbumImageURLUsingGoogle("Modest Mouse Moon Antarctica"))
            {
                Assert.NotNull(task, "Task created");
                Assert.IsFalse(task.Status.Equals(TaskStatus.Created), "Task started");

                while (!SpinWait.SpinUntil(() => !task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 1000, "Tasks took too long. Probably not cached correctly.");
                }

                watch.Stop();

                Assert.NotNull(task.Result);
                Assert.Greater(task.Result.Results.Count, 0, "Urls fetched");
            }

            Assert.AreEqual(1, Logger.GoogleRequestCounter, "Only requested one image from Google.");
        }
        #endregion
        #region GetLoadedImage
        [Test]
        [Category("CacheManagerTests")]
        public void GetLoadedImageTest()
        {
            using (var task = _cache.GetLoadedImage(_imageFile))
            {
                Assert.IsNotNull(task);

                var watch = new Stopwatch();
                watch.Start();

                task.Start();
                while (!SpinWait.SpinUntil(() => task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 1000, "Task Timed out");
                }
                watch.Stop();

                using (Image image = task.Result)
                {
                    Assert.NotNull(image, "Image loaded");
                }
            }
        }

        [Test]
        [Category("CacheManagerTests")]
        public void GetLoadedImageNullInputTest()
        {
            using (var task = _cache.GetLoadedImage(null))
            {
                Assert.Null(task);
            }
        }

        [Test]
        [Category("CacheManagerTests")]
        public void GetLoadedImageBadInputTest()
        {
            using (var task = _cache.GetLoadedImage(new FileInfo("!@$#^#$^")))
            {
                Assert.IsNotNull(task);

                var watch = new Stopwatch();
                watch.Start();

                task.Start();
                while (!SpinWait.SpinUntil(() => task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 10000000, "Task Timed out");
                }
                watch.Stop();

                using (Image image = task.Result)
                {
                    Assert.Null(image, "Image not loaded");
                }
            }
        }

        [Test]
        [Category("CacheManagerTests")]
        public void GetLoadedImageCachedTest()
        {
            var watch = new Stopwatch();
            watch.Start();
            Parallel.For(0, 100, i =>
            {
                var itask = _cache.GetLoadedImage(_imageFile);
                try
                {
                    itask.Start();
                }
                catch (InvalidOperationException)
                {
                    /* Already started */
                }
            });

            using (var task = _cache.GetLoadedImage(_imageFile))
            {
                Assert.NotNull(task, "Task created");
                Assert.IsFalse(task.Status.Equals(TaskStatus.Created), "Task started");

                while (!SpinWait.SpinUntil(() => !task.IsCompleted, 100))
                {
                    /* Spin spin spin */
                    Assert.Less(watch.ElapsedMilliseconds, 1000, "Tasks took too long. Probably not cached correctly.");
                }

                watch.Stop();


                using (Image image = task.Result)
                {
                    Assert.NotNull(image, "Image not loaded");
                }
            }

            Assert.AreEqual(1, Logger.LoadedImageCount, "Only loaded one image.");
        }
        #endregion
    }
}
