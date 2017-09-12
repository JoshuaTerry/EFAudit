using EFAudit.Interfaces;
using System;

namespace EFAudit.Translation
{
    public class GuidTranslator : IBinder, ISerializer
    {
        public bool Supports(Type type)
        {
            return type == typeof(Guid);
        }

        public object Bind(string raw, Type type, object existingValue)
        {
            try
            {
                if (raw == null)
                    return null;

                return Guid.Parse(raw);
            }
            catch (ArgumentNullException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
        }

        public string Serialize(object obj)
        {
            return (obj != null ? obj.ToString() : null);
        }
    }
}
