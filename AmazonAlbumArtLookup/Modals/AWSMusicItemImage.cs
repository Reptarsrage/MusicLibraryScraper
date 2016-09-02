namespace AmazonAlbumArtLookup.Modals
{
    using System.Xml.Serialization;

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
