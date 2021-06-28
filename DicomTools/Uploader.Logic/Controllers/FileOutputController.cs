using System;
using System.IO;

namespace Uploader.Logic.Controllers
{
    /// <summary>
    /// Implementation of <see cref="IIOController"/> that
    /// outputs all information to a text file.
    /// </summary>
    internal class FileOutputController : IIOController
    {
        /// <summary>
        /// Gets or sets the filename to which all output will be written.
        /// </summary>
        internal static string FileName { get; set; } = @$".\UNDEFINED-OUTPUT-{DateTime.UtcNow.ToString().Replace(":", string.Empty)}.txt";

        /// <summary>
        ///     Writes the text representation of the specified array of objects, followed by
        ///     the current line terminator, to the standard output stream using the specified
        ///     format information.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An array of objects to write using format.</param>
        /// <exception cref="ArgumentNullException">format or arg is null.</exception>
        /// <exception cref="FormatException">The format specification in format is invalid.</exception>
        public void WriteLine(string format, params object[] arg)
        {
            string parentFolder = new FileInfo(FileName).DirectoryName;

            if (!Directory.Exists(parentFolder))
            {
                Directory.CreateDirectory(parentFolder);
            }
            using (StreamWriter streamWriter = File.Exists(FileName) ? File.AppendText(FileName) : File.CreateText(FileName))
            {
                streamWriter.WriteLine(format, arg);
            }
        }

        /// <summary>
        ///     Writes the specified string value, followed by the current line terminator, to
        ///     the standard output stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public void WriteLine(string value)
        {
            string parentFolder = new FileInfo(FileName).DirectoryName;
            if (!Directory.Exists(parentFolder))
            {
                Directory.CreateDirectory(parentFolder);
            }
            using (StreamWriter streamWriter = File.Exists(FileName) ? File.AppendText(FileName) : File.CreateText(FileName))
            {
                streamWriter.WriteLine(value);
            }
        }
    }
}
