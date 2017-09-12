using EFAudit.Exceptions;
using EFAudit.Filter;
using EFAudit.Interfaces;
using EFAudit.Logging;
using EFAudit.Transactions;
using EFAudit.Translation;
using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Configuration;

namespace EFAudit
{
    public static class AuditModule
    {
        private const string AuditEnabledTag = "AuditEnabled";
        private static object syncRoot = new object();
        private static bool? isAuditEnabled = null;
        public static bool IsAuditEnabled
        {
            get
            {
                if (!isAuditEnabled.HasValue)
                {
                    lock (syncRoot)
                    {
                        try
                        {
                            var auditTag =  WebConfigurationManager.AppSettings["AuditEnabled"];
                            isAuditEnabled = string.Compare(auditTag, "true", true) == 0;
                        }
                        catch (Exception)
                        {
                            isAuditEnabled = false;
                        }
                    }
                }

                return isAuditEnabled.Value;
            }
        }

    }
    public class AuditModule<TChangeSet, TPrincipal> where TChangeSet : IChangeSet<TPrincipal>
    {
        public bool Enabled { get; set; }
        private IChangeSetFactory<TChangeSet, TPrincipal> factory;
        private IAuditLogContext<TChangeSet, TPrincipal> context;
        private ILoggingFilter filter;
        private ISerializationManager serializer;
        private DbContext dbcontext;

        public AuditModule(IChangeSetFactory<TChangeSet, TPrincipal> factory,
            IAuditLogContext<TChangeSet, TPrincipal> context,
            DbContext dbcontext,
            ILoggingFilterProvider filter = null,
            ISerializationManager serializer = null)
        {
            this.factory = factory;
            this.context = context;
            this.dbcontext = dbcontext;
            this.filter = (filter ?? Filters.Default).Get(context);
            this.serializer = (serializer ?? new ValueTranslationManager(context));
            Enabled = true;
        }

        /// <summary>
        /// Save the changes and log them as controlled by the logging filter. 
        /// A TransactionScope is used to wrap save, which will use an ambient transaction if available, or create a new one.
        ///  
        /// If you are using an explicit transaction, and not using the TransactionScope Use SaveChangesWithinExplicitTransaction.
        /// </summary>
        public ISaveResult<TChangeSet> SaveChanges(TPrincipal principal)
        {
            if (dbcontext.Database.CurrentTransaction != null)
            {
                return SaveChangesWithinExplicitTransaction(principal);
            }
            else
            {
                return SaveChanges(principal, new TransactionOptions());
            }
        }

        public ISaveResult<TChangeSet> SaveChanges(TPrincipal principal, TransactionOptions transactionOptions)
        {
            return SaveChanges(principal, new TransactionScopeProvider(transactionOptions));
        }

        public ISaveResult<TChangeSet> SaveChangesWithinExplicitTransaction(TPrincipal principal)
        { 
            return SaveChanges(principal, new NullTransactionProvider());
        }

        protected ISaveResult<TChangeSet> SaveChanges(TPrincipal principal, ITransactionProvider transactionProvider)
        {
            if (!Enabled)
                return new SaveResult<TChangeSet, TPrincipal>(context.SaveAndAcceptChanges());

            var result = new SaveResult<TChangeSet, TPrincipal>();

            transactionProvider.InTransaction(() =>
            {
                var logger = new ChangeLogger<TChangeSet, TPrincipal>(context, dbcontext, factory, filter, serializer);
                var oven = (IDeferredChangeManager<TChangeSet, TPrincipal>)null;

                context.DetectChanges();

                result.AffectedObjectCount = context.SaveAndAcceptChanges((sender, args) =>
                {
                    oven = logger.Log(context.ObjectStateManager);
                });

                if (oven == null)
                    throw new ChangesNotDetectedException();

                if (oven.HasChangeSet)
                {
                    result.ChangeSet = oven.ProcessDeferredChanges(DateTime.UtcNow, principal);
                    context.AddChangeSet(result.ChangeSet);
                    context.DetectChanges();

                    context.SaveChanges(SaveOptions.AcceptAllChangesAfterSave);
                }
            });

            return result;
        }

        /// <summary>        
        /// If you are using an explicit transaction, and not using the TransactionScope Use SaveChangesWithinExplicitTransaction.
        /// </summary>
        public async Task<ISaveResult<TChangeSet>> SaveChangesAsync(TPrincipal principal, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await SaveChangesAsync(principal, new TransactionOptions(), cancellationToken);
        }

        /// <summary>
        /// If you are using an explicit transaction, and not using the TransactionScope Use SaveChangesWithinExplicitTransaction.
        /// </summary>
        public async Task<ISaveResult<TChangeSet>> SaveChangesAsync(TPrincipal principal, TransactionOptions transactionOptions, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await saveChangesAsync(principal, new TransactionScopeProvider(transactionOptions), cancellationToken);
        }

        /// <summary>
        /// Save the changes and log them as controlled by the logging filter. 
        /// Only use this overload if you are wrapping the call to AuditModule in your own transaction. 
        /// This keeps me from automatically creating a transaction.        
        /// </summary>    
        public async Task<ISaveResult<TChangeSet>> SaveChangesWithinExplicitTransactionAsync(TPrincipal principal, CancellationToken cancellationToken = default(CancellationToken))
        {
            // If there is already an explicit transaction in use, use the NullTransactionProvider
            return await saveChangesAsync(principal, new NullTransactionProvider(), cancellationToken);
        }

        protected async Task<ISaveResult<TChangeSet>> saveChangesAsync(TPrincipal principal, ITransactionProvider transactionProvider, CancellationToken cancellationToken)
        {
            if (!Enabled)
                return new SaveResult<TChangeSet, TPrincipal>(await context.SaveAndAcceptChangesAsync(cancellationToken: cancellationToken));

            var result = new SaveResult<TChangeSet, TPrincipal>();

            cancellationToken.ThrowIfCancellationRequested();

            await transactionProvider.InTransactionAsync(async () =>
            {
                var logger = new ChangeLogger<TChangeSet, TPrincipal>(context, dbcontext, factory, filter, serializer);
                var oven = (IDeferredChangeManager<TChangeSet, TPrincipal>)null;

                cancellationToken.ThrowIfCancellationRequested();
                context.DetectChanges();
                cancellationToken.ThrowIfCancellationRequested();

                result.AffectedObjectCount = await context.SaveAndAcceptChangesAsync(cancellationToken: cancellationToken, onSavingChanges:
                    (sender, args) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        oven = logger.Log(context.ObjectStateManager);

                        // NOTE: This is the last chance to cancel the save.
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                );

                if (oven == null)
                    throw new ChangesNotDetectedException();

                if (oven.HasChangeSet)
                {
                    result.ChangeSet = oven.ProcessDeferredChanges(DateTime.UtcNow, principal);
                    context.AddChangeSet(result.ChangeSet);
                    context.DetectChanges();

                    await context.SaveChangesAsync(SaveOptions.AcceptAllChangesAfterSave);
                }
            });

            return result;
        }
    }
}
