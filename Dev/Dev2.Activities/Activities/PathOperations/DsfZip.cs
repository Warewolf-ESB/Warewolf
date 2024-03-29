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
using System.ComponentModel;
using System.Linq;
using Dev2.Activities;
using Dev2.Activities.PathOperations;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.Util;
using Dev2.PathOperations;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Security.Encryption;
using Warewolf.Storage.Interfaces;


namespace Unlimited.Applications.BusinessDesignStudio.Activities

{

    /// <summary>
    /// PBI : 1172
    /// Status : New 
    /// Purpose : To provide an activity that can zip the contents of a file/folder from FTP, FTPS and file system
    /// </summary>
    [ToolDescriptorInfo("FileFolder-Zip", "Zip", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "File, FTP, FTPS & SFTP", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_File_Zip")]
    public class DsfZip : DsfAbstractMultipleFilesActivity, IZip, IPathInput, IPathOutput, IPathOverwrite,
                          IDestinationUsernamePassword, IEquatable<DsfZip>
    {
        string _compressionRatio;

        public DsfZip()
            : base("Zip")
        {
            ArchiveName = string.Empty;
            CompressionRatio = string.Empty;
            ArchivePassword = string.Empty;
        }
        protected override bool AssignEmptyOutputsToRecordSet => true;
        IWarewolfIterator _archPassItr;
        IWarewolfIterator _compresItr;
        IWarewolfIterator _archNameItr;
        string _archivePassword;

        #region Properties
        /// <summary>
        /// Gets or sets the archive password.
        /// </summary>      
        [Inputs("Archive Password")]
        [FindMissing]
        public string ArchivePassword
        {
            get => _archivePassword;
            set
            {
                if (DataListUtil.ShouldEncrypt(value))
                {
                    try
                    {
                        _archivePassword = DpapiWrapper.Encrypt(value);
                    }
                    catch (Exception)
                    {
                        _archivePassword = value;
                    }
                }
                else
                {
                    _archivePassword = value;
                }
            }
        }



        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        protected string DecryptedArchivePassword => DataListUtil.NotEncrypted(ArchivePassword) ? ArchivePassword : DpapiWrapper.Decrypt(ArchivePassword);


        /// <summary>
        /// Gets or sets the name of the archive.
        /// </summary>
        [Inputs("Archive Name")]
        [FindMissing]
        public string ArchiveName { get; set; }

        /// <summary>
        /// Gets or sets the compression ratio.
        /// </summary>
        [Inputs("Compession Ratio"), FindMissing]
        public string CompressionRatio
        {
            get => _compressionRatio;
            set => _compressionRatio = string.IsNullOrEmpty(value) ? value : value.Replace(" ", "");
        }
        #endregion Properties
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
                    Name = nameof(ArchiveName),
                    Value = ArchiveName,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(CompressionRatio),
                    Value = CompressionRatio,
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
        protected override void AddDebugInputItems(IExecutionEnvironment environment, int update)
        {
            AddDebugInputItem(CompressionRatio, "Compression Ratio", environment, update);
        }

        protected override void AddItemsToIterator(IExecutionEnvironment environment, int update)
        {
            _compresItr = new WarewolfIterator(environment.Eval(CompressionRatio, update));
            ColItr.AddVariableToIterateOn(_compresItr);

            _archNameItr = new WarewolfIterator(environment.Eval(ArchiveName, update));
            ColItr.AddVariableToIterateOn(_archNameItr);

            _archPassItr = new WarewolfIterator(environment.Eval(DecryptedArchivePassword, update));
            ColItr.AddVariableToIterateOn(_archPassItr);

        }

        protected override string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint)
        {

            var zipTo = ActivityIOFactory.CreateZipTO(ColItr.FetchNextValue(_compresItr),
                                                                         ColItr.FetchNextValue(_archPassItr),
                                                                         ColItr.FetchNextValue(_archNameItr),
                                                                         Overwrite);

            return broker.Zip(scrEndPoint, dstEndPoint, zipTo);
        }

        protected override void MoveRemainingIterators()
        {
            ColItr.FetchNextValue(_compresItr);
            ColItr.FetchNextValue(_archPassItr);
            ColItr.FetchNextValue(_archNameItr);
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach (Tuple<string, string> t in updates)
            {
                if (t.Item1 == ArchiveName)
                {
                    ArchiveName = t.Item2;
                }

                if (t.Item1 == ArchivePassword)
                {
                    ArchivePassword = t.Item2;
                }

                if (t.Item1 == CompressionRatio)
                {
                    CompressionRatio = t.Item2;
                }

                if (t.Item1 == InputPath)
                {
                    InputPath = t.Item2;
                }

                if (t.Item1 == OutputPath)
                {
                    OutputPath = t.Item2;
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if (itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(ArchiveName, ArchivePassword, CompressionRatio, InputPath, OutputPath);
        #endregion

        public bool Equals(DsfZip other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var passWordsCompare = CommonEqualityOps.PassWordsCompare(ArchivePassword, other.ArchivePassword);
            return base.Equals(other)
                && string.Equals(CompressionRatio, other.CompressionRatio)
                && passWordsCompare
                && string.Equals(ArchiveName, other.ArchiveName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
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

            return Equals((DsfZip)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (CompressionRatio != null ? CompressionRatio.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ArchivePassword != null ? ArchivePassword.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ArchiveName != null ? ArchiveName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
