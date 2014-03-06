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
using Infragistics.Calculations.Primitives;

namespace Infragistics.Calculations
{

	/// <summary>
	/// Main purpose of this class is to allow one to be able to use <see cref="ListCalculator"/> and provide the source
	/// list and calc manager using bindings. Also it exposes Results as a dictionary that supports notifications to allow one
	/// to bind to a calculation result.
	/// </summary>
	[ContentProperty("Calculator")]
	public class ListCalculatorElement : ItemCalculatorElementBase
	{

		#region Base class overrides

		#region ItemCalculatorBase

		internal override ItemCalculatorBase ItemCalculatorBase { get { return this.Calculator; } }

		#endregion //ItemCalculatorBase

		#endregion //Base class overrides

		#region Calculator

		/// <summary>
		/// Identifies the <see cref="Calculator"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalculatorProperty = DependencyPropertyUtilities.Register("Calculator",
			typeof(ListCalculator), typeof(ListCalculatorElement),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnCalculatorChanged))
			);

		private static void OnCalculatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ListCalculatorElement instance = (ListCalculatorElement)d;
			
			ListCalculator oldCalc = e.OldValue as ListCalculator;
			ListCalculator newCalc = e.NewValue as ListCalculator;

			XamCalculationManager mgr = instance.CalculationManager;

			if (mgr != null)
			{
				// sync up the CalculationManager property on the old and new ListCalculator
				if (oldCalc != null && oldCalc.CalculationManager == mgr)
						oldCalc.CalculationManager = null;

				if (newCalc != null)
					newCalc.CalculationManager = mgr;
			}

			IEnumerable itemSource = instance.ItemsSource;

			if (itemSource != null)
			{
				// sync up the List property on the old and new ListCalculator
				if (oldCalc != null && oldCalc.ItemsSource == itemSource)
					oldCalc.ItemsSource = null;

				if (newCalc != null)
					newCalc.ItemsSource = itemSource;
			}

		}

		/// <summary>
		/// Returns or sets the <see cref="ListCalculator"/>
		/// </summary>
		/// <seealso cref="CalculatorProperty"/>
		public ListCalculator Calculator
		{
			get
			{
				return (ListCalculator)this.GetValue(ListCalculatorElement.CalculatorProperty);
			}
			set
			{
				this.SetValue(ListCalculatorElement.CalculatorProperty, value);
			}
		}

		#endregion //Calculator

		#region ItemsSource

		/// <summary>
		/// Identifies the <see cref="ItemsSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemsSourceProperty = DependencyPropertyUtilities.Register("ItemsSource",
			typeof(IEnumerable), typeof(ListCalculatorElement),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnItemsSourceChanged))
			);

		private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ListCalculatorElement instance = (ListCalculatorElement)d;

			ListCalculator calculator = instance.Calculator;

			if ( calculator != null)
				calculator.ItemsSource = e.NewValue as IEnumerable;
		}

		/// <summary>
		/// Returns or sets an enumerable used to populate the <see cref="ListCalculator.Items"/> collection
		/// </summary>
		/// <seealso cref="ItemsSourceProperty"/>
		public IEnumerable ItemsSource
		{
			get
			{
				return (IEnumerable)this.GetValue(ListCalculatorElement.ItemsSourceProperty);
			}
			set
			{
				this.SetValue(ListCalculatorElement.ItemsSourceProperty, value);
			}
		}

		#endregion //ItemsSource

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