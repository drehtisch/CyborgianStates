using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using NationStatesSharp;
using NationStatesSharp.Enums;
using NationStatesSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CyborgianStates.Tests.CommandHandling
{
    public class RequestMockWrapper
    {
        public RequestStatus Status { get; init; }
        public object Response { get; init; }
        public Exception Exception { get; init; }
    }

    public class TestRequestDispatcher : IRequestDispatcher
    {
        private static List<RequestMockWrapper> requests = new List<RequestMockWrapper>();

        public static void PrepareNextRequest(RequestStatus status = RequestStatus.Success, object response = null, Exception exception = null)
        {
            requests.Add(new RequestMockWrapper()
            {
                Status = status,
                Response = response,
                Exception = exception,
            });
        }

        public void Dispatch(Request request, int priority)
        {
            if (!requests.Any())
            {
                var reason = "TestDispatcher does not accept unprepared requests.";
                request.Fail(new InvalidOperationException(reason));
            }
            else
            {
                if (requests.Any())
                {
                    var mockRequest = requests.Take(1).First();
                    requests.Remove(mockRequest);
                    if (mockRequest.Status == RequestStatus.Success)
                    {
                        request.Complete(mockRequest.Response);
                    }
                    else if (mockRequest.Status == RequestStatus.Failed)
                    {
                        request.Fail(mockRequest.Exception);
                    }
                }
                else
                {
                    var reason = $"No request has been prepared.";
                    request.Fail(new InvalidOperationException(reason));
                }
            }
        }

        public void Dispatch(IEnumerable<Request> requests, int priority) => throw new NotImplementedException();

        public void Shutdown() => throw new NotImplementedException();

        public void Start() => throw new NotImplementedException();
    }
}