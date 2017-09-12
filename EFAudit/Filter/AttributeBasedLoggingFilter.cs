using EFAudit.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;

namespace EFAudit.Filter
{
    public abstract class AttributeBasedLoggingFilter : ILoggingFilter
    {
        private FilterAttributeCache cache;
        public AttributeBasedLoggingFilter(IAuditLogContext context)
        {
            cache = FilterAttributeCache.For(context);
        }
        public bool ShouldLog(Type type)
        {
            return ShouldLogFromAttributes(cache.AttributesFor(type));
        }
        public bool ShouldLog(NavigationProperty property)
        {
            return ShouldLogFromAttributes(cache.AttributesFor(property));
        }
        public bool ShouldLog(Type type, string propertyName)
        {
            return ShouldLogFromAttributes(cache.AttributesFor(type, propertyName));
        }

        protected abstract bool ShouldLogFromAttributes(IEnumerable<IFilterAttribute> filters);
    }
}
