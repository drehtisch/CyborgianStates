using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Tests.CommandHandling
{
    public class RequestTests
    {
        [Fact]
        public async Task TestCancel()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            request.Priority = 1;
            request.Status.Should().Be(RequestStatus.Pending);
            _ = Task.Run(async () =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                tokenSource.Cancel(false);
            });
            request.Priority.Should().Be(1);
            await Assert.ThrowsAsync<TaskCanceledException>(async () => await request.WaitForResponseAsync(tokenSource.Token).ConfigureAwait(false));
            request.Status.Should().Be(RequestStatus.Canceled);
            tokenSource.Dispose();
        }

        [Fact]
        public async Task TestComplete()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            var requestType = request.Type;
            request.Params.Add("nationName", "Testlandia");
            request.Status.Should().Be(RequestStatus.Pending);
            _ = Task.Run(async () =>
              {
                  await Task.Delay(100).ConfigureAwait(false);
                  request.Complete("Success !");
              });
            await request.WaitForResponseAsync(tokenSource.Token).ConfigureAwait(false);
            request.Status.Should().Be(RequestStatus.Success);
            request.Response.Should().Be("Success !");
            tokenSource.Dispose();
        }

        [Fact]
        public async Task TestFailure()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Request request = new();
            request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            request.Status.Should().Be(RequestStatus.Pending);
            _ = Task.Run(async () =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                request.Fail("Failed !", null);
            });
            await Assert.ThrowsAsync<Exception>(async () => await request.WaitForResponseAsync(tokenSource.Token).ConfigureAwait(false));
            request.Status.Should().Be(RequestStatus.Failed);
            request.FailureReason.Should().Be("Failed !");
            tokenSource.Dispose();
        }
    }
}