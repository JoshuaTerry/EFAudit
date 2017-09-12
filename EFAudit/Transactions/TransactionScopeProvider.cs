using EFAudit.Exceptions;
using EFAudit.Interfaces;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;

namespace EFAudit.Transactions
{
    /// <summary>
    /// Wraps the given operations in a TransactionScope-based transaction
    /// </summary>
    public partial class TransactionScopeProvider : ITransactionProvider
    {
        private readonly TransactionOptions options;

        public TransactionScopeProvider(TransactionOptions options)
        {
            this.options = options;
        }

        public void InTransaction(Action action)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required, options))
                {
                    action();
                    scope.Complete();
                }
            }
            catch (EntityException e)
            {
                if (ConflictingTransactionException.Matches(e))
                    throw new ConflictingTransactionException(e);
                else
                    throw;
            }
        }

        public async Task InTransactionAsync(Func<Task> taskAction)
        {
            if (taskAction == null)
                return;

            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled))
                {
                    await taskAction();
                    scope.Complete();
                }
            }
            catch (EntityException e)
            {
                if (ConflictingTransactionException.Matches(e))
                    throw new ConflictingTransactionException(e);

                throw;
            }
        }
    }
} 
