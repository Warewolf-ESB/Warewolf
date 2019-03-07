/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.DropBox2016.Result;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Util;
using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Wrappers;
using Dev2.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Dev2.Common.State;

namespace Dev2.Activities.DropBox2016.DeleteActivity
{
    [ToolDescriptorInfo("Dropbox", "Delete", ToolType.Native, "8AC94835-0A28-4166-A53A-D7B07730C135", "Dev2.Activities", "1.0.0.0", "Legacy", "Storage: Dropbox", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Dropbox_Delete")]
    public class DsfDropBoxDeleteActivity : DsfDropBoxBaseActivity, IEquatable<DsfDropBoxDeleteActivity>
    {
        protected Exception Exception;
        protected IDropboxSingleExecutor<IDropboxResult> DropboxSingleExecutor;

        public DsfDropBoxDeleteActivity()
            : this(new DropboxClientWrapperFactory())
        {
        }
        public DsfDropBoxDeleteActivity(IDropboxClientFactory dropboxClientFactory)
            : base(dropboxClientFactory)
        {
            DisplayName = "Delete from Dropbox";
        }

        public OauthSource SelectedSource { get; set; }

        [Inputs("Path in the user's Dropbox")]
        [FindMissing]
        public string DeletePath { get; set; }
        
        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            DropboxSingleExecutor = new DropboxDelete(evaluatedValues["DeletePath"]);
            SetupDropboxClient(SelectedSource.AccessToken);
            var dropboxExecutionResult = DropboxSingleExecutor.ExecuteTask(_dropboxClient);
            if (dropboxExecutionResult is DropboxDeleteSuccessResult dropboxSuccessResult)
            {
                dropboxSuccessResult.GerFileMetadata();
                return new List<string> { GlobalConstants.DropBoxSuccess };
            }
            if (dropboxExecutionResult is DropboxFailureResult dropboxFailureResult)
            {
                Exception = dropboxFailureResult.GetException();
            }
            var executionError = Exception.InnerException?.Message ?? Exception.Message;
            throw new Exception(executionError);
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.StaticActivity;

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            if (string.IsNullOrEmpty(DeletePath))
            {
                dataObject.Environment.AddError(ErrorResource.DropBoxConfirmCorrectFileLocation);
            }
            base.ExecuteTool(dataObject, update);
        }

        public bool Equals(DsfDropBoxDeleteActivity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var isSourceEqual = CommonEqualityOps.AreObjectsEqual<IResource>(SelectedSource, other.SelectedSource);
            return base.Equals(other) 
                && isSourceEqual
                && string.Equals(DisplayName, other.DisplayName)
                && string.Equals(DeletePath, other.DeletePath);
        }

        public override bool Equals(object obj)
        {
            if (obj is DsfDropBoxDeleteActivity)
            {
                return Equals((DsfDropBoxDeleteActivity) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SelectedSource != null ? SelectedSource.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DeletePath != null ? DeletePath.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name = "SelectedSource.ResourceID",
                    Type = StateVariable.StateType.Input,
                    Value = SelectedSource.ResourceID.ToString()
                },
                new StateVariable
                {
                    Name = "DeletePath",
                    Type = StateVariable.StateType.Input,
                    Value = DeletePath
                },
                new StateVariable
                {
                    Name="Result",
                    Type = StateVariable.StateType.Output,
                    Value = Result
                }
            };
        }
    }
}