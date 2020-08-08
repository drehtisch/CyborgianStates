using CyborgianStates.CommandHandling;
using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Tests.CommandHandling
{
    public class NationStatesApiRequestQueueTests : IDisposable
    {
        CancellationTokenSource source = new CancellationTokenSource();
        private bool disposedValue;

        [Fact]
        public async Task TestEnqueueRequest()
        {
            using (var dummyResponseMessage = GetDummyResponse())
            {
                Mock<IDataService> dataService = new Mock<IDataService>(MockBehavior.Strict);
                dataService.Setup(d => d.ExecuteRequest(It.IsAny<Request>())).Returns(Task.FromResult((object)dummyResponseMessage));
                dataService.Setup(d => d.WaitForAction(It.IsAny<RequestType>())).Returns(Task.CompletedTask);

                GetQueueAndRequest(dataService, out NationStatesApiRequestQueue queue, out Request request);

                await Assert.ThrowsAsync<ArgumentNullException>(async () => await queue.Enqueue(null).ConfigureAwait(false)).ConfigureAwait(false);
                await ExecuteWithExpectedResult(dataService, RequestStatus.Success).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestEnqueueRequestWithExecuteKnowFailure()
        {
            using (var dummyResponseMessage = GetDummyResponse())
            {
                Mock<IDataService> dataService = new Mock<IDataService>(MockBehavior.Strict);
                dataService.Setup(d => d.ExecuteRequest(It.IsAny<Request>())).Returns(() => throw new ApplicationException("Unit Test: Forced Execution Failure"));
                dataService.Setup(d => d.WaitForAction(It.IsAny<RequestType>())).Returns(Task.CompletedTask);

                await ExecuteWithExpectedResult(dataService, RequestStatus.Failed).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestEnqueueRequestWithExecuteUnexpectedFailure()
        {
            using (var dummyResponseMessage = GetDummyResponse())
            {
                Mock<IDataService> dataService = new Mock<IDataService>(MockBehavior.Strict);
                dataService.Setup(d => d.ExecuteRequest(It.IsAny<Request>())).Returns(() => throw new Exception("Unit Test: Forced Execution Failure"));
                dataService.Setup(d => d.WaitForAction(It.IsAny<RequestType>())).Returns(Task.CompletedTask);
                await ExecuteWithExpectedResult(dataService, RequestStatus.Failed).ConfigureAwait(false);
            }
        }


        [Fact]
        public async Task TestEnqueueRequestWithWrongExecuteResult()
        {
            using (var dummyResponseMessage = GetDummyResponse())
            {
                Mock<IDataService> dataService = new Mock<IDataService>(MockBehavior.Strict);
                dataService.Setup(d => d.ExecuteRequest(It.IsAny<Request>())).Returns(() => Task.FromResult((object)null));
                dataService.Setup(d => d.WaitForAction(It.IsAny<RequestType>())).Returns(Task.CompletedTask);
                await ExecuteWithExpectedResult(dataService, RequestStatus.Failed).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestEnqueueMultipleRequests()
        {
            using (var dummyResponseMessage = GetDummyResponse())
            {
                Mock<IDataService> dataService = new Mock<IDataService>(MockBehavior.Strict);
                dataService.Setup(d => d.ExecuteRequest(It.IsAny<Request>())).Returns(() => Task.FromResult((object)GetDummyResponse()));
                dataService.Setup(d => d.WaitForAction(It.IsAny<RequestType>())).Returns(() => Task.Delay(10));

                GetQueueAndRequest(dataService, out NationStatesApiRequestQueue queue, out Request request);
                GetQueueAndRequest(dataService, out _, out Request request2);
                await queue.Enqueue(request).ConfigureAwait(false);
                var position = await queue.Enqueue(request2).ConfigureAwait(false);
                Assert.Equal(2, position);
                await request.WaitForResponse(source.Token).ConfigureAwait(false);
                Assert.Equal(RequestStatus.Success, request.Status);
                await request2.WaitForResponse(source.Token).ConfigureAwait(false);
                Assert.Equal(RequestStatus.Success, request2.Status);
            }
        }
        #region Helpers
        private static void VerifyDataServiceCalls(Mock<IDataService> dataService)
        {
            dataService.Verify(d => d.WaitForAction(It.IsAny<RequestType>()), Times.AtLeastOnce);
            dataService.Verify(d => d.ExecuteRequest(It.IsAny<Request>()), Times.AtLeastOnce);
        }
        private async Task ExecuteWithExpectedResult(Mock<IDataService> dataService, RequestStatus expectedStatus)
        {
            GetQueueAndRequest(dataService, out NationStatesApiRequestQueue queue, out Request request);

            await queue.Enqueue(request).ConfigureAwait(false);
            await request.WaitForResponse(source.Token).ConfigureAwait(false);
            Assert.Equal(expectedStatus, request.Status);
            VerifyDataServiceCalls(dataService);
        }
        private static void GetQueueAndRequest(Mock<IDataService> dataService, out NationStatesApiRequestQueue queue, out Request request)
        {
            queue = new NationStatesApiRequestQueue(dataService.Object);
            request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
        }
        private static HttpResponseMessage GetDummyResponse()
        {
            string dummyXml = "<?xml version=\"1.0\"?><Dummy>1</Dummy>";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(dummyXml)
            };

        }
        #endregion
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    source.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
