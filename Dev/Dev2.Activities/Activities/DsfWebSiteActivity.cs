using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
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

            throw new NotImplementedException("WebSite");
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
