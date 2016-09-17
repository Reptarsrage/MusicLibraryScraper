namespace MusicLibraryScraper.Tasks
{
    using Managers;
    using Modals;

    class GoogleImageUrlTask : BaseTask<AlbumArtResults>
    {

        public static AlbumArtResults GetAlbumImageURL(string query) {
            Logger.WriteLine($"Fetching image from Google using query: {query}");
            Logger.IncrementGooglerequestCounter();
            var lookup = new GoogleImageDownloadManager();
            var res =  lookup.GetGoogleResults(query);

            if (res != null)
            {
                Logger.WriteSuccess($"Image found on google for {query}.");
                return new AlbumArtResults(res);
            }
            else
            {
                return null;
            }
        }

        public GoogleImageUrlTask(string query) : base(() => GetAlbumImageURL(query))
        {
        }
    }
}
