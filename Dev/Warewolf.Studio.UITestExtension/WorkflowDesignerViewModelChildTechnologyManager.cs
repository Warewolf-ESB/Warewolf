using System;
using System.Collections;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UITest.Extension;

namespace Warewolf.Studio.UITestExtension
{
    [ComVisible(true)]
    internal class WorkflowDesignSurfaceChildTechnologyManager : UITechnologyManager
    {
        public override IUITechnologyElement GetParent(IUITechnologyElement element)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator GetChildren(IUITechnologyElement element, object parsedQueryIdCookie)
        {
            throw new NotImplementedException();
        }

        public override IUITechnologyElement GetElementFromPoint(int pointX, int pointY)
        {
            throw new NotImplementedException();
        }

        public override IUITechnologyElement GetNextSibling(IUITechnologyElement element)
        {
            throw new NotImplementedException();
        }

        public override IUITechnologyElement GetPreviousSibling(IUITechnologyElement element)
        {
            throw new NotImplementedException();
        }

        public override IUITechnologyElement GetFocusedElement(IntPtr handle)
        {
            throw new NotImplementedException();
        }

        public override bool AddEventHandler(IUITechnologyElement element, UITestEventType eventType, IUITestEventNotify eventSink)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveEventHandler(IUITechnologyElement element, UITestEventType eventType, IUITestEventNotify eventSink)
        {
            throw new NotImplementedException();
        }

        public override IUISynchronizationWaiter GetSynchronizationWaiter(IUITechnologyElement element, UITestEventType eventType)
        {
            throw new NotImplementedException();
        }

        public override object[] Search(object parsedQueryIdCookie, IUITechnologyElement parentElement, int maxDepth)
        {
            throw new NotImplementedException();
        }

        public override string ParseQueryId(string queryElement, out object parsedQueryIdCookie)
        {
            throw new NotImplementedException();
        }

        public override bool MatchElement(IUITechnologyElement element, object parsedQueryIdCookie, out bool useEngine)
        {
            throw new NotImplementedException();
        }

        public override ILastInvocationInfo GetLastInvocationInfo()
        {
            throw new NotImplementedException();
        }

        public override void CancelStep()
        {
            throw new NotImplementedException();
        }

        public override IUITechnologyElement ConvertToThisTechnology(IUITechnologyElement elementToConvert, out int supportLevel)
        {
            throw new NotImplementedException();
        }

        public override int GetControlSupportLevel(IntPtr windowHandle)
        {
            throw new NotImplementedException();
        }

        public override string TechnologyName { get; }

        public override bool AddGlobalEventHandler(UITestEventType eventType, IUITestEventNotify eventSink)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveGlobalEventHandler(UITestEventType eventType, IUITestEventNotify eventSink)
        {
            throw new NotImplementedException();
        }

        public override void ProcessMouseEnter(IntPtr handle)
        {
            throw new NotImplementedException();
        }

        public override void StartSession(bool recordingSession)
        {
            throw new NotImplementedException();
        }

        public override void StopSession()
        {
            throw new NotImplementedException();
        }

        public override IUITechnologyElement GetElementFromNativeElement(object nativeElement)
        {
            throw new NotImplementedException();
        }

        public override IUITechnologyElement GetElementFromWindowHandle(IntPtr handle)
        {
            throw new NotImplementedException();
        }
    }
}
