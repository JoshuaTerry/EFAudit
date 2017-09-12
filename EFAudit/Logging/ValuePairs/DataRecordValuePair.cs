using EFAudit.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EFAudit.Logging.ValuePairs
{
    internal class DataRecordValuePair : ValuePair
    {
        internal DataRecordValuePair(ValuePair pair)
            : base(pair.OriginalValue, pair.NewValue, pair.PropertyName, pair.State) { }

        private Func<IDataRecord> OriginalRecord
        {
            get { return () => ((IDataRecord)OriginalValue()); }
        }
        private Func<IDataRecord> NewRecord
        {
            get { return () => ((IDataRecord)NewValue()); }
        }

        internal IEnumerable<IValuePair> SubValuePairs
        {
            get
            {
                foreach (var fieldName in FieldNames)
                {
                    var o = GetOrNull(OriginalRecord, fieldName);
                    var n = GetOrNull(NewRecord, fieldName);
                    string name = string.Format("{0}.{1}", propertyName, fieldName);

                    foreach (var child in ValuePairSource.Get(o, n, name, state))
                    {
                        yield return child;
                    }
                }
            }
        }

        private Func<object> GetOrNull(Func<IDataRecord> record, string fieldName)
        {
            return () =>
            {
                var obj = (record != null ? record() : null);
                if (obj != null)
                    return obj[fieldName];
                else
                    return null;
            };
        }

        private IEnumerable<string> FieldNames
        {
            get
            {
                // All the field names that exist in at least one of the data records
                return FieldNamesFor(OriginalRecord).Union(FieldNamesFor(NewRecord));
            }
        }

        private IEnumerable<string> FieldNamesFor(Func<IDataRecord> record)
        {
            var obj = record();

            if (obj != null)
            {
                foreach (int index in Enumerable.Range(0, obj.FieldCount))
                {
                    yield return obj.GetName(index);
                }
            }
        }
    }
}
