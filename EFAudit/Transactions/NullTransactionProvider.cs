using EFAudit.Interfaces;
using System;
using System.Threading.Tasks;

namespace EFAudit.Transactions
{
    /// <summary>
    /// Does not wrap the action in a transaction - just invokes it.
    /// This is for when there is an existing transaction already open,
    /// and Audit doesn't need to do anything to be covered by it.
    /// </summary>
    public class NullTransactionProvider : ITransactionProvider
    {
        public void InTransaction(Action action)
        {
            action();
        }

        public async Task InTransactionAsync(Func<Task> taskAction)
        {
            if (taskAction != null)
                await taskAction();
        }
    }
}
