namespace AmazonAlbumArtLookup.Modals
{
    using System.Xml.Serialization;

    public class XmlSerialization
    {
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

        [XmlRoot(ElementName = "ItemSearchErrorResponse")]
        public class ItemSearchErrorResponse
        {
            [XmlElement("Error")]
            public ItemSearchErrorResponseError Error { get; set; }

            [XmlElement("RequestId")]
            public string RequestId { get; set; }
        }

        public class ItemSearchErrorResponseError
        {
            [XmlElement("Code")]
            public ItemSearchErrorResponseError Code { get; set; }

            [XmlElement("Message")]
            public ItemSearchErrorResponseError Message { get; set; }
        }

        [XmlRoot(ElementName = "ItemAttributes")]
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

        public class AWSMusicItemImage
        {
            [XmlElement("URL")]
            public string Url { get; set; }

            [XmlElement("Height")]
            public string Height { get; set; }

            [XmlElement("Width")]
            public string Width { get; set; }
        }
    }
}
