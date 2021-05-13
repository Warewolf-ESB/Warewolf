/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Core
{
    public interface IActiveDataList
    {
        IDataListViewModel ActiveDataList { get; set; }
    }

    class ActiveDataListImplementation : IActiveDataList
    {
        public IDataListViewModel ActiveDataList { get; set; }
    }

    /// <summary>
    /// Acts as a backing store for the current datalist
    /// Object stores the active data list and can be queried by any view/viewmodel for the current datalist
    /// </summary>
    public static class DataListSingleton
    {
        static IActiveDataList _instance;
        static object _lock = new object();
        public static IActiveDataList Instance
        {
            get
            {
                if (_instance is null)
                {
                    lock (_lock)
                    {
                        if (_instance is null)
                        {
                            _instance = new ActiveDataListImplementation();
                        }
                    }
                }
                return _instance;
            }
        }

        public static IDataListViewModel ActiveDataList => Instance.ActiveDataList;

        public static void SetDataList(IDataListViewModel activeDataList)
        {
            lock (_lock)
            {
                Instance.ActiveDataList = activeDataList;
            }
        }
    }
}
