using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DataPresenter;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	/// <summary>
	/// Exposes <see cref="LabelPresenter"/> types to UI Automation
	/// </summary>
	public class LabelPresenterAutomationPeer : FrameworkElementAutomationPeer
		// AS 1/19/10 TFS26545
		, ILabelPeer
	{
		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="LabelPresenterAutomationPeer"/> class
		/// </summary>
		/// <param name="owner">The <see cref="LabelPresenter"/> for which the peer is being created</param>
		public LabelPresenterAutomationPeer(LabelPresenter owner)
			: base(owner)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>HeaderItem</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.HeaderItem;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="LabelPresenter"/>
		/// </summary>
		/// <returns>A string that contains 'LabelPresenter'</returns>
		protected override string GetClassNameCore()
		{
			return "LabelPresenter";
		}

		#endregion //GetClassNameCore

		#region GetPattern
		/// <summary>
		/// Gets the control pattern for the System.Windows.UIElement that is associated with this System.Windows.Automation.Peers.UIElementAutomationPeer.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			// AS 1/19/10 TFS26545
			// When the labels are with the cells then the LabelAutomationPeer is 
			// not used and the label itself must implement the patterns. To help 
			// this I moved the pattern impl from the LabelAutomationPeer into a 
			// helper class and used that here and there.
			//
			if (patternInterface == PatternInterface.Invoke)
			{
				LabelPresenter lp = (LabelPresenter)this.Owner;
				Field f = lp.Field;
				FieldLayout fl = f != null ? f.Owner : null;

				if (null != fl && fl.LabelLocationResolved == LabelLocation.InCells)
				{
					return this.PatternProvider;
				}
			}

			return base.GetPattern(patternInterface);
		}
		#endregion //GetPattern

		#region IsEnabledCore
		/// <summary>
		/// Returns a value indicating whether the <see cref="System.Windows.UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/> can receive and send events.
		/// </summary>
		/// <returns><b>True</b> if the <see cref="System.Windows.UIElement"/> can send and receive events; otherwise, <b>false</b>.</returns>
		protected override bool IsEnabledCore()
		{
			// AS 1/19/10 TFS26545
			if (null == ((LabelPresenter)this.Owner).Field)
				return false;

			return base.IsEnabledCore();
		}
		#endregion //IsEnabledCore

		#endregion //Base class overrides

		#region Properties

		// AS 1/19/10 TFS26545
		#region PatternProvider
		private LabelPatternProvider _patternProvider;

		private LabelPatternProvider PatternProvider
		{
			get
			{
				if (_patternProvider == null)
					_patternProvider = new LabelPatternProvider(this);

				return _patternProvider;
			}
		}
		#endregion //PatternProvider

		#endregion //Properties

		// AS 1/19/10 TFS26545
		#region ILabelPeer Members

		Field ILabelPeer.Field
		{
			get { return ((LabelPresenter)this.Owner).Field; }
		}

		LabelPresenter ILabelPeer.Label
		{
			get { return this.Owner as LabelPresenter; }
		}

		#endregion //ILabelPeer Members
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