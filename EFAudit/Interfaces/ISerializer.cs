namespace EFAudit.Interfaces
{
    public interface ISerializer : IValueTranslator
    {
        /// <summary>
        /// Convert the object in to a serialized string
        /// </summary>
        /// <param name="obj">The value to be serialized</param>
        /// <returns>The serizlied value, as a string</returns>
        string Serialize(object obj);
    }
}
