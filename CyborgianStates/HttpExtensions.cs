using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace CyborgianStates
{
    public static class HttpExtensions
    {
        public static void AddCyborgianStatesUserAgent(this HttpClient client, string version, string contact)
        {
            if (client is null) throw new ArgumentNullException(nameof(client));
            client.DefaultRequestHeaders.Add("User-Agent", $"CyborgianStates/{version}");
            client.DefaultRequestHeaders.Add("User-Agent", $"(contact {contact};)");
        }

        public static async Task<XmlDocument> ReadXml(this HttpResponseMessage httpResponse)
        {
            if (httpResponse is null) throw new ArgumentNullException(nameof(httpResponse));
            using (Stream stream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(stream);
                    return xml;
                }
                catch (XmlException ex)
                {
                    throw new ApplicationException($"A error while loading xml occured.", ex);
                }
            }
        }
    }
}
