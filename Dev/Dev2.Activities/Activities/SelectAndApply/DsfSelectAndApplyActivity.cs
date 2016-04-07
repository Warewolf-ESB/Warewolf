using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Toolbox;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;

namespace Dev2.Activities.SelectAndApply
{
      [ToolDescriptorInfo("", "Select and apply", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090D8C8FA3E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Storage", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfSelectAndApplyActivity:DsfActivityAbstract<bool>
    {
          public DsfSelectAndApplyActivity()
          {
              DisplayName = "Select and apply";
          }
        #region Overrides of DsfNativeActivity<bool>

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return null;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return null;
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
        }

        #endregion
    }
}
