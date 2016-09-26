/// <summary>
/// Author: Justin Robb
/// Date: 9/25/2016
/// 
/// Project Description:
/// Uses Amazon AWS Advertising API to retrieve album art.
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
    using System.Text.RegularExpressions;
    using Modals;

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
        public List<AWSMusicItem> GetAlbumArt(string album, string artist, bool verbose = false)
        {
            if (album == null || artist == null || (string.IsNullOrEmpty(album) && string.IsNullOrEmpty(artist)))
            {
                return null;
            }

            IDictionary<string, string> request = new Dictionary<string, String>();
            request["Service"] = "AWSECommerceService";
            request["Operation"] = "ItemSearch";
            request["Title"] = album; // Keywords
            request["Version"] = "2011-08-01";
            request["SearchIndex"] = "Music";
            request["Artist"] = artist;
            request["AssociateTag"] = ConfigurationManager.AppSettings["AWSAssociateTag"];
            request["ResponseGroup"] = "Images,ItemAttributes";

            var result = GetAlbumArt(request, verbose);
            if (result != null)
            {
                result.RemoveAll(item => (item?.Attributes?.Artist ?? null) == null || (item?.Attributes?.Title ?? null) == null);
                result.RemoveAll(item => (item?.LargeImage?.Url ?? null) == null);
            }
            return result;
        }

        /// <summary>
        /// Retrieves album art from AWS using the given search parameters. 
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="verbose">Prints helpful debugging info to console.</param>
        /// <returns> The URL for the largest image listed for the item which bestb matches the search parameters.</returns>
        public List<AWSMusicItem> GetAlbumArt(string query, bool verbose = false)
        {
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }

            IDictionary<string, string> request = new Dictionary<string, String>();
            request["Service"] = "AWSECommerceService";
            request["Operation"] = "ItemSearch";
            request["Keywords"] = query;
            request["Version"] = "2011-08-01";
            request["SearchIndex"] = "Music";
            request["AssociateTag"] = ConfigurationManager.AppSettings["AWSAssociateTag"];
            request["ResponseGroup"] = "Images,ItemAttributes";

            var result = GetAlbumArt(request, verbose);
            if (result != null)
            {
                result.RemoveAll(item => (item?.Attributes?.Artist ?? null) == null || (item?.Attributes?.Title ?? null) == null);
                result.RemoveAll(item => (item?.LargeImage?.Url ?? null) == null);
            }
            return result;
        }

        /// <summary>
        /// Helper to constrct API query.
        /// </summary>
        private List<AWSMusicItem> GetAlbumArt(IDictionary<string, string> requestDict, bool verbose = false) {


            SignedRequestHelper helper = new SignedRequestHelper(ConfigurationManager.AppSettings["AWSKeyID"], 
                ConfigurationManager.AppSettings["AWSSecretKey"], ConfigurationManager.AppSettings["AWSDestination"]);

            var requestUrl = helper.Sign(requestDict);
            var items = FetchImageObjects(requestUrl, verbose);

            if (verbose)
            {
                Console.WriteLine($"Found ({(items?.Count ?? 0)}) items.");

                foreach (var image in items ?? new List<AWSMusicItem>())
                {
                    Console.WriteLine($"Item '{image.Attributes.Title}' by '{image.Attributes.Artist}' Large image URL: {image.LargeImage.Url}");
                }
            }

            return (items?.Count ?? 0) == 0 ? null : items;
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
                expRetry = Math.Min(expRetry*2, new Random(DateTime.Now.Millisecond).Next(5000, 15000)); // cap at 5-15 seconds
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
                            expRetry = Math.Min(expRetry * 2, new Random(DateTime.Now.Millisecond).Next(5000, 15000)); // cap at 5-15 seconds
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

                    var xml = new XmlSerializer(typeof(T), ConfigurationManager.AppSettings["AWSXMLNamespace"]);
                    T result = (xml.Deserialize(stm) as T);

                    return result;
                }
            }
        }
    }
}
