using EFAudit.Interfaces;
using System;
using System.Linq;

namespace EFAudit.Translation
{     
    public class NullableBinder : IBinder
    {
        private IBindManager bindManager;

        public NullableBinder(IBindManager bindManager)
        {
            this.bindManager = bindManager;
        }

        public bool Supports(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public object Bind(string raw, Type type, object existingValue)
        {
            if (raw == null)
                return null;

            return bindManager.Bind(raw, underlyingType(type), existingValue);
        }

        private Type underlyingType(Type type)
        {
            return type.GetGenericArguments().Single();
        }
    }
}
