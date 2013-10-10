using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.CodedUI.Tests;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;

namespace Dev2.Studio.UI.Tests.UIMaps
{
    class WizardsUIMap
    {
        /// <summary>
        /// Returns true if found in the timeout period.
        /// </summary>
        public static bool WaitForWizard(int timeOut)
        {
            var uiBusinessDesignStudioWindow = new UIBusinessDesignStudioWindow();
            Type type = null;
            var timeNow = 0;
            while(type != typeof(WpfImage))
            {
                timeNow = timeNow + 100;
                Playback.Wait(100);
                var tryGetDialog = uiBusinessDesignStudioWindow.GetChildren()[0].GetChildren()[0];
                type = tryGetDialog.GetType();
                if(timeNow > timeOut)
                {
                    break;
                }
            }
            //wait for render
            Playback.Wait(2000);
            return type == typeof(WpfImage);
        }
    }
}
