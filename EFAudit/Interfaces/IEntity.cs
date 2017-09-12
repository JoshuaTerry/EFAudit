using System;

namespace EFAudit.Interfaces
{
    public interface IEntity
    {
        Guid Id { get; set; }
        string DisplayName { get; set; }
    }
}
