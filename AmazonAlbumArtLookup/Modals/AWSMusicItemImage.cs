/// <summary>
/// Author: Justin Robb
/// Date: 9/25/2016
/// 
/// Project Description:
/// Uses Amazon AWS Advertising API to retrieve album art.
/// 
/// </summary>

namespace AmazonAlbumArtLookup.Modals
{
    using System.Xml.Serialization;

    /// <summary>
    /// A Serializable object used to communicate with Amazon 
    /// and external class Libraries.
    /// </summary>
    public class AWSMusicItemImage
    {
        [XmlElement("URL")]
        public string Url { get; set; }

        [XmlElement("Height")]
        public int? Height { get; set; }

        [XmlElement("Width")]
        public int? Width { get; set; }
    }
}
