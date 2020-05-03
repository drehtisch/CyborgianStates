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
using System.Text;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
        public async Task TestExecuteRequest()
        {
            var httpService = new Mock<IHttpDataService>(MockBehavior.Strict);
            httpService
                .Setup(m => m.ExecuteRequest(It.IsAny<HttpRequestMessage>(), It.IsAny<EventId>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
            var dataService = new NationStatesApiDataService(httpService.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => { await dataService.ExecuteRequest(null).ConfigureAwait(false); }).ConfigureAwait(false);

            var failureRequest = new Request(RequestType.UnitTest, ResponseFormat.HttpResponseMessage, DataSourceType.NationStatesAPI);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => { await dataService.ExecuteRequest(failureRequest).ConfigureAwait(false); })
                .ConfigureAwait(false);

            var missingParamRequest = new Request(RequestType.GetBasicNationStats, ResponseFormat.HttpResponseMessage, DataSourceType.NationStatesAPI);
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => { await dataService.ExecuteRequest(missingParamRequest).ConfigureAwait(false); })
                .ConfigureAwait(false);

            var successRequest = new Request(RequestType.GetBasicNationStats, ResponseFormat.HttpResponseMessage, DataSourceType.NationStatesAPI);
            successRequest.Params.Add("nationName", Helpers.ToID("Testlandia"));
            var res = await dataService.ExecuteRequest(successRequest).ConfigureAwait(false);
            res.Should().BeOfType<HttpResponseMessage>();
            (res as HttpResponseMessage).StatusCode.Should().Be(HttpStatusCode.OK);
        }
        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
        public async Task TestIsActionReady()
        {
            var httpService = new Mock<IHttpDataService>(MockBehavior.Strict);
            httpService
                .Setup(m => m.ExecuteRequest(It.IsAny<HttpRequestMessage>(), It.IsAny<EventId>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
            var dataService = new NationStatesApiDataService(httpService.Object);

            var res = await dataService.IsActionReady(RequestType.GetBasicNationStats).ConfigureAwait(false);
            res.Should().BeTrue();

            res = await dataService.IsActionReady(RequestType.UnitTest).ConfigureAwait(false);
            res.Should().BeFalse();
        }
        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2008:Keine Tasks ohne Übergabe eines TaskSchedulers erstellen", Justification = "<Ausstehend>")]
        public async Task TestWaitForAction()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            var httpService = new Mock<IHttpDataService>(MockBehavior.Strict);
            httpService
                .Setup(m => m.ExecuteRequest(It.IsAny<HttpRequestMessage>(), It.IsAny<EventId>()))
                .Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
            var dataService = new NationStatesApiDataService(httpService.Object);

            var successRequest = new Request(RequestType.GetBasicNationStats, ResponseFormat.HttpResponseMessage, DataSourceType.NationStatesAPI);
            successRequest.Params.Add("nationName", Helpers.ToID("Testlandia"));

            await dataService.ExecuteRequest(successRequest).ConfigureAwait(false);
            await dataService.WaitForAction(RequestType.GetBasicNationStats, TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);

            await dataService.ExecuteRequest(successRequest).ConfigureAwait(false);
            await dataService.WaitForAction(RequestType.GetBasicNationStats, TimeSpan.FromMilliseconds(50), tokenSource.Token).ConfigureAwait(false);

            await Task.Delay(TimeSpan.FromMilliseconds(200)).ContinueWith(x => tokenSource.Cancel()).ConfigureAwait(false);
            await dataService.WaitForAction(RequestType.UnitTest, TimeSpan.FromMilliseconds(100), tokenSource.Token).ConfigureAwait(false);
        }
    }
}
