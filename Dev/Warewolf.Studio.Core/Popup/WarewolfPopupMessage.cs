﻿using System.Windows;
using Dev2;
using Dev2.Common.Interfaces.PopupController;

namespace Warewolf.Studio.Core.Popup
{
    public class WarewolfPopupMessage : IDev2MessageBoxViewModel
    {
        #region Implementation of IDev2MessageBoxViewModel

        public  WarewolfPopupMessage(IPopupMessage msg,IPopupWindow popupWindow)
        {
            VerifyArgument.IsNotNull("msg",msg);
            Message = msg;
            PopupWindow = popupWindow;
        }

        /// <summary>
        /// Focus the okay button when default
        /// </summary>
        public bool FocusOk { 
            get
            {
                return Message.DefaultResult == MessageBoxResult.OK;
            } 
        }
        /// <summary>
        /// Focus the yes button when default
        /// </summary>
        public bool FocusYes
        {
            get
            {
                return Message.DefaultResult == MessageBoxResult.Yes;
            }
        }
        /// <summary>
        /// Focus the no button when default
        /// </summary>
        public bool FocusNo
        {
            get
            {
                return Message.DefaultResult == MessageBoxResult.No;
            }
        }
        /// <summary>
        /// Focus the cancel button when default
        /// </summary>
        public bool FocusCancel
        {
            get
            {
                return Message.DefaultResult == MessageBoxResult.Cancel;
            }
        }
        /// <summary>
        /// message to display
        /// </summary>
        public IPopupMessage Message { get; set; }
        public IPopupWindow PopupWindow { get; set; }
        /// <summary>
        /// result
        /// </summary>
        public MessageBoxResult Result { get; set; }
        /// <summary>
        /// owning object
        /// </summary>
        public object Parent { get; set; }
        /// <summary>
        /// is this popup active
        /// </summary>
        public bool IsActive { get; set; }

        #endregion

        public MessageBoxResult Show()
        {
            return PopupWindow.Show(Message);
        }
    }
}