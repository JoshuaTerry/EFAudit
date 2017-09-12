using EFAudit.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace EFAudit.Logging
{
    public class DeferredObjectChange<TPrincipal>
    {
        private readonly IObjectChange<TPrincipal> objectChange;
        private readonly DeferredValue futureReference;
        private readonly DeferredValueMap futureValue;
        private readonly ISerializationManager serializer;
        private readonly DbContext dbContext;
        private readonly object entity;

        public DeferredObjectChange(IObjectChange<TPrincipal> objectChange, Func<string> deferredReference, ISerializationManager serializer, object entity, DbContext context)
        {
            this.objectChange = objectChange;
            this.futureReference = new DeferredValue(deferredReference);
            this.futureValue = new DeferredValueMap(objectChange);
            this.serializer = serializer;
            this.entity = entity;
            this.dbContext = context;
        }
         
        public void ProcessDeferredValues()
        {
            objectChange.EntityId = (string)futureReference.CalculateAndRetrieve();

            var deferredValues = futureValue.CalculateAndRetrieve();
            foreach (KeyValuePair<string, object> kv in deferredValues)
            {
                var propertyChanges = objectChange.PropertyChanges.Where(pc => pc.PropertyName == kv.Key).ToList();
                int i = -1;
                foreach (var change in propertyChanges)
                {
                    i++;
                    if (kv.Value != null)
                    {
                        var values = kv.Value.ToString().Split(',');
                        if (values.Length > i)
                            SetValue(change, values[i]);
                    }
                }
            }
        }
        private void SetValue(IPropertyChange<TPrincipal> propertyChange, object value)
        {
            if (propertyChange.IsForeignKey)
            {
                var fkNameLookup = entity.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ForeignKeyAttribute))).ToDictionary(p => p.GetCustomAttribute<ForeignKeyAttribute>().Name, p => p.Name);
                try
                {
                    var reference = dbContext.Entry(entity).Reference(fkNameLookup[propertyChange.PropertyName]);
                    if (reference != null)
                    {
                        reference.Load();
                    }
                }
                catch (ArgumentException)
                {
                    // Ignore any argument exceptions - these result from trying to load a reference for a non-mapped property that has a foreign key attribute.  (Address is an example.)
                }

                var obj = entity.GetType().GetProperty(fkNameLookup[propertyChange.PropertyName]).GetValue(entity);
                if (obj != null && obj.GetType().GetInterfaces().Contains(typeof(IEntity)))
                {
                    propertyChange.NewDisplayName = (obj as IEntity).DisplayName;
                }
            }
            else if (propertyChange.IsManyToMany)
            {
                var collection = (IEnumerable)entity.GetType().GetProperty(propertyChange.PropertyName).GetValue(entity);
                var item = collection.Cast<IEntity>().FirstOrDefault(i => i.Id == Guid.Parse(Convert.ToString(value)));
                if (item != null)
                    propertyChange.NewDisplayName = (item as IEntity).DisplayName;
            }

            string valueAsString = ValueToString(value);
            propertyChange.NewValue = valueAsString;

            if (string.IsNullOrEmpty(propertyChange.NewDisplayName) && !string.IsNullOrEmpty(propertyChange.NewValue))
                propertyChange.NewDisplayName = propertyChange.NewValue;
        }
        private string ValueToString(object value)
        {
            if (value == null)
            {
                return null;
            }
            else if (serializer != null)
            {
                return serializer.Serialize(value);
            }
            else
            {
                return value.ToString();
            }
        }

        public IObjectChange<TPrincipal> ObjectChange
        {
            get { return objectChange; }
        }
        public DeferredValue FutureReference
        {
            get { return futureReference; }
        }
        public DeferredValueMap FutureValues
        {
            get { return futureValue; }
        }
    }
}
