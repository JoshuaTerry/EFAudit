using System;

namespace EFAudit.Interfaces
{
    public interface IChangeType
    {
        bool IsA(Type type);
    }
}
