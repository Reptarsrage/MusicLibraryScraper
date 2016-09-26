/// <summary>
/// Author: Justin Robb
/// Date: 9/25/2016
/// 
/// Project Description:
/// Adds album art to each file in a library of music using online image sources.
/// 
/// </summary>

namespace MusicLibraryScraper.Modals
{
    /// <summary>
    /// Enum used to determine which provider the results came from.
    /// </summary>
    public enum ResultType {
        Amazon,
        Google
    }

    /// <summary>
    /// A Serializable object used to communicate with internal 
    /// and external class Libraries.
    /// </summary>
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
