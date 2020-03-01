using CyborgianStates.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CyborgianStates.Interfaces
{
    public interface IHttpDataService
    {
        Task<HttpResponseMessage> ExecuteRequest(HttpRequestMessage httpRequest, EventId eventId);
        Task<XmlDocument> ExecuteRequestWithXmlResult(HttpRequestMessage httpRequest, EventId eventId);
        Task<Stream> ExecuteRequestWithStreamResult(HttpRequestMessage httpRequest, EventId eventId);
    }
}
