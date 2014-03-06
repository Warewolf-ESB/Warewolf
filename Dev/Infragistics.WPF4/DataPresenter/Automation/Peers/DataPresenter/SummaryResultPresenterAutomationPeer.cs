using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.DataPresenter;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.Diagnostics;
using System.Data;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{

	#region SummaryResultPresenterAutomationPeer

	// SSP 10/16/09 TFS19535
	// 
	/// <summary>
	/// Exposes <see cref="SummaryResultPresenter"/> types to UI Automation
	/// </summary>
	public class SummaryResultPresenterAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
	{
		#region Constructor

		/// <summary>
		/// Creates a new instance of the <see cref="SummaryResultPresenterAutomationPeer"/> class.
		/// </summary>
		/// <param name="owner">The <see cref="SummaryResultPresenter"/> for which the peer is being created</param>
		public SummaryResultPresenterAutomationPeer( SummaryResultPresenter owner )
			: base( owner )
		{
		}

		#endregion //Constructor

		#region Base class overrides

		#region GetAutomationControlTypeCore

		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Custom</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore( )
		{
			return AutomationControlType.Custom;
		}

		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore

		/// <summary>
		/// Returns the name of the <see cref="CellValuePresenter"/>
		/// </summary>
		/// <returns>A string that contains 'CellValuePresenter'</returns>
		protected override string GetClassNameCore( )
		{
			string className = base.GetClassNameCore( );
			if ( string.IsNullOrEmpty( className ) )
			{
				SummaryResultPresenter resultPresenter = this.Owner as SummaryResultPresenter;
				SummaryResult result = null != resultPresenter ? resultPresenter.SummaryResult : null;
				if ( null != result )
				{
					// SSP 9/30/11 Calc
					// Enclosed the existing code in the if block and added the else block.
					// If the summary is a formula summary then the source field and calculator will
					// not be set. Therefore we need to use the key in that case.
					// 
					SummaryDefinition summaryDef = result.SummaryDefinition;
					if ( null != result.SourceField && null != summaryDef.Calculator )
					{
						className = string.Format( "FieldName={0}, Calculator={1}",
							result.SourceField.Name, summaryDef.Calculator.Name );
					}
					else
					{
						className = string.Format( "Key={0}", summaryDef.Key );
					}
				}
			}

			return className;
		}

		#endregion // GetClassNameCore

		#region GetNameCore
		/// <summary>
		/// Returns the text label for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The text label</returns>
		protected override string GetNameCore( )
		{
			string name = base.GetNameCore( );

			if ( string.IsNullOrEmpty( name ) )
				name = ( (IValueProvider)this ).Value;

			return name;
		}
		#endregion //GetNameCore

		#endregion // Base class overrides

		#region IValueProvider Members

		bool IValueProvider.IsReadOnly
		{
			get
			{
				return true;
			}
		}

		void IValueProvider.SetValue( string value )
		{
			throw new ReadOnlyException( );
		}

		string IValueProvider.Value
		{
			get
			{
				SummaryResultPresenter result = this.Owner as SummaryResultPresenter;
				return null != result ? result.SummaryResult.DisplayText : null;
			}
		}

		#endregion
	}

	#endregion // SummaryResultPresenterAutomationPeer

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