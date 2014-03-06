using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Editors;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.Windows;

namespace Infragistics.Windows.Automation.Peers.Editors
{
	/// <summary>
	/// Exposes <see cref="ValueEditor"/> types to UI Automation
	/// </summary>
	public class ValueEditorAutomationPeer : FrameworkElementAutomationPeer,
		IValueProvider
	{
		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="ValueEditorAutomationPeer"/> class
		/// </summary>
		/// <param name="owner">The <see cref="ValueEditor"/> for which the peer is being created</param>
		public ValueEditorAutomationPeer(ValueEditor owner)
			: base(owner)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="ValueEditor"/>
		/// </summary>
		/// <returns>A string that contains 'ValueEditor'</returns>
		protected override string GetClassNameCore()
		{
			return "ValueEditor";
		}

		#endregion //GetClassNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the element that is associated with this <see cref="LabelAutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Value)
				return this;

			return base.GetPattern(patternInterface);
		}

		#endregion //GetPattern

		// JJD 08/06/12 - TFS18020 - added
		#region SetFocusCore

		/// <summary>
		/// Called to focus the associtaed element
		/// </summary>
		protected override void SetFocusCore()
		{
			ValueEditor editor = this.Editor;

			// see if the editor is focusable
			if (editor.Focusable)
			{
				// if the Focus method returns true we can just return
				if (editor.Focus() || editor.IsFocusWithin)
					return;
			}

			// Otherwise, throw the appropriate exception
			throw new InvalidOperationException(XamMaskedEditor.GetString("LE_InvalidOperationException_11"));
		}

		#endregion //SetFocusCore	
    
		#endregion //Base class overrides

		#region Properties

		#region Editor

		/// <summary>
		/// Returns the <see cref="ValueEditor"/> associated with the automation peer.
		/// </summary>
		private ValueEditor Editor
		{
			get { return (ValueEditor)this.Owner; }
		}

		#endregion //Editor

		#endregion //Properties

		#region Methods

		#region RaiseValuePropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal void RaiseValuePropertyChangedEvent(string oldValue, string newValue)
		{
			if (null != this.GetPattern(PatternInterface.Value))
				this.RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, oldValue, newValue);
		}

		#endregion //RaiseValuePropertyChangedEvent

		#region RaiseReadOnlyPropertyChangedEvent


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal void RaiseReadOnlyPropertyChangedEvent(bool oldValue, bool newValue)
		{
			if (null != this.GetPattern(PatternInterface.Value))
				this.RaisePropertyChangedEvent(ValuePatternIdentifiers.IsReadOnlyProperty, oldValue, newValue);
		}

		#endregion //RaiseReadOnlyPropertyChangedEvent	

		// AS 2/3/10 TFS26590
		// Moved here from the Value set and changed it so that we enter 
		// edit mode through the hosting valuepresenter if there is one.
		//
		#region VerifyIsInEditMode
		/// <summary>
		/// Ensures that the editor is in edit mode.
		/// </summary>
		/// <remarks>
		/// <p class="body">This method will attempt to put the editor into edit mode if it is not in edit mode. If that fails it will raise an exception.</p>
		/// </remarks>
		protected void VerifyIsInEditMode()
		{
			ValueEditor editor = this.Editor;

			if (editor.IsInEditMode == false)
			{
				// If the editor is hosted go through the host method.
				if (editor.Host != null)
					editor.Host.StartEditMode();
				else
					editor.StartEditMode();

				if (!editor.IsInEditMode)
					throw new InvalidOperationException(ValueEditor.GetString("LE_InvalidOperationException_1"));
			}
		}
		#endregion //VerifyIsInEditMode

		#endregion //Methods	

		#region IValueProvider

		bool IValueProvider.IsReadOnly
		{
			get { return this.Editor.IsReadOnly; }
		}

		void IValueProvider.SetValue(string value)
		{
			if (this.IsEnabled() == false)
				throw new ElementNotEnabledException();

			// AS 2/3/10 TFS26590
			// Moved to a helper method and changed to start edit mode on the owner if its hosted.
			//
			//if (this.Editor.IsInEditMode == false)
			//{
			//    if (this.Editor.StartEditMode() == false)
			//        throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_1"));
			//}
			this.VerifyIsInEditMode();

			// SSP 5/10/12 TFS100047
			// 
			//this.Editor.Text = value;
			this.Editor.Text_CurrentValue = value;
		}

		string IValueProvider.Value
		{
			get { return this.Editor.Text; }
		}

		#endregion //IValueProvider
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