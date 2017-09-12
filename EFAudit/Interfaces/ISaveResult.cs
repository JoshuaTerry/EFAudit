using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFAudit.Interfaces
{
    public interface ISaveResult<TChangeSet>
    {
        int AffectedObjectCount { get; }
        TChangeSet ChangeSet { get; }
    }
}
