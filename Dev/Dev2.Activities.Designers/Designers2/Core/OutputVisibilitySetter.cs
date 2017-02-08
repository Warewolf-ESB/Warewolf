using System;
using Dev2.Common.Interfaces.ToolBase;

namespace Dev2.Activities.Designers2.Core
{
    internal static class OutputVisibilitySetter
    {
        internal static void SetGenerateOutputsVisible(IGenerateInputArea inputArea, IGenerateOutputArea outputArea, Action<bool> action, bool value)
        {
            if (value)
            {
                inputArea.IsEnabled = true;
                outputArea.IsEnabled = false;
                action(false);
            }
            else
            {
                inputArea.IsEnabled = false;
                outputArea.IsEnabled = false;
                action(true);
            }

        }
    }
}