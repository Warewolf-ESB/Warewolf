using System;
using System.Collections.Generic;
using System.Activities;
using Microsoft.VisualBasic.Activities;
using Dev2.DataList.Contract.Binary_Objects;


namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public class InjectingToDataList : DsfActivityAbstract<bool> {

        public string ServiceHost { get; set; }


        public InjectingToDataList() {
            AmbientDataList = new VisualBasicReference<List<string>> { ExpressionText = "AmbientDataList" };
        }

        public void InjectToDataList(string eval, string FieldName, NativeActivityContext context) {

            //IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();            
            //IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            //IDataListBinder binder = context.GetExtension<IDataListBinder>();

            //Stopwatch s = new Stopwatch();
            //Stopwatch overall = new Stopwatch();

            //// -------[ Start Input Shape Region ]------- 
            //string preExecute = dataObject.XmlData;

            //// Shape Input to what is exepected
            //// Do this by shapping current ADL
            //dataObject.XmlData = compiler.ShapeInput(dataObject.XmlData, InputMapping, dataObject.DataList);

            //// -------[ End Input Shape Region ]-------

            //string message = string.Empty;

            //if ((dataObject != null) && !string.IsNullOrEmpty(dataObject.XmlData)) {
            //    if (AmbientDataList.Get(context) == null) {
            //        AmbientDataList.Set(context, new List<string>() { dataObject.XmlData });
            //    }
            //    dataObject.WorkflowInstanceId = context.WorkflowInstanceId.ToString();
            //}
            //else {
            //    if (AmbientDataList.Get(context) == null) {
            //        AmbientDataList.Set(context, new List<string>());
            //    }
            //}

            //IApplicationMessage messageNotification = context.GetExtension<IApplicationMessage>();



            //string NameCollection = string.Empty;
            //string ValueCollection = string.Empty;
            //string bookmarkName = Guid.NewGuid().ToString();


            //s.Start();
            //if (eval.Contains("@")) {
            //    if (dataObject != null) {
            //        eval = eval.Replace("@Service", dataObject.ServiceName).Replace("@Instance", context.WorkflowInstanceId.ToString()).Replace("@Bookmark", bookmarkName);
            //        Uri hostUri = null;
            //        if (Uri.TryCreate(ServiceHost, UriKind.Absolute, out hostUri)) {
            //            eval = eval.Replace("@Host", ServiceHost);
            //        }

            //        eval = binder.BindEnvironmentVariables(eval, dataObject.ServiceName);

            //    }
            //}
            //if (eval.Contains("[[") || eval.Contains("{{")) {

            //    //eval = binder.TextAndJScriptRegionEvaluator(new List<string> { ExplicitDataList.Get(context) }, eval, "", DatabindRecursive, dataObject != null ? dataObject.ServiceName : string.Empty);
            //    // 


            //    eval = binder.TextAndJScriptRegionEvaluator(new List<string> { dataObject.XmlData, compiler.ExtractSystemTagRegion(preExecute, dataObject.DataList) }, eval, "", DatabindRecursive, dataObject != null ? dataObject.ServiceName : string.Empty);
            //    NameCollection = FieldName;
            //    ValueCollection = eval;
            //}
            //else {
            //    NameCollection = FieldName;
            //    ValueCollection = eval;
            //}

            //// Remove due to DL update
            ////var dataListObj = binder.DataListToUnlimitedObject(AmbientDataList.Get(context));

            //var dataListObj = binder.DataListToUnlimitedObject(new List<string> { dataObject.XmlData });        

            //dataListObj.RemoveElementsByTagName("BDSDebugMode");


            //// replace below 

            ////if (Add) {
            ////    dataListObj.CreateElement(FieldName).SetValue(eval);
            ////}
            ////else {

            ////    dataListObj.GetElement(FieldName).SetValue(eval);

            ////}


            ////if (UpdateAllOccurrences) {

            ////    dataListObj.SetValueOfAll(FieldName, eval);
            ////}

            ////Notify(messageNotification, string.Format("\r\n<Assign  Field=\"{0}\">\r\n<AssignValue>\r\n\t{1}\r\n</AssignValue>\r\n</Assign>\r\n", FieldName, eval));
            ////We are removing the ambient data list as we need to update it with new data

            //s.Start();

            //// why???
            //AmbientDataList.Set(context, new List<string> { dataListObj.XmlString });


            //s.Stop();
            //message = string.Format("{0}: Reset Ambient Data List", s.ElapsedMilliseconds);
            //TraceWriter.WriteTrace(message);
            //s.Reset();

            ////AmbientDataList.Get(context).Clear();
            ////We are creating a new ambient data list item that contains all previous items in a single string.
            ////AmbientDataList.Get(context).Add(dataListObj.XmlString);



            //if (dataObject != null) {

            //    dataObject.XmlData = DataListFactory.CreateDataListCompiler().StripCrap(dataListObj.XmlString);

            //    s.Start();
            //    if (string.IsNullOrEmpty(ParentServiceName)) {
            //        ParentServiceName = dataObject.ParentServiceName;
            //    }
            //    s.Stop();
            //    message = string.Format("{0}: Update Parent Service Name", s.ElapsedMilliseconds);
            //    TraceWriter.WriteTrace(message);
            //    s.Reset();

            //    s.Start();
            //    if (string.IsNullOrEmpty(ParentWorkflowInstanceId)) {
            //        ParentWorkflowInstanceId = dataObject.ParentWorkflowInstanceId;
            //    }
            //    s.Stop();
            //    message = string.Format("{0}: Update Parent WorkFlow Instance Id", s.ElapsedMilliseconds);
            //    TraceWriter.WriteTrace(message);
            //    s.Reset();
            //}

            //// -------[ Start Post Execute Cleanup ]--------

            //// Execute ADL filter activity, will get post execute ADL from activity and set to clean
            ////context.ScheduleActivity(_dlFilterActivity);

            //string filteredADL = compiler.ShapeOutput(dataObject.XmlData, preExecute, OutputMapping, dataObject.DataList);

            //// now set as the new and adjusted dataList
            //AmbientDataList.Set(context, new List<string> { filteredADL });
            //dataObject.XmlData = filteredADL;

            //// -------[ End Post Execute Cleanup ]--------

            ////if (CreateBookmark && dataObject != null && !_IsDebug) {
            ////    dataObject.CurrentBookmarkName = bookmarkName;
            ////    context.CreateBookmark(bookmarkName, Resumed);

            ////    DataListUtil.ConditionalMerge(compiler, DataListMergeFrequency.Always | DataListMergeFrequency.OnBookmark,
            ////    dataObject.DatalistMergeID, dataObject.DataListID, dataObject.DatalistMergeFrequency, dataObject.DatalistMergeType, dataObject.DatalistMergeDepth);
            //      ExecutionStatusCallbackDispatcher.Instance.Post(dataObject.BookmarkExecutionCallbackID, ExecutionStatusCallbackMessageType.BookmarkedCallback);
            ////}

            //overall.Stop();

            //message = string.Format("\r\n\r\n<Perf>Assigning '{0}' Variables took {1} millisecs to execute</Perf>\r\n\r\n", NameCollection, overall.ElapsedMilliseconds.ToString());
            //Notify(messageNotification, message);

            ////message = string.Format("{2}: End Assign Name='{0}' Val=[{1}]", FieldName, FieldValue ?? "[NULL]]", overall.ElapsedMilliseconds.ToString());

            //TraceWriter.WriteTrace(message);
        }

        //public override void Notify(IApplicationMessage messageNotifier, string message) {
        //    if (messageNotifier != null && !string.IsNullOrEmpty(message)) {
        //        messageNotifier.SendMessage(message);
        //    }
        //}

        protected override void OnExecute(NativeActivityContext test) {

        }

        #region Overridden ActivityAbstact Methods

        public override IBinaryDataList GetInputs()
        {
            return null;
        }

        public override IBinaryDataList GetOutputs()
        {
            return null;
        }

        #endregion Overridden ActivityAbstact Methods

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }
    }
}
