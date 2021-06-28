using System;
using System.IO;

namespace Uploader.Logic.Controllers
{
    /// <summary>
    /// Implementation of <see cref="IIOController"/> that 
    /// writes all output to the console session, the current application runs in.1
    /// </summary>
    internal class ConsoleController : IIOController
    {
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
            Console.WriteLine(format, arg);
        }

        /// <summary>
        ///     Writes the specified string value, followed by the current line terminator, to
        ///     the standard output stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }
    }


}
