#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
        static Dictionary<string, MessageBoxResult> _dontShowAgainOptions;

        #region Static Methods

        static void LoadDontShowAgainOptions()
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
