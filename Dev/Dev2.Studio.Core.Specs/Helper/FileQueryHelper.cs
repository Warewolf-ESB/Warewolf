
using System.Linq;
using Dev2.Intellisense.Helper;
using System.Collections.Generic;

namespace Dev2.Studio.Core.Specs.Helper
{
    public class FileQueryHelper : IFileSystemQuery
    {
        public FileQueryHelper()
        {
            _queryCollection = new List<string>();
        }

        #region Implementation of IFileSystemQuery

        public List<string> QueryCollection
        {
            get;
            private set;
        }

        private readonly List<string> _queryCollection;

        public void Add(string path)
        {
            _queryCollection.Add(path);
        }

        public void QueryList(string searchPath)
        {
            QueryCollection = _queryCollection.Where(s => s.StartsWith(searchPath)).ToList();
        }

        #endregion
    }
}
