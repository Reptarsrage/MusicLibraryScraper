namespace MusicLibraryScraper.Tasks
{
    using System.Threading.Tasks;

    class ImageUrlTask : Task<string>
    {

        public static string GetAlbumImageURL(string artist, string album) {
            Logger.WriteLine($"Fetching image from amazon for {album} by {artist}");
            Logger.IncrementUrlCount();
            var lookup = new AmazonAlbumArtLookup.AlbumArtLookup();
            var res =  lookup.GetAlbumArt(album, artist);
            Logger.WriteSuccess($"Image found for {album}, by {artist}.");
            return res;
        }

        public ImageUrlTask(string artist, string album) : base(() => GetAlbumImageURL(artist, album))
        {
        }
    }
}
