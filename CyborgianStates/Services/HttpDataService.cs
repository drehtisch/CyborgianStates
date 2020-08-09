using CyborgianStates.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CyborgianStates.Services
{
    public class HttpDataService : IHttpDataService
    {
        private AppSettings _config;
        private HttpMessageHandler _httpMessageHandler = null;
        private ILogger<HttpDataService> _logger;

        public HttpDataService(IOptions<AppSettings> config, ILogger<HttpDataService> logger)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));
            if (logger is null) throw new ArgumentNullException(nameof(logger));
            _config = config.Value;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> ExecuteRequest(HttpRequestMessage httpRequest, EventId eventId)
        {
            if (httpRequest is null) throw new ArgumentNullException(nameof(httpRequest));
            if (string.IsNullOrWhiteSpace(_config.Contact)) throw new InvalidOperationException("No Request can be send when contact info hasn't been provided.");
            using (HttpClient client = GetHttpClient())
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

        public HttpClient GetHttpClient()
        {
            if (_httpMessageHandler != null)
            {
                return new HttpClient(_httpMessageHandler);
            }
            else
            {
                return new HttpClient();
            }
        }

        public void SetHttpMessageHandler(HttpMessageHandler httpMessageHandler)
        {
            _httpMessageHandler = httpMessageHandler;
        }
    }
}