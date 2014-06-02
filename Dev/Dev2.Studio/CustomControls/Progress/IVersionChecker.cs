using System;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.Helpers
// ReSharper restore CheckNamespace
{
    public interface IVersionChecker
    {
        Version Latest { get; }
        Version Current { get; }
        string StartPageUri { get; }
        string LatestVersionCheckSum { get; }
    }
}