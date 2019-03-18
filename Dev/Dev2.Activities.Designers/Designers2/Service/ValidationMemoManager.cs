#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Communication;
using Dev2.Providers.Errors;
using Dev2.Services;
using Warewolf.Resource.Errors;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Studio.Interfaces;

namespace Dev2.Activities.Designers2.Service
{
    public class ValidationMemoManager
    {
        readonly ServiceDesignerViewModel _serviceDesignerViewModel;
        internal readonly string SourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.ServiceDesignerSourceNotFound;
        public static readonly ErrorInfo NoError = new ErrorInfo
        {
            ErrorType = ErrorType.None,
            Message = @"Service Working Normally"
        };
        IErrorInfo _worstDesignError;
        bool _versionsDifferent;

        internal ValidationMemoManager(ServiceDesignerViewModel serviceDesignerViewModel)
        {
            _serviceDesignerViewModel = serviceDesignerViewModel;
        }

        public DesignValidationMemo LastValidationMemo { get; set; }
        public ObservableCollection<IErrorInfo> DesignValidationErrors { get; set; }
        public ErrorType WorstError
        {
            get
            {
                return (ErrorType)_serviceDesignerViewModel.GetValue(ServiceDesignerViewModel.WorstErrorProperty);
            }
            private set
            {
                _serviceDesignerViewModel.SetValue(ServiceDesignerViewModel.WorstErrorProperty, value);
            }
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

        public void SetVersionsDifferent(bool value)
        {
            _versionsDifferent = value;
        }

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
            IDesignValidationService _validationService;
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
                    default:
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
                    ApplyReloadMappingFix();
                    break;
                case FixType.IsRequiredChanged:
                    FixIsRequiredChanged();
                    break;
                case FixType.None:
                case FixType.Delete:
                case FixType.InvalidPermissions:
                default:
                    return;
            }
        }

        private void FixIsRequiredChanged()
        {
            _serviceDesignerViewModel.ShowLarge = true;
            var inputOutputViewModels = _serviceDesignerViewModel.MappingManager.DeserializeMappings(true, _serviceDesignerViewModel.MappingManager.FetchXElementFromFixData());
            foreach (var inputOutputViewModel in inputOutputViewModels.Where(c => c.Required))
            {
                var model = inputOutputViewModel;
                var actualViewModel = _serviceDesignerViewModel.MappingManager.DataMappingViewModel.Inputs.FirstOrDefault(c => c.Name == model.Name);
                if (actualViewModel != null && actualViewModel.Value == string.Empty)
                {
                    actualViewModel.RequiredMissing = true;
                }

            }
        }

        private void ApplyReloadMappingFix()
        {
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
            else
            {
                if (_versionsDifferent)
                {
                    _serviceDesignerViewModel.ResourceModel = _serviceDesignerViewModel.NewModel;
                    _serviceDesignerViewModel.MappingManager.InitializeMappings();
                    RemoveErrors(
                        LastValidationMemo.Errors.Where(a => a.Message.Contains(@"Incorrect Version")).ToList());
                    UpdateWorstError();
                }
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
    }
}