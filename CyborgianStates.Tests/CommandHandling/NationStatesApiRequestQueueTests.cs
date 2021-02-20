using CyborgianStates.CommandHandling;
using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Tests.CommandHandling
{
    public class NationStatesApiRequestQueueTests : IDisposable
    {
        private bool disposedValue;
        private CancellationTokenSource source;

        public NationStatesApiRequestQueueTests()
        {
            disposedValue = false;
            source = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task TestEnqueueMultipleRequests()
        {
            using (var dummyResponseMessage = GetDummyResponse())
            {
                Mock<IDataService> dataService = new Mock<IDataService>(MockBehavior.Strict);
                dataService.Setup(d => d.ExecuteRequestAsync(It.IsAny<Request>())).Returns(() => Task.CompletedTask);

                GetQueueAndRequest(dataService, out NationStatesApiRequestWorker queue, out Request request);
                GetQueueAndRequest(dataService, out _, out Request request2);
                queue.Enqueue(request);
                queue.Enqueue(request2);
                _ = Task.Run(async () =>
                {
                    await Task.Delay(100);
                    request.Complete(null);
                    await Task.Delay(100);
                    request2.Complete(null);
                }
                );
                await request.WaitForResponseAsync(source.Token).ConfigureAwait(false);
                request.Status.Should().Be(RequestStatus.Success);
                await request2.WaitForResponseAsync(source.Token).ConfigureAwait(false);
                request2.Status.Should().Be(RequestStatus.Success);
            }
        }

        [Fact]
        public async Task TestEnqueueRequest()
        {
            using (var dummyResponseMessage = GetDummyResponse())
            {
                Mock<IDataService> dataService = new Mock<IDataService>(MockBehavior.Strict);
                dataService.Setup(d => d.ExecuteRequestAsync(It.IsAny<Request>())).Returns(Task.FromResult((object)dummyResponseMessage));

                GetQueueAndRequest(dataService, out NationStatesApiRequestWorker queue, out Request request);

                Assert.Throws<ArgumentNullException>(() => queue.Enqueue(null));
                await ExecuteWithExpectedResult(dataService, RequestStatus.Success).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestEnqueueRequestWithExecuteKnowFailure()
        {
            using (var dummyResponseMessage = GetDummyResponse())
            {
                Mock<IDataService> dataService = new Mock<IDataService>(MockBehavior.Strict);
                dataService.Setup(d => d.ExecuteRequestAsync(It.IsAny<Request>())).Returns(() => throw new ApplicationException("Unit Test: Forced Execution Failure"));

                await ExecuteWithExpectedResult(dataService, RequestStatus.Failed).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestEnqueueRequestWithExecuteUnexpectedFailure()
        {
            using (var dummyResponseMessage = GetDummyResponse())
            {
                Mock<IDataService> dataService = new Mock<IDataService>(MockBehavior.Strict);
                dataService.Setup(d => d.ExecuteRequestAsync(It.IsAny<Request>())).Returns(() => throw new Exception("Unit Test: Forced Execution Failure"));
                await ExecuteWithExpectedResult(dataService, RequestStatus.Failed).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestEnqueueRequestWithWrongExecuteResult()
        {
            using (var dummyResponseMessage = GetDummyResponse())
            {
                Mock<IDataService> dataService = new Mock<IDataService>(MockBehavior.Strict);
                dataService.Setup(d => d.ExecuteRequestAsync(It.IsAny<Request>())).Returns(() => Task.FromResult((object)null));
                await ExecuteWithExpectedResult(dataService, RequestStatus.Failed).ConfigureAwait(false);
            }
        }

        #region Helpers

        private static HttpResponseMessage GetDummyResponse()
        {
            string dummyXml = "<?xml version=\"1.0\"?><Dummy>1</Dummy>";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(dummyXml)
            };
        }

        private void GetQueueAndRequest(Mock<IDataService> dataService, out NationStatesApiRequestWorker queue, out Request request)
        {
            var _queue = new NationStatesApiRequestWorker(dataService.Object);
            queue = _queue;
            request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            var tmpSource = new CancellationTokenSource();
            Task.Run(async () => await _queue.RunAsync(tmpSource.Token));
            tmpSource.Cancel();
        }

        private async Task ExecuteWithExpectedResult(Mock<IDataService> dataService, RequestStatus expectedStatus)
        {
            GetQueueAndRequest(dataService, out NationStatesApiRequestWorker queue, out Request request);

            queue.Enqueue(request);
            _ = Task.Run(async () =>
            {
                await Task.Delay(100);
                if (expectedStatus == RequestStatus.Success)
                {
                    request.Complete(null);
                }
                else if (expectedStatus == RequestStatus.Failed)
                {
                    request.Fail("Unit Test", new Exception());
                }
            });
            if (expectedStatus == RequestStatus.Success)
            {
                await request.WaitForResponseAsync(source.Token).ConfigureAwait(false);
            }
            else
            {
                await Assert.ThrowsAnyAsync<Exception>(async () => await request.WaitForResponseAsync(source.Token).ConfigureAwait(false));
            }
            expectedStatus.Should().Be(request.Status);
        }

        #endregion Helpers

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
    }
}