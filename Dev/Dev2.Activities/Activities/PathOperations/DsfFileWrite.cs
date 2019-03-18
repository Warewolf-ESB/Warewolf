#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
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
using System.Linq;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Common.Utils;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.PathOperations;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Storage;


namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    [ToolDescriptorInfo("FileFolder-Write", "Write File", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "File, FTP, FTPS & SFTP", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_File_Write_File")]
    public class DsfFileWrite : DsfAbstractFileActivity, IFileWrite, IPathOutput, IPathOverwrite, IEquatable<DsfFileWrite>
    {

        public DsfFileWrite()
            : base("Write File")
        {
            OutputPath = string.Empty;
            FileContents = string.Empty;
        }

        protected override bool AssignEmptyOutputsToRecordSet => true;
        protected override IList<OutputTO> TryExecuteConcreteAction(IDSFDataObject context, out ErrorResultTO error, int update)
        {
            IList<OutputTO> outputs = new List<OutputTO>();

            error = new ErrorResultTO();
            var colItr = new WarewolfListIterator();

            //get all the possible paths for all the string variables
            var inputItr = new WarewolfIterator(context.Environment.Eval(OutputPath, update));
            colItr.AddVariableToIterateOn(inputItr);

            var passItr = new WarewolfIterator(context.Environment.Eval(DecryptedPassword, update));
            colItr.AddVariableToIterateOn(passItr);

            var privateKeyItr = new WarewolfIterator(context.Environment.Eval(PrivateKeyFile, update));
            colItr.AddVariableToIterateOn(privateKeyItr);

            var contentItr = new WarewolfIterator(context.Environment.Eval(FileContents, update));
            colItr.AddVariableToIterateOn(contentItr);

            outputs.Add(DataListFactory.CreateOutputTO(Result));


            if (context.IsDebugMode())
            {
                AddDebugInputItem(OutputPath, "Output Path", context.Environment, update);
                AddDebugInputItem(new DebugItemStaticDataParams(GetMethod(), "Method"));
                AddDebugInputItemUserNamePassword(context.Environment, update);
                if (!string.IsNullOrEmpty(PrivateKeyFile))
                {
                    AddDebugInputItem(PrivateKeyFile, "Private Key File", context.Environment, update);
                }
                AddDebugInputItem(FileContents, "File Contents", context.Environment, update);
            }

            while (colItr.HasMoreData())
            {
                var broker = ActivityIOFactory.CreateOperationsBroker();
                var writeType = GetCorrectWriteType();
                var putTo = ActivityIOFactory.CreatePutRawOperationTO(writeType, TextUtils.ReplaceWorkflowNewLinesWithEnvironmentNewLines(colItr.FetchNextValue(contentItr)));
                var opath = ActivityIOFactory.CreatePathFromString(colItr.FetchNextValue(inputItr), Username,
                                                                                colItr.FetchNextValue(passItr),
                                                                                true, colItr.FetchNextValue(privateKeyItr));
                var endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(opath);

                try
                {
                    if (error.HasErrors())
                    {
                        outputs[0].OutputStrings.Add(null);
                    }
                    else
                    {
                        var result = broker.PutRaw(endPoint, putTo);
                        outputs[0].OutputStrings.Add(result);
                    }
                }
                catch (Exception e)
                {
                    outputs[0].OutputStrings.Add(null);
                    error.AddError(e.Message);
                    break;
                }
            }

            return outputs;
        }

        WriteType GetCorrectWriteType()
        {
            if (AppendBottom)
            {
                return WriteType.AppendBottom;
            }
            if (AppendTop)
            {
                return WriteType.AppendTop;
            }
            if (Overwrite)
            {
                return WriteType.Overwrite;
            }
            return WriteType.AppendBottom;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DsfFileWrite" /> is append.
        /// </summary>
        [Inputs("Append")]
        public bool Append
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file contents.
        /// </summary>
        [Inputs("File Contents")]
        [FindMissing]
        public string FileContents
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        [Inputs("Output Path")]
        [FindMissing]
        public string OutputPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DsfFileWrite" /> is overwrite.
        /// </summary>
        [Inputs("Overwrite")]
        public bool Overwrite
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DsfFileWrite" /> is append top.
        /// </summary>
        [Inputs("Append Top")]
        public bool AppendTop
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DsfFileWrite" /> is append bottom.
        /// </summary>
        [Inputs("Append Bottom")]
        public bool AppendBottom
        {
            get;
            set;
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = nameof(OutputPath),
                    Value = OutputPath,
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = nameof(Overwrite),
                    Value = Overwrite.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(AppendTop),
                    Value = AppendTop.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(AppendBottom),
                    Value = AppendBottom.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(FileContents),
                    Value = FileContents,
                    Type = StateVariable.StateType.InputOutput
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
                    Name = nameof(Result),
                    Value = Result,
                    Type = StateVariable.StateType.Output
                }
            };
        }


        string GetMethod() => GetCorrectWriteType().GetDescription();

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if (updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {
                    if (t.Item1 == OutputPath)
                    {
                        OutputPath = t.Item2;
                    }

                    if (t.Item1 == FileContents)
                    {
                        FileContents = t.Item2;
                    }
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


        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(OutputPath, FileContents);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        public bool Equals(DsfFileWrite other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other)
                && Append == other.Append
                && string.Equals(FileContents, other.FileContents)
                && string.Equals(OutputPath, other.OutputPath)
                && Overwrite == other.Overwrite
                && AppendTop == other.AppendTop
                && AppendBottom == other.AppendBottom;
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

            return Equals((DsfFileWrite)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Append.GetHashCode();
                hashCode = (hashCode * 397) ^ (FileContents != null ? FileContents.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (OutputPath != null ? OutputPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Overwrite.GetHashCode();
                hashCode = (hashCode * 397) ^ AppendTop.GetHashCode();
                hashCode = (hashCode * 397) ^ AppendBottom.GetHashCode();
                return hashCode;
            }
        }
    }
}
