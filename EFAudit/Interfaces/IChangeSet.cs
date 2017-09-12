using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFAudit.Interfaces
{
    public interface IChangeSet<TPrincipal>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        long Id { get; set; }
        IEnumerable<IObjectChange<TPrincipal>> ObjectChanges { get; }
        void Add(IObjectChange<TPrincipal> objectChange);
        DateTime Timestamp { get; set; }
        string UserName { get; set; }
        Guid UserId { get; set; }
        TPrincipal User { get; set; }
    }
}
