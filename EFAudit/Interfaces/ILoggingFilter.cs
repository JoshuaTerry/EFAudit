using System;
using System.Data.Entity.Core.Metadata.Edm;

namespace EFAudit.Interfaces
{
    public interface ILoggingFilter
    {
        bool ShouldLog(Type type);
        bool ShouldLog(NavigationProperty property);
        bool ShouldLog(Type type, string propertyName);
    }
}
