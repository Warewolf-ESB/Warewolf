using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestStep : BindableBase, IServiceTestStep
    {
        private string _stepDescription;
        private StepType _type;
        private string _activityType;
        private ObservableCollection<IServiceTestOutput> _stepOutputs;
        private Guid _uniqueId;
        private IServiceTestStep _parent;
        private ObservableCollection<IServiceTestStep> _children;
        private bool _isTestStepExpanded;
        private bool _isTestStepExpanderEnabled;
        private bool _assertSelected;
        private bool _mockSelected;
        private ImageSource _stepIcon;

        public ServiceTestStep(Guid uniqueId, string activityTypeName, ObservableCollection<IServiceTestOutput> serviceTestOutputs, StepType stepType)
        {
            UniqueId = uniqueId;
            ActivityType = activityTypeName;
            StepOutputs = serviceTestOutputs;
            Type = stepType;
            StepDescription = activityTypeName;
            Children = new ObservableCollection<IServiceTestStep>();
            AssertSelected = Type == StepType.Assert;
            MockSelected = Type == StepType.Mock;
            IsTestStepExpanded = StepOutputs?.Count > 0;
            IsTestStepExpanderEnabled = StepOutputs?.Count > 0;
        }

        public Guid UniqueId
        {
            get { return _uniqueId; }
            set
            {
                _uniqueId = value;
                OnPropertyChanged(() => UniqueId);
            }
        }

        public ImageSource StepIcon
        {
            get { return _stepIcon; }
            set
            {
                _stepIcon = value;
                OnPropertyChanged(() => StepIcon);
            }
        }

        public string ActivityType
        {
            get { return _activityType; }
            set
            {
                _activityType = value;
                OnPropertyChanged(() => ActivityType);
            }
        }

        public StepType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged(() => Type);
            }
        }

        public ObservableCollection<IServiceTestOutput> StepOutputs
        {
            get { return _stepOutputs; }
            set
            {
                _stepOutputs = value;
                OnPropertyChanged(() => StepOutputs);
            }
        }

        public IServiceTestStep Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                OnPropertyChanged(() => Parent);
            }
        }

        public ObservableCollection<IServiceTestStep> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                OnPropertyChanged(() => Children);
            }
        }

        public string StepDescription
        {
            get { return _stepDescription; }
            set
            {
                _stepDescription = value;
                OnPropertyChanged(() => StepDescription);
            }
        }

        public bool IsTestStepExpanded
        {
            get { return _isTestStepExpanded; }
            set
            {
                _isTestStepExpanded = value;
                OnPropertyChanged(() => IsTestStepExpanded);
            }
        }

        public bool IsTestStepExpanderEnabled
        {
            get { return _isTestStepExpanderEnabled; }
            set
            {
                _isTestStepExpanderEnabled = value;
                OnPropertyChanged(() => IsTestStepExpanderEnabled);
            }
        }

        public bool IsExpanderVisible => Children.Count > 0;

        public bool AssertSelected
        {
            get { return _assertSelected; }
            set
            {
                _assertSelected = value;
                if (_assertSelected)
                {
                    Type = StepType.Assert;
                    foreach (var serviceTestOutput in StepOutputs)
                    {
                        var item = serviceTestOutput as ServiceTestOutput;
                        item?.OnSearchTypeChanged();
                    }
                }
                OnPropertyChanged(() => AssertSelected);
            }
        }

        public bool MockSelected
        {
            get { return _mockSelected; }
            set
            {
                _mockSelected = value;
                if (_mockSelected)
                {
                    Type = StepType.Mock;
                    foreach (var serviceTestOutput in StepOutputs)
                    {
                        var item = serviceTestOutput as ServiceTestOutput;
                        if (item != null)
                        {
                            if (!item.IsSearchCriteriaEnabled)
                            {
                                item.IsSearchCriteriaEnabled = true;
                            }
                            if (!item.IsSinglematchCriteriaVisible)
                            {
                                item.IsSinglematchCriteriaVisible = true;
                                item.IsBetweenCriteriaVisible = false;
                            }
                        }
                    }
                }
                OnPropertyChanged(() => MockSelected);
            }
        }

        public void AddNewOutput(string varName)
        {
            if (DataListUtil.IsValueRecordset(varName))
            {
                if (DataListUtil.GetRecordsetIndexType(varName) == enRecordsetIndexType.Numeric)
                {
                    var extractedIndex = DataListUtil.ExtractIndexRegionFromRecordset(varName);
                    int intIndex;
                    if (int.TryParse(extractedIndex, out intIndex))
                    {
                        intIndex++;
                        var blankName = DataListUtil.ReplaceRecordsetIndexWithBlank(varName);
                        var indexedName = DataListUtil.ReplaceRecordsetBlankWithIndex(blankName, intIndex);
                        var serviceTestOutput = new ServiceTestOutput(indexedName, "", "", "") { AddNewAction = () => AddNewOutput(indexedName) };
                        if (StepOutputs.FirstOrDefault(output=>output.Variable.Equals(indexedName,StringComparison.InvariantCultureIgnoreCase))==null)
                        {
                            StepOutputs.Add(serviceTestOutput);
                        }
                    }
                }
                else
                {
                    var serviceTestOutput = new ServiceTestOutput(varName,"","","");
                    serviceTestOutput.AddNewAction = ()=>AddNewOutput(varName);
                    //if (StepOutputs.FirstOrDefault(output => output.Variable.Equals(varName, StringComparison.InvariantCultureIgnoreCase)) == null)
                    {
                        StepOutputs.Add(serviceTestOutput);
                    }
                }
            }
        }
    }
}