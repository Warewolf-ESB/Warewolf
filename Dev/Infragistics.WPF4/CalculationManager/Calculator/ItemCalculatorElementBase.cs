using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using System.Windows.Data;



using Infragistics.Windows.Licensing;


using Infragistics.Calculations;
using Infragistics.Calculations.Engine;

namespace Infragistics.Calculations.Primitives
{

	/// <summary>
	/// Base class for <see cref="ItemCalculatorElement"/> and <see cref="ListCalculatorElement"/>.
	/// </summary>
	public abstract class ItemCalculatorElementBase : FrameworkElement



	{
   
		#region Constructor

		internal ItemCalculatorElementBase()
		{


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion //Constructor	

		#region Base class overrides
    
		#region OnInitialized


		/// <summary>
		/// called after the control has been initialized
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			this.VerifyCalculatorReferenceId();
		}


		#endregion //OnInitialized

		#endregion //Base class overrides	
        
		#region Properties

		#region Public Properties

		#region CalculationManager

		/// <summary>
		/// Identifies the <see cref="CalculationManager"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalculationManagerProperty = DependencyPropertyUtilities.Register("CalculationManager",
			typeof(XamCalculationManager), typeof(ItemCalculatorElementBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnCalculationManagerChanged))
			);

		private static void OnCalculationManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ItemCalculatorElementBase instance = (ItemCalculatorElementBase)d;

			ItemCalculatorBase calculator = instance.ItemCalculatorBase;

			// sync up the property on the calculator
			if (calculator != null)
			{
				calculator.CalculationManager = e.NewValue as XamCalculationManager;

				instance.VerifyCalculatorReferenceId();

			}
		}

		/// <summary>
		/// Returns or sets the CalculationManager used to perform calculations.
		/// </summary>
		/// <seealso cref="CalculationManagerProperty"/>
		public XamCalculationManager CalculationManager
		{
			get
			{
				return (XamCalculationManager)this.GetValue(ItemCalculatorElementBase.CalculationManagerProperty);
			}
			set
			{
				this.SetValue(ItemCalculatorElementBase.CalculationManagerProperty, value);
			}
		}

		#endregion //CalculationManager

		#endregion //Public Properties	
    
		#region Internal Properties

		#region ItemCalculatorBase

		internal abstract ItemCalculatorBase ItemCalculatorBase { get; }

		#endregion //ItemCalculatorBase

		#endregion //Internal Properties	
            
		#endregion //Properties	

		#region Methods

		#region Private Methods

		#region VerifyCalculatorReferenceId

		private void VerifyCalculatorReferenceId()
		{
			ItemCalculatorBase calculator = this.ItemCalculatorBase;

			if (calculator != null)
			{
				string name = this.Name;

				if (!string.IsNullOrWhiteSpace(name))
					calculator.InitializeDefaultRefId(name);
			}
		}

		#endregion //VerifyCalculatorReferenceId

		#endregion //Private Methods

		#endregion //Methods	
            	
		#region ISupportInitialize Members


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

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