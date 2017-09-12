using System;

namespace EFAudit.Interfaces
{
    public interface IBinder : IValueTranslator
    {
        /// <summary>
        /// Convert the serialized string value back in to the original value
        /// </summary>
        /// <param name="raw">The value to be bound</param>
        /// <param name="type">The data type to bind as</param>
        /// <param name="existingValue">
        /// If not null, the value that this bind operation will override.
        /// </param>
        /// <returns>The bound value, the requested data type</returns>
        object Bind(string raw, Type type, object existingValue = null);
    }
}
