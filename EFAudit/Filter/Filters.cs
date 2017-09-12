using EFAudit.Interfaces;

namespace EFAudit.Filter
{
    public static class Filters
    {
        // By default logs everything. A property is not logged only if it has at least one IFilterAttribute that returns "false".         
        public static readonly ILoggingFilterProvider Greedy = new BlacklistLoggingFilter.Provider();

        // By default, logs nothing. A property is logged only if it has at least one IFilterAttribute, and all IFilterAttributes return "true". 
        // To log a given property you first need to mark the class with DoLog, and then the property.
        public static readonly ILoggingFilterProvider Sparse = new WhitelistLoggingFilter.Provider();

        // By default, logs everything. A property or type is not logged only if it has at least one IFilterAttribute that returns "false".
        public static ILoggingFilterProvider Default { get { return Greedy; } }
    }
}
