using EFAudit.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace EFAudit.Logging.ValuePairs
{
    public static class ValuePairSource
    {
        /// <summary>
        /// Returns the changes for a property as ValuePair objects.
        /// We can have more than one change for a single property if it is a complex type.
        /// </summary>
        public static IEnumerable<IValuePair> Get(ObjectStateEntry entry, string propertyName)
        {
            var state = entry.State;
            switch (state)
            {
                case EntityState.Added:
                    return Get(null, () => entry.CurrentValues[propertyName], propertyName, state);
                case EntityState.Deleted:
                    return Get(() => entry.OriginalValues[propertyName], null, propertyName, state);
                case EntityState.Modified:
                    return Get(() => entry.OriginalValues[propertyName],
                        () => entry.CurrentValues[propertyName], propertyName, state);
                default:
                    throw new NotImplementedException(string.Format("Unable to deal with ObjectStateEntry in '{0}' state",
                        state));
            }
        }
        public static IEnumerable<IValuePair> Get(Func<object> oldValue, Func<object> newValue, string propertyName, EntityState state)
        {
            var pair = new ValuePair(oldValue, newValue, propertyName, state);
            if (pair.Type.IsA(typeof(System.Data.IDataRecord)))
            {
                // Is it a complex type? If so, retrieve the value pairs for each sub-property.
                return new DataRecordValuePair(pair).SubValuePairs;
            }
            else
            {
                // Otherwise just return the change
                return new List<ValuePair>() { pair };
            }
        }
    }
}
