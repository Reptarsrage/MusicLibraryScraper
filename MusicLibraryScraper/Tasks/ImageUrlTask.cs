namespace MusicLibraryScraper.Tasks
{
    using Modals;
    using System.Threading.Tasks;

    class ImageUrlTask : Task<AlbumArtResults>
    {

        public static AlbumArtResults GetAlbumImageURL(string artist, string album) {
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

        public ImageUrlTask(string artist, string album) : base(() => GetAlbumImageURL(artist, album))
        {
        }
    }
}
