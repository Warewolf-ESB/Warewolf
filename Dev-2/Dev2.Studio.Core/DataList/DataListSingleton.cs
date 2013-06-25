using Dev2.Studio.Core.Interfaces.DataList;

namespace Dev2.Studio.Core {
    /// <summary>
    /// Acts as a backing store for the current datalist
    /// Object stores the active data list and can be queried by any view/viewmodel for the current datalist
    /// </summary>
    public static class DataListSingleton  {

        #region Locals

        private static IDataListViewModel _activeDataList;

        #endregion Locals

        #region Properties

        public static string DataListAsXmlString {
            get {
                return _activeDataList.Resource.DataList;
            }
        }       

        public static IDataListViewModel ActiveDataList {
            get {
                return _activeDataList;
            }
        }

        #endregion Properties

        #region Methods

        public static void SetDataList(IDataListViewModel activeDataList) {
            _activeDataList = activeDataList;
        }

        #endregion Methods
    }
}
