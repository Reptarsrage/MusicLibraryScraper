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
    using AmazonAlbumArtLookup.Modals;
    using System.Collections.Generic;

    /// <summary>
    /// A Serializable object used to communicate with internal 
    /// and external class Libraries.
    /// </summary>
    public class AlbumArtResults
    {
        private List<AlbumArtResult> _list;
        
        public List<AlbumArtResult> Results { get { return _list; } }

        public AlbumArtResults()
        {
            _list = new List<AlbumArtResult>();
        }

        public AlbumArtResults(List<AWSMusicItem> results) : this()
        {
            foreach (var result in results ?? new List<AWSMusicItem>())
            {
                var r = new AlbumArtResult();
                r.Url = result.LargeImage.Url;
                r.Width = result.LargeImage.Width ?? 0;
                r.Height = result.LargeImage.Height ?? 0;
                r.ArtistOrDescription = result.Attributes.Artist;
                r.AlbumOrTitle = result.Attributes.Title;
                r.ResultType = ResultType.Amazon;
                r.ImageType = "png";
                _list.Add(r);
            }
        }

        public AlbumArtResults(GoogleResults results) : this()
        {
            foreach (var result in results?.Results ?? new List<GoogleResult>())
            {
                var r = new AlbumArtResult();
                r.Url = result.Url;
                r.Width = result.Width ?? 0;
                r.Height = result.Height ?? 0;
                r.ArtistOrDescription = result.Description;
                r.AlbumOrTitle = result.Title;
                r.ImageType = result.Type;
                r.ResultType = ResultType.Google;
                _list.Add(r);
            }
        }

    }
}
