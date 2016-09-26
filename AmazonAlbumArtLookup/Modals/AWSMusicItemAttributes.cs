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
    [XmlRoot("ItemAttributes")]
    public class AWSMusicItemAttributes
    {
        [XmlElement("Artist")]
        public string Artist { get; set; }

        [XmlElement("Binding")]
        public string Binding { get; set; }

        [XmlElement("Brand")]
        public string Brand { get; set; }

        [XmlElement("Title")]
        public string Title { get; set; }

        [XmlElement("Studio")]
        public string Studio { get; set; }

        [XmlElement("ReleaseDate")]
        public string ReleaseDate { get; set; }
    }
}
