using System;
using Dev2.Diagnostics;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    public class DebugCopyException : Exception
    {
        public DebugItem Item { get; set; }

        public DebugCopyException(string msg, DebugItem item) : base(msg)
        {
            Item = item;

        }
    }
}