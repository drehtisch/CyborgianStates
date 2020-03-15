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
    }
}
