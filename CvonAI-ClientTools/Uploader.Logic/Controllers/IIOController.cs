using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Uploader.Logic.Controllers
{
    internal interface IIOController
    {
        //
        // Summary:
        //     Writes the text representation of the specified array of objects, followed by
        //     the current line terminator, to the standard output stream using the specified
        //     format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg:
        //     An array of objects to write using format.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //
        //   T:System.ArgumentNullException:
        //     format or arg is null.
        //
        //   T:System.FormatException:
        //     The format specification in format is invalid.
        void WriteLine(string format, params object[] arg);

        //
        // Summary:
        //     Writes the specified string value, followed by the current line terminator, to
        //     the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        void WriteLine(string value);

    }

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

        public List<IIOController> Controllers { get; }

        //
        // Summary:
        //     Writes the text representation of the specified array of objects, followed by
        //     the current line terminator, to the standard output stream using the specified
        //     format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg:
        //     An array of objects to write using format.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //
        //   T:System.ArgumentNullException:
        //     format or arg is null.
        //
        //   T:System.FormatException:
        //     The format specification in format is invalid.
        public void WriteLine(string format, params object[] arg)
        {
            foreach (IIOController controller in this.Controllers)
            {
                controller.WriteLine(format, arg);
            }
        }

        //
        // Summary:
        //     Writes the specified string value, followed by the current line terminator, to
        //     the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public void WriteLine(string value)
        {
            foreach (IIOController controller in this.Controllers)
            {
                controller.WriteLine(value);
            }
        }
    }

    internal class ConsoleController : IIOController
    {
        //
        // Summary:
        //     Writes the text representation of the specified array of objects, followed by
        //     the current line terminator, to the standard output stream using the specified
        //     format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg:
        //     An array of objects to write using format.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //
        //   T:System.ArgumentNullException:
        //     format or arg is null.
        //
        //   T:System.FormatException:
        //     The format specification in format is invalid.
        public void WriteLine(string format, params object[] arg)
        {
            Console.WriteLine(format, arg);
        }

        //
        // Summary:
        //     Writes the specified string value, followed by the current line terminator, to
        //     the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }
    }

    internal class FileOutputController : IIOController
    {
        internal static string FileName { get; set; } = @$".\UNDEFINED-OUTPUT-{DateTime.UtcNow.ToString().Replace(":", string.Empty)}.txt";
        //
        // Summary:
        //     Writes the text representation of the specified array of objects, followed by
        //     the current line terminator, to the standard output stream using the specified
        //     format information.
        //
        // Parameters:
        //   format:
        //     A composite format string.
        //
        //   arg:
        //     An array of objects to write using format.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
        //
        //   T:System.ArgumentNullException:
        //     format or arg is null.
        //
        //   T:System.FormatException:
        //     The format specification in format is invalid.
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

        //
        // Summary:
        //     Writes the specified string value, followed by the current line terminator, to
        //     the standard output stream.
        //
        // Parameters:
        //   value:
        //     The value to write.
        //
        // Exceptions:
        //   T:System.IO.IOException:
        //     An I/O error occurred.
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
