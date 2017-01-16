/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Utils;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.PathOperations;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide an activity that can write a file and its contents via FTP, FTPS and file system
    /// </summary>
    [ToolDescriptorInfo("FileFolder-Write", "Write File", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "File, FTP, FTPS & SFTP", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_File_Write_File")]
    public class DsfFileWrite : DsfAbstractFileActivity, IFileWrite, IPathOutput, IPathOverwrite
    {

        public DsfFileWrite()
            : base("Write File")
        {
            OutputPath = string.Empty;
            FileContents = string.Empty;
        }

        protected override IList<OutputTO> ExecuteConcreteAction(IDSFDataObject dataObject, out ErrorResultTO allErrors, int update)
        {
            IList<OutputTO> outputs = new List<OutputTO>();

            allErrors = new ErrorResultTO();
            var colItr = new WarewolfListIterator();

            //get all the possible paths for all the string variables
            var inputItr = new WarewolfIterator(dataObject.Environment.Eval(OutputPath, update));
            colItr.AddVariableToIterateOn(inputItr);

            var unameItr = new WarewolfIterator(dataObject.Environment.Eval(Username, update));
            colItr.AddVariableToIterateOn(unameItr);

            var passItr = new WarewolfIterator(dataObject.Environment.Eval(DecryptedPassword,update));
            colItr.AddVariableToIterateOn(passItr);

            var privateKeyItr = new WarewolfIterator(dataObject.Environment.Eval(PrivateKeyFile, update));
            colItr.AddVariableToIterateOn(privateKeyItr);

            var contentItr =new WarewolfIterator(dataObject.Environment.Eval(FileContents, update));
            colItr.AddVariableToIterateOn(contentItr);

            outputs.Add(DataListFactory.CreateOutputTO(Result));


            if(dataObject.IsDebugMode())
            {
                AddDebugInputItem(OutputPath, "Output Path", dataObject.Environment, update);
                AddDebugInputItem(new DebugItemStaticDataParams(GetMethod(), "Method"));
                AddDebugInputItemUserNamePassword(dataObject.Environment, update);
                if (!string.IsNullOrEmpty(PrivateKeyFile))
                {
                    AddDebugInputItem(PrivateKeyFile, "Private Key File", dataObject.Environment, update);
                }
                AddDebugInputItem(FileContents, "File Contents", dataObject.Environment, update);
            }

            while(colItr.HasMoreData())
            {
                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
                var writeType = GetCorrectWriteType();
                Dev2PutRawOperationTO putTo = ActivityIOFactory.CreatePutRawOperationTO(writeType, TextUtils.ReplaceWorkflowNewLinesWithEnvironmentNewLines(colItr.FetchNextValue(contentItr)));
                IActivityIOPath opath = ActivityIOFactory.CreatePathFromString(colItr.FetchNextValue(inputItr),
                                                                                colItr.FetchNextValue(unameItr),
                                                                                colItr.FetchNextValue(passItr),
                                                                                true, colItr.FetchNextValue(privateKeyItr));
                IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(opath);

                try
                {
                    if(allErrors.HasErrors())
                    {
                        outputs[0].OutputStrings.Add(null);
                    }
                    else
                    {
                        string result = broker.PutRaw(endPoint, putTo);
                        outputs[0].OutputStrings.Add(result);
                    }
                }
                catch(Exception e)
                {
                    outputs[0].OutputStrings.Add(null);
                    allErrors.AddError(e.Message);
                    break;
                }
            }

            return outputs;
        }

        WriteType GetCorrectWriteType()
        {
            if(AppendBottom)
            {
                return WriteType.AppendBottom;
            }
            if(AppendTop)
            {
                return WriteType.AppendTop;
            }
            if(Overwrite)
            {
                return WriteType.Overwrite;
            }
            return WriteType.AppendBottom;
        }

        #region Properties

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

        #endregion Properties

        #region Private Methods


        private string GetMethod()
        {
            return GetCorrectWriteType().GetDescription();
        }

        #endregion

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {
                    if(t.Item1 == OutputPath)
                    {
                        OutputPath = t.Item2;
                    }

                    if(t.Item1 == FileContents)
                    {
                        FileContents = t.Item2;
                    }
                }
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

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(OutputPath, FileContents);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
