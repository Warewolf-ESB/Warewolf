using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows;

namespace Warewolf.Studio.CustomControls
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [ComVisible(true)]
    public class ScriptHelper
    {
        WebBrowserView mExternalWPF;
        public ScriptHelper(WebBrowserView w)
        {
            this.mExternalWPF = w;
        }
        public void InvokeMeFromJavascript(string jsscript)
        {
            //this.mExternalWPF.tbMessageFromBrowser.Text = string.Format("Message :{0}", jsscript);
            var objArray = new Object[1];
            objArray[0] = (Object)"text will be filled in username input field";
            this.mExternalWPF.webBrowser.InvokeScript("function", objArray);// calling java script function(this call is not working when javascript is in some external file)

        }
    }
}