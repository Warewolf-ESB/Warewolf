/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Dev2.Intellisense.Helper;

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
