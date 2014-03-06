using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Abstract base class for all toolbars in the <see cref="XamRibbon"/>.
	/// </summary>
	public abstract class ToolbarBase : ItemsControl
	{
		#region Member Variables
		
		private List<IRibbonTool> _registeredTools;

		#endregion //Member Variables
		
		#region Constructor

		internal ToolbarBase()
		{
			this._registeredTools = new List<IRibbonTool>();
		}

		static ToolbarBase()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarBase), new FrameworkPropertyMetadata(typeof(ToolbarBase)));

			// AS 10/9/07
			// The qat was getting focus. Since the appmenufootertoolbar shouldn't either I'm handling both here.
			//
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(ToolbarBase), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
		}

		#endregion //Constructor	

		#region Properties

			#region Public Properties

			#endregion //Public Properties

			#region Attached Properties

				#region ContainingToolbar

		internal static readonly DependencyPropertyKey ContainingToolbarPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("ContainingToolbar",
			typeof(ToolbarBase), typeof(ToolbarBase), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnContainingToolbarChanged)));

		private static void OnContainingToolbarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// we need to keep track of the menus/galleries and panels in each ribbon group
			ToolbarBase oldToolbar = (ToolbarBase)e.OldValue;
			ToolbarBase newToolbar = (ToolbarBase)e.NewValue;

			if (oldToolbar != null)
			{
				if (d is IRibbonTool)
					oldToolbar._registeredTools.Remove((IRibbonTool)d);
			}

			if (newToolbar != null)
			{
				if (d is IRibbonTool)
					newToolbar._registeredTools.Add((IRibbonTool)d);
			}
		}

		/// <summary>
		/// Identifies the ContainingToolbar" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetContainingToolbar"/>
		internal static readonly DependencyProperty ContainingToolbarProperty =
			ContainingToolbarPropertyKey.DependencyProperty;


		/// <summary>
		/// Gets the value of the 'ContainingToolbar' attached readonly property
		/// </summary>
		/// <seealso cref="ContainingToolbarProperty"/>
		internal static ToolbarBase GetContainingToolbar(DependencyObject d)
		{
			return (ToolbarBase)d.GetValue(ToolbarBase.ContainingToolbarProperty);
		}

				#endregion //ContainingToolbar

			#endregion //Attached Properties
		
			#region Internal Properties

				#region RegisteredTools
		internal List<IRibbonTool> RegisteredTools
		{
			get { return this._registeredTools; }
		}
				#endregion //RegisteredTools
	
			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region ContainsId

		internal virtual bool ContainsId(string toolId)
		{
			ItemCollection	items = this.Items;
			int				count = items.Count;
			for (int i = 0; i < count; i++)
			{
				FrameworkElement item = items[i] as FrameworkElement;
				if (item != null)
				{
					string toolIdCurrent = item.GetValue(RibbonToolHelper.IdProperty) as string;
					if (toolIdCurrent == toolId)
						return true;
				}
			}

			return false;
		}

				#endregion //ContainsId

				#region ContainsToolInstance

		internal virtual bool ContainsToolInstance(FrameworkElement tool)
		{
			ItemCollection	items = this.Items;
			int				count = items.Count;
			for (int i = 0; i < count; i++)
			{
				if (items[i] == tool)
					return true;
			}

			return false;
		}

				#endregion //ContainsToolInstance	

			#endregion //Internal Methods

		#endregion //Methods
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