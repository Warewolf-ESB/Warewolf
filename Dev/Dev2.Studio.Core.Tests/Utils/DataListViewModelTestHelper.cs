
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core;
using Dev2.Studio.Core.ViewModels;
using Dev2.Core.Tests.ProperMoqs;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.ViewModels.DataList;

namespace Dev2.Core.Tests.Utils {
    public sealed class DataListViewModelTestHelper {

        #region DataListItemViewModel Creation Methods

        internal IDataListViewModel CreateDataListViewModel() {
            return default(IDataListViewModel);
        }

        internal IList<IDataListItemModel> CreateDataListItemViewModel(string name, int numberOfChildren, IDataListViewModel dataListVM) {
            IList<IDataListItemModel> dataListItemViewModels = new OptomizedObservableCollection<IDataListItemModel>();
            if(numberOfChildren == 1) {
                dataListItemViewModels.Add(CreateDataListItemViewModel(name, dataListVM));
            }
            else {
                for(int i = 0; i < numberOfChildren; i++) {
                    IDataListItemModel dLVM = CreateDataListItemViewModel((string.Format("{0}{1}", name, (i + 1).ToString())), dataListVM);
                    dataListItemViewModels.Add(dLVM);
                }
            }

            return dataListItemViewModels;
        }

        internal IDataListItemModel CreateDataListItemViewModel(string name, IDataListViewModel dataListVM) {
            IDataListItemModel dLVM = DataListItemModelFactory.CreateDataListModel(name);
            
            
            return dLVM;
        }

        internal IDataListItemModel CreateRecordSetDataListItem(string name, int numberOfRecords, string recordSetPrefix, IDataListViewModel dLVM) {
            string[] records = new string[numberOfRecords];
            for(int i = 0; i < numberOfRecords; i++) {
                records[i] = string.Format("{0}{1}", recordSetPrefix, (i + 1).ToString());
            }
            IDataListItemModel dLIVM = CreateRecordSetDataListItem(name, records, dLVM);

            return dLIVM;

        }

        internal IDataListItemModel CreateRecordSetDataListItem(string name, string[] recordNames, IDataListViewModel dLVM) {
            IDataListItemModel dLIVM = CreateDataListItemViewModel(name, dLVM);

            foreach(var nameResources in recordNames) {
                IDataListItemModel dataListRecordSetField = CreateDataListItemViewModel(nameResources, dLVM);
                dataListRecordSetField.Parent = dLIVM;
                dLIVM.Children.Add(dataListRecordSetField);
            }

            return dLIVM;
        }

        #endregion DataListItemViewModel Creation Methods

        #region DataListViewModel Creation Methods

        internal IDataListViewModel CreateDataListViewModelWithDefaultResourceModel() {
            TestResourceModel testResourceModel = new TestResourceModel();
            IDataListViewModel dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(testResourceModel);

            return dataListViewModel;
        }

        internal IDataListViewModel CreateDataListViewModel(IResourceModel resourceModel) {
            DataListViewModel dLVM = new DataListViewModel();
            dLVM.InitializeDataListViewModel(resourceModel);

            return dLVM;
        }

        internal IDataListViewModel CreateDataListViewModel(IResourceModel resourceModel, IDataListItemModel dataListItemViewModel) {
            IDataListViewModel dlVM = CreateDataListViewModel(resourceModel);
            dlVM.DataList.Add(dataListItemViewModel);

            return dlVM;

        }

        #endregion DataListViewModel Creation Methdos
    }
}
