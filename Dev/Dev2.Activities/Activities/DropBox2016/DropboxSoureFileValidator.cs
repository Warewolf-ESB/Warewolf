using System;
using System.IO;
using System.Security;
using Dev2.Common.Interfaces;
using Warewolf.Resource.Errors;

namespace Dev2.Activities.DropBox2016
{
    public class DropboxSoureFileValidator : IFilenameValidator
    {
        readonly string _dropBoxSource;

        public DropboxSoureFileValidator(string dropBoxSource)
        {
            _dropBoxSource = dropBoxSource;
        }

        public void Validate()
        {
            try
            {
                // .NET (Core/8+) no longer throws NotSupportedException for paths that contain a
                // misplaced colon, whereas .NET Framework did. Preserve the original validation
                // contract by detecting an invalid colon (one outside the drive-letter specifier).
                var pathToCheck = _dropBoxSource;
                if (!string.IsNullOrEmpty(pathToCheck) && pathToCheck.Length >= 2 &&
                    char.IsLetter(pathToCheck[0]) && pathToCheck[1] == ':')
                {
                    pathToCheck = pathToCheck.Substring(2);
                }
                if (pathToCheck != null && pathToCheck.Contains(':'))
                {
                    throw new NotSupportedException(ErrorResource.DropBoxPathContainsColon);
                }

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
    }
}