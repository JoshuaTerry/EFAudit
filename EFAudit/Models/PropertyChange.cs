using EFAudit.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFAudit.Models
{
    public class PropertyChange : IPropertyChange<IdentityUser>
    {
        [MaxLength(10)]
        public string ChangeType { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long ObjectChangeId { get; set; }
        [ForeignKey("ObjectChangeId")]
        public ObjectChange ObjectChange { get; set; }
        [MaxLength(128)]
        public string PropertyName { get; set; }
        [NotMapped]
        public Type PropertyType { get; set; }
        [MaxLength(128)]
        public string PropertyTypeName { get; set; }
        [MaxLength(512)]
        public string OriginalDisplayName { get; set; }
        [MaxLength(512)]
        public string OriginalValue { get; set; }
        [MaxLength(512)]
        public string NewValue { get; set; }
        [MaxLength(512)]
        public string NewDisplayName { get; set; }
        [NotMapped]
        bool IPropertyChange<IdentityUser>.IsForeignKey { get; set; }
        [NotMapped]
        bool IPropertyChange<IdentityUser>.IsManyToMany { get; set; }
        IObjectChange<IdentityUser> IPropertyChange<IdentityUser>.ObjectChange
        {
            get { return ObjectChange; }
            set { ObjectChange = (ObjectChange)value; }
        }
        public override string ToString()
        {
            return string.Format("{0}:{1}", PropertyName, NewValue);
        }
    }
}
