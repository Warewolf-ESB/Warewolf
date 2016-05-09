using System;
using System.IO;
using System.Security;
using Dev2.Common.Interfaces;

namespace Dev2.Activities.DropBox2016
{
    public class DropboxSoureFileValidator : IFilenameValidator
    {
        private readonly string _dropBoxSource;

        public DropboxSoureFileValidator(string dropBoxSource)
        {
            _dropBoxSource = dropBoxSource;
        }

        #region Implementation of IFilenameValidator

        public void Validate()
        {
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Path.GetFullPath(_dropBoxSource);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("Cannot locate local file/s to be uploaded.Please confirm that the correct file location has been entered");
            }
            catch (NotSupportedException)
            {
                throw new NotSupportedException("Path contains a colon (\":\") that is not part of a volume identifier (for example, \"c:\")");
            }
            catch (PathTooLongException)
            {
                throw new PathTooLongException("The specified path, file name, or both exceed the system-defined maximum length");
            }
            catch (SecurityException)
            {
                throw new SecurityException("The caller does not have the required permissions");
            }
        }

        #endregion
    }
}