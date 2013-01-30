using Dev2.Studio.Core.Interfaces.DataList;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace Dev2.Studio.Core.AppResources.Repositories 
{
    [Export(typeof(IFrameworkRepository<IDataListViewModel>))]
    public class DataListRepository : IFrameworkRepository<IDataListViewModel> 
    {
        private readonly List<IDataListViewModel> _dataListViewModels;

        public DataListRepository() {
            _dataListViewModels = new List<IDataListViewModel>();
        }

        public ICollection<IDataListViewModel> All() {
            return _dataListViewModels;
        }

        public ICollection<IDataListViewModel> Find(System.Linq.Expressions.Expression<Func<IDataListViewModel, bool>> expression) {
            throw new NotImplementedException();
        }

        public IDataListViewModel FindSingle(System.Linq.Expressions.Expression<Func<IDataListViewModel, bool>> expression) {
            throw new NotImplementedException();
        }

        public event EventHandler ItemAdded;

        protected void OnItemAdded()
        {
            if (ItemAdded != null)
            {
                ItemAdded(this, new System.EventArgs());
            }
        }

        public void Load() {
            throw new InvalidOperationException("This repository does not require loading. It is intended to be added to at runtime");
        }

        public void Remove(ICollection<IDataListViewModel> instanceObjs) {
            instanceObjs.ToList().ForEach(datalist=> _dataListViewModels.Remove(datalist) );
        }

        public void Remove(IDataListViewModel instanceObj) {
            _dataListViewModels.Remove(instanceObj);
        }

        public void Save(ICollection<IDataListViewModel> instanceObjs) {
            instanceObjs.ToList().ForEach(datalist => _dataListViewModels.Add(datalist));
        }

        public void Save(IDataListViewModel instanceObj) {
            _dataListViewModels.Add(instanceObj);
        }
    }
}
