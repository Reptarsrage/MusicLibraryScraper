namespace AmazonAlbumArtLookup.Modals
{
    using System.Xml.Serialization;

    [XmlRoot( "ItemSearchErrorResponse")]
    public class ItemSearchErrorResponse
    {
        [XmlElement("Error")]
        public ItemSearchErrorResponseError Error { get; set; }

        [XmlElement("RequestId")]
        public string RequestId { get; set; }
    }
}
