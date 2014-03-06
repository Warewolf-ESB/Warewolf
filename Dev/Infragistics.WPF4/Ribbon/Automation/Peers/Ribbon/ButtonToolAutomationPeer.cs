using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Ribbon;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;

namespace Infragistics.Windows.Automation.Peers.Ribbon
{
    /// <summary>
    /// Exposes <see cref="ButtonTool"/> types to UI Automation
    /// </summary>
    public class ButtonToolAutomationPeer : ButtonBaseAutomationPeer, IInvokeProvider
    {
        #region Constructor
        /// <summary>
        /// Creates a new instance of the <see cref="ButtonToolAutomationPeer"/> class
        /// </summary>
        /// <param name="owner">The <see cref="ButtonTool"/> for which the peer is being created</param>
        public ButtonToolAutomationPeer(ButtonTool owner)
            : base(owner)
        {
        }
        #endregion //Constructor

        #region Base class overrides

        #region GetAutomationControlTypeCore
        /// <summary>
        /// Returns an enumeration indicating the type of control represented by the automation peer.
        /// </summary>
        /// <returns>The <b>Button</b> enumeration value</returns>
        override protected AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Button;
        }
        #endregion //GetAutomationControlTypeCore

        #region GetClassNameCore
        /// <summary>
        /// Returns the name of the <see cref="ButtonTool"/>
        /// </summary>
        /// <returns>A string that contains 'ButtonTool'</returns>
        protected override string GetClassNameCore()
        {
            return "ButtonTool";
        }

        #endregion //GetClassNameCore

		// AS 6/24/09 TFS18346
		#region GetNameCore
		/// <summary>
		/// Gets a human readable name for <see cref="ButtonTool"/>. 
		/// </summary>
		protected override string GetNameCore()
		{
			return ButtonToolAutomationPeer.GetToolName(this.Owner, base.GetNameCore());
		}
		#endregion //GetNameCore

        #region GetPattern
        /// <summary>
        /// 
        /// </summary>
        /// <param name="patternInterface"></param>
        /// <returns></returns>
        override public object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Invoke)
                return this;

            return null;
        }
        #endregion // GetPattern

        #endregion //Base class overrides

		#region Methods

		// AS 6/24/09 TFS18346
		#region GetToolName
		internal static string GetToolName(UIElement element, string baseName)
		{
			string name = baseName;

			if (string.IsNullOrEmpty(name))
			{
				name = element.GetValue(ButtonTool.CaptionProperty) as string;
			}

			return name;
		}
		#endregion //GetToolName

		#endregion //Methods
        #region IInvokeProvider Members

        void IInvokeProvider.Invoke()
        {
            if (!IsEnabled())
                throw new ElementNotEnabledException();

            // Async call of click event 
            // In ClickHandler opens a dialog and suspend the execution we don't want to block this thread 
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new DispatcherOperationCallback(delegate(object param)
            {
				// AS 9/17/09 TFS20136
				// We want to go through the OnClick so the command can be invoked, etc.
				//
				////((ButtonTool)Owner).AutomationButtonBaseClick();
				//Owner.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
				((ButtonTool)this.Owner).AutomationButtonClick();

                return null;
            }), null);
		}

        #endregion
    }
}
#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved