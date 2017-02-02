using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface IFileChooserModel
    {
        IList<IFileListing> FetchFiles(IFileListing file);
        IList<IFileListing> FetchDrives();
    }
}
