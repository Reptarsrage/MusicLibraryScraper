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
    using Modals;

    /// <summary>
    /// Task which fetches album image URL's from Amazon AWS.
    /// </summary>
    class GetUrlsFromAmazonTask : BaseTask<AlbumArtResults>
    {
        private static AlbumArtResults GetAlbumImageURL(string artist, string album) {
            Logger.WriteLine($"Fetching image from amazon for {album} by {artist}");
            Logger.IncrementUrlCount();
            var lookup = new AmazonAlbumArtLookup.AlbumArtLookup();
            var res =  lookup.GetAlbumArt(album, artist);
            if (res != null)
            {
                Logger.WriteSuccess($"Image found for {album}, by {artist}.");
                return new AlbumArtResults(res);
            }
            else
            {
                return null;
            }
        }

        public GetUrlsFromAmazonTask(string artist, string album) : base(() => GetAlbumImageURL(artist, album))
        {
        }
    }
}
