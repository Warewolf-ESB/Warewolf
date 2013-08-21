using System;

namespace Dev2.Studio.Core.Helpers
{
    public interface IVersionChecker
    {
        Version Latest { get; }
        Version Current { get; }
        string StartPageUri { get; }
    }
}