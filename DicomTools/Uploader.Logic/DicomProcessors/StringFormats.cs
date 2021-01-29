namespace Uploader.Logic.DicomProcessors
{
    public class StringFormats
    {
        /// <summary>
        /// Defines the ISO 8601 date format.
        /// </summary>
        /// <remarks>
        /// This formats to a date only notation. Please use "o" or "O" 
        /// for a full ISO 8601 date/time format including time-zone notation
        /// or "s" for a short ISO 8601 date/time notation, excluding the time-zone.</remarks>
        public const string Iso8601DateFormat = "yyyy-MM-dd";
    }
}