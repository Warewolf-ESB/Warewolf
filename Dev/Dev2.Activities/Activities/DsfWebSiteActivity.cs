using System.Activities;
using Dev2;
using System.Collections.Generic;

using Dev2.DataList.Contract;
using System;
using Dev2.Common;
using Unlimited.Framework;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {

    public class DsfWebSiteActivity : DsfActivityAbstract<bool>{
        DsfMultiAssignActivity _assignActivity;
        ActivityAction _delegate;
        private string _xmlConfig = "<WebsiteConfig/>";
        private string _html = @"";
        private IList<ActivityDTO> fields = new List<ActivityDTO>();

        public string XMLConfiguration {
            get {
                return _xmlConfig;
            }
            set {
                _xmlConfig = value;
            }
        }

        public string Html {
            get {
                return _html;
            }
            set {
                _html = value;
            }
        }

        public DsfWebSiteActivity() {
            _delegate = new ActivityAction {
                DisplayName = "AssignWebsiteHtml",
            };
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata) {
            base.CacheMetadata(metadata);
            fields.Add(new ActivityDTO("[[FormView]]", Html, 0));
            _assignActivity = new DsfMultiAssignActivity { OutputMapping = null, FieldsCollection = fields, InputMapping = null};

            // FieldName = "FormView", FieldValue = Html, Add = true };
            metadata.AddChild(_assignActivity);
            metadata.AddDelegate(_delegate);
        }

        protected override void OnExecute(NativeActivityContext context) {

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>(); ;
            IDataListBinder binder = context.GetExtension<IDataListBinder>();

            // 2012.11.05 : Travis.Frisinger - Added for Binary DataList -- Shape Input
            //IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            Guid dlID = dataObject.DataListID;
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            // Save exeuction DLID for output shaping....
            Guid executionDLID = compiler.Shape(dlID, enDev2ArgumentType.Input, InputMapping, out errors);

            // Process if no errors
            try {
                // TODO : Fill with execution logic
                //IList<string> ambientData = new List<string> { dataObject.XmlData };

                //dynamic data = binder.DataListToUnlimitedObject(ambientData);

                // Upsert the FormView tag into the dataList structure
                //dataObject.DataList = Compiler.UpsertSystemTagIntoDataList(dataObject.DataList, enSystemTag.FormView);
                //compiler.UpsertSystemTag(executionDLID, enSystemTag.FormView, string.Empty, out errors);
                if (errors.HasErrors()) {
                    allErrors.MergeErrors(errors);
                }

                // TODO : Load XMLConfiguration into IBinaryDataList for manipulation... Remember to Delete once done!!
                Guid configID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), XMLConfiguration, XMLConfiguration, out errors); // TODO : Worry about translation....

                if (errors.HasErrors()) {
                    allErrors.MergeErrors(errors);
                }

                //var configData = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(XMLConfiguration);

                bool isEditing = false;
                string defaultWebpage = string.Empty;
                string dev2WebServer = compiler.EvaluateSystemEntry(executionDLID, enSystemTag.Dev2WebServer, out errors);
                if (errors.HasErrors()) {
                    allErrors.MergeErrors(errors);
                }
                string metatags = string.Empty;
                string html = Html;

                if (compiler.EvaluateSystemEntry(configID, enSystemTag.DEV2WebsiteEditingMode, out errors) != string.Empty) {
                    isEditing = true;
                }

                metatags = compiler.EvaluateSystemEntry(configID, enSystemTag.DEV2MetaTags, out errors);
                // Evalaute and replace Meta Tags
                if (metatags.Length > 0) {
                    string metaKeyword = string.Format(@"<meta name=""keywords"" content=""{0}"" />", metatags);
                    html = html.Replace(GlobalConstants.MetaTagsHolder, metaKeyword);
                } else {
                    html = html.Replace(GlobalConstants.MetaTagsHolder, string.Empty);
                }

                defaultWebpage = compiler.EvaluateSystemEntry(configID, enSystemTag.DEV2DefaultWebpage, out errors);

                // Now delete the configDL
                compiler.ForceDeleteDataListByID(configID);

                if (!isEditing && !string.IsNullOrEmpty(defaultWebpage) && !string.IsNullOrEmpty(dev2WebServer)) {
                    string webpage = string.Format("{0}/services/{1}", dev2WebServer, defaultWebpage);
                    string payload = string.Format(@"<html><head><META HTTP-EQUIV=""refresh"" content=""0;URL={0}""></head></html>", webpage);
                    compiler.UpsertSystemTag(executionDLID, enSystemTag.FormView, payload, out errors);
                    if (errors.HasErrors()) {
                        allErrors.MergeErrors(errors);
                    }

                    //fields.Clear();
                    //fields.Add(new ActivityDTO(frmView, payload, 0));

                    //_assignActivity.FieldValue = string.Format(@"<html><head><META HTTP-EQUIV=""refresh"" content=""0;URL={0}""></head></html>", webpage);
                } else {
                    //var htmlResponse = binder.ParseHTML(html, XMLConfiguration, context.GetExtension<IFrameworkDataChannel>(), dataObject);
                    //string payload = htmlResponse;
                    string payload = html;
                    compiler.UpsertSystemTag(executionDLID, enSystemTag.FormView, payload, out errors);

                    if (errors.HasErrors()) {
                        allErrors.MergeErrors(errors);

                    }

                    // Handle Errors
                    if (allErrors.HasErrors()) {
                        string err = DisplayAndWriteError("DsfWebpageActivity", allErrors);
                        compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                    }



                    //fields.Clear();
                    //fields.Add(new ActivityDTO("[[FormView]]", payload, 0));
                    //_assignActivity.FieldValue = htmlResponse;
                }


                //context.ScheduleActivity(_assignActivity);
            } finally {
                Guid shapeID = compiler.Shape(executionDLID, enDev2ArgumentType.Output, OutputMapping, out errors);
                if (errors.HasErrors()) {
                    allErrors.MergeErrors(errors);
                }

                // Handle Errors
                if (allErrors.HasErrors()) {
                    string err = DisplayAndWriteError("DsfWebSiteActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, err, out errors);
                }

                // clean up 
                compiler.ForceDeleteDataListByID(executionDLID);

            }  
        }

        //private string InsertKeywords(string metatags) {
        //    string returnValue = Html;

        //    if (!string.IsNullOrEmpty(Html)) {
        //        try {
        //            var data = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(Html);

        //            var head = data.head;

        //            if (head is UnlimitedObject) {

        //                    string metaKeyword = string.Format(@"<meta name=""keywords"" content=""{0}"" />", metatags);

        //                    head.Add(UnlimitedObject.GetStringXmlDataAsUnlimitedObject(metaKeyword));

        //                    returnValue = data.XmlString;

        //            }

        //        }
        //        catch { }
                

        //    }

        //    return returnValue;
        //}

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
