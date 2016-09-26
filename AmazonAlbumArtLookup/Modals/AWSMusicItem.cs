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
    [XmlRoot(ElementName = "Item")]
    public class AWSMusicItem
    {
        [XmlElement("ASIN")]
        public string Id { get; set; }

        [XmlElement("SmallImage")]
        public AWSMusicItemImage SmallImage { get; set; }

        [XmlElement("MediumImage")]
        public AWSMusicItemImage MediumImage { get; set; }

        [XmlElement("LargeImage")]
        public AWSMusicItemImage LargeImage { get; set; }

        [XmlElement("ItemAttributes")]
        public AWSMusicItemAttributes Attributes { get; set; }
    }
}
