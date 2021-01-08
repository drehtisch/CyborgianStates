using CyborgianStates.CommandHandling;
using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Tests.CommandHandling
{
    public class RequestQueueTests
    {
        [Fact]
        public void TestEnqueue()
        {
            RequestPriorityQueue queue = new();
            Assert.Throws<NotImplementedException>(() => queue.EnqueueWithoutDuplicates(new Request(RequestType.UnitTest, ResponseFormat.Stream, DataSourceType.NationStatesAPI), 0));
            Task.Run(async () => await queue.WaitForNextItemAsync(CancellationToken.None));

            var completedRequest = new Request(RequestType.UnitTest, ResponseFormat.Stream, DataSourceType.NationStatesAPI);
            queue.Enqueue(completedRequest, 1);
            completedRequest.Complete(null);
            queue.Count.Should().Be(1);
            queue.Enqueue(new Request(RequestType.UnitTest, ResponseFormat.Stream, DataSourceType.NationStatesAPI), 2);
            queue.Count.Should().Be(1);
            var dequeuedRequest = queue.Dequeue();
            dequeuedRequest.Status.Should().Be(RequestStatus.Pending);
            dequeuedRequest.ExpectedReponseFormat.Should().Be(ResponseFormat.Stream);
        }
    }
}