
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpFramework.Helpers
{
    /// <summary>
    /// Used to handle INI files.
    /// </summary>
    public class IniFile
    {
        private readonly Dictionary<string, IniFileSection> _sections = new Dictionary<string, IniFileSection>();

        /// <summary>
        /// Gets a section.
        /// </summary>
        /// <param name="name">Name of section.</param>
        /// <returns><see cref="IniFileSection"/> if found; otherwise <c>null</c>.</returns>
        public IniFileSection this[string name]
        {
            get
            {
                IniFileSection section;
                return _sections.TryGetValue(name, out section) ? section : null;
            }
        }

        /// <summary>
        /// Add a new section.
        /// </summary>
        /// <param name="name">Name of section.</param>
        /// <example>
        /// <code>
        /// iniFile.Add("users");
        /// iniFile["users"].Add("Jonas");
        /// </code>
        /// </example>
        public void Add(string name)
        {
            if (_sections.ContainsKey(name))
                throw new ArgumentOutOfRangeException("name", "Section do already exist: " + name);
            _sections.Add(name, new IniFileSection(name));
        }

        private void Add(string name, IniFileSection section)
        {
            _sections.Add(name, section);
        }

        /// <summary>
        /// Checks if ini file contains the specified section.
        /// </summary>
        /// <param name="sectionName">Name of section</param>
        /// <returns><c>true</c> if section exists; otherwise <c>false</c>.</returns>
        public bool Contains(string sectionName)
        {
            return _sections.ContainsKey(sectionName);
        }

        /// <summary>
        /// Load a INI file from a stream reader.
        /// </summary>
        /// <param name="reader">StreamReader containing INI file data.</param>
        /// <returns><see cref="IniFile"/> if sucessfully parsed.</returns>
        /// <exception cref="FormatException">If stream do not contain a valid INI file.</exception>
        public static IniFile Load(TextReader reader)
        {
            var sections = new IniFile();
            IniFileSection currentSection = null;

            while (true)
            {
                string line = reader.ReadLine();
                if (line == null)
                    break;
                line = line.Trim();
                if (line == string.Empty || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("["))
                {
                    if (!line.EndsWith("]"))
                        throw new FormatException("Expected section to end with ']': " + line);
                    string sectionName = line.Substring(1, line.Length - 2);
                    currentSection = new IniFileSection(sectionName);
                    sections.Add(sectionName, currentSection);
                    continue;
                }

                int pos = line.IndexOf(':');
                if (pos == -1)
                    throw new FormatException("Expected to find colon (header/value separator): " + line);
                if (line.EndsWith(":") && line.IndexOf(':', pos + 1) == -1)
                    throw new FormatException("Line should not end with a colon: " + line);
                if (currentSection == null)
                    throw new FormatException("Missing first section header.");

                string header = line.Substring(0, pos).Trim();
                string value = line.Substring(pos + 1).Trim();
                currentSection.Add(header, value);
            }

            reader.Close();
            return sections;
        }

        /// <summary>
        /// Read ini file data from a stream.
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <returns><see cref="IniFile"/> if sucessfully parsed.</returns>
        /// <exception cref="FormatException">If stream do not contain a valid INI file.</exception>
        public static IniFile Load(Stream stream)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Stream is not readable.", "stream");

            return Load(new StreamReader(stream));
        }

        /// <summary>
        /// Read a ini file.
        /// </summary>
        /// <param name="fileName">Path and filename</param>
        /// <returns><see cref="IniFile"/> if sucessfully parsed.</returns>
        /// <exception cref="FormatException">If stream do not contain a valid INI file.</exception>
        public static IniFile Load(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Filename is not specified");
            if (!File.Exists(fileName))
                throw new FileNotFoundException(fileName, "fileName");

            return Load(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        /// <summary>
        /// Load INI file from a string variable.
        /// </summary>
        /// <param name="value">Contains ini file contents.</param>
        /// <returns>A <see cref="IniFile"/>.</returns>
        /// <exception cref="FormatException">If stream do not contain a valid INI file.</exception>
        public static IniFile LoadString(string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return Load(new StringReader(value));
        }
        /// <summary>
        /// Write a INI file to disk.
        /// </summary>
        /// <param name="fileName">Path and filename</param>
        /// <exception cref="IOException">If file is not writable or path do not exist.</exception>
        public void Save(string fileName)
        {
            var stream = new FileStream(fileName, FileMode.Create);
            var writer = new StreamWriter(stream);
            foreach (var section in _sections)
            {
                writer.WriteLine('[' + section.Key + ']');
                foreach (var parameter in section.Value)
                    writer.WriteLine(parameter.Key + ": " + parameter.Value);
                writer.WriteLine();
            }
            writer.Close();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var section in _sections)
            {
                sb.AppendLine('[' + section.Key + ']');
                foreach (var parameter in section.Value)
                    sb.AppendLine(parameter.Key + ": " + parameter.Value);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// A section in a INI file.
    /// </summary>
    public class IniFileSection : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IniFileSection"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public IniFileSection(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            Name = name;
        }

        /// <summary>
        /// Gets or sets a parameter value.
        /// </summary>
        /// <param name="name">Name of parameter.</param>
        /// <returns>Value or <c>null</c> if parameter was not found.</returns>
        public string this[string name]
        {
            get
            {
                string value;
                return _values.TryGetValue(name, out value) ? value : null;
            }
            set { _values[name] = value; }
        }

        /// <summary>
        /// Gets or sets name of section.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Add a new parameter
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="value">Value</param>
        public void Add(string name, string value)
        {
            _values[name] = value;
        }

        /// <summary>
        /// Checks if section contains a parameter
        /// </summary>
        /// <param name="name">Name of parameter</param>
        /// <returns><c>true</c> if found; otherwise <c>false</c>.</returns>
        public bool Contains(string name)
        {
            return _values.ContainsKey(name);
        }

        /// <summary>
        /// Remove a parameter
        /// </summary>
        /// <param name="name">Name of parameter.</param>
        /// <returns><c>true</c> if found and removed; otherwise <c>false</c>.</returns>
        public bool Remove(string name)
        {
            return _values.Remove(name);
        }

        #region IEnumerable<KeyValuePair<string,string>> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
