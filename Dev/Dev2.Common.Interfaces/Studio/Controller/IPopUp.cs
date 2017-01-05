/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// ReSharper disable CheckNamespace

using System.Collections.Generic;
using System.Windows;
using Dev2.Common.Interfaces.PopupController;

namespace Dev2.Common.Interfaces.Studio.Controller
// ReSharper restore CheckNamespace
{
    public interface IPopupController
    {
        string Header { get; set; }
        string Description { get; set; }
        string Question { get; set; }
        MessageBoxImage ImageType { get; set; }
        MessageBoxButton Buttons { get; set; }
        string DontShowAgainKey { get; set; }
        bool DeleteAnyway { get; }
        bool ApplyToAll { get; }
        MessageBoxResult Show(IPopupMessage popupMessage);
        MessageBoxResult Show();

        MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image,
            string dontShowAgainKey, bool isDependenciesButtonVisible, bool isError, bool isInfo, bool isQuestion, bool isDeleteAnywayButtonVisible, bool applyToAll);

        MessageBoxResult ShowNotConnected();
        MessageBoxResult ShowDeleteConfirmation(string nameOfItemBeingDeleted);
        MessageBoxResult ShowCorruptTaskResult(string errorMessage);
        MessageBoxResult ShowNameChangedConflict(string oldName, string newName);
        MessageBoxResult ShowDeployConflict(int conflictCount);
        MessageBoxResult ShowDeployServerVersionConflict(string sourceServerVersion, string destinationServerVersion);
        MessageBoxResult ShowConnectServerVersionConflict(string selectedServerVersion, string currentServerVersion);
        MessageBoxResult ShowDeployResourceNameConflict(string conflictResourceName);
        MessageBoxResult ShowSettingsCloseConfirmation();
        MessageBoxResult ShowSchedulerCloseConfirmation();

        MessageBoxResult ShowSaveErrorDialog(string errorMessage);
        MessageBoxResult ShowConnectionTimeoutConfirmation(string serverName);
        MessageBoxResult ShowDeleteVersionMessage(string displayName);

        void ShowInvalidCharacterMessage(string invalidText);

        MessageBoxResult ShowDeployNameConflict(string message);
        MessageBoxResult ShowDeploySuccessful(string message);

        MessageBoxResult ShowDeployServerMinVersionConflict(string sourceServerVersion, string destinationServerVersion);

        MessageBoxResult ShowServerNotConnected(string server);

        IPopupMessage GetDeleteConfirmation(string nameOfItemBeingDeleted);
        IPopupMessage GetDuplicateMessage(string name);

        MessageBoxResult ShowNoInputsSelectedWhenClickLink();

        MessageBoxResult ShowRollbackVersionMessage(string displayName);
        MessageBoxResult ShowResourcesConflict(List<string> resourceDuplicates);
    }
}