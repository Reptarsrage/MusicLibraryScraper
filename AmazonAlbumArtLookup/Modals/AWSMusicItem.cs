namespace AmazonAlbumArtLookup.Modals
{
    using System.Xml.Serialization;

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
