using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Unlimited.Framework;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    // TODO: DELETE UNUSED
    public class DsfForEachActivity_Old : DsfActivityAbstract<bool>
    {
        //string _previousParentID;
        #region Variables

        private int _count = 0;

        private string _forEachElementName;
        private string _displayName;
        private int _numOfIterations = 0;

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


        private Variable<IEnumerator<UnlimitedObject>> dataTags = new Variable<IEnumerator<UnlimitedObject>>("dataTags");
        private Variable<UnlimitedObject> inputData = new Variable<UnlimitedObject>("inputData");
        private List<bool> results = new List<bool>();


        DelegateInArgument<string> actionArgument = new DelegateInArgument<string>("explicitDataFromParent");

        // used to avoid IO mapping adjustment issues ;)
        private Variable<string> _origInput = new Variable<string>("origInput");
        private Variable<string> _origOutput = new Variable<string>("origOutput");

        #endregion Properties

        #region Ctor

        public DsfForEachActivity_Old()
        {
            DataFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>(string.Format("explicitData_{0}", DateTime.Now.ToString("yyyyMMddhhmmss")))

            };
            this.DisplayName = "For Each";
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
            //Added for new datalist - start
            IList<OutputTO> outputs = new List<OutputTO>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();

            Guid dlID = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);
            if(errors.HasErrors())
            {
                allErrors.MergeErrors(errors);
            }

            try
            {
                // Process if no errors
                //if (!errors.HasErrors())
                //{
                //Added for new datalist - end
                _count = 0;
                enForEachExecutionType exeType = enForEachExecutionType.Scalar;

                IApplicationMessage messageNotification = context.GetExtension<IApplicationMessage>();
                //List<string> datad = AmbientDataList.Get(context) as List<string>;
                IDataListBinder binder = context.GetExtension<IDataListBinder>();

                //if (datad == null) {
                this.ParentWorkflowInstanceId = dataObject.ParentWorkflowInstanceId;

                //if ((dataObject != null) && !string.IsNullOrEmpty(dataObject.XmlData)) {
                //    datad = new List<string>() { dataObject.XmlData };
                //} else {
                //    datad = new List<string>();
                //}
                // }

                /* 03.07.2012 - Travis
                 * 
                 * Detect resursively bound variables -- This means we need dynamic datalist and input mapping injection below
                 * Else just evaluate the variable
                 * 
                 */
                string elmName = ForEachElementName;
                string error = string.Empty;

                results = new List<bool>();

                //Calculating the number of times to execute the inner activity and what type of execution to use
                if(!DataListUtil.IsValueRecordset(elmName))
                {
                    //Try evaluate the scalar to a numeric value if it doesnt then set the number of executions to zero 
                    //Added for new datalist 
                    IBinaryDataListEntry entry = compiler.Evaluate(executionId, enActionType.User, elmName, true, out errors);
                    if(errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                    }
                    string evaluatedValue = DataListUtil.GetValueAtIndex(entry, 0, out error);
                    try
                    {
                        _numOfIterations = Convert.ToInt32(DataListUtil.StripBracketsFromValue(evaluatedValue));
                        exeType = enForEachExecutionType.Numeric;
                    }
                    catch(Exception ex)
                    {
                        if(!DataListUtil.isRootVariable(ForEachElementName))
                        {
                            exeType = enForEachExecutionType.GhostService;
                        }
                        else
                        {
                            //Insert the error on the datalist for the user to see
                            //Added for new datalist 
                            allErrors.AddError(ex.Message);
                            //DataObject.XmlData = DisplayAndWriteError("DsfForEachActivity", ex, DataObject.XmlData);
                        }
                    }
                }
                else
                {
                    //Check if the variable exisits in the data list
                    //Added for new datalist 
                    IBinaryDataListEntry entry = compiler.Evaluate(executionId, enActionType.User, elmName, true, out errors);
                    if(errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                    }
                    string evalValue = DataListUtil.GetValueAtIndex(entry, 0, out error);
                    if(!string.IsNullOrEmpty(evalValue))
                    {
                        //Gets the index type of the recordset
                        enRecordsetIndexType recsetIndexType = DataListUtil.GetRecordsetIndexType(ForEachElementName);

                        exeType = enForEachExecutionType.Numeric;

                        if(recsetIndexType == enRecordsetIndexType.Error)
                        {
                            _numOfIterations = 0;
                        }
                        else if(recsetIndexType == enRecordsetIndexType.Numeric)
                        {

                            string recsetIndex = DataListUtil.ExtractIndexRegionFromRecordset(elmName);
                            _numOfIterations = Convert.ToInt32(recsetIndex);
                        }
                        else
                        {
                            //Added for new datalist 

                            //If the recordset index type is star or blank the the number of executions has to be equal to the number of records in the recordset
                            //ScopingObject = DataListFactory.CreateRecordsetScopingObject(DataObject.DataList, DataObject.XmlData);                        
                            _numOfIterations = compiler.GetMaxNumberOfExecutions(executionId, new List<string>() { elmName });
                            //string recsetName = DataListUtil.ExtractRecordsetNameFromValue(DataListUtil.StripBracketsFromValue(elmName));
                            //IRecordsetTO recset = ScopingObject.GetRecordset(recsetName);
                            //if (recsetIndexType == enRecordsetIndexType.Star || recsetIndexType == enRecordsetIndexType.Blank)
                            //{
                            //    //Setting the number of executions to the number of records in the recordset
                            //    _numOfIterations = recset.RecordCount;
                            //}
                            //else
                            //{
                            //    _numOfIterations = 0;
                            //}
                        }
                    }
                    else
                    {
                        exeType = enForEachExecutionType.Scalar;
                        _numOfIterations = 0;
                    }
                }

                if(!string.IsNullOrEmpty(elmName))
                {
                    if(!DataListUtil.isRootVariable(ForEachElementName))
                    {
                        elmName = compiler.Evaluate(dlID, enActionType.User, elmName, true, out errors).FetchScalar().TheValue;
                        if(errors.HasErrors())
                        {
                            allErrors.MergeErrors(errors);
                        }

                        //elmName = Compiler.EvaluateToRootVariable(dataObject.XmlData, dataObject.DataList, elmName);
                    }
                    // some final clean up
                    if(elmName != null)
                    {
                        elmName = elmName.Replace("[[", "").Replace("]]", "");
                        if(elmName.Contains("(") && elmName.Contains(")"))
                        {
                            elmName = elmName.Remove(elmName.IndexOf("("));
                        }
                        ElementName = elmName;
                    }
                }
                //Keeping the original input output mapping to use on each iteration
                var innerActivity = GetInnerActivity();
                if(innerActivity != null)
                {
                    if(innerActivity.InputMapping != null)
                    {
                        _origInput.Set(context, innerActivity.InputMapping);
                    }
                    if(innerActivity.OutputMapping != null)
                    {
                        _origOutput.Set(context, innerActivity.OutputMapping);
                    }
                }

                /* 06.08.2012 - Mo
                 * 
                 * Execute the service for the number of executions that were specified
                 * 
                 */
                if(exeType == enForEachExecutionType.Numeric || exeType == enForEachExecutionType.Scalar)
                {
                    #region Index Execution
                    if(dlID != Guid.Empty)
                    {
                        //Continue while the count is less then the number of iterations
                        if(_count < _numOfIterations)
                        {
                            _count++;
                            if(DataFunc.Handler != null)
                            {
                                innerActivity = GetInnerActivity();
                                //Check if the inner activity is not null and contains input output mapping isnt null
                                if(innerActivity != null && innerActivity.InputMapping != null && innerActivity.OutputMapping != null)
                                {
                                    string inMap = innerActivity.InputMapping;
                                    string outMap = innerActivity.OutputMapping;
                                    if(!string.IsNullOrEmpty(inMap))
                                    {
                                        XmlDocument inxDoc = new XmlDocument();
                                        inxDoc.LoadXml(inMap);
                                        //check the inputs to see if they contain star and inject the right index number
                                        foreach(XmlNode child in inxDoc.FirstChild.ChildNodes)
                                        {
                                            string test = child.Attributes["Source"].Value;
                                            if(test.Contains("(") && test.Contains(")"))
                                            {
                                                int startIndex = test.IndexOf("(");
                                                int endIndex = test.IndexOf(")");
                                                string indexsec = test.Substring(startIndex + 1, endIndex - startIndex - 1);
                                                if(indexsec == "*")
                                                {
                                                    //Replace the star with the index number 
                                                    test = test.Replace("*", _count.ToString());
                                                    child.Attributes["Source"].Value = test;
                                                }
                                            }
                                        }
                                        //Save the original input mapping before replaceing it with the new input mapping
                                        _origInput.Set(context, innerActivity.InputMapping);
                                        innerActivity.InputMapping = inxDoc.OuterXml;
                                    }

                                    if(!string.IsNullOrEmpty(outMap))
                                    {
                                        XmlDocument outxDoc = new XmlDocument();
                                        outxDoc.LoadXml(outMap);
                                        //check the outputs to see if they contain star and inject the right index number
                                        foreach(XmlNode child in outxDoc.FirstChild.ChildNodes)
                                        {
                                            string test = child.Attributes["Value"].Value;
                                            if(test.Contains("(") && test.Contains(")"))
                                            {
                                                int startIndex = test.IndexOf("(");
                                                int endIndex = test.IndexOf(")");
                                                string indexsec = test.Substring(startIndex + 1, endIndex - startIndex - 1);
                                                if(indexsec == "*")
                                                {
                                                    //Replace the star with the index number
                                                    test = test.Replace("*", _count.ToString());
                                                    child.Attributes["Value"].Value = test;
                                                }
                                            }
                                        }
                                        //Save the original output mapping before replaceing it with the new output mapping
                                        _origOutput.Set(context, innerActivity.OutputMapping);
                                        innerActivity.OutputMapping = outxDoc.OuterXml;
                                    }
                                    //Setting the flag to remove unchanged input data before outputing
                                    (DataFunc.Handler as DsfActivity).RemoveInputFromOutput = true;
                                }
                                //Schedule the function/activity again
                                context.ScheduleFunc<string, bool>(DataFunc, string.Empty, IndexExecution);
                            }
                            else
                            {
                                Notify(messageNotification, string.Format("\r\n<ForEachActivity>No child activity found to execute</ForEachActivity>\r\n"));
                            }
                        }
                    }

                    #endregion Index Execution
                }
                else
                {
                    #region GhostService Execution
                    // Travis.Frisinger : 04.07.2012
                    // Variable for dynamic injection
                    string dlFragment = string.Empty;
                    string inputFragment = string.Empty;

                    if(dlID != Guid.Empty)
                    {
                        //var data = binder.FindDataObjectByTagName(datad, elmName);
                        // Fetch data to iterate over....
                        IBinaryDataListEntry data = compiler.Evaluate(executionId, enActionType.User, elmName, false, out errors);
                        if(errors.HasErrors())
                        {
                            allErrors.MergeErrors(errors);
                        }

                        // TODO : Place the 

                        // We need to shape the Ghost DataList to being populated with just the data we require....
                        if(DataFunc.Handler != null)
                        {
                            string comparer = string.Empty;
                            if(innerActivity != null)
                            {
                                _count++;

                                string inMap = "<Inputs/>";
                                string outMap = innerActivity.OutputMapping;

                                try
                                {
                                    _origInput.Set(context, innerActivity.InputMapping);
                                }
                                catch(Exception) { }

                                innerActivity.InputMapping = inMap;
                                PreservedDataList = dataObject.DataList; // save old pre mutated data list
                                try
                                {
                                    _origOutput.Set(context, innerActivity.OutputMapping);
                                }
                                catch(Exception) { }

                                innerActivity.OutputMapping = "<Outputs/>";

                            }

                            // Adjust the DataListID accordingly, remember to wipe out the prev exectionID

                            context.ScheduleFunc<string, bool>(DataFunc, string.Empty, ActivityCompleted);

                        }
                        else
                        {
                            Notify(messageNotification, string.Format("\r\n<ForEachActivity>No child activity found to execute</ForEachActivity>\r\n"));
                        }
                    }
                    #endregion GhostService Execution
                }

            }
            finally
            {
                compiler.Shape(executionId, enDev2ArgumentType.Output, OutputMapping, out errors);
                if(errors.HasErrors())
                {
                    allErrors.MergeErrors(errors);
                }

                // Handle Errors
                if(allErrors.HasErrors())
                {
                    string err = DisplayAndWriteError("DsfForEachActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }

            }
        }

        #endregion Execute

        #region Private Methods

        private void RestoreHandlerFn(NativeActivityContext context)
        {
            var activity = (DataFunc.Handler as DsfActivity);
            activity.InputMapping = _origInput.Get(context);
            activity.OutputMapping = _origOutput.Get(context);
        }

        #region Ghost Service Handler
        private void ActivityCompleted(NativeActivityContext context, ActivityInstance instance, bool result)
        {
            IApplicationMessage messageNotification = context.GetExtension<IApplicationMessage>();
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            results.Add(result);

            if(results.Where(c => !c).Count() == 0)
            {
                Result.Set(context, true);
            }
            else
            {
                Result.Set(context, false);
            }

            IEnumerator<UnlimitedObject> dat = dataTags.Get(context);
            bool more = dat.MoveNext();

            if(more)
            {
                if(DataFunc.Handler != null)
                {
                    var innerActivity = DataFunc.Handler as DsfActivity;
                    if(innerActivity != null)
                    {
                        _count++;
                        string inMap = innerActivity.InputMapping;
                        string outMap = innerActivity.OutputMapping;
                        if(!string.IsNullOrEmpty(inMap))
                        {
                            XmlDocument inxDoc = new XmlDocument();
                            inxDoc.LoadXml(inMap);
                            innerActivity.InputMapping = inxDoc.OuterXml;
                        }
                        if(!string.IsNullOrEmpty(outMap))
                        {
                            XmlDocument outxDoc = new XmlDocument();
                            outxDoc.LoadXml(outMap);
                            innerActivity.OutputMapping = outxDoc.OuterXml;
                        }
                    }

                    // Travis.Frisinger : 07.04.2012 - Must set DataObject not rely upon what is passed in via the context!!!
                    // We must dynamically remove old data and inject the next record
                    string nextPayload = dat.Current.XmlString;
                    nextPayload = nextPayload.Replace("<" + ElementName + ">", "").Replace("</" + ElementName + ">", "");
                    // string newDL = compiler.ReplaceValue(DataObject.XmlData, ElementName, nextPayload);

                    //DataObject.XmlData = newDL;

                    //Notify(messageNotification, string.Format("\r\n<ForEachInput>\r\n{0}\r\n</ForEachInput>\r\n", newDL));

                    context.ScheduleFunc<string, bool>(DataFunc, string.Empty, ActivityCompleted);
                }
                else
                {
                    Notify(messageNotification, string.Format("\r\n<ForEachActivity>No child activity found to execute</ForEachActivity>\r\n"));

                }
            }
            else
            {
                // Travis.Frisinger : 04.07.2012
                // no more data, reset datalist if need be -- This is for a ghost or recursively bound input variable correction step
                if(PreservedDataList != null)
                {
                    dataObject.DataList = PreservedDataList;
                    // We need to scrub the last record from the input stream in this case, else it will be transfered in the mapping ;)
                    //Added for new datalist 
                    ErrorResultTO errors = new ErrorResultTO();
                    dataObject.DataListID = compiler.Shape(dataObject.DataListID, enDev2ArgumentType.Output, OutputMapping, out errors);
                    //string filteredADL = compiler.ShapeOutput(DataObject.XmlData, null, OutputMapping, DataObject.DataList);

                    // now set as the new and adjusted dataList
                    //DataObject.XmlData = filteredADL;
                }

                // 31.07.2012 - Travis 
                // Restore the Design time Handler function
                RestoreHandlerFn(context);
            }
        }
        #endregion

        private void IndexExecution(NativeActivityContext context, ActivityInstance instance, bool result)
        {
            IApplicationMessage messageNotification = context.GetExtension<IApplicationMessage>();
            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            if(_count < _numOfIterations)
            {
                _count++;
                if(DataFunc.Handler != null)
                {
                    var innerActivity = GetInnerActivity();
                    //Check if the inner activity is not null and contains input output mapping isnt null
                    if(innerActivity != null && innerActivity.InputMapping != null && innerActivity.OutputMapping != null)
                    {
                        string inMap = _origInput.Get(context);
                        string outMap = _origOutput.Get(context);
                        if(!string.IsNullOrEmpty(inMap))
                        {
                            XmlDocument inxDoc = new XmlDocument();
                            inxDoc.LoadXml(inMap);
                            //check the inputs to see if they contain star and inject the right index number
                            foreach(XmlNode child in inxDoc.FirstChild.ChildNodes)
                            {
                                string test = child.Attributes["Source"].Value;
                                if(test.Contains("(") && test.Contains(")"))
                                {
                                    int startIndex = test.IndexOf("(");
                                    int endIndex = test.IndexOf(")");
                                    string indexsec = test.Substring(startIndex + 1, endIndex - startIndex - 1);
                                    if(indexsec == "*")
                                    {
                                        //Replace the star with the index number
                                        test = test.Replace("*", _count.ToString());
                                        child.Attributes["Source"].Value = test;
                                    }
                                }
                            }
                            innerActivity.InputMapping = inxDoc.OuterXml;
                        }

                        if(!string.IsNullOrEmpty(outMap))
                        {
                            XmlDocument outxDoc = new XmlDocument();
                            outxDoc.LoadXml(outMap);
                            //check the outputs to see if they contain star and inject the right index number
                            foreach(XmlNode child in outxDoc.FirstChild.ChildNodes)
                            {
                                string test = child.Attributes["Value"].Value;
                                if(test.Contains("(") && test.Contains(")"))
                                {
                                    int startIndex = test.IndexOf("(");
                                    int endIndex = test.IndexOf(")");
                                    string indexsec = test.Substring(startIndex + 1, endIndex - startIndex - 1);
                                    if(indexsec == "*")
                                    {
                                        //Replace the star with the index number
                                        test = test.Replace("*", _count.ToString());
                                        child.Attributes["Value"].Value = test;
                                    }
                                }
                            }
                            innerActivity.OutputMapping = outxDoc.OuterXml;
                        }
                        (DataFunc.Handler as DsfActivity).RemoveInputFromOutput = true;
                    }
                    //Schedule the function/activity again
                    context.ScheduleFunc<string, bool>(DataFunc, string.Empty, IndexExecution);

                }
                else
                {
                    Notify(messageNotification, string.Format("\r\n<ForEachActivity>No child activity found to execute</ForEachActivity>\r\n"));
                }
            }
            else
            {
                // Travis.Frisinger : 04.07.2012
                // no more data, reset datalist if need be -- This is for a ghost or recursively bound input variable correction step
                if(PreservedDataList != null)
                {
                    dataObject.DataList = PreservedDataList;
                    // We need to scrub the last record from the input stream in this case, else it will be transfered in the mapping ;)
                    //Added for new datalist 
                    ErrorResultTO errors = new ErrorResultTO();
                    dataObject.DataListID = compiler.Shape(dataObject.DataListID, enDev2ArgumentType.Output, OutputMapping, out errors);
                    //string filteredADL = compiler.ShapeOutput(DataObject.XmlData, null, OutputMapping, DataObject.DataList);

                    // now set as the new and adjusted dataList
                    //DataObject.XmlData = filteredADL;
                }

                // 31.07.2012 - Travis 
                // Restore the Design time Handler function in the activity and its mappings arnt null
                var innerActivity = GetInnerActivity();
                if(innerActivity != null && innerActivity.InputMapping != null && innerActivity.OutputMapping != null)
                {
                    RestoreHandlerFn(context);
                }
            }
        }

        private DsfActivity GetInnerActivity()
        {
            DsfActivity result = null;

            try
            {
                result = DataFunc.Handler as DsfActivity;
            }
            catch(Exception) { }

            return result;
        }
        #endregion Private Methodss

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
