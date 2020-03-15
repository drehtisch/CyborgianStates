using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CyborgianStates.Test
{
    public class RequestTests
    {
        [Fact]
        public async Task TestComplete()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            var requestType = request.Type;
            request.Params.Add("nationName", "Testlandia");
            var expectedResponseFormat = request.ExpectedReponseFormat;
            var priority = request.Priority++;
            Assert.True(request.Status == RequestStatus.Pending);
            _ = Task.Run(async () =>
              {
                  await Task.Delay(100).ConfigureAwait(false);
                  request.Complete("Success !");
              });
            await request.WaitForResponse(tokenSource.Token).ConfigureAwait(false);
            Assert.True(request.Status == RequestStatus.Success);
            Assert.True(request.Response.ToString() == "Success !");
            tokenSource.Dispose();
        }

        [Fact]
        public async Task TestFailure()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            Assert.True(request.Status == RequestStatus.Pending);
            _ = Task.Run(async () =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                request.Fail("Failed !");
            });
            await request.WaitForResponse(tokenSource.Token).ConfigureAwait(false);
            Assert.True(request.Status == RequestStatus.Failed);
            Assert.True(request.FailureReason == "Failed !");
            tokenSource.Dispose();
        }

        [Fact]
        public async Task TestCancel()
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            Assert.True(request.Status == RequestStatus.Pending);
            _ = Task.Run(async () =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                tokenSource.Cancel();
            });
            await request.WaitForResponse(tokenSource.Token).ConfigureAwait(false);
            Assert.True(request.Status == RequestStatus.Canceled);
            tokenSource.Dispose();
        }
    }
}
