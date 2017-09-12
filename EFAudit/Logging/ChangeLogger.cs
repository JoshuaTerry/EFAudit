using EFAudit.Interfaces;
using EFAudit.Logging.ValuePairs;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;

namespace EFAudit.Logging
{
    internal class ChangeLogger<TChangeSet, TPrincipal>
        where TChangeSet : IChangeSet<TPrincipal>
    {
        private IAuditLogContext<TChangeSet, TPrincipal> context;
        private IChangeSetFactory<TChangeSet, TPrincipal> factory;
        private Recorder<TChangeSet, TPrincipal> recorder;
        private ILoggingFilter filter;
        private DbContext dbContext;

        public ChangeLogger(IAuditLogContext<TChangeSet, TPrincipal> context, DbContext dbContext, IChangeSetFactory<TChangeSet, TPrincipal> factory, ILoggingFilter filter, ISerializationManager serializer)
        {
            this.context = context;
            this.dbContext = dbContext;
            this.factory = factory;
            this.recorder = new Recorder<TChangeSet, TPrincipal>(factory, serializer, dbContext);
            this.filter = filter;
        }
         
        public IDeferredChangeManager<TChangeSet, TPrincipal> Log(ObjectStateManager objectStateManager)
        {
            var entries = objectStateManager.GetObjectStateEntries(EntityState.Added | EntityState.Modified | EntityState.Deleted);

            foreach (var entry in entries)
            {
                Process(entry);
            }

            return recorder;
        }

        private void Process(ObjectStateEntry entry)
        {
            if (entry.IsRelationship)
            {
                LogRelationshipChange(entry);
            }
            else
            {
                LogScalarChange(entry);
            }
        }

        private void LogScalarChange(ObjectStateEntry entry)
        { 
            object entity = entry.Entity;           
            Type type = entity.GetType();         

            if (!filter.ShouldLog(type))
            {
                return;
            }

            foreach (string propertyName in GetChangedProperties(entry))
            {
                if (filter.ShouldLog(type, propertyName))
                {
                    // We can have multiple changes for the same property if its a complex type
                    var valuePairs = ValuePairSource.Get(entry, propertyName).Where(p => p.HasChanged);

                    foreach (var valuePair in valuePairs)
                    {
                        var pair = valuePair;                        
                        recorder.Record(entry, entity, () => context.GetReferenceForObject(entity), valuePair.PropertyName, pair.NewValue);
                    }
                }
            }
        }

        /// <summary>
        /// This is where Navigation Properties will get updated
        /// </summary> 
        private void LogRelationshipChange(ObjectStateEntry entry)
        {                      
            if (entry.State == EntityState.Added || entry.State == EntityState.Deleted)
            {
                // Each relationship has two ends. Log both directions.
                var ends = GetAssociationEnds(entry);

                foreach (var localEnd in ends)
                {
                    var foreignEnd = GetOtherAssociationEnd(entry, localEnd);
                    LogForeignKeyChange(entry, localEnd, foreignEnd);
                }
            }
        }

        private void LogForeignKeyChange(ObjectStateEntry entry, AssociationEndMember localEnd, AssociationEndMember foreignEnd)
        {
            // A "key" is an in-memory unique reference to objects at each end of an association.
            // This will give you the Entity Type and the Key you need
            var key = GetEndEntityKey(entry, localEnd);

            // Get the object identified by the local key
            object entity = context.GetObjectByKey(key);
            if (!filter.ShouldLog(entity.GetType()))
                return;

            // The property on the "local" object that navigates to the "foreign" object
            var property = GetProperty(entry, localEnd, foreignEnd, key);

            if (property == null || !filter.ShouldLog(property))
                return;

            // Generate the change
            var value = GetForeignValue(entry, entity, foreignEnd, property.Name);

            recorder.Record(entry, entity, () => context.GetReferenceForObject(entity), property.Name, value);
        }

        private Func<object> GetForeignValue(ObjectStateEntry entry, object entity, AssociationEndMember foreignEnd, string propertyName)
        {
            if (foreignEnd.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                return ManyToManyValue(entity, propertyName);
            }
            else
            {
                if (entry.State == EntityState.Added)
                    return () =>
                    {
                        // Get the key that identifies the the object we are making or breaking a relationship with
                        var foreignKey = GetEndEntityKey(entry, foreignEnd);
                        return GetKeyReference(foreignKey);
                    };
                else
                    return null;
            }
        }

        private Func<object> ManyToManyValue(object entity, string propertyName)
        {
            return () =>
            {
                // In this case the key id represents an object being added to or removed from a set.
                // We use reflection to get the current contents of the set (so after the change we are logging).
                if (entity == null)
                {
                    throw new InvalidOperationException("Attempted to log change to null object of type " + entity.GetType().Name);
                }

                var property = entity.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property == null)
                {
                    throw new InvalidOperationException(string.Format("Unable to find a property with name '{0}' on type '{1}'", propertyName, entity.GetType()));
                }

                var value = property.GetValue(entity, null);
                if (value == null)
                {
                    throw new InvalidOperationException(string.Format("Many-to-many set '{0}.{1}' was null", entity.GetType().Name, property.Name));
                }

                IEnumerable<string> current = ((IEnumerable<object>)value)
                    .Select(e => context.GetReferenceForObject(e))
                    .Distinct()
                    .OrderBy(reference => reference);

                return ToIdList(current);
            };
        }
        private string ToIdList(IEnumerable<string> references)
        {
            return string.Format("{0}", string.Join(",", references));
        }

        private string GetKeyReference(EntityKey key)
        {
            var entity = context.GetObjectByKey(key);
            return context.GetReferenceForObject(entity);
        }

        private IEnumerable<string> GetChangedProperties(ObjectStateEntry entry)
        {
            var values = UseableValues(entry);
            for (int i = 0; i < values.FieldCount; i++)
                yield return values.GetName(i);
        }

        /// <summary>
        /// Gets either CurrentValues or OriginalValues depending on the entry type.
        /// Added will have no OriginalValues, and Deleted only have OriginalValues).
        /// </summary>
        private IExtendedDataRecord UseableValues(ObjectStateEntry entry)
        {
            return entry.State == EntityState.Deleted ? (IExtendedDataRecord)entry.OriginalValues : entry.CurrentValues;
        }

        private AssociationEndMember[] GetAssociationEnds(ObjectStateEntry entry)
        {
            var fieldMetadata = UseableValues(entry).DataRecordInfo.FieldMetadata;
            return fieldMetadata.Select(m => m.FieldType as AssociationEndMember).ToArray();
        }
                
        private AssociationEndMember GetOtherAssociationEnd(ObjectStateEntry entry, AssociationEndMember end)
        {
            AssociationEndMember[] ends = GetAssociationEnds(entry);
            if (ends[0] == end)
                return ends[1];
            else
                return ends[0];
        }

        /// <summary>
        /// Gets the EntityKey associated with this end of the association
        /// </summary>
        private EntityKey GetEndEntityKey(ObjectStateEntry entry, AssociationEndMember end)
        {
            AssociationEndMember[] ends = GetAssociationEnds(entry);
            if (ends[0] == end)
                return UseableValues(entry)[0] as EntityKey;
            else
                return UseableValues(entry)[1] as EntityKey;
        }

        /// <summary>  
        /// Gets the NavigationProperty that that will let you navigate to the entity 
        /// at the local and foreign ends.
        /// </summary>
        private NavigationProperty GetProperty(ObjectStateEntry entry, AssociationEndMember localEnd, AssociationEndMember foreignEnd, EntityKey key)
        {
            var relationshipType = entry.EntitySet.ElementType;
            var entitySet = key.GetEntitySet(entry.ObjectStateManager.MetadataWorkspace);

            return entitySet.ElementType.NavigationProperties
                            .Where(p => p.RelationshipType == relationshipType
                                     && p.FromEndMember == localEnd
                                     && p.ToEndMember == foreignEnd).SingleOrDefault();
        }
    }
}