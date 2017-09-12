using System;

namespace EFAudit.Exceptions
{
    public class ChangesNotDetectedException : Exception
    {
        private const string message = "Unable to prepare log objects whilst intercepting DbContext.SaveChanges().";

        public ChangesNotDetectedException(Exception innerException = null) : base(message, innerException)
        {
        }
    }
}
