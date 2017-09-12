using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace EFAudit.Helpers
{
    public static class Extensions
    {
        public static TAttribute GetAttribute<TAttribute>(this MemberInfo self, bool inherit = false)
            where TAttribute : Attribute
        {
            if (self.IsAttributeDefined<TAttribute>(inherit))
            {
                return self.GetCustomAttribute<TAttribute>();
            }

            return null;
        }
        
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this MemberInfo self, bool inherit = false)
            where TAttribute : Attribute
        {
            var attributes = new List<TAttribute>();

            if (self.IsAttributeDefined<TAttribute>(inherit))
            {
                attributes.AddRange(self.GetCustomAttributes<TAttribute>(inherit));
            }

            return attributes;
        }
        
        public static bool IsAttributeDefined<TAttribute>(this MemberInfo self, bool inherit = false)
        {
            return self.IsDefined(typeof(TAttribute), inherit);
        }
        
        public static bool IsAttributeDefined(this MemberInfo self, Type attributeType, bool inherit = false)
        {
            return self.IsDefined(attributeType, inherit);
        }
        public static T GetAttribute<T>(this MemberInfo member)
        {
            return member.GetCustomAttributes(typeof(T), true).Cast<T>().SingleOrDefault();
        }
        public static IEnumerable<T> GetAttributes<T>(this PropertyInfo property)
        {
            return property.GetAttributes<T>(GetMetadataTypes(property.DeclaringType));
        }
        private static IEnumerable<T> GetAttributes<T>(this PropertyInfo property, IEnumerable<Type> types)
        {
            List<T> attributes = new List<T>();
            foreach (Type type in types)
            {
                PropertyInfo metaProperty = type.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (metaProperty != null)
                {
                    attributes.AddRange(Attribute.GetCustomAttributes(metaProperty, true).Where(a => a is T).Cast<T>());
                }
            }

            return attributes;
        }
        public static IEnumerable<T> GetAttributes<T>(this Type type)
        {
            return GetMetadataTypes(type).GetAttributes<T>();
        }
        private static IEnumerable<T> GetAttributes<T>(this IEnumerable<Type> types)
        {
            List<T> attributes = new List<T>();

            foreach (Type type in types)
            {
                attributes.AddRange(type.GetCustomAttributes(true).Where(a => a is T).Cast<T>());
            }

            return attributes;
        }
        private static IEnumerable<Type> GetMetadataTypes(Type type)
        {
            yield return type;

            var meta = type.GetAttribute<MetadataTypeAttribute>();

            if (meta != null)
                yield return meta.MetadataClassType;
        }
    }
}
