using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Utils;
using Dev2.Communication;
using Dev2.Data.Util;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Activities.Designers2.Core
{
    public class DotNetMethodOutputsRegion : IMethodOutputRegion
    {
        private bool _isEnabled;
        public DotNetMethodOutputsRegion(IMethodToolRegion<IPluginAction> action)
        {
            ToolRegionName = "DotNetMethodOutputsRegion";
            _selectedMethod = action.SelectedMethod;
            SetDefaultState();
            UpdateOnMethodSelection();
            _shellViewModel = CustomContainer.Get<IShellViewModel>();
            action.SomethingChanged += SourceOnSomethingChanged;
        }

        private void SetDefaultState()
        {
            IsVoid = true;
            ObjectResult = string.Empty;
            ObjectName = string.Empty;
            RecordsetName = string.Empty;
            IsEnabled = false;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        //Needed for Deserialization
        public DotNetMethodOutputsRegion()
        {
            ToolRegionName = "DotNetMethodOutputsRegion";
            _shellViewModel = CustomContainer.Get<IShellViewModel>();
        }

        private string _recordsetName;
        private bool _isVoid;
        private string _objectName;
        private string _objectResult;
        private IShellViewModel _shellViewModel;
        private IPluginAction _selectedMethod;

        #region Implementation of IToolRegion

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            try
            {
                Errors.Clear();

                var dotNetMethodRegion = sender as DotNetMethodRegion;
                if (dotNetMethodRegion?.SelectedMethod != null)
                {
                    _selectedMethod = dotNetMethodRegion.SelectedMethod;
                }
                // ReSharper disable once ExplicitCallerInfoArgument
                UpdateOnMethodSelection();
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged(@"Inputs");
                OnPropertyChanged(@"IsEnabled");
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
            }
            finally
            {
                CallErrorsEventHandler();
            }
        }

        private void CallErrorsEventHandler()
        {
            ErrorsHandler?.Invoke(this, new List<string>(Errors));
        }

        private void UpdateOnMethodSelection()
        {
            IsEnabled = false;
            if (_selectedMethod != null)
            {
                _selectedMethod.Dev2ReturnType = string.Empty;
                IsVoid = _selectedMethod.IsVoid;
                if (_selectedMethod.IsObject)
                {
                    ObjectName = _selectedMethod.OutputVariable;
                    ObjectResult = _selectedMethod.Dev2ReturnType;
                }
                else
                {
                    RecordsetName = _selectedMethod.OutputVariable;
                }
                IsEnabled = true;
            }
        }

        public string ToolRegionName { get; set; }
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {
            var ser = new Dev2JsonSerializer();
            return ser.Deserialize<IToolRegion>(ser.SerializeToBuilder(this));
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as DotNetMethodOutputsRegion;
            if (region != null)
            {
                RecordsetName = region.RecordsetName;
                IsEnabled = toRestore.IsEnabled;
                IsVoid = ((IMethodOutputRegion)toRestore).IsVoid;
                ObjectResult = region.ObjectResult;
                ObjectName = region.ObjectName;
                // ReSharper disable once ExplicitCallerInfoArgument
                OnPropertyChanged("IsOutputsEmptyRows");
            }
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        #endregion

        #region Implementation of IOutputsToolRegion

        public bool IsVoid
        {
            get
            {
                return _isVoid;
            }
            set
            {
                _isVoid = value;
                OnPropertyChanged();
            }
        }
        public string RecordsetName
        {
            get
            {
                if (string.IsNullOrEmpty(_recordsetName))
                {
                    if (_selectedMethod != null) _recordsetName = _selectedMethod.OutputVariable;
                }
                return _recordsetName;
            }
            set
            {
                _recordsetName = value;
                OnPropertyChanged();
            }
        }

        public bool IsObject => _selectedMethod != null && _selectedMethod.IsObject;

        public string ObjectName
        {
            get { return _objectName; }
            set
            {
                if (IsObject &&!string.IsNullOrEmpty(ObjectResult))
                {
                    try
                    {
                        if (value != null)
                        {
                            _objectName = value;
                            OnPropertyChanged();
                            var language = FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(value);
                            if (language.IsJsonIdentifierExpression)
                            {
                                _shellViewModel.UpdateCurrentDataListWithObjectFromJson(DataListUtil.RemoveLanguageBrackets(value), ObjectResult);
                            }                            
                        }
                        else
                        {
                            _objectName = string.Empty;
                            OnPropertyChanged();
                        }
                    }
                    catch(Exception)
                    {
                        //Is not an object identifier
                    }
                }
            }
        }

        public string ObjectResult
        {
            get { return _objectResult; }
            set
            {
                if (value != null)
                {
                    _objectResult = JSONUtils.Format(value);
                    OnPropertyChanged();
                }
                else
                {
                    _objectResult = string.Empty;
                    OnPropertyChanged();
                }
            }
        }

        public IList<string> Errors
        {
            get
            {
                var errors = new List<string>();
                try
                {
                    
                }
                catch(Exception e)
                {
                    errors.Add(e.Message);
                }
                if(IsObject && string.IsNullOrEmpty(ObjectName))
                {
                    errors.Add(ErrorResource.NoObjectName);
                }
                return errors;
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}