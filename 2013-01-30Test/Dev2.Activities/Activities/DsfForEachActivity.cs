using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using System;
using System.Activities;
using System.Collections;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Value_Objects;
using Unlimited.Framework;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{

    public class DsfForEachActivity : DsfActivityAbstract<bool>
    {
        //string _previousParentID;
        #region Variables

        private string _forEachElementName;
        private string _displayName;

        // ReSharper disable InconsistentNaming
        private ForEachBootstrapTO operationalData;
        // ReSharper restore InconsistentNaming

        #endregion Variables

        #region Properties

        [Inputs("FromDisplayName")]
        public string FromDisplayName
        {
            get
            {
                return _displayName;
            }
            set
            {
                _displayName = value;
                ForEachElementName = value;
            }
        }

        [Inputs("ForEachElementName")]
        public string ForEachElementName
        {
            get
            {
                return _forEachElementName;
            }
            set
            {
                _forEachElementName = value;
            }
        }

        public Variable test { get; set; }
        public ActivityFunc<string, bool> DataFunc { get; set; }

        public bool FailOnFirstError { get; set; }
        public string ElementName { private set; get; }
        public string PreservedDataList { private set; get; }

        // REMOVE : Travis.Frisinger - 28.11.2012 : The two variables below are no longer required
        private Variable<IEnumerator<UnlimitedObject>> dataTags = new Variable<IEnumerator<UnlimitedObject>>("dataTags");
        private Variable<UnlimitedObject> inputData = new Variable<UnlimitedObject>("inputData");
        private List<bool> results = new List<bool>();

        // REMOVE : No longer used
        DelegateInArgument<string> actionArgument = new DelegateInArgument<string>("explicitDataFromParent");

        // used to avoid IO mapping adjustment issues ;)
        // REMOVE : 2 variables below not used any more.....
        private Variable<string> _origInput = new Variable<string>("origInput");
        private Variable<string> _origOutput = new Variable<string>("origOutput");


        #endregion Properties

        #region Ctor

        public DsfForEachActivity()
        {
            DataFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>(string.Format("explicitData_{0}", DateTime.Now.ToString("yyyyMMddhhmmss")))

            };
            DisplayName = "For Each";
        }

        #endregion Ctor

        #region CacheMetaData

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddDelegate(DataFunc);
            metadata.AddImplementationVariable(dataTags);
            metadata.AddImplementationVariable(inputData);
            metadata.AddImplementationVariable(_origInput);
            metadata.AddImplementationVariable(_origOutput);

            base.CacheMetadata(metadata);
        }

        #endregion CacheMetaData

        #region Execute

        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();

            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors;
            Guid executionID = DataListExecutionID.Get(context);

            try
            {
                string elmName = ForEachElementName;

                ForEachBootstrapTO exePayload = FetchExecutionType(elmName, executionID, compiler, out errors);
                allErrors.MergeErrors(errors);
                string error;
                ForEachInnerActivityTO innerA = GetInnerActivity(out error);
                allErrors.AddError(error);
                exePayload.InnerActivity = innerA;

                operationalData = exePayload;
                // flag it as scoped so we can use a single DataList
                dataObject.IsDataListScoped = true;

                if(exePayload.MaxExecutions > 0)
                {

                    // set the iteration data ;)
                    IterateIOMapping(1, context);

                    // schedule the func to execute ;)
                    // ReSharper disable RedundantTypeArgumentsOfMethod
                    context.ScheduleFunc<string, bool>(DataFunc, string.Empty, ActivityCompleted);
                    // ReSharper restore RedundantTypeArgumentsOfMethod
                }

                /*
                 * 1. Extract IO Mapping if Activity
                 * 2. Extract Wizard Mapping if Coded Activity
                 * 3. Build a new DataList per execution
                 * 4. Delete DataList after execution
                 * 5. 
                 */

            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                if(allErrors.HasErrors())
                {
                    string err = DisplayAndWriteError("DsfForEachActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }
            }
        }

        /// <summary>
        /// Iterates the IO mapping.
        /// </summary>
        // ReSharper disable InconsistentNaming
        private void IterateIOMapping(int idx, NativeActivityContext context)
        // ReSharper restore InconsistentNaming
        {
            string newInputs = string.Empty;
            string newOutputs = string.Empty;

            // Now mutate the mappings ;)
            if(operationalData.InnerActivity.OrigInnerInputMapping != null)
            {
                // (*) == ({idx}) ;)
                newInputs = operationalData.InnerActivity.OrigInnerInputMapping;
                newInputs = newInputs.Replace("(*)", "(" + idx + ")");
            }
            else
            {
                // coded activity

                #region Coded Activity IO ManIP

                var tmp = (operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<string>);

                string token = "*";

                if(idx > 1)
                {
                    token = (idx - 1).ToString();
                }

                if(tmp != null)
                {
                    IList<DsfForEachItem> data = tmp.GetForEachInputs(context);
                    IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                    // amend inputs ;)
                    foreach(DsfForEachItem d in data)
                    {
                        string input = d.Value;
                        input = input.Replace("(" + token + ")", "(" + idx + ")");

                        updates.Add(new Tuple<string, string>(d.Value, input));
                    }

                    // push updates for Inputs
                    tmp.UpdateForEachInputs(updates, context);
                    if(idx == 1)
                    {
                        operationalData.InnerActivity.OrigCodedInputs = updates;
                    }

                    operationalData.InnerActivity.CurCodedInputs = updates;


                    // Process outputs
                    data = tmp.GetForEachOutputs(context);
                    updates = new List<Tuple<string, string>>();

                    // amend inputs ;)
                    foreach(DsfForEachItem d in data)
                    {
                        string input = d.Value;
                        input = input.Replace("(" + token + ")", "(" + idx + ")");

                        updates.Add(new Tuple<string, string>(d.Value, input));
                    }

                    // push updates 
                    tmp.UpdateForEachOutputs(updates, context);
                    if(idx == 1)
                    {
                        operationalData.InnerActivity.OrigCodedOutputs = updates;
                    }

                    operationalData.InnerActivity.CurCodedOutputs = updates;
                }
                else if(tmp == null)
                {
                    var tmp2 = (operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<bool>);

                    if(tmp2 != null && !(tmp2 is DsfForEachActivity))
                    {
                        IList<DsfForEachItem> data = tmp2.GetForEachInputs(context);
                        IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                        // amend inputs ;)
                        foreach(DsfForEachItem d in data)
                        {
                            string output = d.Value;
                            output = output.Replace("(" + token + ")", "(" + idx + ")");

                            updates.Add(new Tuple<string, string>(d.Value, output));
                        }

                        // push updates 
                        tmp2.UpdateForEachInputs(updates, context);
                        if(idx == 1)
                        {
                            operationalData.InnerActivity.OrigCodedInputs = updates;
                        }
                        operationalData.InnerActivity.CurCodedInputs = updates;

                        // Process outputs
                        data = tmp2.GetForEachOutputs(context);
                        updates = new List<Tuple<string, string>>();

                        // amend inputs ;)
                        foreach(DsfForEachItem d in data)
                        {
                            string input = d.Value;
                            input = input.Replace("(" + token + ")", "(" + idx + ")");

                            updates.Add(new Tuple<string, string>(d.Value, input));
                        }

                        // push updates 
                        tmp2.UpdateForEachOutputs(updates, context);
                        if(idx == 1)
                        {
                            operationalData.InnerActivity.OrigCodedOutputs = updates;
                        }

                        operationalData.InnerActivity.CurCodedOutputs = updates;
                    }
                }

                #endregion

            }

            if(operationalData.InnerActivity.OrigInnerOutputMapping != null)
            {
                // (*) == ({idx}) ;)
                newOutputs = operationalData.InnerActivity.OrigInnerOutputMapping;
                newOutputs = newOutputs.Replace("(*)", "(" + idx + ")");
            }


            var dev2ActivityIoMapping = DataFunc.Handler as IDev2ActivityIOMapping;
            if(dev2ActivityIoMapping != null)
            {
                dev2ActivityIoMapping.InputMapping = newInputs;
            }

            var activityIoMapping = DataFunc.Handler as IDev2ActivityIOMapping;
            if(activityIoMapping != null)
            {
                activityIoMapping.OutputMapping = newOutputs;
            }

        }

        /// <summary>
        /// Fetches the type of the execution.
        /// </summary>
        /// <param name="iterateToken">The iterate token.</param>
        /// <param name="dlID">The dl ID.</param>
        /// <param name="compiler">The compiler.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        private ForEachBootstrapTO FetchExecutionType(string iterateToken, Guid dlID, IDataListCompiler compiler, out ErrorResultTO errors)
        {
            IBinaryDataListEntry token = compiler.Evaluate(dlID, enActionType.User, iterateToken, false, out errors);
            ForEachBootstrapTO result = new ForEachBootstrapTO(enForEachExecutionType.Scalar, 0, null);

            if(token != null)
            {
                int totalPasses;

                if(token.IsRecordset)
                {
                    // Extract the index for iteration count...
                    string idx = DataListUtil.ExtractIndexRegionFromRecordset(iterateToken);

                    if(!Int32.TryParse(idx, out totalPasses))
                    {
                        totalPasses = token.FetchLastRecordsetIndex();
                    }

                    result = new ForEachBootstrapTO(enForEachExecutionType.Recordset, totalPasses, token);
                }
                else
                {
                    // ghost 
                    if(!DataListUtil.isRootVariable(iterateToken))
                    {
                        result = new ForEachBootstrapTO(enForEachExecutionType.GhostService, Int32.MaxValue, token);
                    }
                    else
                    {
                        // numeric
                        if(Int32.TryParse(token.FetchScalar().TheValue, out totalPasses))
                        {
                            result = new ForEachBootstrapTO(enForEachExecutionType.Numeric, totalPasses, null);
                        }
                    }
                }
            }
            else
            {
                errors.AddError("Cannot evaluate [ " + iterateToken + " ] for ForEach execution");
            }

            return result;
        }

        /// <summary>
        /// Restores the handler fn.
        /// </summary>
        private void RestoreHandlerFn(NativeActivityContext context)
        {

            var activity = (DataFunc.Handler as IDev2ActivityIOMapping);

            if(activity != null)
            {

                if(operationalData.InnerActivity.OrigCodedInputs != null)
                {
                    var tmp = (operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<string>);

                    int idx = operationalData.IterationCount;

                    if(tmp != null)
                    {
                        // Restore Inputs ;)
                        IList<DsfForEachItem> data = tmp.GetForEachInputs(context);
                        IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                        // amend inputs ;)
                        foreach(DsfForEachItem d in data)
                        {
                            string input = d.Value;
                            input = input.Replace("(" + idx + ")", "(*)");

                            updates.Add(new Tuple<string, string>(d.Value, input));
                        }

                        // push updates for Inputs
                        tmp.UpdateForEachInputs(updates, context);


                        // Restore Outputs ;)
                        data = tmp.GetForEachInputs(context);
                        updates = new List<Tuple<string, string>>();

                        // amend inputs ;)
                        foreach(DsfForEachItem d in data)
                        {
                            string input = d.Value;
                            input = input.Replace("(" + idx + ")", "(*)");

                            updates.Add(new Tuple<string, string>(d.Value, input));
                        }

                        // push updates for Inputs
                        tmp.UpdateForEachOutputs(updates, context);

                    }
                    else
                    {
                        var tmp2 = (operationalData.InnerActivity.InnerActivity as DsfActivityAbstract<bool>);

                        // Restore Inputs ;)
                        if(tmp2 != null)
                        {
                            IList<DsfForEachItem> data = tmp2.GetForEachInputs(context);
                            IList<Tuple<string, string>> updates = new List<Tuple<string, string>>();

                            // amend inputs ;)
                            foreach(DsfForEachItem d in data)
                            {
                                string input = d.Value;
                                input = input.Replace("(" + idx + ")", "(*)");

                                updates.Add(new Tuple<string, string>(d.Value, input));
                            }

                            // push updates for Inputs
                            tmp2.UpdateForEachInputs(updates, context);


                            // Restore Outputs ;)
                            data = tmp2.GetForEachInputs(context);
                            updates = new List<Tuple<string, string>>();

                            // amend inputs ;)
                            foreach(DsfForEachItem d in data)
                            {
                                string input = d.Value;
                                input = input.Replace("(" + idx + ")", "(*)");

                                updates.Add(new Tuple<string, string>(d.Value, input));
                            }

                            // push updates for Inputs
                            tmp2.UpdateForEachOutputs(updates, context);
                        }
                    }
                }
                else
                {
                    activity.InputMapping = operationalData.InnerActivity.OrigInnerInputMapping;
                    activity.OutputMapping = operationalData.InnerActivity.OrigInnerOutputMapping;
                }
            }
            else
            {
                throw new Exception("DsfForEachActivity - RestoreHandlerFunction has encountered a null Function");
            }
        }

        private ForEachInnerActivityTO GetInnerActivity(out string error)
        {
            ForEachInnerActivityTO result = null;
            error = string.Empty;

            try
            {
                IDev2ActivityIOMapping tmp = DataFunc.Handler as IDev2ActivityIOMapping;
                result = new ForEachInnerActivityTO(tmp);
            }
            catch(Exception e)
            {
                error = e.Message;
            }

            return result;
        }

        private void ActivityCompleted(NativeActivityContext context, ActivityInstance instance, bool result)
        {

            if(operationalData != null)
            {
                operationalData.IncIterationCount();

                if(operationalData.HasMoreData())
                {
                    // Re-jigger the mapping ;)
                    IterateIOMapping((operationalData.IterationCount + 1), context);
                    // ReSharper disable RedundantTypeArgumentsOfMethod
                    context.ScheduleFunc<string, bool>(DataFunc, string.Empty, ActivityCompleted);
                    // ReSharper restore RedundantTypeArgumentsOfMethod
                }
                else
                {
                    // that is all she wrote ;)
                    IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
                    dataObject.IsDataListScoped = false;
                    // return it all to normal
                    RestoreHandlerFn(context);

                }
            }
        }

        #endregion Execute

        #region Private Methods



        //#region Ghost Service Handler
        //private void ActivityCompleted(NativeActivityContext context, ActivityInstance instance, bool result)
        //{
        //    IApplicationMessage messageNotification = context.GetExtension<IApplicationMessage>();
        //    IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
        //    IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

        //    results.Add(result);

        //    if (results.Where(c => !c).Count() == 0)
        //    {
        //        Result.Set(context, true);
        //    }
        //    else
        //    {
        //        Result.Set(context, false);
        //    }

        //    IEnumerator<UnlimitedObject> dat = dataTags.Get(context);
        //    bool more = dat.MoveNext();

        //    if (more)
        //    {
        //        if (DataFunc.Handler != null)
        //        {
        //            var innerActivity = DataFunc.Handler as DsfActivity;
        //            if (innerActivity != null)
        //            {
        //                _count++;
        //                string inMap = innerActivity.InputMapping;
        //                string outMap = innerActivity.OutputMapping;
        //                if (!string.IsNullOrEmpty(inMap))
        //                {
        //                    XmlDocument inxDoc = new XmlDocument();
        //                    inxDoc.LoadXml(inMap);
        //                    innerActivity.InputMapping = inxDoc.OuterXml;
        //                }
        //                if (!string.IsNullOrEmpty(outMap))
        //                {
        //                    XmlDocument outxDoc = new XmlDocument();
        //                    outxDoc.LoadXml(outMap);
        //                    innerActivity.OutputMapping = outxDoc.OuterXml;
        //                }
        //            }

        //            // Travis.Frisinger : 07.04.2012 - Must set DataObject not rely upon what is passed in via the context!!!
        //            // We must dynamically remove old data and inject the next record
        //            string nextPayload = dat.Current.XmlString;
        //            nextPayload = nextPayload.Replace("<" + ElementName + ">", "").Replace("</" + ElementName + ">", "");
        //           // string newDL = compiler.ReplaceValue(DataObject.XmlData, ElementName, nextPayload);

        //            //DataObject.XmlData = newDL;

        //            //Notify(messageNotification, string.Format("\r\n<ForEachInput>\r\n{0}\r\n</ForEachInput>\r\n", newDL));

        //            context.ScheduleFunc<string, bool>(DataFunc, string.Empty, ActivityCompleted);
        //        }
        //        else
        //        {
        //            Notify(messageNotification, string.Format("\r\n<ForEachActivity>No child activity found to execute</ForEachActivity>\r\n"));

        //        }
        //    }
        //    else
        //    {
        //        // Travis.Frisinger : 04.07.2012
        //        // no more data, reset datalist if need be -- This is for a ghost or recursively bound input variable correction step
        //        if (PreservedDataList != null)
        //        {
        //            dataObject.DataList = PreservedDataList;
        //            // We need to scrub the last record from the input stream in this case, else it will be transfered in the mapping ;)
        //            //Added for new datalist 
        //            ErrorResultTO errors = new ErrorResultTO();
        //            dataObject.DataListID = compiler.Shape(dataObject.DataListID, enDev2ArgumentType.Output, OutputMapping, out errors);
        //            //string filteredADL = compiler.ShapeOutput(DataObject.XmlData, null, OutputMapping, DataObject.DataList);

        //            // now set as the new and adjusted dataList
        //            //DataObject.XmlData = filteredADL;
        //        }

        //        // 31.07.2012 - Travis 
        //        // Restore the Design time Handler function
        //        RestoreHandlerFn(context);
        //    }
        //}
        //#endregion

        //private void IndexExecution(NativeActivityContext context, ActivityInstance instance, bool result)
        //{
        //    IApplicationMessage messageNotification = context.GetExtension<IApplicationMessage>();
        //    IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
        //    IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

        //    if (_count < _numOfIterations)
        //    {
        //        _count++;
        //        if (DataFunc.Handler != null)
        //        {
        //            var innerActivity = GetInnerActivity();
        //            //Check if the inner activity is not null and contains input output mapping isnt null
        //            if (innerActivity != null && innerActivity.InputMapping != null && innerActivity.OutputMapping != null)
        //            {
        //                string inMap = _origInput.Get(context);
        //                string outMap = _origOutput.Get(context);
        //                if (!string.IsNullOrEmpty(inMap))
        //                {
        //                    XmlDocument inxDoc = new XmlDocument();
        //                    inxDoc.LoadXml(inMap);
        //                    //check the inputs to see if they contain star and inject the right index number
        //                    foreach (XmlNode child in inxDoc.FirstChild.ChildNodes)
        //                    {
        //                        string test = child.Attributes["Source"].Value;
        //                        if (test.Contains("(") && test.Contains(")"))
        //                        {
        //                            int startIndex = test.IndexOf("(");
        //                            int endIndex = test.IndexOf(")");
        //                            string indexsec = test.Substring(startIndex + 1, endIndex - startIndex - 1);
        //                            if (indexsec == "*")
        //                            {
        //                                //Replace the star with the index number
        //                                test = test.Replace("*", _count.ToString());
        //                                child.Attributes["Source"].Value = test;
        //                            }
        //                        }
        //                    }
        //                    innerActivity.InputMapping = inxDoc.OuterXml;
        //                }

        //                if (!string.IsNullOrEmpty(outMap))
        //                {
        //                    XmlDocument outxDoc = new XmlDocument();
        //                    outxDoc.LoadXml(outMap);
        //                    //check the outputs to see if they contain star and inject the right index number
        //                    foreach (XmlNode child in outxDoc.FirstChild.ChildNodes)
        //                    {
        //                        string test = child.Attributes["Value"].Value;
        //                        if (test.Contains("(") && test.Contains(")"))
        //                        {
        //                            int startIndex = test.IndexOf("(");
        //                            int endIndex = test.IndexOf(")");
        //                            string indexsec = test.Substring(startIndex + 1, endIndex - startIndex - 1);
        //                            if (indexsec == "*")
        //                            {
        //                                //Replace the star with the index number
        //                                test = test.Replace("*", _count.ToString());
        //                                child.Attributes["Value"].Value = test;
        //                            }
        //                        }
        //                    }
        //                    innerActivity.OutputMapping = outxDoc.OuterXml;
        //                }
        //                (DataFunc.Handler as DsfActivity).RemoveInputFromOutput = true;
        //            }
        //            //Schedule the function/activity again
        //            context.ScheduleFunc<string, bool>(DataFunc, string.Empty, IndexExecution);

        //        }
        //        else
        //        {
        //            Notify(messageNotification, string.Format("\r\n<ForEachActivity>No child activity found to execute</ForEachActivity>\r\n"));
        //        }
        //    }
        //    else
        //    {
        //        // Travis.Frisinger : 04.07.2012
        //        // no more data, reset datalist if need be -- This is for a ghost or recursively bound input variable correction step
        //        if (PreservedDataList != null)
        //        {
        //            dataObject.DataList = PreservedDataList;
        //            // We need to scrub the last record from the input stream in this case, else it will be transfered in the mapping ;)
        //            //Added for new datalist 
        //            ErrorResultTO errors = new ErrorResultTO();
        //            dataObject.DataListID = compiler.Shape(dataObject.DataListID, enDev2ArgumentType.Output, OutputMapping, out errors);
        //            //string filteredADL = compiler.ShapeOutput(DataObject.XmlData, null, OutputMapping, DataObject.DataList);

        //            // now set as the new and adjusted dataList
        //            //DataObject.XmlData = filteredADL;
        //        }

        //        // 31.07.2012 - Travis 
        //        // Restore the Design time Handler function in the activity and its mappings arnt null
        //        var innerActivity = GetInnerActivity();
        //        if (innerActivity != null && innerActivity.InputMapping != null && innerActivity.OutputMapping != null)
        //        {
        //            RestoreHandlerFn(context);
        //        }
        //    }
        //}


        #endregion Private Methodss

        #region Get Debug Inputs/Outputs

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            return GetDebugItems(dataList, StateType.Before, ForEachElementName);
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            return GetDebugItems(dataList, StateType.After, ForEachElementName.Replace("*", ""));
        }

        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, ForEachElementName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, ForEachElementName.Replace("*", ""));
        }

        #endregion

    }
}
