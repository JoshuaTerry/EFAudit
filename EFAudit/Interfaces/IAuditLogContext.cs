using System;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace EFAudit.Interfaces
{
    public interface IAuditLogContext
    {
        Type UnderlyingContextType { get; }
        MetadataWorkspace Workspace { get; }
    }

    public interface IAuditLogContext<TChangeSet, TPrincipal> : IHistoryContext<TChangeSet, TPrincipal>, IAuditLogContext
        where TChangeSet : IChangeSet<TPrincipal>
    {
        ObjectStateManager ObjectStateManager { get; }
        void DetectChanges();
        Task<int> SaveChangesAsync(SaveOptions saveOptions);
        Task<int> SaveAndAcceptChangesAsync(EventHandler onSavingChanges = null, CancellationToken cancellationToken = default(CancellationToken));


        // Save changes inline with the specified save options.        
        // No custom logic is invoked here to keep consumer from interupting logging        
        int SaveChanges(SaveOptions saveOptions);

        /// <summary>
        /// Save and accept only, no change detection is performed here.  Custom Logic can be executed here. 
        /// 
        /// A callback that should be invoked AFTER saving the changes but BEFORE accepting them.
        /// The event sequeuce should be as follows.
        /// 
        ///     1. Save the changes
        ///     2. Invoke 'onSavingChanges'
        ///     3. Accept the changes
        /// 
        /// </summary>
        int SaveAndAcceptChanges(EventHandler onSavingChanges = null);
        object GetObjectByKey(EntityKey key);
        void AddChangeSet(TChangeSet changeSet);
    }
}
