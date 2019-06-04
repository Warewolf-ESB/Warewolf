#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Interfaces.Enums;
using Dev2.DataList;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Interfaces;

namespace Dev2.Studio.ViewModels.DataList
{
    public class DataMappingViewModel : SimpleBaseViewModel, IDataMappingViewModel
    {
        IWebActivity _activity;

        bool _isInitialLoad;

        string _activityName;
        string _xmlOutput;

        IInputOutputViewModel _currentlySelectedOutput;
        IInputOutputViewModel _currentlySelectedInput;

        public DataMappingViewModel(IWebActivity activity)
            : this(activity, null)
        {
        }

        public DataMappingViewModel(IWebActivity activity, NotifyCollectionChangedEventHandler mappingCollectionChangedEventHandler)
        {
            _activity = activity;
            Inputs = new ObservableCollection<IInputOutputViewModel>();
            Outputs = new ObservableCollection<IInputOutputViewModel>();

            if(mappingCollectionChangedEventHandler != null)
            {
                Inputs.CollectionChanged += mappingCollectionChangedEventHandler;
                Outputs.CollectionChanged += mappingCollectionChangedEventHandler;
            }
            Initialize(_activity);
        }

        internal void Initialize(IWebActivity activity)
        {
            Activity = activity;
            ActivityName = activity.ServiceName;

            var activeDataList = string.Empty;

            if(DataListSingleton.ActiveDataList != null)
            {
                activeDataList = DataListSingleton.ActiveDataList?.Resource?.DataList ?? "";
            }

            var ioBuilder = new ActivityDataMappingBuilder
            {
                DataList = activeDataList,
            };

            ioBuilder.SetupActivityData(activity);

            var mappingData = ioBuilder.Generate();
            foreach(var ioViewModel in mappingData.Outputs)
            {
                Outputs.Add(ioViewModel);
            }
            foreach(var ioViewModel in mappingData.Inputs)
            {
                Inputs.Add(ioViewModel);
            }
            var toSaveOutputMapping = ioBuilder.SavedOutputMapping;
            var toSaveInputMapping = ioBuilder.SavedInputMapping;

            XmlOutput = toSaveOutputMapping;
            Activity.SavedOutputMapping = toSaveOutputMapping;
            Activity.LiveOutputMapping = toSaveOutputMapping;

            XmlOutput = XmlOutput + toSaveInputMapping;
            Activity.SavedInputMapping = toSaveInputMapping;
            Activity.LiveInputMapping = toSaveInputMapping;

        }

        public string XmlOutput
        {
            get => _xmlOutput;
            set
            {
                _xmlOutput = value;
                OnPropertyChanged("XmlOutput");
            }
        }

        public IWebActivity Activity
        {
            get => _activity;
            set
            {
                _activity = value;
                OnPropertyChanged("Activity");
            }
        }

        public bool IsInitialLoad
        {
            get => _isInitialLoad;
            set
            {
                _isInitialLoad = value;
            }
        }

        public string ActivityName
        {
            get => _activityName;
            set
            {
                _activityName = value;
                OnPropertyChanged("ActivityName");
            }
        }

        public ObservableCollection<IInputOutputViewModel> Outputs { get; set; }

        public ObservableCollection<IInputOutputViewModel> Inputs { get; set; }

        public string GetInputString(IList<IInputOutputViewModel> inputData)
        {
            var inputString = string.Empty;
            IList<IDev2Definition> inputs = new List<IDev2Definition>();
            if(inputData.Count != 0)
            {
                foreach(IInputOutputViewModel itp in inputData)
                {
                    inputs.Add(itp.GetGenerationTO());
                }
                inputString = DataMappingListFactory.GenerateMapping(inputs, enDev2ArgumentType.Input);
            }
            return inputString;
        }

        public string GetOutputString(IList<IInputOutputViewModel> outputData)
        {
            var outputString = string.Empty;
            IList<IDev2Definition> outputs = new List<IDev2Definition>();
            if(outputData.Count != 0)
            {
                foreach(IInputOutputViewModel otp in outputData)
                {
                    outputs.Add(otp.GetGenerationTO());
                }
                outputString = DataMappingListFactory.GenerateMapping(outputs, enDev2ArgumentType.Output);
            }
            return outputString;
        }

        public IInputOutputViewModel CurrentlySelectedOutput
        {
            get => _currentlySelectedOutput;
            set
            {
                _currentlySelectedOutput = value;
                OnPropertyChanged("CurrentlySelectedOutput");
            }
        }

        public IInputOutputViewModel CurrentlySelectedInput
        {
            get => _currentlySelectedInput;
            set
            {
                _currentlySelectedInput = value;
                OnPropertyChanged("CurrentlySelectedInput");
            }
        }
    }
}
