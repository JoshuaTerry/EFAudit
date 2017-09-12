using EFAudit.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFAudit.Models
{
    public class ChangeSet : IChangeSet<IdentityUser>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public List<ObjectChange> ObjectChanges { get; set; } = new List<ObjectChange>();

        IEnumerable<IObjectChange<IdentityUser>> IChangeSet<IdentityUser>.ObjectChanges
        {
            get { return ObjectChanges; }
        }

        [MaxLength(64)]
        public string UserName { get; set; }

        public IdentityUser User { get; set; }
        public Guid UserId { get; set; }
        void IChangeSet<IdentityUser>.Add(IObjectChange<IdentityUser> objectChange)
        {
            ObjectChanges.Add((ObjectChange)objectChange);
        }

        public override string ToString()
        {
            return $"By {UserName} on {Timestamp} with {ObjectChanges.Count} changes.";
        }
    }
}
