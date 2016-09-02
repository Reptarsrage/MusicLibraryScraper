namespace MusicLibraryScraperTests.AmazonAlbumArtLookupTests
{
    using NUnit.Framework;
    using AmazonAlbumArtLookup;

    /// <summary>
    /// Summary description for AlbumArtLookupTests
    /// </summary>
    [TestFixture]
    public class GetAlbumArtWithQuery
    {
        private AlbumArtLookup _amazonLookup;

        public GetAlbumArtWithQuery()
        {
            _amazonLookup = new AlbumArtLookup();
        }

        [Test]
        [TestCase("Cage the Elephant")]
        [TestCase("We were dead before the ship even sank")]
        [TestCase("Modest Mouse")]
        [TestCase("Americana")]
        public void AlbumArtQueryLookupSucceedsTest(string query)
        {
            var results = _amazonLookup.GetAlbumArt(query, false);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Count > 0);
            Assert.IsFalse(string.IsNullOrEmpty(results[0]?.LargeImage?.Url ?? null));
        }

        [Test]
        [TestCase("Cage the Elephant")]
        [TestCase("We were dead before the ship even sank")]
        [TestCase("Modest Mouse")]
        [TestCase("Americana")]
        public void AlbumArtQueryLookupVerifyXMLResponseTest(string query)
        {
            var results = _amazonLookup.GetAlbumArt(query, false);
            Assert.IsNotNull(results);
            foreach (var result in results)
            {
                Assert.IsNotNull(result, "Result is not null");
                Assert.IsNotNull(result.LargeImage, "Image is not null");
                Assert.IsNotNull(result.LargeImage.Url, "Image URL is not null");

                Assert.IsNotNull(result.Attributes, "Attributes are not null");
                Assert.IsNotNull(result.Attributes.Title, "Title is not null");
                Assert.IsNotNull(result.Attributes.Artist, "Artist is not null");
            }
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("!@#NGFNFGH&*I*OTGQREFWV#@")]
        public void AlbumArtQueryLookupFailsTest(string query)
        {
            var results = _amazonLookup.GetAlbumArt(query, false);
            Assert.IsNull(results);
        }
    }
}
