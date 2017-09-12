using EFAudit.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFAudit.Filter
{
    public class BlacklistLoggingFilter : AttributeBasedLoggingFilter, ILoggingFilter
    {
        public BlacklistLoggingFilter(IAuditLogContext context) : base(context) { }

        protected override bool ShouldLogFromAttributes(IEnumerable<IFilterAttribute> filters)
        {
            return filters.All(f => f.ShouldLog());
        }

        public class Provider : ILoggingFilterProvider
        {
            public ILoggingFilter Get(IAuditLogContext context)
            {
                return new BlacklistLoggingFilter(context);
            }
        }
    }
}
