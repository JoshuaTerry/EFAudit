using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace EFAudit.Helpers
{
    public class DisposableSavingChangesListener : IDisposable
    {
        private readonly ObjectContext context;
        private readonly EventHandler handler;

        public DisposableSavingChangesListener(DbContext context, EventHandler eventHandler) : this(((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext, eventHandler)
        {
        }

        public DisposableSavingChangesListener(ObjectContext context, EventHandler eventHandler)
        {
            // Wrapped event handler to keep from triggering even in the wrong context 
            this.context = context;
            this.handler = (sender, args) =>
            {
                // If our context didn't trigger this event, ignore it. 
                if (sender != context)
                    return;

                // Call the child event handler
                if (eventHandler != null)
                    eventHandler.Invoke(sender, args);
            };

            // Register the event handler
            if (context != null)
                context.SavingChanges += handler;
        }

        public void Dispose()
        {
            // Degregister the eventr handler
            if (context != null)
                context.SavingChanges -= handler;
        }
    }
}
