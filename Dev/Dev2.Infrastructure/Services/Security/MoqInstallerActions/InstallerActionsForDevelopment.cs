/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Security.Principal;

namespace Dev2.Services.Security.MoqInstallerActions
{
    public class InstallerActionsForDevelopment : IMoqInstallerActions
    {
        #region Implementation of MoqInstallerActions

        public void ExecuteMoqInstallerActions()
        {
            IWarewolfSecurityOperations wso = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            if (!wso.DoesWarewolfGroupExist())
            {
                wso.AddWarewolfGroup();
            }
            else
            {
                wso.DeleteWarewolfGroup();
                wso.AddWarewolfGroup();
            }

            CreateWarewolfGroupAndAddCurrentUser();
            AddAdministratorsToWarewolfGroup();
        }

        #endregion

        #region Private Actions

        private void AddAdministratorsToWarewolfGroup()
        {
            IWarewolfSecurityOperations wso = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            wso.AddAdministratorsGroupToWarewolf();
        }

        /// <summary>
        ///     Creates the warewolf group and adds the current user.
        /// </summary>
        private void CreateWarewolfGroupAndAddCurrentUser()
        {
            IWarewolfSecurityOperations wso = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            // Get the current executing user ;)
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent(false);
            string machineName = Environment.MachineName;
            string userAddString = wso.FormatUserForInsert(currentUser.Name, machineName);

            wso.AddUserToWarewolf(userAddString);
        }

        #endregion
    }
}