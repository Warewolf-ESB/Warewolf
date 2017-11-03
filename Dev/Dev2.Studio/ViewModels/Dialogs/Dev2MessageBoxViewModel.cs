/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using FontAwesome.WPF;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Dev2.Studio.Interfaces;


namespace Dev2.Studio.ViewModels.Dialogs
{
    public class Dev2MessageBoxViewModel : Screen
    {
        private static Dictionary<string, MessageBoxResult> _dontShowAgainOptions;

        #region Static Methods
        
        private static void LoadDontShowAgainOptions()
        {
            var filePersistenceProviderInst = CustomContainer.Get<IFilePersistenceProvider>();
            _dontShowAgainOptions = new Dictionary<string, MessageBoxResult>();
        }

        public static Tuple<bool, MessageBoxResult> GetDontShowAgainOption(string dontShowAgainKey)
        {
            // If no key then return false result
            if(string.IsNullOrEmpty(dontShowAgainKey))
            {
                return new Tuple<bool, MessageBoxResult>(false, MessageBoxResult.None);
            }

            // Load if null
            if(_dontShowAgainOptions == null)
            {
                LoadDontShowAgainOptions();
            }

            // Check if there an option for the key
            Tuple<bool, MessageBoxResult> result;
            result = _dontShowAgainOptions != null && _dontShowAgainOptions.TryGetValue(dontShowAgainKey, out MessageBoxResult tmp) ? new Tuple<bool, MessageBoxResult>(true, tmp) : new Tuple<bool, MessageBoxResult>(false, MessageBoxResult.None);

            return result;
        }

        public static MessageBoxViewModel Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, string dontShowAgainKey, bool isDependenciesButtonVisible,
            bool isError, bool isInfo, bool isQuestion, List<string> urlsFound, bool isDeleteAnywayButtonVisible, bool applyToAll)
        {
            MessageBoxResult defaultResult;
            switch (button)
            {
                case MessageBoxButton.YesNo:
                case MessageBoxButton.YesNoCancel:
                    defaultResult = MessageBoxResult.Yes;
                    break;
                default:
                    defaultResult = MessageBoxResult.OK;
                    break;
            }

            return Show(messageBoxText, caption, button, icon, defaultResult, dontShowAgainKey, isDependenciesButtonVisible, isError, isInfo, isQuestion, urlsFound, isDeleteAnywayButtonVisible, applyToAll);
        }

        public static MessageBoxViewModel Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon,
                                            MessageBoxResult defaultResult, string dontShowAgainKey, bool isDependenciesButtonVisible,
            bool isError, bool isInfo, bool isQuestion, List<string> urlsFound, bool isDeleteAnywayButtonVisible, bool applyToAll)
        {
            var urlsFoundInput = urlsFound;
            var dontShowAgainOption = GetDontShowAgainOption(dontShowAgainKey);
            var msgBoxViewModel = new MessageBoxViewModel(messageBoxText, caption, button, FontAwesomeIcon.ExclamationTriangle, isDependenciesButtonVisible, isError, isInfo, isQuestion, urlsFoundInput, isDeleteAnywayButtonVisible, applyToAll);
            if (dontShowAgainOption.Item1)
            {
                msgBoxViewModel.Result = dontShowAgainOption.Item2;
                return msgBoxViewModel;
            }

            if (caption != "Duplicated Resources")
            {
                urlsFoundInput = new List<string>();
            }            

            var msgBoxView = new MessageBoxView
            {
                DataContext = msgBoxViewModel
            };
            msgBoxViewModel.IsDuplicatesVisible = urlsFoundInput.Count > 0;
            msgBoxViewModel.IsError = isError;
            msgBoxViewModel.IsInfo = isInfo;
            msgBoxViewModel.IsQuestion = isQuestion;
            msgBoxViewModel.IsDependenciesButtonVisible = isDependenciesButtonVisible;
            msgBoxViewModel.IsDeleteAnywayButtonVisible = isDeleteAnywayButtonVisible;
            msgBoxViewModel.ApplyToAll = applyToAll;

            msgBoxView.ShowDialog();

            return msgBoxViewModel;
        }

        #endregion Static Methods
    }
}
