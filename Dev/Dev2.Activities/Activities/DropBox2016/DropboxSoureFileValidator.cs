using System;
using System.IO;
using System.Security;
using Dev2.Common.Interfaces;
using Warewolf.Resource.Errors;

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
                throw new ArgumentException(ErrorResource.DropBoxCannotLocateSpecifiedFiles);
            }
            catch (NotSupportedException)
            {
                throw new NotSupportedException(ErrorResource.DropBoxPathContainsColon);
            }
            catch (PathTooLongException)
            {
                throw new PathTooLongException(ErrorResource.DropBoxSpecifiedPathExceedMaxLength);
            }
            catch (SecurityException)
            {
                throw new SecurityException(ErrorResource.DropBoxCallerHasNoPermission);
            }
        }

        #endregion
    }
}