using EFAudit.Interfaces;
using EFAudit.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace EFAudit.Logging
{
    public class ChangeSetFactory : IChangeSetFactory<ChangeSet, IdentityUser>
    {
        public ChangeSet ChangeSet()
        {             
            return new ChangeSet();
        }

        public IObjectChange<IdentityUser> ObjectChange()
        { 
            return new ObjectChange();
        }

        public IPropertyChange<IdentityUser> PropertyChange()
        {
            return new PropertyChange();
        }
    }
}
