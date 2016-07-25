/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dev2.Studio.Core.Interfaces.DataList;
using Warewolf.Resource.Errors;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.AppResources.Repositories
{
    public class DataListRepository 
    {
        private readonly List<IDataListViewModel> _dataListViewModels;
        private bool _isDisposed;

        public DataListRepository()
        {
            _dataListViewModels = new List<IDataListViewModel>();
        }

        public ICollection<IDataListViewModel> All()
        {
            return _dataListViewModels;
        }

        public ICollection<IDataListViewModel> Find(Expression<Func<IDataListViewModel, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public IDataListViewModel FindSingle(Expression<Func<IDataListViewModel, bool>> expression)
        {
            throw new NotImplementedException();
        }
        

        public void Load()
        {
            throw new InvalidOperationException(ErrorResource.RepositoryDoesNotRequireLoading);
        }

        public void Remove(ICollection<IDataListViewModel> instanceObjs)
        {
            instanceObjs.ToList().ForEach(datalist => _dataListViewModels.Remove(datalist));
        }

        public void Remove(IDataListViewModel instanceObj)
        {
            _dataListViewModels.Remove(instanceObj);
        }

        public void Save(ICollection<IDataListViewModel> instanceObjs)
        {
            instanceObjs.ToList().ForEach(datalist => _dataListViewModels.Add(datalist));
        }

        public string Save(IDataListViewModel instanceObj)
        {
            _dataListViewModels.Add(instanceObj);
            return "Saved";
        }

        #region Implementation of IDisposable

        ~DataListRepository()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.                    
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                _isDisposed = true;
            }
        }

        #endregion
    }
}
