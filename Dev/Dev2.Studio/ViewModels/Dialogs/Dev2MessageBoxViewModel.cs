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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Studio.Core.Interfaces;
using FontAwesome.WPF;
using Warewolf.Studio.ViewModels;
using Warewolf.Studio.Views;
using Dev2.Studio.Core;
// ReSharper disable PossibleNullReferenceException

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Dialogs
{
    public class Dev2MessageBoxViewModel : Screen
    {
        #region Fields

        private string _messageBoxText;
        private string _caption;
        private MessageBoxButton _button;
        private MessageBoxImage _icon;
        private MessageBoxResult _result;
        private readonly MessageBoxResult _defaultResult;
        private readonly string _dontShowAgainKey;
        private bool _dontShowAgain;
        private static bool _deleteAnyway;

        private static Dictionary<string, MessageBoxResult> _dontShowAgainOptions;

        #endregion Fields

        #region Constructor

        public Dev2MessageBoxViewModel(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon,
                                       MessageBoxResult defaultResult, string dontShowAgainKey)
        {
            _messageBoxText = messageBoxText;
            _caption = caption;
            _button = button;
            _icon = icon;
            _result = defaultResult;
            _defaultResult = defaultResult;
            _messageBoxText = messageBoxText;
            _dontShowAgainKey = dontShowAgainKey;
        }

        #endregion

        #region Properties

        public bool FocusOk => _defaultResult == MessageBoxResult.OK;

        public bool FocusYes => _defaultResult == MessageBoxResult.Yes;

        public bool FocusNo => _defaultResult == MessageBoxResult.No;

        public bool FocusCancel => _defaultResult == MessageBoxResult.Cancel;

        public string MessageBoxText
        {
            get
            {
                return _messageBoxText;
            }
            set
            {
                _messageBoxText = value;
                NotifyOfPropertyChange(() => MessageBoxText);
            }
        }

        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                _caption = value;
                NotifyOfPropertyChange(() => Caption);
            }
        }

        public MessageBoxButton Button
        {
            get
            {
                return _button;
            }
            set
            {
                _button = value;
                NotifyOfPropertyChange(() => Button);
            }
        }

        public MessageBoxImage Icon
        {
            get
            {
                return _icon;
            }
            set
            {
                _icon = value;
                NotifyOfPropertyChange(() => Icon);
            }
        }

        public MessageBoxResult Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
                NotifyOfPropertyChange(() => Result);
            }
        }

        public bool DontShowAgain
        {
            get
            {
                return _dontShowAgain;
            }
            set
            {
                _dontShowAgain = value;
                NotifyOfPropertyChange(() => DontShowAgain);
            }
        }

        public string DontShowAgainKey => _dontShowAgainKey;

        public bool DeleteAnyway
        {
            get { return _deleteAnyway; }
            set
            {
                _deleteAnyway = value; 
            }
        }

        #endregion Properties

        #region Static Methods

        private static string GetDontShowAgainPersistencePath()
        {
            var path = Path.Combine(new[] 
                { 
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    StringResources.App_Data_Directory, 
                    StringResources.User_Interface_Layouts_Directory 
                });

            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Path.Combine(path, "DontShowAgainOptions.xml");
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static void LoadDontShowAgainOptions()
        {
            try
            {
                var filePersistenceProviderInst = CustomContainer.Get<IFilePersistenceProvider>();
                var data = filePersistenceProviderInst.Read(GetDontShowAgainPersistencePath());
                _dontShowAgainOptions = new Dictionary<string, MessageBoxResult>();

                foreach(var element in XElement.Parse(data).Elements())
                {
                    var xAttribute = element.Attribute("Key");
                    if(xAttribute != null)
                    {
                        string key = xAttribute.Value;
                        var attribute = element.Attribute("Value");
                        if(attribute != null)
                        {
                            MessageBoxResult val = (MessageBoxResult)Enum.Parse(typeof(MessageBoxResult), attribute.Value);
                            _dontShowAgainOptions.Add(key, val);
                        }
                    }
                }
            }
            catch(Exception)
            {
                // If deserialization fails then create a blank dicitonary so that when a save occurs it will be saved in teh correct format.
                _dontShowAgainOptions = new Dictionary<string, MessageBoxResult>();
            }
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
            MessageBoxResult tmp;
            if(_dontShowAgainOptions != null && _dontShowAgainOptions.TryGetValue(dontShowAgainKey, out tmp))
            {
                result = new Tuple<bool, MessageBoxResult>(true, tmp);
            }
            else
            {
                result = new Tuple<bool, MessageBoxResult>(false, MessageBoxResult.None);
            }

            return result;
        }

        public static MessageBoxViewModel Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, string dontShowAgainKey, bool isDependenciesButtonVisible,
            bool isError, bool isInfo, bool isQuestion, List<string> urlsFound, bool isDeleteAnywayButtonVisible, bool applyToAll)
        {
            // Claculate the appropriate default result
            var defaultResult = MessageBoxResult.OK;
            switch(button)
            {
                case MessageBoxButton.OK:
                case MessageBoxButton.OKCancel:
                    defaultResult = MessageBoxResult.OK;
                    break;
                case MessageBoxButton.YesNo:
                case MessageBoxButton.YesNoCancel:
                    defaultResult = MessageBoxResult.Yes;
                    break;
            }

            return Show(messageBoxText, caption, button, icon, defaultResult, dontShowAgainKey, isDependenciesButtonVisible, isError, isInfo, isQuestion, urlsFound, isDeleteAnywayButtonVisible, applyToAll);
        }

        public static MessageBoxViewModel Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon,
                                            MessageBoxResult defaultResult, string dontShowAgainKey, bool isDependenciesButtonVisible,
            bool isError, bool isInfo, bool isQuestion, List<string> urlsFound, bool isDeleteAnywayButtonVisible, bool applyToAll)
        {
            // Check for don't show again option
            var dontShowAgainOption = GetDontShowAgainOption(dontShowAgainKey);
            var msgBoxViewModel = new MessageBoxViewModel(messageBoxText, caption, button, FontAwesomeIcon.ExclamationTriangle, isDependenciesButtonVisible, isError, isInfo, isQuestion, urlsFound, isDeleteAnywayButtonVisible, applyToAll);
            if (dontShowAgainOption.Item1)
            {
                // Return the remembered option
                msgBoxViewModel.Result = dontShowAgainOption.Item2;
                return msgBoxViewModel;
            }

            if (caption != "Duplicated Resources")
            {
                urlsFound = new List<string>();
            }

            

            var msgBoxView = new MessageBoxView
            {
                DataContext = msgBoxViewModel
            };
            msgBoxViewModel.IsDuplicatesVisible = urlsFound.Count > 0;
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
