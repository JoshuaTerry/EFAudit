using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Reflection;

namespace EFAudit.Filter
{
    internal class TypeLookup
    {
        private ConcurrentDictionary<string, Type> lookup;
        private List<ReflectionTypeLoadException> typeLoadErrors;

        internal TypeLookup()
        {
            this.typeLoadErrors = new List<ReflectionTypeLoadException>();
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetTypes).Distinct();
            lookup = new ConcurrentDictionary<string, Type>();

            foreach (var type in types)
            {
                // If there are two types with the same namespace-qualified name, only one of them will end up in the lookup.
                lookup[type.FullName] = type;
            }
        }

        internal Type Map(StructuralType objectSpaceType)
        {
            Type type;

            if (!lookup.TryGetValue(objectSpaceType.FullName, out type))
            {
                throw new UnknownTypeException(objectSpaceType.FullName, typeLoadErrors);
            }

            return type;
        }

        /// <summary>
        /// Gets the types and stores the errors for later. 
        /// </summary>
        private IEnumerable<Type> GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                typeLoadErrors.Add(e);
                return new List<Type>();
            }
        }
    }
}
