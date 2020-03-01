using CyborgianStates.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace CyborgianStates.Services
{
    public class HttpDataService : IHttpDataService
    {
        public Task<HttpResponseMessage> ExecuteRequest(HttpRequestMessage httpRequest, EventId eventId)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> ExecuteRequestWithStreamResult(HttpRequestMessage httpRequest, EventId eventId)
        {
            throw new NotImplementedException();
        }

        public Task<XmlDocument> ExecuteRequestWithXmlResult(HttpRequestMessage httpRequest, EventId eventId)
        {
            throw new NotImplementedException();
        }
    }
}
