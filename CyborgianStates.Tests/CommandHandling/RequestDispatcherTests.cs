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
        public void TestRegisterAndThrowOnDispatch()
        {
            RequestDispatcher dispatcher = new RequestDispatcher();
            var request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            Assert.Throws<ArgumentNullException>(() => dispatcher.Dispatch(null, 0));
            Assert.Throws<InvalidOperationException>(() => dispatcher.Dispatch(request, 0));
        }

        [Fact]
        public void TestReqisterAndDispatch()
        {
            RequestDispatcher dispatcher = new RequestDispatcher();
            var requestQueue = new Mock<IRequestWorker>(MockBehavior.Strict);
            requestQueue.Setup(r => r.Enqueue(It.IsAny<Request>(), It.IsAny<int>()));
            dispatcher.Register(DataSourceType.NationStatesAPI, requestQueue.Object);
            dispatcher.Start();
            Assert.Throws<InvalidOperationException>(() => dispatcher.Register(DataSourceType.NationStatesAPI, requestQueue.Object));
            Assert.Throws<InvalidOperationException>(() => dispatcher.Start());
            var request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            dispatcher.Dispatch(request, 0);
            requestQueue.Raise(m => m.RestartRequired += null, requestQueue.Object, null);
            requestQueue.Verify(r => r.Enqueue(It.IsAny<Request>(), It.IsAny<int>()), Times.Once);
            dispatcher.Shutdown();
        }
    }
}