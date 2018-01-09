using System;
using System.Text;

namespace Dev2.Common.Interfaces
{
    public interface IResourceDefinationCleaner
    {
        StringBuilder GetResourceDefinition(bool prepairForDeployment, Guid resourceId, StringBuilder contents);
        StringBuilder DecryptAllPasswords(StringBuilder stringBuilder);
    }
}