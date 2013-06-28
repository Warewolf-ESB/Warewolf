using System;
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Web;

namespace Dev2.DataList.Contract
{
    public sealed class DataListFormat 
    {
        #region Static Members
        private static object _formatLock = new object();
        private static Dictionary<string, DataListFormat> _existingFormats = new Dictionary<string, DataListFormat>(StringComparer.Ordinal);

        /// <summary>
        /// Gets the DatalistFormat instance that represents the given <paramref name="formatName" />, or creates a new one if a DatalistFormat instance
        /// does not exist for the given <paramref name="formatName" />.
        /// </summary>
        /// <param name="formatName">The display name of the datalist format.</param>
        /// <param name="publicFormatName">Name of the public format.</param>
        /// <param name="headerType">Type of the header.</param>
        /// <returns>
        /// An instance of the DatalistFormat type that is unique to the given <paramref name="formatName" />
        /// </returns>
        /// <exception cref="System.ArgumentException">formatName cannot be null or empty string.</exception>
        public static DataListFormat CreateFormat(string formatName, EmitionTypes publicFormatName = EmitionTypes.XML, string headerType = "")
        {
            if (String.IsNullOrEmpty(formatName)) throw new ArgumentException("formatName cannot be null or empty string.");
            DataListFormat format = null;

            lock (_formatLock)
            {
                if (!_existingFormats.TryGetValue(formatName, out format))
                {
                    format = new DataListFormat(formatName, publicFormatName, headerType);
                    _existingFormats.Add(formatName, format);
                }
            }

            return format;
        }
        #endregion

        #region Instance Fields
        private string _formatName;
        #endregion

        #region Public Properties
        /// <summary>
        /// The display name of the format.
        /// </summary>
        public string FormatName { get { return _formatName; } }

        public EmitionTypes PublicFormatName { get; set; }

        public string ContentType { get; set; }
        #endregion

        #region Constructor
        private DataListFormat(string formatName)
        {
            _formatName = formatName;
        }

        private DataListFormat(string formatName, EmitionTypes publicType, string headerType)
        {
            _formatName = formatName;
            PublicFormatName = publicType;
            ContentType = headerType;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return "Datalist Format [" + _formatName + "]";
        }
        #endregion
    }
}
