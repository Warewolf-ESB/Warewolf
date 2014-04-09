using System;

namespace Dev2.Scheduler.Interfaces
{
    public interface ISecurityWrapper : IDisposable
    {
        /// <summary>
        /// Reads the user accounts which have the specific privilege
        /// </summary>
        /// <param name="privilege">The name of the privilege for which the accounts with this right should be enumerated</param>
        /// <param name="userName"></param>
        bool IsWindowsAuthorised(string privilege, string userName);
        /// <summary>
        /// Checks if the user has warewolf permissions for a resource guid
        /// </summary>
        /// <param name="privilege"></param>
        /// <param name="userName"></param>
        /// <param name="resourceGuid"></param>
        /// <returns></returns>
        bool IsWarewolfAuthorised(string privilege, string userName, string resourceGuid);
    }
}