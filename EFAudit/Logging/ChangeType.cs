using EFAudit.Interfaces;
using System;

namespace EFAudit.Logging
{            
    public class ConcreteChangeType : IChangeType
    {
        private Type wrappedType;

        public ConcreteChangeType(Type type)
        {
            this.wrappedType = type;
        }

        public bool IsA(Type type)
        {
            return type.IsAssignableFrom(wrappedType);
        }
    }
    public class UnknownChangeType : IChangeType
    {
        public bool IsA(Type type)
        {
            return false;
        }
    }
}
