using System;

namespace EFAudit.Exceptions
{
    public class ObjectTypeDoesNotExistInDataModelException : Exception
    {
        private readonly Type type;

        private const string message = "Objects of type '{0}' do not exist in the data model. ";

        public ObjectTypeDoesNotExistInDataModelException(Type type) : base(string.Format(message, type))
        {
            this.type = type;
        }

        public Type Type
        {
            get { return type; }
        }
    }
}
