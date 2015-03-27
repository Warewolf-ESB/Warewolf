
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Data;
using Dev2.DataList.Contract;
using Dev2.PathOperations;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
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
    public class DsfPathDelete : DsfAbstractFileActivity, IPathInput
    {

        public DsfPathDelete()
            : base("Delete")
        {
            InputPath = string.Empty;
        }

        protected override IList<OutputTO> ExecuteConcreteAction(NativeActivityContext context, out ErrorResultTO allErrors)
        {

            IList<OutputTO> outputs = new List<OutputTO>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();


            allErrors = new ErrorResultTO();

            var colItr = new WarewolfListIterator();

            //get all the possible paths for all the string variables

            var inputItr = new WarewolfIterator(dataObject.Environment.Eval(InputPath));
            colItr.AddVariableToIterateOn(inputItr);

            var unameItr = new WarewolfIterator(dataObject.Environment.Eval(Username));
            colItr.AddVariableToIterateOn(unameItr);

            var passItr = new WarewolfIterator(dataObject.Environment.Eval(Password));
            colItr.AddVariableToIterateOn(passItr);


            outputs.Add(DataListFactory.CreateOutputTO(Result));

            if(dataObject.IsDebugMode())
            {
                AddDebugInputItem(InputPath, "Input Path", dataObject.Environment);
                AddDebugInputItemUserNamePassword(dataObject.Environment);
            }

            while(colItr.HasMoreData())
            {
                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();

                try
                {
                    IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(colItr.FetchNextValue(inputItr),
                                                                                colItr.FetchNextValue(unameItr),
                                                                                colItr.FetchNextValue(passItr),
                                                                                true);

                    IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
                    string result = broker.Delete(dstEndPoint);
                    outputs[0].OutputStrings.Add(result);
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

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null && updates.Count == 1)
            {
                InputPath = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if(itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
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
