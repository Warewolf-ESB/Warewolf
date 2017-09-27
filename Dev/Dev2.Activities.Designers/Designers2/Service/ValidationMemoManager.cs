using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Communication;
using Dev2.Providers.Errors;
using Dev2.Services;
using Warewolf.Resource.Errors;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Studio.Interfaces;
using System;

namespace Dev2.Activities.Designers2.Service
{
    public class ValidationMemoManager : IDisposable
    {
        private readonly ServiceDesignerViewModel _serviceDesignerViewModel;
        public readonly string SourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.ServiceDesignerSourceNotFound;
        public static readonly ErrorInfo NoError = new ErrorInfo
        {
            ErrorType = ErrorType.None,
            Message = @"Service Working Normally"
        };
        private IDesignValidationService _validationService;
        private IErrorInfo _worstDesignError;
        private bool _versionsDifferent;

        internal ValidationMemoManager(ServiceDesignerViewModel serviceDesignerViewModel)
        {
            _serviceDesignerViewModel = serviceDesignerViewModel;
        }

        public DesignValidationMemo LastValidationMemo { get; set; }
        public ObservableCollection<IErrorInfo> DesignValidationErrors { get; set; }
        public ErrorType WorstError
        {
            get { return (ErrorType)_serviceDesignerViewModel.GetValue(ServiceDesignerViewModel.WorstErrorProperty); }
            private set {
                _serviceDesignerViewModel.SetValue(ServiceDesignerViewModel.WorstErrorProperty, value); }
        }
        public IErrorInfo WorstDesignError
        {
            get { return _worstDesignError; }
            private set
            {
                if (_worstDesignError != value)
                {
                    _worstDesignError = value;
                    _serviceDesignerViewModel.IsWorstErrorReadOnly = value == null || value.ErrorType == ErrorType.None || value.FixType == FixType.None || value.FixType == FixType.Delete;
                    WorstError = value?.ErrorType ?? ErrorType.None;
                }
            }
        }
        public bool VersionsDifferent
        {
            set
            {
                _versionsDifferent = value;
            }
        }
        public IDesignValidationService ValidationService => _validationService;

        public void RemovePermissionsError()
        {
            var errorInfos = DesignValidationErrors.Where(info => info.FixType == FixType.InvalidPermissions);
            RemoveErrors(errorInfos.ToList());
        }

        public void InitializeLastValidationMemo(IServer server)
        {
            var uniqueId = _serviceDesignerViewModel.UniqueID;
            var designValidationMemo = new DesignValidationMemo
            {
                InstanceID = uniqueId,
                ServiceID = _serviceDesignerViewModel.ResourceID,
                IsValid = _serviceDesignerViewModel.RootModel.Errors.Count == 0
            };
            designValidationMemo.Errors.AddRange(_serviceDesignerViewModel.RootModel.GetErrors(uniqueId).Cast<ErrorInfo>());

            if (server == null)
            {
                designValidationMemo.IsValid = false;
                designValidationMemo.Errors.Add(new ErrorInfo
                {
                    ErrorType = ErrorType.Critical,
                    FixType = FixType.None,
                    InstanceID = uniqueId,
                    Message = ErrorResource.ServerSourceNotFound
                });
            }

            _serviceDesignerViewModel.MappingManager.UpdateLastValidationMemo(designValidationMemo);
        }

        public void UpdateLastValidationMemoWithVersionChanged()
        {
            var memo = new DesignValidationMemo
            {
                InstanceID = _serviceDesignerViewModel.UniqueID,
                IsValid = false,
            };
            memo.Errors.Add(new ErrorInfo
            {
                InstanceID = _serviceDesignerViewModel.UniqueID,
                ErrorType = ErrorType.Critical,
                FixType = FixType.ReloadMapping,
                Message = @"Incorrect Version. The remote workflow has changed.Please refresh"
            });
            _serviceDesignerViewModel.MappingManager.UpdateLastValidationMemo(memo, false);
        }

        public void InitializeValidationService(IServer server)
        {
            if (server?.Connection?.ServerEvents != null)
            {
                _validationService = new DesignValidationService(server.Connection.ServerEvents);
                _validationService.Subscribe(_serviceDesignerViewModel.UniqueID, a => _serviceDesignerViewModel.MappingManager.UpdateLastValidationMemo(a));
            }
        }

        public void UpdateLastValidationMemoWithSourceNotFoundError()
        {
            var memo = new DesignValidationMemo
            {
                InstanceID = _serviceDesignerViewModel.UniqueID,
                IsValid = false,
            };
            memo.Errors.Add(new ErrorInfo
            {
                InstanceID = _serviceDesignerViewModel.UniqueID,
                ErrorType = ErrorType.Critical,
                FixType = FixType.None,
                Message = SourceNotFoundMessage
            });
            UpdateDesignValidationErrors(memo.Errors);
        }

        public void UpdateLastValidationMemoWithOfflineError(ConnectResult result)
        {
            _serviceDesignerViewModel.Dispatcher.Invoke(() =>
            {
                switch (result)
                {
                    case ConnectResult.Success:
                        break;
                    case ConnectResult.ConnectFailed:
                    case ConnectResult.LoginFailed:
                        var uniqueId = _serviceDesignerViewModel.UniqueID;
                        var memo = new DesignValidationMemo
                        {
                            InstanceID = uniqueId,
                            IsValid = false,
                        };
                        memo.Errors.Add(new ErrorInfo
                        {
                            InstanceID = uniqueId,
                            ErrorType = ErrorType.Warning,
                            FixType = FixType.None,
                            Message = result == ConnectResult.ConnectFailed
                                ? @"Server is offline. This service will only execute when the server is online."
                                : @"Server login failed. This service will only execute when the login permissions issues have been resolved."
                        });
                        _serviceDesignerViewModel.MappingManager.UpdateLastValidationMemo(memo);
                        break;
                    default:
                        break;
                }
            });
        }

        public void FixErrors()
        {
            if (!_versionsDifferent && (WorstDesignError.ErrorType == ErrorType.None || WorstDesignError.FixData == null))
            {
                return;
            }

            switch (WorstDesignError.FixType)
            {
                case FixType.ReloadMapping:
                    _serviceDesignerViewModel.ShowLarge = true;
                    if (!_versionsDifferent)
                    {
                        var xml = _serviceDesignerViewModel.MappingManager.FetchXElementFromFixData();
                        var inputs = _serviceDesignerViewModel.MappingManager.GetMapping(xml, true, _serviceDesignerViewModel.MappingManager.DataMappingViewModel.Inputs);
                        var outputs = _serviceDesignerViewModel.MappingManager.GetMapping(xml, false, _serviceDesignerViewModel.MappingManager.DataMappingViewModel.Outputs);

                        _serviceDesignerViewModel.MappingManager.DataMappingViewModel.Inputs.Clear();
                        foreach (var input in inputs)
                        {
                            _serviceDesignerViewModel.MappingManager.DataMappingViewModel.Inputs.Add(input);
                        }

                        _serviceDesignerViewModel.MappingManager.DataMappingViewModel.Outputs.Clear();
                        foreach (var output in outputs)
                        {
                            _serviceDesignerViewModel.MappingManager.DataMappingViewModel.Outputs.Add(output);
                        }
                        _serviceDesignerViewModel.MappingManager.SetInputs();
                        _serviceDesignerViewModel.MappingManager.SetOuputs();
                        RemoveError(WorstDesignError);
                        UpdateWorstError();
                    }
                    else if (_versionsDifferent)
                    {
                        _serviceDesignerViewModel.ResourceModel = _serviceDesignerViewModel.NewModel;
                        _serviceDesignerViewModel.MappingManager.InitializeMappings();
                        RemoveErrors(
                            LastValidationMemo.Errors.Where(a => a.Message.Contains(@"Incorrect Version")).ToList());
                        UpdateWorstError();
                    }
                    break;

                case FixType.IsRequiredChanged:
                    _serviceDesignerViewModel.ShowLarge = true;
                    var inputOutputViewModels = _serviceDesignerViewModel.MappingManager.DeserializeMappings(true, _serviceDesignerViewModel.MappingManager.FetchXElementFromFixData());
                    foreach (var inputOutputViewModel in inputOutputViewModels.Where(c => c.Required))
                    {
                        IInputOutputViewModel model = inputOutputViewModel;
                        var actualViewModel = _serviceDesignerViewModel.MappingManager.DataMappingViewModel.Inputs.FirstOrDefault(c => c.Name == model.Name);
                        if (actualViewModel != null)
                        {
                            if (actualViewModel.Value == string.Empty)
                            {
                                actualViewModel.RequiredMissing = true;
                            }
                        }
                    }

                    break;
                case FixType.None:
                    break;
                case FixType.Delete:
                    break;
                case FixType.InvalidPermissions:
                    break;
                default:
                    break;
            }
        }

        public void RemoveError(IErrorInfo worstError)
        {
            DesignValidationErrors.Remove(worstError);
            _serviceDesignerViewModel.RootModel.RemoveError(worstError);
        }

        public void RemoveErrors(IList<IErrorInfo> worstErrors)
        {
            worstErrors.ToList().ForEach(RemoveError);
        }

        public void UpdateWorstError()
        {
            if (DesignValidationErrors.Count == 0)
            {
                DesignValidationErrors.Add(NoError);
                if (!_serviceDesignerViewModel.RootModel.HasErrors)
                {
                    _serviceDesignerViewModel.RootModel.IsValid = true;
                }
            }

            IErrorInfo[] worstError = { DesignValidationErrors[0] };

            foreach (var error in DesignValidationErrors.Where(error => error.ErrorType > worstError[0].ErrorType))
            {
                worstError[0] = error;
                if (error.ErrorType == ErrorType.Critical)
                {
                    break;
                }
            }
            WorstDesignError = worstError[0];
        }

        public void UpdateDesignValidationErrors(IEnumerable<IErrorInfo> errors)
        {
            DesignValidationErrors.Clear();
            _serviceDesignerViewModel.RootModel.ClearErrors();
            foreach (var error in errors)
            {
                DesignValidationErrors.Add(error);
                _serviceDesignerViewModel.RootModel.AddError(error);
            }
            UpdateWorstError();
        }

        public void Dispose()
        {
            _validationService.Dispose();
        }
    }
}