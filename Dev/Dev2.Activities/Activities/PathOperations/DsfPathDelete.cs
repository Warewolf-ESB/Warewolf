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
using Dev2.Common.Interfaces.Toolbox;
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
    /// Purpose : To provide an activity that can delete a file/folder and its contents via FTP, FTPS and file system
    /// </summary>
    [ToolDescriptorInfo("FileFolder-Delete", "Delete", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "File, FTP, FTPS & SFTP", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_File_Delete")]
    public class DsfPathDelete : DsfAbstractFileActivity, IPathInput
    {

        public DsfPathDelete()
            : base("Delete")
        {
            InputPath = string.Empty;
        }

        protected override IList<OutputTO> ExecuteConcreteAction(IDSFDataObject dataObject, out ErrorResultTO allErrors, int update)
        {
            IList<OutputTO> outputs = new List<OutputTO>();

            allErrors = new ErrorResultTO();

            var colItr = new WarewolfListIterator();

            //get all the possible paths for all the string variables

            var inputItr = new WarewolfIterator(dataObject.Environment.Eval(InputPath, update));
            colItr.AddVariableToIterateOn(inputItr);

            var unameItr = new WarewolfIterator(dataObject.Environment.Eval(Username, update));
            colItr.AddVariableToIterateOn(unameItr);

            var passItr = new WarewolfIterator(dataObject.Environment.Eval(DecryptedPassword,update));
            colItr.AddVariableToIterateOn(passItr);

            var privateKeyItr = new WarewolfIterator(dataObject.Environment.Eval(PrivateKeyFile, update));
            colItr.AddVariableToIterateOn(privateKeyItr);

            outputs.Add(DataListFactory.CreateOutputTO(Result));

            if(dataObject.IsDebugMode())
            {
                AddDebugInputItem(InputPath, "Input Path", dataObject.Environment, update);
                AddDebugInputItemUserNamePassword(dataObject.Environment, update);
                if (!string.IsNullOrEmpty(PrivateKeyFile))
                {
                    AddDebugInputItem(PrivateKeyFile, "Private Key File", dataObject.Environment, update);
                }
            }
            while (colItr.HasMoreData())
            {
                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

                try
                {
                     var dst = ActivityIOFactory.CreatePathFromString(colItr.FetchNextValue(inputItr),
                         colItr.FetchNextValue(unameItr),
                         colItr.FetchNextValue(passItr),
                         true, colItr.FetchNextValue(privateKeyItr));

                    IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
                    
                    string result = broker.Delete(dstEndPoint);
                    outputs[0].OutputStrings.Add(result);
                }
                catch(Exception e)
                {
                    outputs.Add(DataListFactory.CreateOutputTO(Result, "Failure"));
                    allErrors.AddError(e.Message);
                    break;
                }

                
            }

            return outputs;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Inputs("Input Path")]
        [FindMissing]
        public string InputPath
        {
            get;
            set;
        }

        #endregion Properties

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null && updates.Count == 1)
            {
                InputPath = updates[0].Item2;
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
            return GetForEachItems(InputPath);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
