using CyborgianStates.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace CyborgianStates.Services
{
    public class HttpDataService : IHttpDataService
    {
        AppSettings _config;
        ILogger<HttpDataService> _logger;
        public HttpDataService(IOptions<AppSettings> config, ILogger<HttpDataService> logger)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));
            _config = config.Value;
            _logger = logger;
        }
        public async Task<HttpResponseMessage> ExecuteRequest(HttpRequestMessage httpRequest, EventId eventId)
        {
            if (httpRequest is null) throw new ArgumentNullException(nameof(httpRequest));
            if (string.IsNullOrWhiteSpace(_config.Contact)) throw new InvalidOperationException("No Request can be send when contact info hasn't been provided.");
            using (HttpClient client = new HttpClient())
            {
                client.AddCyborgianStatesUserAgent(AppSettings.VERSION, _config.Contact);
                _logger.LogDebug(eventId, LogMessageBuilder.Build(eventId, $"Executing {httpRequest.Method}-Request to {httpRequest.RequestUri}"));
                HttpResponseMessage response = await client.SendAsync(httpRequest).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(eventId, LogMessageBuilder.Build(eventId, $"Request finished with response: {(int)response.StatusCode}: {response.ReasonPhrase}"));
                }
                else
                {
                    _logger.LogDebug(eventId, LogMessageBuilder.Build(eventId, $"Request finished with response: {(int)response.StatusCode}: {response.ReasonPhrase}"));
                }
                return response;
            }
        }

        public async Task<Stream> ExecuteRequestWithStreamResult(HttpRequestMessage httpRequest, EventId eventId)
        {
            HttpResponseMessage response = await ExecuteRequest(httpRequest, eventId).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            }
            else if(response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException();
            }
            else
            {
                throw new ApplicationException("The request failed. No stream could be returned.");
            }
        }

        public async Task<XmlDocument> ExecuteRequestWithXmlResult(HttpRequestMessage httpRequest, EventId eventId)
        {
            using (var stream = await ExecuteRequestWithStreamResult(httpRequest, eventId).ConfigureAwait(false))
            {
                try
                {
                    XmlDocument xml = new XmlDocument();
                    xml.Load(stream);
                    return xml;
                }
                catch (XmlException ex)
                {
                    string xml;
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        xml = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                    _logger.LogCritical(eventId, ex, LogMessageBuilder.Build(eventId, $"A critical error while loading xml occured. XML:{Environment.NewLine}{xml}"));
                    throw;
                }
            }
        }
    }

    public static class HttpClientExtensions
    {
        public static void AddCyborgianStatesUserAgent(this HttpClient client, string version, string contact)
        {
            if (client is null) throw new ArgumentNullException(nameof(client));
            client.DefaultRequestHeaders.Add("User-Agent", $"CyborgianStates/{version}");
            client.DefaultRequestHeaders.Add("User-Agent", $"(contact {contact};)");
        }
    }
}
