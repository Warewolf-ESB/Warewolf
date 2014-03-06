using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Editors;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation;

namespace Infragistics.Windows.Automation.Peers.Editors
{
	/// <summary>
	/// Exposes <see cref="XamCheckEditor"/> types to UI Automation
	/// </summary>
	public class XamCheckEditorAutomationPeer : ValueEditorAutomationPeer,
		IToggleProvider
	{
		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="XamCheckEditorAutomationPeer"/> class
		/// </summary>
		/// <param name="owner">The <see cref="XamCheckEditor"/> for which the peer is being created</param>
		public XamCheckEditorAutomationPeer(XamCheckEditor owner)
			: base(owner)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Edit</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.CheckBox;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="XamCheckEditor"/>
		/// </summary>
		/// <returns>A string that contains 'XamCheckEditor'</returns>
		protected override string GetClassNameCore()
		{
			return "XamCheckEditor";
		}

		#endregion //GetClassNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the element that is associated with this <see cref="LabelAutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			// this editor will not use the value provider - it will use the toggle provider
			if (patternInterface == PatternInterface.Value)
				return null;

			if (patternInterface == PatternInterface.Toggle)
				return this;

			return null;
		}
		#endregion //GetPattern

		#endregion //Base class overrides

		#region Properties

		#region Editor

		private XamCheckEditor Editor
		{
			get { return (XamCheckEditor)this.Owner; }
		}

		#endregion //Editor

		#endregion //Properties

		#region Methods

		// AS 6/5/09 TFS18161
		#region ConvertToToggleState
		internal ToggleState ConvertToToggleState(bool? value)
		{
			if (value == null)
				return ToggleState.Indeterminate;

			return value == true ? ToggleState.On : ToggleState.Off;
		}
		#endregion //ConvertToToggleState

		#region RaiseToggleStatePropertyChangedEvent
		internal void RaiseToggleStatePropertyChangedEvent(bool? oldValue, bool? newValue)
		{
			if (null != this.GetPattern(PatternInterface.Toggle))
				// AS 6/5/09 TFS18161
				//this.RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty, oldValue, newValue);
				this.RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty, ConvertToToggleState(oldValue), ConvertToToggleState(newValue));
		} 
		#endregion //RaiseToggleStatePropertyChangedEvent

		#endregion //Methods	

		#region IToggleProvider

		void IToggleProvider.Toggle()
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			// AS 2/3/10 TFS26590
			this.VerifyIsInEditMode();

			this.Editor.ToggleIsChecked();
		}

		ToggleState IToggleProvider.ToggleState
		{
			get 
			{
				bool? isChecked = this.Editor.IsChecked;
				ToggleState state;

				if (isChecked.HasValue == false)
					state = ToggleState.Indeterminate;
				else if (isChecked.Value)
					state = ToggleState.On;
				else
					state = ToggleState.Off;

				return state;
			}
		}

		#endregion //IToggleProvider
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