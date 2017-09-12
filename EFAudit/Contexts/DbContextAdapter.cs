using EFAudit.Helpers;
using EFAudit.Interfaces;
using System;
using System.Data.Entity;

namespace EFAudit.Contexts
{
    public abstract class DbContextAdapter<TChangeSet, TPrincipal> : ObjectContextAdapter<TChangeSet, TPrincipal> where TChangeSet : IChangeSet<TPrincipal>
    {
        private readonly DbContext context;

        public DbContextAdapter(DbContext context) : base(((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext)
        {
            this.context = context;
        }

        public override int SaveAndAcceptChanges(EventHandler onSavingChanges = null)
        {
            // Save is wrapped in disposable listener for SaveChanges.  
            // Handler is invoked AFTER saving but BEFORE accepting changes.
            using (new DisposableSavingChangesListener(context, onSavingChanges))
            {
                return context.SaveChanges();
            }
        }

        
    }
}
