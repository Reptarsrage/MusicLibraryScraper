namespace MusicLibraryScraper.Modals
{
    public enum ResultType {
        Amazon,
        Google
    }


    public class AlbumArtResult
    {
        public string Url { get; set; }
        public string ArtistOrDescription { get; set; }
        public string AlbumOrTitle { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string ImageType { get; set; }
        public ResultType ResultType { get; set; }
    }
}
