namespace MusicLibraryScraper.Modals
{
    using System.Xml.Serialization;

    [XmlRoot(IsNullable = true)]
    public class GoogleResult
    {
        [XmlElement("Title")]
        public string Title { get; set; }
        [XmlElement("Description")]
        public string Description { get; set; }
        [XmlElement("Url")]
        public string Url { get; set; }
        [XmlElement("Height")]
        public int? Height { get; set; }
        [XmlElement("Width")]
        public int? Width { get; set; }
        [XmlElement("Type")]
        public string Type { get; set; }
    }
}
