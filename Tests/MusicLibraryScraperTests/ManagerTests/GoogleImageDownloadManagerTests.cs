namespace MusicLibraryScraperTests.AmazonAlbumArtLookupTests
{
    using NUnit.Framework;
    using MusicLibraryScraper.Managers;
    using System.Collections.Generic;
    using System;
    using System.Threading.Tasks;
    using MusicLibraryScraper;

    /// <summary>
    /// Summary description for ImageManagerTests
    /// </summary>
    [TestFixture]
    public class GoogleImageDownloadManagerTests
    {
        private GoogleImageDownloadManager _manager;


        [OneTimeSetUp]
        public void GoogleImageDownloadManagerTestsSetUp()
        {
            _manager = new GoogleImageDownloadManager();
        }

        [OneTimeTearDown]
        public void GoogleImageDownloadManagerTestsTearDown()
        {
            _manager = null;
        }

        [Test]
        [Category("GoogleImageDownloadManagerTests")]
        [TestCase("We were dead before the ship even sank by Modest Mouse")]
        [TestCase("Starbomb")]
        [TestCase("Ludo hum along")]
        [TestCase("Pokémon")]
        [TestCase("\"Basshunter LOL <(^^,)>\"")]
        public void BasicQueryTest(string query)
        {
            var results = _manager.GetGoogleResults(query);
            Assert.NotNull(results, "Google results are not null");
            Assert.NotNull(results.Results, "Google results are not null");
            Assert.IsNull(results.Error, $"Google results do not have any error. Error: {results.Error}");
        }

        [Test]
        [Category("GoogleImageDownloadManagerTests")]
        [TestCase("We were dead before the ship even sank by Modest Mouse")]
        [TestCase("Starbomb")]
        [TestCase("Ludo hum along")]
        [TestCase("Pokémon")]
        [TestCase("\"Basshunter LOL <(^^,)>\"")]
        public void QueryResultsTest(string query)
        {
            var results = _manager.GetGoogleResults(query);


            Assert.IsTrue(results.Results.Count > 0);
            foreach (var result in results.Results)
            {
                Assert.IsFalse(string.IsNullOrEmpty(result.Url), "Result has a valid URL.");
                Assert.DoesNotThrow(() => new Uri(result.Url), "Result URL can be resolved.");
            }
        }

        [Test]
        [Category("GoogleImageDownloadManagerTests")]
        public void GoogleQueryLoadTest()
        {
            var list = new List<Tuple<string, string>>();
            for (int i = 0; i < 50; i++)
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
                    var results = _manager.GetGoogleResults($"{tuple.Item1} by {tuple.Item2}");
                    bool good = results != null;
                    good &= results.Results.Count > 0;
                    good &= !string.IsNullOrEmpty(results.Results[0]?.Url ?? null);
                    return good;
                });

                Assert.IsTrue(task.Wait(30000), "Timed out getting response from Google.");
                Assert.IsTrue(task.Result, "Error getting response from Google.");
            });

            Assert.IsTrue(result.IsCompleted);
        }
    }
}
