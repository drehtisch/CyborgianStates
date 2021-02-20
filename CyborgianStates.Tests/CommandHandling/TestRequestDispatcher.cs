using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CyborgianStates.Tests.CommandHandling
{
    public class RequestMockWrapper
    {
        public RequestType Type { get; init; }
        public RequestStatus Status { get; init; }
        public object Response { get; init; }
        public Exception Exception { get; init; }
        public string FailReason { get; init; }
    }
    public class TestRequestDispatcher : IRequestDispatcher
    {
        private static List<RequestMockWrapper> requests = new List<RequestMockWrapper>();
        public static void PrepareNextRequest(RequestType type, RequestStatus status = RequestStatus.Success, object response = null, Exception exception = null, string failReason = "")
        {
            requests.Add(new RequestMockWrapper()
            {
                Type = type,
                Status = status,
                Response = response,
                Exception = exception,
                FailReason = failReason
            });
        }

        public void Dispatch(Request request, int priority)
        {
            if (!requests.Any())
            {
                var reason = "TestDispatcher does not accept unprepared requests.";
                request.Fail(reason, new InvalidOperationException(reason));
            }
            else
            {
                if (requests.Any(r => r.Type == request.Type))
                {
                    var mockRequest = requests.Where(r => r.Type == request.Type).Take(1).First();
                    requests.Remove(mockRequest);
                    if (mockRequest.Status == RequestStatus.Success)
                    {
                        request.Complete(mockRequest.Response);
                    }
                    else if (mockRequest.Status == RequestStatus.Failed)
                    {
                        request.Fail(mockRequest.FailReason, mockRequest.Exception);
                    }
                }
                else
                {
                    var reason = $"No request with type '{request.Type}' has been prepared.";
                    request.Fail(reason, new InvalidOperationException(reason));
                }
            }
        }
        public void Register(DataSourceType dataSource, IRequestWorker requestQueue) => throw new NotImplementedException();
        public void Shutdown() => throw new NotImplementedException();
        public void Start() => throw new NotImplementedException();
    }
}