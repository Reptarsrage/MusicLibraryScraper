namespace AmazonAlbumArtLookup.Modals
{
    using System.Xml.Serialization;

    public class ItemSearchErrorResponseError
    {
        [XmlElement("Code")]
        public ItemSearchErrorResponseError Code { get; set; }

        [XmlElement("Message")]
        public ItemSearchErrorResponseError Message { get; set; }
    }
}
