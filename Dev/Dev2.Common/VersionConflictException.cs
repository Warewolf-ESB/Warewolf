using System;

namespace Dev2.Common
{
    public class VersionConflictException : Exception
    {
        private Version SourceVersionNumber { get; set; }
        private Version DestVersionNumber { get; set; }

        public VersionConflictException(Version sourceVersionNumber, Version destVersionNumber)
        {
            SourceVersionNumber = sourceVersionNumber;
            DestVersionNumber = destVersionNumber;
        }

        #region Overrides of Exception

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <returns>
        /// The error message that explains the reason for the exception, or an empty string ("").
        /// </returns>
        public override string Message
        {
            get
            {
                return "Server version "+SourceVersionNumber+" is incompatiable with version "+DestVersionNumber;
            }
        }

        #endregion
    }
}