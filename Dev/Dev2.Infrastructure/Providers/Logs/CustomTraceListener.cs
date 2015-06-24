
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;

namespace Dev2.Providers.Logs
{
    /// <summary>
    /// This is the trace writer used by the studio. Note other than testing there are no usages
    /// for this class as it is initialized from the app.config
    /// </summary>
    public class CustomTextWriter : TraceListener
    {
        public static string LoggingFileName
        {
            get
            {

                return Path.Combine(StudioLogPath, "Warewolf Studio.log");
            }
        }

        public static string WarewolfAppPath
        {
            get
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var warewolfAppPath = Path.Combine(appDataFolder, "Warewolf");
                if(!Directory.Exists(warewolfAppPath))
                {
                    Directory.CreateDirectory(warewolfAppPath);
                }
                return warewolfAppPath;
            }
        }

        public static string StudioLogPath
        {
            get
            {
                var studioLogPath = Path.Combine(WarewolfAppPath, "Studio Logs");
                if(!Directory.Exists(studioLogPath))
                {
                    Directory.CreateDirectory(studioLogPath);
                }
                return studioLogPath;
            }
        }

        public override void Write(string value)
        {
            try
            {
                Dev2Logger.Log.Info(value);
            }
            catch(ObjectDisposedException)
            {
                //ignore this exception
            }
        }

        public override void WriteLine(string value)
        {
            try
            {

                Dev2Logger.Log.Info(value);

            }
            catch(ObjectDisposedException)
            {
                //ignore this exception
            }
        }


        protected override void Dispose(bool disposing)
        {
      
        }
    }

    [ExcludeFromCodeCoverage]
    public class Dev2LoggingTextWriter : TextWriter
    {
        #region Overrides of TextWriter

        /// <summary>
        /// Writes a string followed by a line terminator asynchronously to the text string or stream. 
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous write operation.
        /// </returns>
        /// <param name="value">The string to write. If the value is null, only a line terminator is written. </param><exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception><exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
        public override Task WriteLineAsync(string value)
        {
            Dev2Logger.Log.Error(value);
            return base.WriteLineAsync(value);
        }

        /// <summary>
        /// Writes a character followed by a line terminator asynchronously to the text string or stream.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous write operation.
        /// </returns>
        /// <param name="value">The character to write to the text stream.</param><exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception><exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
        public override Task WriteLineAsync(char value)
        {
            Dev2Logger.Log.Error(value);
            return base.WriteLineAsync(value);
        }

        /// <summary>
        /// Writes a string to the text string or stream asynchronously.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous write operation. 
        /// </returns>
        /// <param name="value">The string to write. If <paramref name="value"/> is null, nothing is written to the text stream.</param><exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception><exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
        public override Task WriteAsync(string value)
        {
            Dev2Logger.Log.Error(value);
            return base.WriteAsync(value);
        }

        /// <summary>
        /// Writes a character to the text string or stream asynchronously.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous write operation.
        /// </returns>
        /// <param name="value">The character to write to the text stream.</param><exception cref="T:System.ObjectDisposedException">The text writer is disposed.</exception><exception cref="T:System.InvalidOperationException">The text writer is currently in use by a previous write operation. </exception>
        public override Task WriteAsync(char value)
        {
            Dev2Logger.Log.Error(value);
            return base.WriteAsync(value);
        }

        /// <summary>
        /// Writes out a formatted string and a new line, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)"/>.
        /// </summary>
        /// <param name="format">A composite format string (see Remarks).</param><param name="arg">An object array that contains zero or more objects to format and write. </param><exception cref="T:System.ArgumentNullException">A string or object is passed in as null. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.FormatException"><paramref name="format"/> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="arg"/> array. </exception>
        public override void WriteLine(string format, params object[] arg)
        {
            Dev2Logger.Log.Error(arg);
        }

        /// <summary>
        /// Writes out a formatted string and a new line, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)"/>.
        /// </summary>
        /// <param name="format">A composite format string (see Remarks).</param><param name="arg0">The first object to format and write. </param><param name="arg1">The second object to format and write. </param><param name="arg2">The third object to format and write. </param><exception cref="T:System.ArgumentNullException"><paramref name="format"/> is null. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.FormatException"><paramref name="format"/> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the number of objects to be formatted (which, for this method overload, is three). </exception>
        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            Dev2Logger.Log.Error(string.Format(format,arg0,arg1,arg2));
        }

        /// <summary>
        /// Writes a formatted string and a new line to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object,System.Object)"/> method.
        /// </summary>
        /// <param name="format">A composite format string (see Remarks).</param><param name="arg0">The first object to format and write. </param><param name="arg1">The second object to format and write. </param><exception cref="T:System.ArgumentNullException"><paramref name="format"/> is null. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.FormatException"><paramref name="format"/> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the number of objects to be formatted (which, for this method overload, is two). </exception>
        public override void WriteLine(string format, object arg0, object arg1)
        {
            Dev2Logger.Log.Error(string.Format(format, arg0, arg1));
        }

        /// <summary>
        /// Writes a formatted string and a new line to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object)"/> method.
        /// </summary>
        /// <param name="format">A composite format string (see Remarks).</param><param name="arg0">The object to format and write. </param><exception cref="T:System.ArgumentNullException"><paramref name="format"/> is null. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.FormatException"><paramref name="format"/> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the number of objects to be formatted (which, for this method overload, is one). </exception>
        public override void WriteLine(string format, object arg0)
        {
            Dev2Logger.Log.Error(string.Format(format, arg0));
        }

        /// <summary>
        /// Writes the text representation of an object by calling the ToString method on that object, followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The object to write. If <paramref name="value"/> is null, only the line terminator is written. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine(object value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes a string followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The string to write. If <paramref name="value"/> is null, only the line terminator is written. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine(string value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of a decimal value followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The decimal value to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine(decimal value)
        {
               Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of a 8-byte floating-point value followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The 8-byte floating-point value to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine(double value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of a 4-byte floating-point value followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The 4-byte floating-point value to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine(float value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of an 8-byte unsigned integer followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The 8-byte unsigned integer to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine(ulong value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of an 8-byte signed integer followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The 8-byte signed integer to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine(long value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of a 4-byte unsigned integer followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The 4-byte unsigned integer to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine(uint value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of a 4-byte signed integer followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The 4-byte signed integer to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine(int value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of a Boolean value followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The Boolean value to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine(bool value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes a character followed by a line terminator to the text string or stream.
        /// </summary>
        /// <param name="value">The character to write to the text stream. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine(char value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes a line terminator to the text string or stream.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine()
        {
            Dev2Logger.Log.Error(Environment.NewLine);
        }

        /// <summary>
        /// Writes a formatted string to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object[])"/> method.
        /// </summary>
        /// <param name="format">A composite format string (see Remarks). </param><param name="arg">An object array that contains zero or more objects to format and write. </param><exception cref="T:System.ArgumentNullException"><paramref name="format"/> or <paramref name="arg"/> is null. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.FormatException"><paramref name="format"/> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the length of the <paramref name="arg"/> array. </exception>
        public override void Write(string format, params object[] arg)
        {
            Dev2Logger.Log.Error(string.Format(format,arg));
        }

        /// <summary>
        /// Writes a formatted string to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object,System.Object,System.Object)"/> method.
        /// </summary>
        /// <param name="format">A composite format string (see Remarks). </param><param name="arg0">The first object to format and write. </param><param name="arg1">The second object to format and write. </param><param name="arg2">The third object to format and write. </param><exception cref="T:System.ArgumentNullException"><paramref name="format"/> is null. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.FormatException"><paramref name="format"/> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the number of objects to be formatted (which, for this method overload, is three). </exception>
        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            Dev2Logger.Log.Error(string.Format(format, arg0,arg1,arg2));
        }

        /// <summary>
        /// Writes a formatted string to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object,System.Object)"/> method.
        /// </summary>
        /// <param name="format">A composite format string (see Remarks). </param><param name="arg0">The first object to format and write. </param><param name="arg1">The second object to format and write. </param><exception cref="T:System.ArgumentNullException"><paramref name="format"/> is null. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.FormatException"><paramref name="format"/> is not a valid composite format string.-or- The index of a format item is less than 0 (zero) or greater than or equal to the number of objects to be formatted (which, for this method overload, is two). </exception>
        public override void Write(string format, object arg0, object arg1)
        {
            Dev2Logger.Log.Error(string.Format(format, arg0,arg1));
        }

        /// <summary>
        /// Writes a formatted string to the text string or stream, using the same semantics as the <see cref="M:System.String.Format(System.String,System.Object)"/> method.
        /// </summary>
        /// <param name="format">A composite format string (see Remarks). </param><param name="arg0">The object to format and write. </param><exception cref="T:System.ArgumentNullException"><paramref name="format"/> is null. </exception><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception><exception cref="T:System.FormatException"><paramref name="format"/> is not a valid composite format string.-or- The index of a format item is less than 0 (zero), or greater than or equal to the number of objects to be formatted (which, for this method overload, is one). </exception>
        public override void Write(string format, object arg0)
        {
            Dev2Logger.Log.Error(string.Format(format, arg0));
        }

        /// <summary>
        /// Writes the text representation of an object to the text string or stream by calling the ToString method on that object.
        /// </summary>
        /// <param name="value">The object to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(object value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes a string to the text string or stream.
        /// </summary>
        /// <param name="value">The string to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(string value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of a decimal value to the text string or stream.
        /// </summary>
        /// <param name="value">The decimal value to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(decimal value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of an 8-byte floating-point value to the text string or stream.
        /// </summary>
        /// <param name="value">The 8-byte floating-point value to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(double value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of a 4-byte floating-point value to the text string or stream.
        /// </summary>
        /// <param name="value">The 4-byte floating-point value to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(float value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of an 8-byte unsigned integer to the text string or stream.
        /// </summary>
        /// <param name="value">The 8-byte unsigned integer to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(ulong value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of an 8-byte signed integer to the text string or stream.
        /// </summary>
        /// <param name="value">The 8-byte signed integer to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(long value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of a 4-byte unsigned integer to the text string or stream.
        /// </summary>
        /// <param name="value">The 4-byte unsigned integer to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(uint value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of a 4-byte signed integer to the text string or stream.
        /// </summary>
        /// <param name="value">The 4-byte signed integer to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(int value)
        {
            Dev2Logger.Log.Error(value);
        }

        /// <summary>
        /// Writes the text representation of a Boolean value to the text string or stream.
        /// </summary>
        /// <param name="value">The Boolean value to write. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(bool value)
        {
            Dev2Logger.Log.Error(value);
        }

      
        /// <summary>
        /// Writes a character to the text string or stream.
        /// </summary>
        /// <param name="value">The character to write to the text stream. </param><exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception><exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write(char value)
        {
            Dev2Logger.Log.Error(value);
        }

        #endregion

        #region Overrides of TextWriter

        /// <summary>
        /// When overridden in a derived class, returns the character encoding in which the output is written.
        /// </summary>
        /// <returns>
        /// The character encoding in which the output is written.
        /// </returns>
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

        #endregion
    }
}
