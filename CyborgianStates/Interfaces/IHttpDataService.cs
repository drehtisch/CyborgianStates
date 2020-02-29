using CyborgianStates.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CyborgianStates.Interfaces
{
    public interface IHttpDataService: IDataService
    {
        Task<HttpResponseMessage> ExecuteRequest(HttpRequestMessage httpRequest);
        Task<XmlDocument> ExecuteRequestWithXmlResult(HttpRequestMessage httpRequest);
        Task<Stream> ExecuteRequestWithStreamResult(HttpRequestMessage httpRequest);
    }
}
