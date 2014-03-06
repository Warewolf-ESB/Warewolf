using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Diagnostics;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Provides information that is used to determine the order/priority in which a <see cref="RibbonGroup"/> will be resized within a <see cref="RibbonTabItem"/>.
	/// </summary>
	/// <remarks>
	/// <p class="body">GroupVariants are used to control the order and type of resize actions that can happen for a 
	/// <see cref="RibbonGroup"/>. By default, the <see cref="RibbonGroup.Variants"/> collection of a RibbonGroup is 
	/// empty. When all of the RibbonGroups within a <see cref="RibbonTabItem"/> have their Variants collection empty, 
	/// the default resizing logic will be used to resize the groups when there is no enough room to display the contents 
	/// of all the RibbonGroups in the tab. The default behavior is to perform each resize action on each RibbonGroup 
	/// from last to first and then move on to the next resize action. To control the order and type of resize actions 
	/// that will occur for a RibbonGroup, you would create one or more <b>GroupVariant</b> instances and add it to the 
	/// Variants collection of the RibbonGroup.</p>
	/// <p class="body">The GroupVariant exposes two properties - <see cref="Priority"/> and <see cref="ResizeAction"/>. 
	/// <b>Priority</b> is used to sort the GroupVariant with respect to the other GroupVariants within the Variants collections 
	/// of all the RibbonGroups in a RibbonTabItem. The lower priority GroupVariants will be processed before those with 
	/// higher Priority values.</p>
	/// <p class="body"><b>ResizeAction</b> is used to determine the type of resize action that will occur when the GroupVariant 
	/// is processed. For example, a value of <b>CollapseRibbonGroup</b> is used to indicate that the RibbonGroup should be collapsed. 
	/// When a RibbonGroup is collapsed, you will see a button containing the caption of the RibbonGroup. Clicking that button will 
	/// display the contents of the RibbonGroup within a popup. A value of <b>ReduceImageAndTextLargeTools</b> is used to indicate that 
	/// all tools whose current <see cref="ButtonTool.SizingMode"/> is <b>ImageAndTextLarge</b> should be reduced to 
	/// <b>ImageAndTextNormal</b>. The <see cref="RibbonGroup.MaximumSizeProperty"/> and <see cref="RibbonGroup.MinimumSizeProperty"/> 
	/// can be used to control the minimum and maximum allowable sizes for tools within the RibbonGroup.</p>
	/// </remarks>
	/// <seealso cref="RibbonGroup.Variants"/>
	public class GroupVariant : DependencyObjectNotifier
	{
		#region Static Instances

		
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		[ThreadStatic()]
		private static GroupVariant[] _cachedVariants;

		#endregion //Static Instances

		#region Constructor

		static GroupVariant()
		{
			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		}
		/// <summary>
		/// Initializes a new <see cref="GroupVariant"/>
		/// </summary>
		public GroupVariant()
		{
		}

		private GroupVariant(GroupVariantResizeAction size)
		{
			this.ResizeAction = size;
		}
		#endregion //Constructor

		#region Base class overrides
		/// <summary>
		/// Invoked when a property on the object has been changed.
		/// </summary>
		/// <param name="e">Provides information about the property that was changed.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			this.RaisePropertyChangedEvent(e.Property.Name);
		}
		#endregion //Base class overrides

		#region Properties

		#region Priority

		/// <summary>
		/// Identifies the <see cref="Priority"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PriorityProperty = DependencyProperty.Register("Priority",
			typeof(int), typeof(GroupVariant), new FrameworkPropertyMetadata(0));

		/// <summary>
		/// Returns/sets the priority assigned to a <see cref="RibbonGroup"/> which determines the order in which <see cref="RibbonTabItem.RibbonGroups"/> are resized when the <see cref="XamRibbon"/> is resized.
		/// </summary>
		/// <remarks>
		/// <p class="body">The priority is used to sort the GroupVariants from the <see cref="RibbonGroup.Variants"/> of all the 
		/// RibbonGroups within a <see cref="RibbonTabItem"/> to determine which GroupVariants are processed first. GroupVariants 
		/// that have been assigned a lower priority will be resized first.</p>
		/// </remarks>
		/// <seealso cref="GroupVariant"/>
		/// <seealso cref="PriorityProperty"/>
		/// <seealso cref="ResizeAction"/>
		/// <seealso cref="RibbonGroup.Variants"/>
		//[Description("Returns/sets the priority assigned to a 'RibbonGroup' which determines the order in which 'RibbonGroups' are resized when the 'XamRibbon' is resized. RibbonGroups that have been assigned a lower priority will be resized first.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public int Priority
		{
			get
			{
				return (int)this.GetValue(GroupVariant.PriorityProperty);
			}
			set
			{
				this.SetValue(GroupVariant.PriorityProperty, value);
			}
		}

		#endregion //Priority

		#region ResizeAction

		/// <summary>
		/// Identifies the <see cref="ResizeAction"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResizeActionProperty = DependencyProperty.Register("ResizeAction",
			typeof(GroupVariantResizeAction), typeof(GroupVariant), new FrameworkPropertyMetadata(GroupVariantResizeAction.CollapseRibbonGroup));

		/// <summary>
		/// Returns/sets the resize action that will be performed when the <see cref="Priority"/> caused the variant to be applied.
		/// </summary>
		/// <remarks>
		/// <p class="body">The ResizeAction determines the type of resize action that will be taken when the <see cref="GroupVariant"/> is processed.</p>
		/// <p class="note"><b>Note:</b> In most cases the order of the sorted ResizeActions (which is based on the <see cref="Priority"/>) is not important 
		/// but there are some cases where defining one resize action to occur before another will prevent the subsequent resize actions from taking place. For 
		/// example, any ResizeAction that occurs after a ResizeAction of <b>CollapseRibbonGroup</b> will be ignored since the contents of the popup of a 
		/// collapsed RibbonGroup are displayed using their preferred size.</p>
		/// </remarks>
		/// <seealso cref="GroupVariant"/>
		/// <seealso cref="ResizeActionProperty"/>
		/// <seealso cref="Priority"/>
		/// <seealso cref="RibbonGroup.Variants"/>
		/// <seealso cref="GroupVariantResizeAction"/>
		//[Description("Returns/sets the resize action that will be performed when the 'Priority' caused the variant to be applied.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public GroupVariantResizeAction ResizeAction
		{
			get
			{
				return (GroupVariantResizeAction)this.GetValue(GroupVariant.ResizeActionProperty);
			}
			set
			{
				this.SetValue(GroupVariant.ResizeActionProperty, value);
			}
		}

		#endregion //ResizeAction

		#endregion //Properties

		#region Methods

		#region GetDefaultVariant
		internal static GroupVariant GetDefaultVariant(GroupVariantResizeAction size)
		{
			
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

			int index = (int)size;

			if (GroupVariant._cachedVariants == null)
				GroupVariant._cachedVariants = new GroupVariant[Enum.GetValues(typeof(GroupVariantResizeAction)).Length];

			if (index > GroupVariant._cachedVariants.Length)
			{
				Debug.Fail("Unrecognized 'GroupVariantSize':" + size.ToString());
				size = GroupVariantResizeAction.CollapseRibbonGroup;
				index = (int)size;
			}

			if (GroupVariant._cachedVariants[index] == null)
				GroupVariant._cachedVariants[index] = new GroupVariant(size);

			return GroupVariant._cachedVariants[index];
		}
		#endregion //GetDefaultVariant

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