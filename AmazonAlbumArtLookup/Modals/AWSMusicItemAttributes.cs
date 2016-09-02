namespace AmazonAlbumArtLookup.Modals
{
    using System.Xml.Serialization;

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
