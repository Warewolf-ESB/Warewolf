
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using Dev2.Common.Interfaces.Data;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.UndoFramework;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.DataList
{
    public class DataMappingViewModel : SimpleBaseViewModel, IDataMappingViewModel
    {
        #region Locals

        private IWebActivity _activity;
// ReSharper disable InconsistentNaming
        public bool _isInitialLoad = false;
// ReSharper restore InconsistentNaming
        DelegateCommand _undo;
        DelegateCommand _redo;
        readonly ActionManager _actionManager;
        private string _activityName;
        private string _xmlOutput;

        private IInputOutputViewModel _currentlySelectedOutput;
        private IInputOutputViewModel _currentlySelectedInput;

        #endregion Locals

        #region Imports

        #endregion Imports

        #region Ctor
        public DataMappingViewModel(IWebActivity activity, NotifyCollectionChangedEventHandler mappingCollectionChangedEventHandler = null)
        {

            _activity = activity;
            _actionManager = new ActionManager();
            Inputs = new ObservableCollection<IInputOutputViewModel>();
            Outputs = new ObservableCollection<IInputOutputViewModel>();

            if(mappingCollectionChangedEventHandler != null)
            {
                Inputs.CollectionChanged += mappingCollectionChangedEventHandler;
                Outputs.CollectionChanged += mappingCollectionChangedEventHandler;
            }
            Initialize(_activity);
        }
        #endregion Ctor

        #region Initialize
        internal void Initialize(IWebActivity activity)
        {
            // -- NEW ;)

            Activity = activity;
            ActivityName = activity.ServiceName;

            var activeDataList = string.Empty;

            if(DataListSingleton.ActiveDataList != null)
            {
                activeDataList = DataListSingleton.DataListAsXmlString;
            }

            ActivityDataMappingBuilder ioBuilder = new ActivityDataMappingBuilder
            {
                DataList = activeDataList,
            };

            ioBuilder.SetupActivityData(activity);

            var mappingData = ioBuilder.Generate();

            // save model data
            //Outputs = mappingData.Outputs.ToObservableCollection();
            foreach(var ioViewModel in mappingData.Outputs)
            {
                Outputs.Add(ioViewModel);
            }
            //Inputs = mappingData.Inputs.ToObservableCollection();
            foreach(var ioViewModel in mappingData.Inputs)
            {
                Inputs.Add(ioViewModel);
            }

            // update special fields on the model?!
            var toSaveOutputMapping = ioBuilder.SavedOutputMapping;
            var toSaveInputMapping = ioBuilder.SavedInputMapping;

            XmlOutput = toSaveOutputMapping;
            Activity.SavedOutputMapping = toSaveOutputMapping;
            Activity.LiveOutputMapping = toSaveOutputMapping;

            XmlOutput = (XmlOutput + toSaveInputMapping);
            Activity.SavedInputMapping = toSaveInputMapping;
            Activity.LiveInputMapping = toSaveInputMapping;

        }

        #endregion Initialize

        #region Bindings
        public string XmlOutput
        {
            get
            {
                return _xmlOutput;
            }
            set
            {
                _xmlOutput = value;
                OnPropertyChanged("XmlOutput");
            }
        }

        public IWebActivity Activity
        {
            get
            {
                return _activity;
            }
            set
            {
                _activity = value;
                OnPropertyChanged("Activity");
            }
        }

        public bool IsInitialLoad
        {
            get
            {
                return _isInitialLoad;
            }
            set
            {
                _isInitialLoad = value;
            }
        }

        public string ActivityName
        {
            get
            {
                return _activityName;
            }
            set
            {
                _activityName = value;
                OnPropertyChanged("ActivityName");
            }
        }

        public ObservableCollection<IInputOutputViewModel> Outputs { get; private set; }

        public ObservableCollection<IInputOutputViewModel> Inputs { get; private set; }

        public ICommand UndoCommand
        {
            get
            {
                return _undo ?? (_undo = new DelegateCommand(c => Undo()));
            }
        }

        public ICommand RedoCommand
        {
            get
            {
                return _redo ?? (_redo = new DelegateCommand(c => Redo()));
            }
        }

        public void Undo()
        {
            UndoFramework.Undo();
        }

        public void Redo()
        {
            UndoFramework.Redo();
        }

        public void CopyFrom(IDataMappingViewModel copyObj)
        {
            IObjectCloner<IInputOutputViewModel> cloner = new ObjectCloner<IInputOutputViewModel>();

            Inputs = cloner.CloneObservableCollection(copyObj.Inputs);
            Outputs = cloner.CloneObservableCollection(copyObj.Outputs);
            XmlOutput = copyObj.XmlOutput;

        }

        public ActionManager UndoFramework
        {
            get
            {
                return _actionManager;
            }
        }
        #endregion Bindings

        #region Final Output
        public void CreateXmlOutput(IList<IInputOutputViewModel> outputData, IList<IInputOutputViewModel> inputData)
        {
            string outputString = GetOutputString(outputData);
            XmlOutput = outputString;
            Activity.SavedOutputMapping = outputString;
            Activity.LiveOutputMapping = Activity.SavedOutputMapping;


            string inputString = GetInputString(inputData);
            XmlOutput = (XmlOutput + inputString);
            Activity.SavedInputMapping = inputString;
            Activity.LiveInputMapping = Activity.SavedInputMapping;
        }
        #endregion Final Output

        #region Work Around Till Refactor

        public string GetInputString(IList<IInputOutputViewModel> inputData)
        {
            string inputString = string.Empty;
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
            string outputString = string.Empty;
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

        #endregion Work Around Till Refactor

        #region Get Current Output Item

        public IInputOutputViewModel CurrentlySelectedOutput
        {
            get
            {
                return _currentlySelectedOutput;
            }
            set
            {
                _currentlySelectedOutput = value;
                base.OnPropertyChanged("CurrentlySelectedOutput");
            }
        }
        #endregion Get Current Output Item

        #region Get Current Input Item

        public IInputOutputViewModel CurrentlySelectedInput
        {
            get
            {
                return _currentlySelectedInput;
            }
            set
            {
                _currentlySelectedInput = value;
                base.OnPropertyChanged("CurrentlySelectedInput");
            }
        }
        #endregion Get Current Input Item

        #region Events
        public void InputLostFocusTextBox(string text)
        {
            CurrentlySelectedInput.MapsTo = text;
            CurrentlySelectedInput.Value = text;
            CreateXmlOutput(Outputs, Inputs);
        }

        public void OutputLostFocusTextBox(string text)
        {
            CurrentlySelectedOutput.Value = text;
            CreateXmlOutput(Outputs, Inputs);
        }

        public void InputTextBoxGotFocus(IInputOutputViewModel selected)
        {
            CurrentlySelectedInput = selected;
            CreateXmlOutput(Outputs, Inputs);
        }

        public void OutputTextBoxGotFocus(IInputOutputViewModel selected)
        {
            //CurrentlySelectedOutput = Selected;
            CreateXmlOutput(Outputs, Inputs);
        }
        #endregion Events
    }
}
