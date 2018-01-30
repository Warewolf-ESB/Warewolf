/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data;
using Dev2.Data.Interfaces;
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
    [ToolDescriptorInfo("FileFolder-Create", "Create", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "File, FTP, FTPS & SFTP", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_File_Create")]
    public class DsfPathCreate : DsfAbstractFileActivity, IPathOutput, IPathOverwrite,IEquatable<DsfPathCreate>
    {

        public DsfPathCreate()
            : base("Create")
        {
            OutputPath = string.Empty;
        }
        protected override bool AssignEmptyOutputsToRecordSet => true;
        protected override IList<OutputTO> ExecuteConcreteAction(IDSFDataObject context, out ErrorResultTO error, int update)
        {

            IList<OutputTO> outputs = new List<OutputTO>();

            error = new ErrorResultTO();
            var colItr = new WarewolfListIterator();

            //get all the possible paths for all the string variables
            
            var outputItr = new WarewolfIterator(context.Environment.Eval(OutputPath, update));
            colItr.AddVariableToIterateOn(outputItr);

            var unameItr = new WarewolfIterator(context.Environment.Eval(Username, update));
            colItr.AddVariableToIterateOn(unameItr);

            var passItr = new WarewolfIterator(context.Environment.Eval(DecryptedPassword,update));
            colItr.AddVariableToIterateOn(passItr);

            var privateKeyItr = new WarewolfIterator(context.Environment.Eval(PrivateKeyFile, update));
            colItr.AddVariableToIterateOn(privateKeyItr);

            if(context.IsDebugMode())
            {
                AddDebugInputItem(new DebugEvalResult(OutputPath, "File or Folder", context.Environment, update));
                AddDebugInputItem(new DebugItemStaticDataParams(Overwrite.ToString(), "Overwrite"));
                AddDebugInputItemUserNamePassword(context.Environment, update);
                if (!string.IsNullOrEmpty(PrivateKeyFile))
                {
                    AddDebugInputItem(PrivateKeyFile, "Destination Private Key File", context.Environment, update);
                }
            }

            while(colItr.HasMoreData())
            {
                var broker = ActivityIOFactory.CreateOperationsBroker();
                var opTo = new Dev2CRUDOperationTO(Overwrite);

                try
                {
                    var dst = ActivityIOFactory.CreatePathFromString(colItr.FetchNextValue(outputItr),
                                                                                colItr.FetchNextValue(unameItr),
                                                                                colItr.FetchNextValue(passItr),
                                                                                true, colItr.FetchNextValue(privateKeyItr));
                    var dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
                    var result = broker.Create(dstEndPoint, opTo, true);
                    outputs.Add(DataListFactory.CreateOutputTO(Result, result));
                }
                catch(Exception e)
                {
                    outputs.Add(DataListFactory.CreateOutputTO(Result, "Failure"));
                    error.AddError(e.Message);
                    break;
                }
            }

            return outputs;
        }

        #region Properties

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
        /// Gets or sets a value indicating whether this <see cref="DsfPathCreate" /> is overwrite.
        /// </summary>
        [Inputs("Overwrite")]
        public bool Overwrite
        {
            get;
            set;
        }

        #endregion Properties

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null && updates.Count == 1)
            {
                OutputPath = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if(itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(OutputPath);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        #endregion

        public bool Equals(DsfPathCreate other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) && string.Equals(OutputPath, other.OutputPath) && Overwrite == other.Overwrite;
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

            return Equals((DsfPathCreate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (OutputPath != null ? OutputPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Overwrite.GetHashCode();
                return hashCode;
            }
        }
    }
}
