using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace FilleUploadCore.Exceptions
{

    [Serializable]
    public class AppException : Exception
    {
        public AppException(string message, string friendlyMessage, Exception innerException)
                : base(message, innerException)
        {
            FriendlyMessage = friendlyMessage;
        }

        public AppException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public AppException(string message, int? errorCode)
           : base(message)
        {
            StatusCode = errorCode;
        }

        public AppException(string message, int? errorCode, object value)
          : base(message)
        {
            StatusCode = errorCode;
            Value = value;
        }

        public AppException(string message, string friendlyMessage)
            : base(message)
        {
            FriendlyMessage = friendlyMessage;
        }

        public AppException(string message)
            : base(message)
        {
        }

        public string FriendlyMessage { get; set; }

        public int? StatusCode { get; set; } = (int)HttpStatusCode.InternalServerError;

        public object Value { get; set; }
    }

}
