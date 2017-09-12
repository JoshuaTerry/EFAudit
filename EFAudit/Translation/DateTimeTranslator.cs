using EFAudit.Interfaces;
using System;
using System.Globalization;

namespace EFAudit.Translation
{
    public class DateTimeTranslator : IBinder, ISerializer
    {
        private readonly bool legacyMode;

        internal DateTimeTranslator(bool legacyMode)
        {
            this.legacyMode = legacyMode;
        }

        public DateTimeTranslator() : this(false) { }

        public bool Supports(Type type)
        {
            return typeof(DateTime?).IsAssignableFrom(type);
        }

        public object Bind(string raw, Type type, object existingValue)
        {
            try
            {
                if (raw == null)
                    return default(DateTime);

                // Check if the date time is represented as numeric ticks
                if (!legacyMode)
                {
                    long ticks;
                    if (long.TryParse(raw, out ticks))
                        return new DateTime(ticks);
                }

                // Else, try parse the date as a string
                // NOTE: This is required to support legacy dates stored as localised strings.  
                //       Localised strings are interrepted differently depending current culture of the machine.
                //       Dates stored as ticks are more reliable as the integer is not subject to localization.
                return (legacyMode)
                    ? DateTime.Parse(raw)
                    : DateTime.Parse(raw, CultureInfo.InvariantCulture);
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
            var dateTime = (DateTime)obj;
            return (legacyMode)
                ? dateTime.ToString()
                : dateTime.Ticks.ToString(CultureInfo.InvariantCulture);
        }
    }
}
