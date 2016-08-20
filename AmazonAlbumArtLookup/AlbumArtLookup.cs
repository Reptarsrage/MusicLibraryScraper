/// <summary>
/// 
/// </summary>

namespace AmazonAlbumArtLookup
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Configuration;
    using static Modals.XmlSerialization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// <para>
    /// Retrieves album art from Amazon using AWS APIs.
    /// </para>
    /// 
    /// <para>
    /// http://docs.aws.amazon.com/AWSECommerceService/latest/DG/CHAP_ApiReference.html
    /// https://affiliate-program.amazon.com/home/account/tag/manage?tracking_id=album0b4-20
    /// https://console.aws.amazon.com/iam/home?rw_useCurrentProtocol=1#security_credential
    /// </para>
    /// </summary>
    public class AlbumArtLookup
    {
        private int expRetry = 500; // ms
        private Regex throttled = new Regex("RequestThrottled");

        public AlbumArtLookup()
        {
        }

        /// <summary>
        /// Retrieves album art from AWS using the given search parameters. 
        /// </summary>
        /// <param name="album">Name of album to search for</param>
        /// <param name="artist">Name of the artist of the album</param>
        /// <param name="verbose">Prints helpful debugging info to console.</param>
        /// <returns> The URL for the largest image listed for the item which bestb matches the search parameters.</returns>
        public string GetAlbumArt(string album, string artist, bool verbose = false) {
            SignedRequestHelper helper = new SignedRequestHelper(ConfigurationManager.AppSettings["AWSKeyID"], 
                ConfigurationManager.AppSettings["AWSSecretKey"], ConfigurationManager.AppSettings["AWSDestination"]);

            IDictionary <string, string> r1 = new Dictionary<string, String>();
            r1["Service"] = "AWSECommerceService";
            r1["Operation"] = "ItemSearch";
            r1["Keywords"] = album;
            r1["Version"] = "2011-08-01";
            r1["SearchIndex"] = "Music";
            r1["Artist"] = artist;
            r1["AssociateTag"] = ConfigurationManager.AppSettings["AWSAssociateTag"];
            r1["ResponseGroup"] = "Images,ItemAttributes";

            var requestUrl = helper.Sign(r1);
            var items = FetchImageObjects(requestUrl, verbose);

            if (verbose)
            {
                Console.WriteLine($"Found ({(items?.Count ?? 0)}) items.");

                foreach (var image in items ?? new List<AWSMusicItem>())
                {
                    Console.WriteLine($"Item '{image.Attributes.Title}' by '{image.Attributes.Artist}' Large image URL: {image.LargeImage.Url}");
                }
            }

            return items?.Count == 0 ? null : items[0]?.LargeImage?.Url ?? null;
        }

        /// <summary>
        /// Helper method to RESTfully call the AWS API endpoint and parse the response.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private List<AWSMusicItem> FetchImageObjects(string url, bool verbose)
        {
            try
            {
                var request = HttpWebRequest.Create(url);
                var response = request.GetResponse();
                var doc = new XmlDocument();
                doc.Load(response.GetResponseStream());

                var nodes = doc.GetElementsByTagName("Item", ConfigurationManager.AppSettings["AWSXMLNamespace"]);
                var list = new List<AWSMusicItem>();
                foreach (XmlNode node in nodes)
                {
                    var deserializeddNode = ConvertNode<AWSMusicItem>(node);
                    list.Add(deserializeddNode);
                }
                return list;

            }
            catch (TimeoutException)
            {
                System.Threading.Thread.Sleep(expRetry);
                expRetry *= 2;
                return FetchImageObjects(url, verbose);
            }
            catch (WebException ex)
            {

                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var doc = new XmlDocument();
                    doc.Load(reader);
                    var errorMessageNodes = doc.GetElementsByTagName("Message", "http://ecs.amazonaws.com/doc/2011-08-01/");
                    var errorCodeNodes = doc.GetElementsByTagName("Code", "http://ecs.amazonaws.com/doc/2011-08-01/");

                    if (errorMessageNodes != null && errorMessageNodes.Count > 0)
                    {
                        var message = errorMessageNodes.Item(0).InnerText;
                        var code = errorCodeNodes.Item(0).InnerText;

                        if (throttled.IsMatch(code))
                        {
                            System.Threading.Thread.Sleep(expRetry);
                            expRetry *= 2;
                            return FetchImageObjects(url, verbose);
                        }

                        throw new Exception($"Error {code}: {message} (but signature worked)");
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Helper method to deserialize an XMLNode object to any object type.
        /// <para>Credit goes to marc_s here:http://stackoverflow.com/questions/1563473/xmlnode-to-objects </para>
        /// </summary>
        /// <typeparam name="T">Type to deserialize into</typeparam>
        /// <param name="node">The <see cref="XmlNode"/> to deserialize</param>
        /// <returns>Deserialized object</returns>
        private static T ConvertNode<T>(XmlNode node) where T : class
        {
            using (MemoryStream stm = new MemoryStream()) {
                using (StreamWriter stw = new StreamWriter(stm))
                {
                    stw.Write(node.OuterXml);
                    stw.Flush();

                    stm.Position = 0;

                    XmlSerializer ser = new XmlSerializer(typeof(T), ConfigurationManager.AppSettings["AWSXMLNamespace"]);
                    T result = (ser.Deserialize(stm) as T);

                    return result;
                }
            }
        }
    }
}
