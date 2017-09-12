using System;

namespace EFAudit.Exceptions
{
    public class FailedToRetrieveObjectException : Exception
    {
        public readonly Type Type;
        public readonly string Reference;

        private const string message = "Failed to retrieve object identified by type '{0}' and reference '{1}'";

        public FailedToRetrieveObjectException(Type type, string reference, Exception innerException = null) : base(string.Format(message, type, reference), innerException)
        {
            Type = type;
            Reference = reference;
        }
    }
}
