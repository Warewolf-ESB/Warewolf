namespace Dev2.CodedUI.Tests.UIMaps.ConnectViewUIMapClasses
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Input;
    using System.CodeDom.Compiler;
    using System.Text.RegularExpressions;
    using Microsoft.VisualStudio.TestTools.UITest.Extension;
    using Microsoft.VisualStudio.TestTools.UITesting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Keyboard = Microsoft.VisualStudio.TestTools.UITesting.Keyboard;
    using Mouse = Microsoft.VisualStudio.TestTools.UITesting.Mouse;
    using MouseButtons = System.Windows.Forms.MouseButtons;
    
    
    public partial class ConnectViewUIMap
    {

        public void InputServerName(string serverName) {
            EnterServerName(serverName);
        }

        public void InputServerAddress(string serverAddress) {
            EnterServerAddress(serverAddress);
        }

        public void ClickConnectButton() {
            ConnectBtnClick();
        }
    }
}
