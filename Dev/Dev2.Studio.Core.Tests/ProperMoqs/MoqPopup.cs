/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using Dev2.Common.Interfaces.PopupController;

namespace Dev2.Core.Tests.ProperMoqs
{
    public class MoqPopup : Common.Interfaces.Studio.Controller.IPopupController
    {
        readonly MessageBoxResult _result;

        public MoqPopup(string headerText, string discriptionText, MessageBoxImage imageType, MessageBoxButton buttons)
        {
            Header = headerText;
            Description = discriptionText;
            ImageType = imageType;
            Buttons = buttons;
        }

        public MoqPopup()
            : this(MessageBoxResult.OK)
        {

        }

        public MoqPopup(MessageBoxResult result)
        {
            _result = result;
        }

        public string Header { get; set; }

        public string Description { get; set; }

        public string Question { get; set; }

        public MessageBoxImage ImageType { get; set; }

        public MessageBoxButton Buttons { get; set; }

        public MessageBoxResult Show(IPopupMessage popupMessage)
        {
            Show(popupMessage.Description, popupMessage.Header, popupMessage.Buttons, popupMessage.Image, popupMessage.DontShowAgainKey, popupMessage.IsDependenciesButtonVisible, popupMessage.IsError, popupMessage.IsInfo, popupMessage.IsQuestion);
            return _result;
        }

        public MessageBoxResult Show()
        {
            ShowHitCount++;
            return _result;
        }

        // public MessageBoxResult Show(string description, string header = "", MessageBoxButton buttons = MessageBoxButton.OK, MessageBoxImage image = MessageBoxImage.Asterisk, string dontShowAgainKey = null)
        public MessageBoxResult Show(string description, string header, MessageBoxButton buttons, MessageBoxImage image, string dontShowAgainKey, bool isDependenciesButtonVisible, bool isError, bool isInfo, bool isQuestion)
        {
            Buttons = buttons;
            Description = description;
            Header = header;
            ImageType = image;
            DontShowAgainKey = dontShowAgainKey;
            return Show();
        }

        public int ShowHitCount { get; private set; }

        public MessageBoxResult ShowNotConnected()
        {
            return _result;
        }

        public MessageBoxResult ShowDeleteConfirmation(string nameOfItemBeingDeleted)
        {
            return _result;
        }

        public MessageBoxResult ShowCorruptTaskResult(string errorMessage)
        {
            return _result;
        }

        public MessageBoxResult ShowNameChangedConflict(string oldName, string newName)
        {
            return _result;
        }

        public MessageBoxResult ShowDeployConflict(int conflictCount)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowDeployServerVersionConflict(string sourceServerVersion, string destinationServerVersion)
        {
            return _result;
        }

        public MessageBoxResult ShowConnectServerVersionConflict(string selectedServerVersion, string currentServerVersion)
        {
            return _result;
        }

        public MessageBoxResult ShowSettingsCloseConfirmation()
        {
            return _result;
        }

        public MessageBoxResult ShowSchedulerCloseConfirmation()
        {
            return _result;
        }

        public MessageBoxResult ShowNoInputsSelectedWhenClickLink()
        {
            return _result;
        }

        public MessageBoxResult ShowSaveErrorDialog(string errorMessage)
        {
            return _result;
        }

        public MessageBoxResult ShowConnectionTimeoutConfirmation(string serverName)
        {
            return _result;
        }

        public void ShowInvalidCharacterMessage(string invalidText)
        {
        }

        public MessageBoxResult ShowItemCloseCloseConfirmation(string nameOfItem)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowItemSourceCloseConfirmation(string nameOfItem)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowDeployNameConflict(string message)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowDeployServerMinVersionConflict(string sourceServerVersion, string destinationServerVersion)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowDeleteVersionMessage(string displayName)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowRollbackVersionMessage(string displayName)
        {
            return MessageBoxResult.None;
        }

        public string DontShowAgainKey { get; set; }


        public MessageBoxResult ShowDeployResourceNameConflict(string conflictResourceName)
        {
            throw new NotImplementedException();
        }


        public MessageBoxResult ShowServerNotConnected(string server)
        {
            throw new NotImplementedException();
        }
    }
}
