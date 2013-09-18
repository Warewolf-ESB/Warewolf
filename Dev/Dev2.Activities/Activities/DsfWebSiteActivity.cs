using System.Activities;
using Dev2;
using System.Collections.Generic;
using Dev2.Activities;
using Dev2.DataList.Contract;
using System;
using Dev2.Common;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{

    public class DsfWebSiteActivity : DsfActivityAbstract<bool>
    {
        DsfMultiAssignActivity _assignActivity;
        ActivityAction _delegate;
        private string _xmlConfig = "<WebsiteConfig/>";
        private string _html = @"";
        private IList<ActivityDTO> fields = new List<ActivityDTO>();

        public string XMLConfiguration
        {
            get
            {
                return _xmlConfig;
            }
            set
            {
                _xmlConfig = value;
            }
        }

        public string Html
        {
            get
            {
                return _html;
            }
            set
            {
                _html = value;
            }
        }

        public DsfWebSiteActivity()
        {
            _delegate = new ActivityAction
            {
                DisplayName = "AssignWebsiteHtml",
            };
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            fields.Add(new ActivityDTO("[[FormView]]", Html, 0));
            _assignActivity = new DsfMultiAssignActivity { OutputMapping = null, FieldsCollection = fields, InputMapping = null };

            metadata.AddChild(_assignActivity);
            metadata.AddDelegate(_delegate);
        }

        protected override void OnExecute(NativeActivityContext context)
        {

            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>(); ;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            Guid dlID = dataObject.DataListID;
            ErrorResultTO errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();
            // Save exeuction DLID for output shaping....
            Guid executionDLID = compiler.Shape(dlID, enDev2ArgumentType.Input, InputMapping, out errors);

            // Process if no errors
            try
            {
                allErrors.MergeErrors(errors);

                // TODO : Load XMLConfiguration into IBinaryDataList for manipulation... Remember to Delete once done!!
                Guid configID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), XMLConfiguration, XMLConfiguration, out errors); // TODO : Worry about translation....
                allErrors.MergeErrors(errors);

                bool isEditing = false;
                string defaultWebpage = string.Empty;
                string dev2WebServer = compiler.EvaluateSystemEntry(executionDLID, enSystemTag.Dev2WebServer, out errors);
                if(errors.HasErrors())
                {
                    allErrors.MergeErrors(errors);
                }
                string metatags = string.Empty;
                string html = Html;

                if(compiler.EvaluateSystemEntry(configID, enSystemTag.DEV2WebsiteEditingMode, out errors) != string.Empty)
                {
                    isEditing = true;
                }

                metatags = compiler.EvaluateSystemEntry(configID, enSystemTag.DEV2MetaTags, out errors);
                // Evalaute and replace Meta Tags
                if(metatags.Length > 0)
                {
                    string metaKeyword = string.Format(@"<meta name=""keywords"" content=""{0}"" />", metatags);
                    html = html.Replace(GlobalConstants.MetaTagsHolder, metaKeyword);
                }
                else
                {
                    html = html.Replace(GlobalConstants.MetaTagsHolder, string.Empty);
                }

                defaultWebpage = compiler.EvaluateSystemEntry(configID, enSystemTag.DEV2DefaultWebpage, out errors);

                // Now delete the configDL
                compiler.ForceDeleteDataListByID(configID);

                if(!isEditing && !string.IsNullOrEmpty(defaultWebpage) && !string.IsNullOrEmpty(dev2WebServer))
                {
                    string webpage = string.Format("{0}/services/{1}", dev2WebServer, defaultWebpage);
                    string payload = string.Format(@"<html><head><META HTTP-EQUIV=""refresh"" content=""0;URL={0}""></head></html>", webpage);
                    compiler.UpsertSystemTag(executionDLID, enSystemTag.FormView, payload, out errors);
                    if(errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);
                    }
                }
                else
                {
                    string payload = html;
                    compiler.UpsertSystemTag(executionDLID, enSystemTag.FormView, payload, out errors);

                    if(errors.HasErrors())
                    {
                        allErrors.MergeErrors(errors);

                    }

                    // Handle Errors
                    if(allErrors.HasErrors())
                    {
                        DisplayAndWriteError("DsfWebpageActivity", allErrors);
                        compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    }
                }

            }
            finally
            {
                Guid shapeID = compiler.Shape(executionDLID, enDev2ArgumentType.Output, OutputMapping, out errors);
                allErrors.MergeErrors(errors);


                // Handle Errors
                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfWebSiteActivity", allErrors);
                    compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }

                // clean up 
                compiler.ForceDeleteDataListByID(executionDLID);

            }
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            throw new NotImplementedException();
        }
    }
}
