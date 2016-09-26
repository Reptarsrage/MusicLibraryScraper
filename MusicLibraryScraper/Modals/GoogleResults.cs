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
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// A Serializable object used to communicate with internal 
    /// and external class Libraries.
    /// </summary>
    [XmlRoot(ElementName = "Output", IsNullable = true)]
    public class GoogleResults
    {
        [XmlElement(ElementName = "Error", IsNullable = true)]
        public string Error { get; set; }

        [XmlElement(ElementName ="Result", IsNullable = true)]
        public List<GoogleResult> Results { get; set; }

    }
}