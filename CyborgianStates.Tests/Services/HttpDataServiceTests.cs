using Castle.Core.Logging;
using CyborgianStates.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
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
    public class HttpDataServiceTests
    {
        [Fact]
        public async Task TestExecuteRequestWithoutContactInfo()
        {
            var options = new Mock<IOptions<AppSettings>>(MockBehavior.Strict);
            options
                .Setup(m => m.Value)
                .Returns(new AppSettings() { Contact = "" });

            ILogger<HttpDataService> logger = ApplicationLogging.CreateLogger<HttpDataService>();
            var httpService = new HttpDataService(options.Object, logger);

            var id = Helpers.GetEventIdByType(Enums.LoggingEvent.TestRequest);
            var message = new HttpRequestMessage(HttpMethod.Get, "0.0.0.0");

            Func<Task> act = async () => { await httpService.ExecuteRequest(message, id).ConfigureAwait(false); };
            await act.Should().ThrowAsync<InvalidOperationException>().ConfigureAwait(false);

            message.Dispose();
        }
        [Fact]
        public async Task TestExecuteRequestWithOutRequest()
        {
            var options = new Mock<IOptions<AppSettings>>(MockBehavior.Strict);
            options
                .Setup(m => m.Value)
                .Returns(new AppSettings() { Contact = "contact@example.com" });

            ILogger<HttpDataService> logger = ApplicationLogging.CreateLogger<HttpDataService>();
            var httpService = new HttpDataService(options.Object, logger);

            var id = Helpers.GetEventIdByType(Enums.LoggingEvent.TestRequest);

            Func<Task> act = async () => { await httpService.ExecuteRequest(null, id).ConfigureAwait(false); };
            await act.Should().ThrowAsync<ArgumentNullException>().ConfigureAwait(false);
        }
        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
        public async Task TestExecuteRequestWithSuccessResponse()
        {
            var options = new Mock<IOptions<AppSettings>>(MockBehavior.Strict);
            options.Setup(m => m.Value).Returns(new AppSettings() { Contact = "contact@example.com" });
            ILogger<HttpDataService> logger = ApplicationLogging.CreateLogger<HttpDataService>();
            var httpService = new HttpDataService(options.Object, logger);

            var id = Helpers.GetEventIdByType(Enums.LoggingEvent.TestRequest);
            var message = new HttpRequestMessage(HttpMethod.Get, "http://0.0.0.0");

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(""),
                })
                .Verifiable();
            httpService.SetHttpMessageHandler(handlerMock.Object);
            var response = await httpService.ExecuteRequest(message, id).ConfigureAwait(false);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get
                    && req.RequestUri == new Uri("http://0.0.0.0")
                ),
                ItExpr.IsAny<CancellationToken>()
           );
           message.Dispose();
        }
        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
        public async Task TestExecuteRequestWithFailureResponse()
        {
            var options = new Mock<IOptions<AppSettings>>(MockBehavior.Strict);
            options.Setup(m => m.Value).Returns(new AppSettings() { Contact = "contact@example.com" });
            ILogger<HttpDataService> logger = ApplicationLogging.CreateLogger<HttpDataService>();
            var httpService = new HttpDataService(options.Object, logger);

            var id = Helpers.GetEventIdByType(Enums.LoggingEvent.TestRequest);
            var message = new HttpRequestMessage(HttpMethod.Get, "http://0.0.0.0");

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadGateway,
                    Content = new StringContent(""),
                })
                .Verifiable();
            httpService.SetHttpMessageHandler(handlerMock.Object);
            var response = await httpService.ExecuteRequest(message, id).ConfigureAwait(false);
            response.StatusCode.Should().Be(HttpStatusCode.BadGateway);

            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get
                    && req.RequestUri == new Uri("http://0.0.0.0")
                ),
                ItExpr.IsAny<CancellationToken>()
           );
            message.Dispose();
        }
    }
}
