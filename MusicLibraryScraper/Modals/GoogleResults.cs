namespace MusicLibraryScraper.Modals
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    [XmlRoot(ElementName = "Output", IsNullable = true)]
    public class GoogleResults
    {
        [XmlElement(ElementName = "Error", IsNullable = true)]
        public string Error { get; set; }

        [XmlElement(ElementName ="Result", IsNullable = true)]
        public List<GoogleResult> Results { get; set; }

    }
}