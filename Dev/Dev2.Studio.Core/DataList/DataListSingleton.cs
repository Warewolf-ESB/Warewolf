
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Studio.Core.Interfaces.DataList;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core
{
    /// <summary>
    /// Acts as a backing store for the current datalist
    /// Object stores the active data list and can be queried by any view/viewmodel for the current datalist
    /// </summary>
    public static class DataListSingleton
    {

        #region Locals

        private static IDataListViewModel _activeDataList;

        #endregion Locals

        #region Properties

        public static string DataListAsXmlString
        {
            get
            {
                if(_activeDataList != null)
                {
                    if(_activeDataList.Resource != null)
                    {
                        if(_activeDataList.Resource.DataList != null)
                        {
                            return _activeDataList.Resource.DataList;
                        }
                    }
                }

                return string.Empty;
            }
        }

        public static IDataListViewModel ActiveDataList
        {
            get
            {
                return _activeDataList;
            }
        }

        #endregion Properties

        #region Methods

        public static void SetDataList(IDataListViewModel activeDataList)
        {
            _activeDataList = activeDataList;
        }

        #endregion Methods
    }
}
