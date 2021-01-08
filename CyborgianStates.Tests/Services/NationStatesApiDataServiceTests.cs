using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using CyborgianStates.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Tests.Services
{
    public class NationStatesApiDataServiceTests
    {
        [Fact]
        public void TestBuildApiRequestUrl()
        {
            var res = NationStatesApiDataService.BuildApiRequestUrl("test");
            res.Should().BeOfType<Uri>();
            res.Should().Be("http://www.nationstates.net/cgi-bin/api.cgi?test&v=10");
        }

        [Fact]
        public async Task TestExecuteRequestFailures()
        {
            var httpService = new Mock<IHttpDataService>(MockBehavior.Strict);
            httpService
                .Setup(m => m.ExecuteRequestAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<EventId>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
            var dataService = new NationStatesApiDataService(httpService.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await dataService.ExecuteRequestAsync(null).ConfigureAwait(false); })
                .ConfigureAwait(false);

            var failureRequest = new Request(RequestType.UnitTest, ResponseFormat.HttpResponseMessage, DataSourceType.NationStatesAPI);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => { await dataService.ExecuteRequestAsync(failureRequest).ConfigureAwait(false); })
                .ConfigureAwait(false);

            var missingParamRequest = new Request(RequestType.GetBasicNationStats, ResponseFormat.HttpResponseMessage, DataSourceType.NationStatesAPI);
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => { await dataService.ExecuteRequestAsync(missingParamRequest).ConfigureAwait(false); })
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task TestExecuteRequest()
        {
            var httpService = new Mock<IHttpDataService>(MockBehavior.Strict);
            httpService
                .Setup(m => m.ExecuteRequestAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<EventId>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<xml></xml>") }));
            var dataService = new NationStatesApiDataService(httpService.Object);

            var request = new Request(RequestType.GetBasicNationStats, ResponseFormat.HttpResponseMessage, DataSourceType.NationStatesAPI);
            request.Params.Add("nationName", Helpers.ToID("Testlandia"));
            await dataService.ExecuteRequestAsync(request).ConfigureAwait(false);
            request.Status.Should().Be(RequestStatus.Success);

            /* Failure Test */

            httpService
                .Setup(m => m.ExecuteRequestAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<EventId>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));
            dataService = new NationStatesApiDataService(httpService.Object);
            await dataService.ExecuteRequestAsync(request).ConfigureAwait(false);
            request.Status.Should().Be(RequestStatus.Failed);

            /* Result Null Test */

            httpService
                .Setup(m => m.ExecuteRequestAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<EventId>()))
                .Returns(Task.FromResult<HttpResponseMessage>(null));
            dataService = new NationStatesApiDataService(httpService.Object);
            await dataService.ExecuteRequestAsync(request).ConfigureAwait(false);
            request.Status.Should().Be(RequestStatus.Failed);
        }
    }
}