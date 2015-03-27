
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
using System.IO;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.DataList.Contract;
using Dev2.PathOperations;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Storage;

namespace Dev2.Activities.PathOperations
{
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide an activity that can move a file/folder via FTP, FTPS and file system
    /// </summary>
    public abstract class DsfAbstractMultipleFilesActivity : DsfAbstractFileActivity
    {
        protected DsfAbstractMultipleFilesActivity(string displayName)
            : base(displayName)
        {
            InputPath = string.Empty;
            OutputPath = string.Empty;
            Overwrite = false;
            DestinationPassword = string.Empty;
            DestinationUsername = string.Empty;
        }

        protected IWarewolfListIterator ColItr;

        protected override IList<OutputTO> ExecuteConcreteAction(NativeActivityContext context,
                                                                 out ErrorResultTO allErrors)
        {
            IList<OutputTO> outputs = new List<OutputTO>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            allErrors = new ErrorResultTO();
            ColItr = new WarewolfListIterator();

            //get all the possible paths for all the string variables          
            var inputItr = new WarewolfIterator(dataObject.Environment.Eval(InputPath));
            ColItr.AddVariableToIterateOn(inputItr);

           
            var outputItr = new WarewolfIterator(dataObject.Environment.Eval(OutputPath));
            ColItr.AddVariableToIterateOn(outputItr);

           
            var unameItr = new WarewolfIterator(dataObject.Environment.Eval(Username));
            ColItr.AddVariableToIterateOn(unameItr);

            
            var passItr = new WarewolfIterator(dataObject.Environment.Eval(Password));
            ColItr.AddVariableToIterateOn(passItr);

            
            var desunameItr = new WarewolfIterator(dataObject.Environment.Eval(DestinationUsername));
            ColItr.AddVariableToIterateOn(desunameItr);

            
            var despassItr = new WarewolfIterator(dataObject.Environment.Eval(DestinationPassword));
            ColItr.AddVariableToIterateOn(despassItr);

            AddItemsToIterator(dataObject.Environment);

            outputs.Add(DataListFactory.CreateOutputTO(Result));

            if(dataObject.IsDebugMode())
            {
                AddDebugInputItem(new DebugEvalResult(InputPath, "Source Path", dataObject.Environment));
                AddDebugInputItemUserNamePassword(dataObject.Environment);
                AddDebugInputItem(new DebugEvalResult(OutputPath, "Destination Path", dataObject.Environment));
                AddDebugInputItemDestinationUsernamePassword(dataObject.Environment, DestinationPassword, DestinationUsername);
                AddDebugInputItem(new DebugItemStaticDataParams(Overwrite.ToString(), "Overwrite"));
                AddDebugInputItems(dataObject.Environment);
            }

            while(ColItr.HasMoreData())
            {
                var hasError = false;
                IActivityIOPath src = null;
                IActivityIOPath dst = null;
                try
                {
                    src = ActivityIOFactory.CreatePathFromString(ColItr.FetchNextValue(inputItr),
                                                                                 ColItr.FetchNextValue(unameItr),
                                                                                 ColItr.FetchNextValue(passItr),
                                                                                 true);


                }
                catch(IOException ioException)
                {
                    allErrors.AddError("Source: " + ioException.Message);
                    hasError = true;
                }
                try
                {
                    dst = ActivityIOFactory.CreatePathFromString(ColItr.FetchNextValue(outputItr),
                                                                                     ColItr.FetchNextValue(desunameItr),
                                                                                     ColItr.FetchNextValue(despassItr),
                                                                                     true);

                }
                catch(IOException ioException)
                {
                    allErrors.AddError("Destination:" + ioException.Message);
                    hasError = true;
                }

                if(hasError)
                {
                    outputs[0].OutputStrings.Add(null);
                    MoveRemainingIterators();
                    continue;
                }
                IActivityIOOperationsEndPoint scrEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(src);
                IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

                try
                {
                    IActivityOperationsBroker broker = GetOperationBroker();
                    var result = ExecuteBroker(broker, scrEndPoint, dstEndPoint);
                    outputs[0].OutputStrings.Add(result);

                }
                catch(Exception e)
                {
                    allErrors.AddError(e.Message);
                    outputs[0].OutputStrings.Add(null);
                }
            }

            return outputs;

        }

        protected virtual void AddItemsToIterator(IExecutionEnvironment environment)
        {
        }

        protected virtual void AddDebugInputItems(IExecutionEnvironment environment)
        {
        }

        protected abstract string ExecuteBroker(IActivityOperationsBroker broker, IActivityIOOperationsEndPoint scrEndPoint, IActivityIOOperationsEndPoint dstEndPoint);
        protected abstract void MoveRemainingIterators();

        public Func<IActivityOperationsBroker> GetOperationBroker = () => ActivityIOFactory.CreateOperationsBroker();

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this is overwrite.
        /// </summary>
        [Inputs("Overwrite")]
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Inputs("Input Path")]
        [FindMissing]
        public string InputPath { get; set; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        [Inputs("Output Path")]
        [FindMissing]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the destination file/folder user name
        /// </summary>
        [Inputs("Destination Username"), FindMissing]
        public string DestinationUsername { get; set; }

        /// <summary>
        /// Gets or sets the destination file/folder password
        /// </summary>
        [Inputs("Destination Password"), FindMissing]
        public string DestinationPassword { get; set; }

        #endregion Properties

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {

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
            return GetForEachItems(InputPath, OutputPath);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
