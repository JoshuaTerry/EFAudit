using EFAudit.Helpers;
using EFAudit.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;


namespace EFAudit.Filter
{
    public class FilterAttributeCache
    {
        private readonly MetadataWorkspace workspace;
        private ConcurrentDictionary<Signature, IEnumerable<IFilterAttribute>> cache;
        private MetadataSpaceMapper mapper;

        private FilterAttributeCache(MetadataWorkspace workspace)
        {
            this.workspace = workspace;
            this.cache = new ConcurrentDictionary<Signature, IEnumerable<IFilterAttribute>>();
            this.mapper = new MetadataSpaceMapper(workspace);
        }

        public IEnumerable<IFilterAttribute> AttributesFor(Type type)
        {
            return WithCache(new Signature(type), () =>
            {
                return type.GetAttributes<IFilterAttribute>();
            });
        }
        public IEnumerable<IFilterAttribute> AttributesFor(NavigationProperty property)
        {
            return WithCache(new Signature(property), () =>
            {
                var info = mapper.Map(property);
                return info.GetAttributes<IFilterAttribute>();
            });
        }
        public IEnumerable<IFilterAttribute> AttributesFor(Type type, string propertyName)
        {
            return WithCache(new Signature(type, propertyName), () =>
            {
                var info = mapper.Map(type, propertyName);
                return info.GetAttributes<IFilterAttribute>();
            });
        }

        /// <summary>
        /// For a given task identified by a Signature either returns 
        /// the cached result of previously running it, or runs it and caches it for later.
        /// </summary>
        private IEnumerable<IFilterAttribute> WithCache(Signature signature, Func<IEnumerable<IFilterAttribute>> attributeRetriever)
        {
            IEnumerable<IFilterAttribute> result;

            if (!cache.TryGetValue(signature, out result))
            {
                result = attributeRetriever();
                cache[signature] = result;
            }
            return result;
        }

        /// <summary>
        /// This provides a unique key for identifying tasks and their results
        /// in a cache.  
        /// </summary>
        private class Signature
        {
            private object[] data;

            public Signature(params object[] data)
            {
                this.data = data;
            }

            public override bool Equals(object obj)
            {
                var other = obj as Signature;
                return other != null && equals(this, other);
            }
            private static bool equals(Signature a, Signature b)
            {
                int i = 0;
                while (i < a.data.Length)
                {
                    if (i >= b.data.Length)
                        return false;

                    if (!object.Equals(a.data[i], b.data[i]))
                        return false;

                    i++;
                }
                if (i < b.data.Length)
                    return false;

                return true;
            }

            public override int GetHashCode()
            {
                return string.Join(",", data.Select(d => d.GetHashCode())).GetHashCode();
            }
            public override string ToString()
            {
                return string.Join(", ", data);
            }
        }

        private static Dictionary<Type, FilterAttributeCache> caches = new Dictionary<Type, FilterAttributeCache>();

        internal static FilterAttributeCache For(IAuditLogContext context)
        {
            var id = context.UnderlyingContextType;
            lock (caches)
            {
                FilterAttributeCache filter;

                if (!caches.TryGetValue(id, out filter))
                {
                    filter = caches[id] = new FilterAttributeCache(context.Workspace);
                }

                return filter;
            }
        }
    }
}
