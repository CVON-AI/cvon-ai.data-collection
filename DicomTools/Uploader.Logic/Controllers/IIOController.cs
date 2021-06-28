﻿using System;
using System.IO;

namespace Uploader.Logic.Controllers
{
    /// <summary>
    /// Interface for types that handle user input and output.
    /// For direct output to the user, this may be a terminal/console window.
    /// For scheduled tasks, this may be output to a log file
    /// </summary>
    internal interface IIOController
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
        void WriteLine(string format, params object[] arg);

        /// <summary>
        ///     Writes the specified string value, followed by the current line terminator, to
        ///     the standard output stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        void WriteLine(string value);

    }
}
