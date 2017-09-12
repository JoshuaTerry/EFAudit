namespace EFAudit.Interfaces
{
    public interface ILoggingFilterProvider
    {
        ILoggingFilter Get(IAuditLogContext context);
    }
}
