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

using System;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Activities.PathOperations;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Storage.Interfaces;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("FileFolder-UnZip", "UnZip", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "File, FTP, FTPS & SFTP", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_File_Unzip")]
    public class DsfUnZip : DsfAbstractMultipleFilesActivity, IUnZip, IPathOverwrite, IPathOutput, IPathInput,
                            IDestinationUsernamePassword, IEquatable<DsfUnZip>
    {
        public DsfUnZip()
            : base("Unzip")
        {
            ArchivePassword = string.Empty;
        }

        WarewolfIterator _archPassItr;
        protected override bool AssignEmptyOutputsToRecordSet => true;
        
        /// <summary>
        /// Gets or sets the archive password.
        /// </summary>      
        [Inputs("Archive Password")]
        [FindMissing]
        public string ArchivePassword { get; set; }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = nameof(InputPath),
                    Value = InputPath,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(Username),
                    Value = Username,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(PrivateKeyFile),
                    Value = PrivateKeyFile,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(OutputPath),
                    Value = OutputPath,
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = nameof(DestinationUsername),
                    Value = DestinationUsername,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(DestinationPrivateKeyFile),
                    Value = DestinationPrivateKeyFile,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(Overwrite),
                    Value = Overwrite.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(Result),
                    Value = Result,
                    Type = StateVariable.StateType.Output
                }
            };
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach(Tuple<string, string> t in updates)
            {
                if(t.Item1 == ArchivePassword)
                {
                    ArchivePassword = t.Item2;
                }

                if(t.Item1 == Overwrite.ToString())
                {
                    bool.TryParse(t.Item2, out var tmpOverwrite);
                    Overwrite = tmpOverwrite;
                }

                if(t.Item1 == InputPath)
                {
                    InputPath = t.Item2;
                }

                if(t.Item1 == OutputPath)
                {
                    OutputPath = t.Item2;
                }
            }
        }

        protected override void AddItemsToIterator(IExecutionEnvironment environment, int update)
        {
            _archPassItr = new WarewolfIterator(environment.Eval(ArchivePassword, update));
            ColItr.AddVariableToIterateOn(_archPassItr);
        }

        protected override void AddDebugInputItems(IExecutionEnvironment environment, int update)
        {
            AddDebugInputItemPassword("Archive Password", ArchivePassword);
        }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint)
        {
            var zipTo = ActivityIOFactory.CreateUnzipTO(ColItr.FetchNextValue(_archPassItr), Overwrite);
            return broker.UnZip(scrEndPoint, dstEndPoint, zipTo);
        }

        protected override void MoveRemainingIterators()
        {
            ColItr.FetchNextValue(_archPassItr);
        }

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(ArchivePassword, Overwrite.ToString(), OutputPath, InputPath);

        public bool Equals(DsfUnZip other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && string.Equals(ArchivePassword, other.ArchivePassword);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfUnZip) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (ArchivePassword != null ? ArchivePassword.GetHashCode() : 0);
            }
        }
    }
}
