using CyborgianStates.CommandHandling;
using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Tests.CommandHandling
{
    public class RequestDispatcherTests
    {
        [Fact]
        public async Task TestRegisterAndThrowOnDispatch()
        {
            RequestDispatcher dispatcher = new RequestDispatcher();
            var request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            await Assert.ThrowsAsync<ArgumentNullException>(() => dispatcher.Dispatch(null)).ConfigureAwait(false);
            await Assert.ThrowsAsync<InvalidOperationException>(() => dispatcher.Dispatch(request)).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestReqisterAndDispatch()
        {
            RequestDispatcher dispatcher = new RequestDispatcher();
            var requestQueue = new Mock<IRequestWorker>(MockBehavior.Strict);
            requestQueue.Setup(r => r.Enqueue(It.IsAny<Request>())).Returns(Task.FromResult(1));
            await dispatcher.Register(DataSourceType.NationStatesAPI, requestQueue.Object).ConfigureAwait(false);
            var request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            await dispatcher.Dispatch(request).ConfigureAwait(false);
            requestQueue.Verify(r => r.Enqueue(It.IsAny<Request>()), Times.Once);
        }
    }
}