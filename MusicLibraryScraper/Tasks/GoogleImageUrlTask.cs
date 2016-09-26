/// <summary>
/// Author: Justin Robb
/// Date: 9/25/2016
/// 
/// Project Description:
/// Adds album art to each file in a library of music using online image sources.
/// 
/// </summary>

namespace MusicLibraryScraper.Tasks
{
    using Managers;
    using Modals;

    /// <summary>
    /// Retrieves albnum art URLs using Google Advanced Image Search
    /// </summary>
    class GoogleImageUrlTask : BaseTask<AlbumArtResults>
    {

        private static AlbumArtResults GetAlbumImageURL(string query) {
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

        /// <summary>
        /// creates a new <see cref="GoogleImageUrlTask"/>
        /// </summary>
        public GoogleImageUrlTask(string query) : base(() => GetAlbumImageURL(query))
        {
        }
    }
}
