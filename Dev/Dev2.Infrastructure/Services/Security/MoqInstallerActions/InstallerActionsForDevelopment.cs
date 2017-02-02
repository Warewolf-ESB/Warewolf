/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Dev2.Services.Security.MoqInstallerActions
{
    public class InstallerActionsForDevelopment : IMoqInstallerActions
    {

        #region Implementation of MoqInstallerActions

        public void ExecuteMoqInstallerActions()
        {
            IWarewolfSecurityOperations wso = MoqInstallerActionFactory.CreateSecurityOperationsObject();

            if(!wso.DoesWarewolfGroupExist())
            {
                wso.AddWarewolfGroup();
            }            
            AddAdministratorsToWarewolfGroup();
        }

        #endregion

        #region Private Actions

        private void AddAdministratorsToWarewolfGroup()
        {
            IWarewolfSecurityOperations wso = MoqInstallerActionFactory.CreateSecurityOperationsObject();
            wso.AddAdministratorsGroupToWarewolf();
        }

        #endregion
    }
}
