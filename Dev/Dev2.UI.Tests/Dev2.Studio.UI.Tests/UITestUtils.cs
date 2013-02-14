using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev2.Studio.UI.Tests
{
    public static class UITestUtils
    {
        public static string GetStudioWindowName()
        {
            return "Business Design Studio (DEV2\\" + Environment.UserName + ")";
        }
    }
}
