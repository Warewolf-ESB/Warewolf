using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows;
using System.Diagnostics;
using Infragistics.Collections;
using Infragistics.Controls;
using System.Windows.Automation;
using Infragistics.Controls.Editors;


namespace Infragistics.AutomationPeers
{
	/// <summary>
	/// Exposes <see cref="ValueInput"/> types to UI Automation
	/// </summary>
	public class ValueInputAutomationPeer : FrameworkElementAutomationPeer,
		IValueProvider
	{
		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="ValueInputAutomationPeer"/> class
		/// </summary>
		/// <param name="owner">The <see cref="ValueInput"/> for which the peer is being created</param>
		public ValueInputAutomationPeer(ValueInput owner)
			: base(owner)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="ValueInput"/>
		/// </summary>
		/// <returns>A string that contains 'ValueInput'</returns>
		protected override string GetClassNameCore()
		{
			return "ValueInput";
		}

		#endregion //GetClassNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the element that is associated with this <see cref="XamMaskedInputAutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Value)
				return this;

			return base.GetPattern(patternInterface);
		}
		#endregion //GetPattern

		#endregion //Base class overrides

		#region Properties

		#region Editor

		/// <summary>
		/// Returns the <see cref="ValueInput"/> associated with the automation peer.
		/// </summary>
		private ValueInput Editor
		{
			get { return (ValueInput)this.Owner; }
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

			this.Editor.Text = value;
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