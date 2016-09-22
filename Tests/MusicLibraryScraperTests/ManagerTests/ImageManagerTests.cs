namespace MusicLibraryScraperTests.AmazonAlbumArtLookupTests
{
    using NUnit.Framework;
    using MusicLibraryScraper.Managers;
    using System.Collections.Generic;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Drawing;

    /// <summary>
    /// Summary description for ImageManagerTests
    /// </summary>
    [TestFixture]
    public class ImageManagerTests
    {
        ImageManager _imageMan;
        DirectoryInfo path;
        Dictionary<string, Tuple<Image, long>> _preLoadedImages;

        [OneTimeSetUp]
        public void ImageManagerTestsSetUp()
        {
            _imageMan = new ImageManager();
            Assembly ass = Assembly.GetExecutingAssembly();
            path = new FileInfo(ass.Location).Directory;
            _preLoadedImages = new Dictionary<string, Tuple<Image, long>>();

            FileInfo testFile = new FileInfo(Path.Combine(path.FullName, @"Resources\test_jpg.jpg"));
            FileInfo testFile1 = new FileInfo(Path.Combine(path.FullName, @"Resources\test2.jpg"));
            FileInfo testFile2 = new FileInfo(Path.Combine(path.FullName, @"Resources\test3.jpg"));
            FileInfo testFile3 = new FileInfo(Path.Combine(path.FullName, @"Resources\test4.jpg"));
            FileInfo testFile4 = new FileInfo(Path.Combine(path.FullName, @"Resources\test5.jpg"));
            FileInfo testFile5 = new FileInfo(Path.Combine(path.FullName, @"Resources\test_png.png"));

            try
            {
                _preLoadedImages.Add(testFile.FullName, new Tuple<Image, long>(_imageMan.loadImage(testFile), testFile.Length));
                _preLoadedImages.Add(testFile1.FullName, new Tuple<Image, long>(_imageMan.loadImage(testFile1), testFile1.Length));
                _preLoadedImages.Add(testFile2.FullName, new Tuple<Image, long>(_imageMan.loadImage(testFile2), testFile2.Length));
                _preLoadedImages.Add(testFile3.FullName, new Tuple<Image, long>(_imageMan.loadImage(testFile3), testFile3.Length));
                _preLoadedImages.Add(testFile4.FullName, new Tuple<Image, long>(_imageMan.loadImage(testFile4), testFile4.Length));
                _preLoadedImages.Add(testFile5.FullName, new Tuple<Image, long>(_imageMan.loadImage(testFile5), testFile5.Length));
            }
            catch
            {
                // oh well..
            }
        }

        [OneTimeTearDown]
        public void ImageManagerTestsTesrDown()
        {
            foreach (var tuple in _preLoadedImages.Values)
            {
                tuple.Item1.Dispose();
            }
        }

        [Test]
        [Category("ImageManagerTests")]
        [TestCase(@"Resources\test_jpg.jpg")]
        [TestCase(@"Resources\test2.jpg")]
        [TestCase(@"Resources\test3.jpg")]
        [TestCase(@"Resources\test4.jpg")]
        [TestCase(@"Resources\test5.jpg")]
        [TestCase(@"Resources\test_png.png")]
        public void LoadImageTest(string filepath)
        {
            FileInfo testFile = new FileInfo(Path.Combine(path.FullName, filepath));
            using (var test = _imageMan.loadImage(testFile))
            {
                Assert.IsNotNull(test);
            }
        }

        [Test]
        [Category("ImageManagerTests")]
        [TestCase(@"Resources\test_jpg.jpg")]
        [TestCase(@"Resources\test2.jpg")]
        [TestCase(@"Resources\test3.jpg")]
        [TestCase(@"Resources\test4.jpg")]
        [TestCase(@"Resources\test5.jpg")]
        [TestCase(@"Resources\test_png.png")]
        public void ResizeImageTest(string filepath)
        {
            FileInfo testFile = new FileInfo(Path.Combine(path.FullName, filepath));
            using (var test = _imageMan.loadImage(testFile))
            using (var resizedTest = _imageMan.ScaleImage(test, ImageManager.MinSize))
            {

                Assert.AreEqual(ImageManager.MinSize, Math.Min(resizedTest.Width, resizedTest.Height));
                Assert.AreEqual(test.Width / test.Height, resizedTest.Width / resizedTest.Height);
            }
        }

        [Test]
        [Category("ImageManagerTests")]
        [TestCase(@"Resources\test_jpg.jpg")]
        [TestCase(@"Resources\test2.jpg")]
        [TestCase(@"Resources\test3.jpg")]
        [TestCase(@"Resources\test4.jpg")]
        [TestCase(@"Resources\test5.jpg")]
        [TestCase(@"Resources\test_png.png")]
        public void SaveImageWithQualityTest(string filepath)
        {
            string outDir = Path.Combine(path.FullName, @"output\");
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }


            FileInfo testFile = new FileInfo(Path.Combine(path.FullName, filepath));
            string outfile = Path.Combine(outDir, $"{Path.GetFileNameWithoutExtension(testFile.Name)} - resized{Path.GetExtension(testFile.Name)}");
            if (File.Exists(outfile))
            {
                File.Delete(outfile);
            }

            using (var test = _imageMan.loadImage(testFile))
            using (var resizedTest = _imageMan.ScaleImage(test, ImageManager.MinSize))
            {
                var outFileinfo = _imageMan.SaveImageWithQuality(resizedTest, outDir, outfile, 90);
                Assert.IsTrue(File.Exists(outFileinfo.FullName), "File exists");
                Assert.IsTrue(outFileinfo.Length < testFile.Length, "File is smaller");
            }
        }

        [Test]
        [Category("ImageManagerTests")]
        [TestCase(@"Resources\test_jpg.jpg")]
        [TestCase(@"Resources\test2.jpg")]
        [TestCase(@"Resources\test3.jpg")]
        [TestCase(@"Resources\test4.jpg")]
        [TestCase(@"Resources\test5.jpg")]
        [TestCase(@"Resources\test_png.png")]
        public void ConvertImagetoQualityTest(string filepath)
        {
            string testFile = Path.Combine(path.FullName, filepath);
            Image test;
            bool dispose = false;
            if (_preLoadedImages.ContainsKey(testFile))
            {
                test = _preLoadedImages[testFile].Item1;
            }
            else
            {
                test = _imageMan.loadImage(new FileInfo(testFile));
                dispose = true;
            }
            var testSize = _preLoadedImages[testFile].Item2;
            using (var resizedTest = _imageMan.ScaleImage(test, ImageManager.MinSize))
            {
                long newSize;
                MemoryStream stream = new MemoryStream();
                var outImage = _imageMan.ConvertImagetoQuality(resizedTest, 90, out newSize, ref stream);
                stream.Close();
                stream.Dispose();
                Assert.IsTrue(newSize < testSize);
                Assert.AreEqual(ImageManager.MinSize, Math.Min(outImage.Width, outImage.Height));
                Assert.AreEqual(test.Width / test.Height, outImage.Width / outImage.Height);
            }

            if (dispose)
            {
                test.Dispose();
            }
        }
    }
}
