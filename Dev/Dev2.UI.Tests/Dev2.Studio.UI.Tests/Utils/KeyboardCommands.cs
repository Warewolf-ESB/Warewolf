using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UITesting;

namespace Dev2.Studio.UI.Tests
{
    public static class KeyboardCommands
    {

        #region Properties

        public static string SpaceCommand { get { return "{SPACE}"; } }
        public static string DelCommand { get { return "{DEL}"; } }
        public static string EscCommand { get { return "{ESC}"; } }
        public static string TabCommand { get { return "{TAB}"; } }
        public static string ShiftCommand { get { return "{SHIFT}"; } }
        public static string SelectAllCommand { get { return "^a"; } }
        public static string CopyCommand { get { return "^c"; } }
        public static string BackspaceCommand { get { return "{BACKSPACE}"; } }
        public static string EnterCommand { get { return "{ENTER}"; } }
        public static string Debug { get { return "{F5}"; } }
        public static string QuickDebug { get { return "{F6}"; } }


        #endregion Properties

        #region Methods

        public static void SelectAllText(UITestControl editBlock)
        {
            SendKeys.SendWait("{HOME}");
            SendKeys.SendWait("{END} + {SHIFT}");
        }

        public static void SendKey(string key, int waitAmt = 15)
        {
            Keyboard.SendKeys(key);
            Playback.Wait(waitAmt);
        }

        public static void SendTabs(int amtOfTabs, int waitAmt = 15)
        {
            for(var i = 0; i < amtOfTabs; i++)
            {
                SendKey(TabCommand, waitAmt);
            }
        }

        public static void SendTab(int waitAmt = 15)
        {
            SendKey(TabCommand, waitAmt);
        }

        public static void SendEnter(int waitAmt = 15)
        {
            SendKey(EnterCommand, waitAmt);
        }

        public static void SendEsc(int waitAmt = 15)
        {
            SendKey(EscCommand, waitAmt);
        }

        public static void SendDel(int waitAmt = 15)
        {
            SendKey(DelCommand, waitAmt);
        }

        public static void SendSpace(int waitAmt = 15)
        {
            SendKey(SpaceCommand, waitAmt);
        }

        public static void SelectAndCopy()
        {
            SendKey(SelectAllCommand, 5);
            SendKey(CopyCommand, 5);
        }

        #endregion Methods
    }
}
