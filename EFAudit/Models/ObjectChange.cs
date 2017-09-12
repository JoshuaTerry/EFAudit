using EFAudit.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFAudit.Models
{
    public class ObjectChange : IObjectChange<IdentityUser>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [MaxLength(128)]
        public string TypeName { get; set; }
        [MaxLength(128)]
        public string DisplayName { get; set; }
        [MaxLength(64)]
        public string ChangeType { get; set; }
        [MaxLength(128)]
        public string EntityId { get; set; }
        public long ChangeSetId { get; set; }
        [ForeignKey("ChangeSetId")]
        public ChangeSet ChangeSet { get; set; }
        public List<PropertyChange> PropertyChanges { get; set; } = new List<PropertyChange>();

        IEnumerable<IPropertyChange<IdentityUser>> IObjectChange<IdentityUser>.PropertyChanges
        {
            get { return PropertyChanges; }
        }
        public void Add(IPropertyChange<IdentityUser> propertyChange)
        {
            PropertyChanges.Add((PropertyChange)propertyChange);
        }
        IChangeSet<IdentityUser> IObjectChange<IdentityUser>.ChangeSet
        {
            get { return ChangeSet; }
            set { ChangeSet = (ChangeSet)value; }
        }

        public override string ToString()
        {
            return $"{TypeName} {EntityId}";
        }
    }
}
