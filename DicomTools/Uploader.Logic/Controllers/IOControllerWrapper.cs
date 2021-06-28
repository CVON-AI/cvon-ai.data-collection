using System;
using System.Collections.Generic;
using System.IO;

namespace Uploader.Logic.Controllers
{
    /// <summary>
    /// Implementation of <see cref="IIOController"/> that
    /// wraps both <see cref="ConsoleController"/> and 
    /// <see cref="FileOutputController"/>.
    /// </summary>
    internal class IOControllerWrapper : IIOController
    {
        internal IOControllerWrapper()
        {
            this.Controllers = new List<IIOController>()
            {
                new ConsoleController(),
                new FileOutputController()
            };
        }

        /// <summary>
        /// Gets a list of all internal instances
        /// of <see cref="IIOController"/> implementations.
        /// </summary>
        public List<IIOController> Controllers { get; }

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
            foreach (IIOController controller in this.Controllers)
            {
                controller.WriteLine(format, arg);
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
            foreach (IIOController controller in this.Controllers)
            {
                controller.WriteLine(value);
            }
        }
    }


}
