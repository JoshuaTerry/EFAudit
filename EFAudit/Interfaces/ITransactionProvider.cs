using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFAudit.Interfaces
{
    public interface ITransactionProvider
    {
        void InTransaction(Action action);
        Task InTransactionAsync(Func<Task> taskAction);
    }
}
