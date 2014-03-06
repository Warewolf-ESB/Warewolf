using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.Automation.Peers
{
	// MD 8/12/10 - TFS26592
	/// <summary>
	/// Exposes the <see cref="SimpleTextBlock"/> to UI Automation.
	/// </summary>
	public class SimpleTextBlockAutomationPeer : FrameworkElementAutomationPeer
		, IValueProvider		// MS 3/22/11 - TFS37255
	{
		#region Constructor

		/// <summary>
		/// Creates a new instance of the <see cref="SimpleTextBlockAutomationPeer"/> class.
		/// </summary>
		/// <param name="owner">The <see cref="SimpleTextBlock"/> for which the peer is being created.</param>
		public SimpleTextBlockAutomationPeer(SimpleTextBlock owner)
			: base(owner) { }

		#endregion // Constructor

		// MS 3/22/11 - TFS37255
		#region IValueProvider Members

		bool IValueProvider.IsReadOnly
		{
			get { return true; }
		}

		void IValueProvider.SetValue(string value)
		{
			throw new ElementNotEnabledException();
		}

		string IValueProvider.Value
		{
			get 
			{
				SimpleTextBlock owner = (SimpleTextBlock)base.Owner;
				return owner.Text;
			}
		}

		#endregion

		#region GetAutomationControlTypeCore

		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Text</b> enumeration value.</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Text;
		}

		#endregion // GetAutomationControlTypeCore

		#region GetClassNameCore

		/// <summary>
		/// Returns the name of the <see cref="SimpleTextBlock"/>.
		/// </summary>
		/// <returns>A string that contains 'SimpleTextBlock'.</returns>
		protected override string GetClassNameCore()
		{
			return typeof(SimpleTextBlock).Name;
		}

		#endregion // GetClassNameCore

		// MS 3/22/11 - TFS37255
		#region GetPattern

		/// <summary>
		/// Gets the pattern for the UIElement that is associated with this automation peer.
		/// </summary>
		/// <param name="patternInterface">The pattern value to get.</param>
		/// <returns>Null or a pattern instance.</returns>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Value)
				return this;

			return base.GetPattern(patternInterface);
		}

		#endregion // GetPattern

		#region IsContentElementCore

		/// <summary>
		/// Returns the value indicating whether the <see cref="SimpleTextBlock"/> associated with this automation peer 
		/// is content in an owning control.
		/// </summary>
		/// <returns>False if the element is part of a template; otherwise True.</returns>
		protected override bool IsContentElementCore()
		{
			return ((SimpleTextBlock)this.Owner).TemplatedParent == null;
		}

		#endregion // IsContentElementCore
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