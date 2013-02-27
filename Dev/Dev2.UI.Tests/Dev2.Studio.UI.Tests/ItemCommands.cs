using Microsoft.VisualStudio.TestTools.UITesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dev2.Studio.UI.Tests
{
    public static class KeyboardCommands
    {

        #region Properties

        public static string TabCommand { get { return "{TAB}"; } }
        public static string ShiftCommand { get { return "{SHIFT}"; } }
        public static string CopyCommand { get { return "^c"; } }
        public static string BackspaceCommand { get { return "{BACKSPACE}"; } }
        public static string EnterCommand { get { return "{ENTER}"; } }


        #endregion Properties

        #region Methods

        public static void SelectAllText(UITestControl editBlock)
        {
            SendKeys.SendWait("{HOME}");
            SendKeys.SendWait("{END} + {SHIFT}");
        }

        #region Methods
    }
}
