using System;

namespace EFAudit.Exceptions
{
    public class ErrorInDeferredCalculation : Exception
    {
        private const string message = "An error of type '{0}' occured during deferred calculation of a value. See inner exception for more details.";
        private const string messageWithKey = "An error of type '{2}' occured during deferred calculation of property '{0}' on container '{1}'. See inner exception for more details.";

        public ErrorInDeferredCalculation(Exception innerException) : base(string.Format(message, innerException.GetType()), innerException) { }

        public ErrorInDeferredCalculation(object container, string key, Exception innerException) : base(string.Format(messageWithKey, key, container, innerException.GetType()), innerException) { }
    }
}
