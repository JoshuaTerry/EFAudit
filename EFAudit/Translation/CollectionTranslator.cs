﻿using EFAudit.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EFAudit.Translation
{
    public class CollectionTranslator :  IBinder, ISerializer
    {
        private IBindManager bindManager;
        private ISerializationManager serializationManager;
        private IHistoryContext db;

        public CollectionTranslator(IBindManager bindManager, ISerializationManager serializationManager, IHistoryContext db)
        {
            this.bindManager = bindManager;
            this.serializationManager = serializationManager;
            this.db = db;
        }
        public bool Supports(Type type)
        {
            return type.IsGenericType
                && typeof(ICollection<>).MakeGenericType(type.GetGenericArguments().First()).IsAssignableFrom(type);
        }
        public virtual object Bind(string raw, Type type, object existingValue)
        {
            var itemType = type.GetGenericArguments().First();
            object collection = existingValue ?? CreateCollection(type, itemType);
            GetType().GetMethod("FillCollection", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .MakeGenericMethod(new Type[] { itemType })
                .Invoke(this, new object[] { collection, raw });
            return collection;
        }
        public string Serialize(object obj)
        {
            if (obj == null)
                return null;

            var items = ((ICollection) obj).OfType<object>();
            var raw = (serializationManager != null)
                ? items.Select(x => serializationManager.Serialize(x))
                : items.Select(x => x.ToString());

            return String.Join(",", raw);
        }
        protected virtual object CreateCollection(Type type, Type itemType)
        {
            if (type.IsInterface)
            {
                var concreteType = typeof(List<>).MakeGenericType(itemType);
                return Activator.CreateInstance(concreteType);
            }
            else
            {
                return Activator.CreateInstance(type);
            }
        }
        protected virtual void FillCollection<ItemType>(ICollection<ItemType> collection, string raw)
        {
            foreach (var reference in raw.Split(new char[] { ',' }).Where(r => !string.IsNullOrEmpty(r)))
            {
                var item = bindManager.Bind<ItemType>(reference);
                if (collection.All(i => !EqualCollectionItems(i, item)))
                    collection.Add(item);
            }
        }        
        protected virtual bool EqualCollectionItems(object a, object b)
        {
            return (db.ObjectHasReference(a))
                ? (db.GetReferenceForObject(a) == db.GetReferenceForObject(b))
                : Equals(a, b);
        }
    }
}
