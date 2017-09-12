using System;

namespace EFAudit.Interfaces
{
    internal interface IDeferredChangeManager<TChangeSet, TPrincipal> where TChangeSet : IChangeSet<TPrincipal>
    {
        bool HasChangeSet { get; }
        TChangeSet ProcessDeferredChanges(DateTime timestamp, TPrincipal author);
    }
}
