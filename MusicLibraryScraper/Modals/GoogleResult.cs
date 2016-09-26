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
    using System.Xml.Serialization;

    /// <summary>
    /// A Serializable object used to communicate with internal 
    /// and external class Libraries.
    /// </summary>
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
