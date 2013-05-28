using System;
using System.Collections.Generic;
using System.Activities;
using Dev2;


namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public class TransformActivity : DsfActivityAbstract<bool> {

        public string Transformation { get; set; }
        public string TransformElementName { get; set; }
        public bool Aggregate { get; set; }
        public string RootTag { get; set; }
        public bool RemoveSourceTagsAfterTransformation { get; set; }


        public TransformActivity() : base() {}

        protected override void CacheMetadata(NativeActivityMetadata metadata) {
            base.CacheMetadata(metadata);
            //metadata.AddDelegate(_delegate);
        }



        protected override void OnExecute(NativeActivityContext context) {
            //IApplicationMessage messageNotification = context.GetExtension<IApplicationMessage>();
            //IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            //List<string> datad = AmbientDataList.Get(context) as List<string>;
            //IDataListBinder binder = context.GetExtension<IDataListBinder>();
            //IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();

            //// -------[ Start Input Shape Region ]------- 

            //    // Set Pre-execute ADL for comparison
            //    //_dlFilterActivity.PreExecuteADL = dataObject.XmlData;
            //    //// Set output mapping as per Property from Abstract base
            //    //_dlFilterActivity.OutputMapping = OutputMapping;

            //    string preExecuteADL = dataObject.XmlData;

            //    // Shape Input to what is exepected
            //    // Do this by shapping current ADL
            //    try {
            //        dataObject.XmlData = compiler.ShapeInput(dataObject.XmlData, InputMapping, dataObject.DataList);
            //    }
            //    catch (Exception) { }

            //// -------[ End Input Shape Region ]------- 

            //if (datad == null) {
            //    this.ParentWorkflowInstanceId = dataObject.ParentWorkflowInstanceId;

            //    if ((dataObject != null) && !string.IsNullOrEmpty(dataObject.XmlData)) {
            //        datad = new List<string>() { dataObject.XmlData };
            //    }
            //    else {
            //        datad = new List<string>();
            //    }
            //}

            //if (!string.IsNullOrEmpty(ExplicitDataList.Get(context))) {
            //    isExplicitDataListBound = true;
            //    datad = new List<string>() { ExplicitDataList.Get(context) };
            //}

            //if (!string.IsNullOrEmpty(TransformElementName)) {
            //    TransformElementName = binder.TextAndJScriptRegionEvaluator(AmbientDataList.Get(context), TransformElementName, string.Empty, DatabindRecursive, dataObject != null ? dataObject.ServiceName : string.Empty);
            //}

            //if (!string.IsNullOrEmpty(RootTag)) {
            //    RootTag = binder.TextAndJScriptRegionEvaluator(AmbientDataList.Get(context), RootTag, string.Empty, DatabindRecursive, dataObject != null ? dataObject.ServiceName : string.Empty);
            //}

            
            //var dataObj = new UnlimitedObject();
            //if (!string.IsNullOrEmpty(RootTag)) {
            //    dataObj = new UnlimitedObject(RootTag);
            //}

            //if (!string.IsNullOrEmpty(TransformElementName)) {
            //    var data = binder.FindDataObjectByTagName(datad, TransformElementName);
            //    IEnumerator<UnlimitedObject> enu = data.GetEnumerator();

            //    var list = binder.DataListToUnlimitedObject(datad);

            //    bool more = enu.MoveNext();

            //    while (more) {
            //        list.RemoveElementsByTagName(TransformElementName);
            //        list.Add(enu.Current);
            //        string output = string.Empty;
            //        if (!string.IsNullOrEmpty(Transformation)) {
            //            output = binder.TextAndJScriptRegionEvaluator(new List<string> { list.XmlString }, Transformation, "", DatabindRecursive, dataObject != null ? dataObject.ServiceName : string.Empty);
            //        }
            //        dataObj.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(output));
            //        more = enu.MoveNext();
            //    }


            //}

            //var ambientData = AmbientDataList.Get(context);
            //ambientData.Add(dataObj.XmlString);
            //AmbientDataList.Set(context, ambientData);

            //var resultDataObj = binder.DataListToUnlimitedObject(ambientData);

            //if (dataObject != null) {
            //    dataObject.XmlData = resultDataObj.XmlString;
            //}

            //var list = ActivityHelper.DataListToUnlimitedObject(AmbientDataList.Get(context));
            //var dataObj = new UnlimitedObject();
            //if (!string.IsNullOrEmpty(RootTag)) {
            //    dataObj = new UnlimitedObject(RootTag);

            //}
            //The user wants to create 1 or more xml nodes by matching the occurence
            //of a specific tag where (n times to execute the transformation = n occurrences of a tag (TranformationElementName property)
            //if (!Aggregate) {
            //    if (!string.IsNullOrEmpty(TransformElementName)) {
            //        var items = list.GetAllElements(TransformElementName);
                    

            ////    string newADL = compiler.ShapeOutput(dataObject.XmlData, preExecuteADL, OutputMapping, dataObject.DataList);
            ////    // now set as the new and adjusted dataList
            ////    AmbientDataList.Set(context, new List<string> { newADL });
            ////    dataObject.XmlData = newADL;

            //// -------[ End Post Execute Cleanup ]--------

            //        foreach (var item in items) {
            //            string trnsfrm = Transform(context, new List<string> { item.XmlString });
            //            dataObj.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(trnsfrm));
            //        }

                    
            //    }
            //}
            //else {
            //    //User wants a single XML node containing the result of the transformation
            //    //by using the entire data context in the workflow as a data source
            //    //if (string.IsNullOrEmpty(TransformElementName)) {
            //    //    dataObj.AddResponse(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(Transform(context, new List<string> { list.XmlString }))); ;
            //    //}
            //    //else {
            //    //    //User wants a single XML node containing the result of the transformation
            //    //    //by using only occurrences of a specific tag and its contents as a data source
            //    //    var items = list.GetAllElements(TransformElementName);
            //    //    var item = UnlimitedObject.GetUnlimitedObjectFromUnlimitedObjects(items);
            //    //    UnlimitedObject.GetStringXmlDataAsUnlimitedObject(Transform(context, new List<string> { item.XmlString }));
            //    //}
            //}

            //AmbientDataList.Get(context).Add(dataObj.XmlString);
        }

        /*private string Transform(NativeActivityContext context, List<string> dataSource, IDataListBinder binder) {
            var returnString = string.Empty;
            if (!string.IsNullOrEmpty(Transformation)) {

                var dataObject = context.GetExtension<IDSFDataObject>();

                string eval = ActivityHelper.TextAndJScriptRegionEvaluator(dataSource, Transformation);
                if (!string.IsNullOrEmpty(eval)) {
                    //AmbientDataList.Get(context).Add(eval);
                    returnString = eval;
                }
            }

            return returnString;
        }*/
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
