using System;

namespace Warewolf.Studio.Core
{
    public interface IWebBrowser
    {
        void Navigate(Uri uri);

        event Action<Uri> Navigated;
    }
}