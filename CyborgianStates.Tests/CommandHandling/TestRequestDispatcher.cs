using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using System;

namespace CyborgianStates.Tests.CommandHandling
{
    public class TestRequestDispatcher : IRequestDispatcher
    {
        private static RequestStatus _status;
        private static object _response;
        private static Exception _exception;
        private static bool _isRequestPrepared;
        private static string _failReason;
        public static void PrepareNextRequest(RequestStatus status = RequestStatus.Success, object response = null, Exception exception = null, string failReason = "")
        {
            _status = status;
            _response = response;
            _exception = exception;
            _isRequestPrepared = true;
            _failReason = failReason;
        }

        public void Dispatch(Request request, int priority)
        {
            if (!_isRequestPrepared)
            {
                throw new InvalidOperationException("TestDispatcher does not accept unprepared requests.");
            }
            else
            {
                if (_status == RequestStatus.Success)
                {
                    request.Complete(_response);
                }
                else if (_status == RequestStatus.Failed)
                {
                    request.Fail(_failReason, _exception);
                }
            }
            _isRequestPrepared = false;
        }
        public void Register(DataSourceType dataSource, IRequestWorker requestQueue) => throw new NotImplementedException();
        public void Shutdown() => throw new NotImplementedException();
        public void Start() => throw new NotImplementedException();
    }
}