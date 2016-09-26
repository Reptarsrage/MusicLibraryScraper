namespace MusicLibraryScraperTests.AmazonAlbumArtLookupTests
{
    using NUnit.Framework;
    using AmazonAlbumArtLookup;
    using System.Collections.Generic;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Summary description for AlbumArtLookupTests
    /// </summary>
    [TestFixture]
    public class GetAlbumArtLookupTests
    {
        private AlbumArtLookup _amazonLookup;

        public GetAlbumArtLookupTests()
        {
            _amazonLookup = new AlbumArtLookup();
        }

        [Test]
        [Category("AlbumArtLookupTests")]
        [TestCase("Cage the Elephant", "Cage The Elephant")]
        [TestCase("We were dead before the ship even sank", "")]
        [TestCase("", "Modest Mouse")]
        [TestCase("Americana", "The  Offspring")]
        public void AlbumArtLookupSucceedsTest(string album, string artist)
        {
            var results = _amazonLookup.GetAlbumArt(album, artist, false);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count > 0);
            Assert.IsFalse(string.IsNullOrEmpty(results[0]?.LargeImage?.Url ?? null));
        }

        [Test]
        [Category("AlbumArtLookupTests")]
        [TestCase("Cage the Elephant", "Cage The Elephant")]
        [TestCase("We were dead before the ship even sank", "")]
        [TestCase("", "Modest Mouse")]
        [TestCase("Americana", "The  Offspring")]
        public void AlbumArtLookupVerifyXMLResponseTest(string album, string artist)
        {
            var results = _amazonLookup.GetAlbumArt(album, artist, false);
            Assert.IsNotNull(results);
            foreach (var result in results)
            {
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.LargeImage);
                Assert.IsNotNull(result.LargeImage.Url);

                Assert.IsNotNull(result.Attributes);
                Assert.IsNotNull(result.Attributes.Title);
                Assert.IsNotNull(result.Attributes.Artist);
            } 
        }

        [Test]
        [Category("AlbumArtLookupTests")]
        [TestCase(null, "Cage The Elephant")]
        [TestCase("We were dead before the ship even sank", null)]
        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("!@#NGFNFGH&*I*OTGQREFWV#@", "#$%RTHFGJYUI^%@!@#!@")]
        public void AlbumArtLookupFailsTest(string album, string artist)
        {
            var results = _amazonLookup.GetAlbumArt(album, artist, false);
            Assert.IsNull(results);
        }

        [Test]
        [Category("AlbumArtLookupTests")]
        [TestCase("Cage the Elephant", "Cage The Elephant", "Cage the Elephant", "Cage the Elephant")]
        [TestCase("We were dead  before the ship even   sank", "", "We Were Dead Before The Ship Even Sank", "Modest Mouse")]
        [TestCase("Americana", "The  Offspring", "Americana", "The Offspring")]
        public void AlbumArtLookupAccuracyTest(string album, string artist, string expectedAlbumTitle, string expectedArtist)
        {
            var results = _amazonLookup.GetAlbumArt(album, artist, false);
            Assert.AreEqual(results?[0]?.Attributes?.Title ?? null, expectedAlbumTitle);
            Assert.AreEqual(results?[0]?.Attributes?.Artist ?? null, expectedArtist);
        }

        [Test]
        [Category("AlbumArtLookupTests")]
        // This test is flakey!
        public void AlbumArtLookupLoadTest()
        {
            var list = new List<Tuple<string, string>>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(new Tuple<string, string>("Anti", "Rihanna"));
            }

            var result = Parallel.ForEach(
                list,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                tuple =>
            {
                Task<bool> task = Task.Factory.StartNew(() =>
                {
                    var results = _amazonLookup.GetAlbumArt(tuple.Item1, tuple.Item2, false);
                    bool good = results != null;
                    good &= results.Count > 0;
                    good &= !string.IsNullOrEmpty(results[0]?.LargeImage?.Url ?? null);
                    return good;
                });

                Assert.IsTrue(task.Wait(30000), "Timed out getting response from Amazon.");
                Assert.IsTrue(task.Result, "Error getting response from Amazon.");
            });

            Assert.IsTrue(result.IsCompleted);
        }
    }
}
