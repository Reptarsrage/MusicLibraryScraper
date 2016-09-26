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
    [XmlRoot( "ItemSearchErrorResponse")]
    public class ItemSearchErrorResponse
    {
        [XmlElement("Error")]
        public ItemSearchErrorResponseError Error { get; set; }

        [XmlElement("RequestId")]
        public string RequestId { get; set; }
    }
}
