using System;

namespace Dev2.Runtime.Hosting
{
    public interface IResourceIterator
    {
        void IterateAll(Guid workspaceID, Func<ResourceIteratorResult, bool> action, params ResourceDelimiter[] delimiters);
        void Iterate(string resourcePath, Guid workspaceID, Func<ResourceIteratorResult, bool> action, params ResourceDelimiter[] delimiters);
    }
}