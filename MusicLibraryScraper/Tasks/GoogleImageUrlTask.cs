namespace MusicLibraryScraper.Tasks
{
    using Modals;
    using System.Threading.Tasks;

    class GoogleImageUrlTask : Task<GoogleResult>
    {

        public static GoogleResult GetAlbumImageURL(string query) {
            Logger.WriteLine($"Fetching image from Google using query: {query}");
            Logger.IncrementGooglerequestCounter();
            var lookup = new GoogleImageDownloadManager();
            var res =  lookup.GetBestMatchURL(query);
            Logger.WriteSuccess($"Image found on google for {query}.");
            return res;
        }

        public GoogleImageUrlTask(string query) : base(() => GetAlbumImageURL(query))
        {
        }
    }
}
