using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Windows.Ribbon.Events;
using Infragistics.Windows.Ribbon.Internal;
using Infragistics.Windows.Helpers;
using System.Collections;
using Infragistics.Shared;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A placeholder used to represent tools or <see cref="RibbonGroup"/> instances that have been placed on the <see cref="QuickAccessToolbar"/>.  This tool is intended for
	/// use on the <see cref="QuickAccessToolbar"/> only. The <see cref="TargetType"/> property is used to determine whether the <see cref="TargetId"/> represents a 
	/// tool or a <see cref="RibbonGroup"/>.
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class QatPlaceholderTool : FrameworkElement
	{
		#region Member Variables

		private XamRibbon										_ribbon = null;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of a <see cref="QatPlaceholderTool"/> class without an associated tool or <see cref="RibbonGroup"/> reference.
		/// </summary>
		public QatPlaceholderTool()
		{
            // AS 2/6/09 TFS11796
            this.Loaded += new RoutedEventHandler(OnLoaded);
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="QatPlaceholderTool"/> class that is associated with a tool instance that has the specified Id.
		/// </summary>
		/// <remarks>
		/// <p class="body">The specified Id must be assigned to a tool defined somewhere on the <see cref="XamRibbon"/>.  The <see cref="QatPlaceholderTool"/> will create a clone of that tool
		/// which is displayed inside this <see cref="QatPlaceholderTool"/>.</p>
		/// </remarks>
		/// <param name="associatedTargetId">The Id of the tool with which to associate this <see cref="QatPlaceholderTool"/>.</param>
		/// <seealso cref="Target"/>
		/// <seealso cref="TargetId"/>
		public QatPlaceholderTool(string associatedTargetId)
            // AS 2/6/09 TFS11796
            : this()
        {
			Debug.Assert(associatedTargetId != null &&  associatedTargetId != string.Empty, "associatedTargetId is empty!");

			this.TargetId = associatedTargetId;
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="QatPlaceholderTool"/> class that is associated with a tool instance that has the specified Id.
		/// </summary>
		/// <remarks>
		/// <p class="body">The specified Id must be assigned to a tool defined somewhere on the <see cref="XamRibbon"/>.  The <see cref="QatPlaceholderTool"/> will create a clone of that tool
		/// which is displayed inside this <see cref="QatPlaceholderTool"/>.</p>
		/// </remarks>
		/// <param name="associatedTargetId">The Id of the tool with which to associate this <see cref="QatPlaceholderTool"/>.</param>
		/// <param name="targetType">The type of the tool with which to associate this <see cref="QatPlaceholderTool"/>.</param>
		/// <seealso cref="Target"/>
		/// <seealso cref="TargetId"/>
		/// <seealso cref="TargetType"/>
		/// <seealso cref="QatPlaceholderToolType"/>
		public QatPlaceholderTool(string associatedTargetId, QatPlaceholderToolType targetType)
            // AS 2/6/09 TFS11796
            : this()
		{
			Debug.Assert(associatedTargetId != null && associatedTargetId != string.Empty, "associatedTargetId is empty!");

			this.TargetId	= associatedTargetId;
			this.TargetType	= targetType;
		}

		static QatPlaceholderTool()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(QatPlaceholderTool), new FrameworkPropertyMetadata(typeof(QatPlaceholderTool)));
		}

		#endregion //Constructor	
    
		#region Base Class Overrides

			#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			if (this.Target != null)
				this.Target.Arrange(new Rect(finalSize));

			return finalSize;
		}

			#endregion //ArrangeOverride	
    
			#region GetVisualChild

		/// <summary>
		/// Gets the visual child at a specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the specific child visual.</param>
		/// <returns>The visual child at the specified index.</returns>
		protected override Visual GetVisualChild(int index)
		{
			if (index < 0)
				return null;

			if (this.Target == null)
				return null;

			if (index > 0)
				return null;

			return this.Target;
		}

			#endregion //GetVisualChild	

			#region LogicalChildren

		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				return new SingleItemEnumerator(this.Target);
			}
		}

			#endregion //LogicalChildren	

			#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			if (this.Target != null)
			{
				this.Target.Measure(availableSize);
				return this.Target.DesiredSize;
			}

			return XamRibbon.DEFAULT_QAT_TOOL_SIZE;
		}

			#endregion //MeasureOverride	

			#region OnVisualParentChanged

		/// <summary>
		/// Invoked when the parent element of this element reports a change to its underlying visual parent.
		/// </summary>
		/// <param name="oldParent">The previous parent. This may be provided as null if the System.Windows.DependencyObject did not have a parent element previously.</param>
		protected override void OnVisualParentChanged(DependencyObject oldParent)
		{
			base.OnVisualParentChanged(oldParent);

			// JM BR26960 10-01-07 - Only allow the QatPlaceholderTool to be sited on the QuickAccessToolbarPanel and QuickAccessToolbarOverflowPanel.
			DependencyObject parent = VisualTreeHelper.GetParent(this);
			if (null	!= parent								&&
				false	== parent is QuickAccessToolbarPanel	&& 
				false	== parent is QuickAccessToolbarOverflowPanel)
				throw new NotSupportedException(XamRibbon.GetString("LE_InvalidQatPlaceholderParent"));

			this.VerifyAssociatedTarget();
		}

			#endregion //OnVisualParentChanged	
   
			#region ToString

		/// <summary>
		/// Returns a string representation of the <see cref="Target"/> (i.e. tool or <see cref="RibbonGroup"/>).
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (this.Target != null)
				return this.Target.ToString();

			return base.ToString();
		}

			#endregion //ToString
    
			#region VisualChildrenCount

		/// <summary>
		/// Returns the total numder of visual children (read-only).
		/// </summary>
		protected override int VisualChildrenCount
		{
			get
			{
				return this.Target != null ? 1 : 0;
			}
		}

			#endregion //VisualChildrenCount	
    
		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region Target

		private static readonly DependencyPropertyKey TargetPropertyKey =
			DependencyProperty.RegisterReadOnly("Target",
			typeof(FrameworkElement), typeof(QatPlaceholderTool), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="Target"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TargetProperty =
			TargetPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the tool or <see cref="RibbonGroup"/> instance associated with this <see cref="QatPlaceholderTool"/>. (read-only)
		/// </summary>
		/// <remarks>
		/// <p class="body">To change the target associated with this <see cref="QatPlaceholderTool"/> set the <see cref="TargetId"/> property.</p>
		/// </remarks>
		/// <seealso cref="TargetProperty"/>
		/// <seealso cref="TargetId"/>
		/// <seealso cref="TargetType"/>
		/// <seealso cref="XamRibbon"/>
		/// <seealso cref="QatPlaceholderTool"/>
		//[Description("Returns the tool instance associated with this QatPlaceholderTool. (read-only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public FrameworkElement Target
		{
			get
			{
				return (FrameworkElement)this.GetValue(QatPlaceholderTool.TargetProperty);
			}
		}

				#endregion //Target

				#region TargetId

		/// <summary>
		/// Identifies the <see cref="TargetId"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TargetIdProperty = DependencyProperty.Register("TargetId",
			typeof(string), typeof(QatPlaceholderTool), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTargetIdChanged)));

		private static void OnTargetIdChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			QatPlaceholderTool qpt = target as QatPlaceholderTool;
			if (qpt != null)
				qpt.VerifyAssociatedTarget();
		}

		/// <summary>
		/// Returns/sets the string <see cref="RibbonToolHelper.IdProperty"/> of the tool or <see cref="RibbonGroup"/> associated with this <see cref="QatPlaceholderTool"/> depending upon the <see cref="TargetType"/> property.
		/// </summary>
		/// <remarks>
		/// <p class="body">Depending on the value of the <see cref="TargetType"/> property, setting this property to the <see cref="RibbonToolHelper.IdProperty"/> of a tool or <see cref="RibbonGroup"/>that has been defined somewhere on the <see cref="XamRibbon"/> causes a clone of that object 
		/// to be created and displayed inside this <see cref="QatPlaceholderTool"/>.</p>
		/// <p class="body">Tools can be added to <see cref="QuickAccessToolbar"/> by the end user (via the right click context menu provided for each tool) or by the developer
		/// (by adding a <see cref="QatPlaceholderTool"/> that references the Id of a tool). RibbonGroups can be added to the <see cref="QuickAccessToolbar"/> by the end user by 
		/// right clicking on the caption of the <see cref="RibbonGroup"/> in a <see cref="RibbonTabItem"/>.</p>
		/// </remarks>
		/// <seealso cref="TargetIdProperty"/>
		/// <seealso cref="Target"/>
		/// <seealso cref="TargetType"/>
		/// <seealso cref="XamRibbon"/>
		/// <seealso cref="QatPlaceholderTool"/>
		//[Description("Returns/sets the string Id of the tool or RibbonGroup associated with this QatPlaceholderTool.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string TargetId
		{
			get
			{
				return (string)this.GetValue(QatPlaceholderTool.TargetIdProperty);
			}
			set
			{
				this.SetValue(QatPlaceholderTool.TargetIdProperty, value);
			}
		}

				#endregion //TargetId

				#region TargetType

		/// <summary>
		/// Identifies the <see cref="TargetType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TargetTypeProperty = DependencyProperty.Register("TargetType",
			typeof(QatPlaceholderToolType), typeof(QatPlaceholderTool), new FrameworkPropertyMetadata(QatPlaceholderToolType.Tool, new PropertyChangedCallback(OnTargetTypeChanged)));

		private static void OnTargetTypeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			QatPlaceholderTool qpt = target as QatPlaceholderTool;
			if (qpt != null)
				qpt.VerifyAssociatedTarget();
		}

		/// <summary>
		/// Returns/sets an enumeration that identifies the type of object (e.g. tool or RibbonGroup) that is represented by the <see cref="QatPlaceholderTool"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">Depending on the value of the <see cref="TargetType"/> property, the <see cref="RibbonToolHelper.IdProperty"/> is used to locate the tool or <see cref="RibbonGroup"/>that has been defined somewhere on the <see cref="XamRibbon"/> and which 
		/// will be cloned and displayed inside this <see cref="QatPlaceholderTool"/>.</p>
		/// </remarks>
		/// <seealso cref="TargetTypeProperty"/>
		/// <seealso cref="Target"/>
		/// <seealso cref="TargetId"/>
		/// <seealso cref="XamRibbon"/>
		/// <seealso cref="QatPlaceholderTool"/>
		//[Description("Returns/sets the enumeration that identifies the type of object (e.g. tool or RibbonGroup) that the placeholder represents.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public QatPlaceholderToolType TargetType
		{
			get
			{
				return (QatPlaceholderToolType)this.GetValue(QatPlaceholderTool.TargetTypeProperty);
			}
			set
			{
				this.SetValue(QatPlaceholderTool.TargetTypeProperty, value);
			}
		}

				#endregion //TargetType

			#endregion //Public Properties

			#region Private Properties

				#region Ribbon

		private XamRibbon Ribbon
		{
			get
			{
				if (this._ribbon == null)
					this._ribbon = Infragistics.Windows.Utilities.GetAncestorFromType(this, typeof(XamRibbon),  true) as XamRibbon;

				return this._ribbon;
			}
		}

				#endregion //Ribbon	

			#endregion //Private Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

			#endregion //Internal Methods

			#region Private Methods

                // AS 2/6/09 TFS11796
                #region OnLoaded
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.VerifyAssociatedTarget();
        } 
                #endregion //OnLoaded

				#region VerifyAssociatedRibbonGroup
		private void VerifyAssociatedRibbonGroup()
		{
			// Get a tool instance that has the same Id that has been assigned to this QatPlaceholderTool.
			RibbonGroup sourceGroup = this.Ribbon.GetRibbonGroup(this.TargetId);

            if (sourceGroup == null)
            {
                // AS 2/10/09 TFS11796
                // Wait until the placeholder is loaded since the QAT is part of the 
                // ribbon's logical tree and may now have access to the ribbon.
                //
                if (!this.IsLoaded)
                    return;

                throw new InvalidOperationException(XamRibbon.GetString("LE_QatItemWithTargetNotInRibbon", this.TargetId));
            }

			FrameworkElement clonedGroup = sourceGroup.CloneForQat();
			this.SetValue(QatPlaceholderTool.TargetPropertyKey, clonedGroup);
			if (this.Target == null)
				return;

			// Add the cloned tool as a visual child.
			this.AddLogicalChild(this.Target);
			this.AddVisualChild(this.Target);
		} 
				#endregion //VerifyAssociatedRibbonGroup

				#region VerifyAssociatedTarget
		private void VerifyAssociatedTarget()
		{
			if (this.Target != null)
			{
				// AS 12/20/07 BR29248
				// According to the docs, we should be releasing the old tool
				// and adding the new one instead of just returning.
				//
				//return;
				if (object.Equals(RibbonToolHelper.GetId(this.Target), this.TargetId))
				{
					// if the associated ribbon group is the one we have...
					if (this.TargetType == QatPlaceholderToolType.RibbonGroup && this.Target is RibbonGroup)
						return;

					// if we're associated with a tool and this is not a ribbon group
					// then exit
					if (this.TargetType == QatPlaceholderToolType.Tool && this.Target is RibbonGroup == false)
						return;
				}

				this.RemoveLogicalChild(this.Target);
				this.RemoveVisualChild(this.Target);
				this.ClearValue(QatPlaceholderTool.TargetPropertyKey);

				// AS 1/2/08 BR29248
				this.InvalidateMeasure();
			}

			if (String.IsNullOrEmpty(this.TargetId))
				return;

			if (this.Ribbon == null)
				return;

            
#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


            // AS 2/6/09 TFS11796
            object oldTarget = this.Target;

			if (this.TargetType == QatPlaceholderToolType.Tool)
				this.VerifyAssociatedTool();
			else
				this.VerifyAssociatedRibbonGroup();

            // AS 2/6/09 TFS11796
            // We also need to explicitly invalidate the measure if we added a target.
            // 
            if (false == object.Equals(oldTarget, this.Target))
                this.InvalidateMeasure();
        } 
				#endregion //VerifyAssociatedTarget

				#region VerifyAssociatedTool

		private void VerifyAssociatedTool()
		{
			// Get a tool instance that has the same Id that has been assigned to this QatPlaceholderTool.
			FrameworkElement fromTool = this.Ribbon.ToolInstanceManager.GetToolInstanceFromToolId(this.TargetId);

            // AS 2/10/09 TFS11796
            // If we can't get the tool wait until we're loaded to try and get it.
            //
            if (null == fromTool && !this.IsLoaded)
                return;

			// AS 10/11/07 BR27304
			// Use the root source tool as the tool to be cloned.
			//
			fromTool = RibbonToolProxy.GetRootSourceTool(fromTool);

			if (fromTool == null)
				throw new InvalidOperationException(XamRibbon.GetString("LE_QatItemWithTargetNotInRibbon", this.TargetId));

			IRibbonTool	irtFromTool = fromTool as IRibbonTool;
			if (irtFromTool == null)
				throw new InvalidOperationException(XamRibbon.GetString("LE_QatToolNotIRibbonTool"));


			// Clone the tool instance
			RibbonToolProxy proxy = irtFromTool.ToolProxy;
			
			if (null == proxy)
				throw new InvalidOperationException(XamRibbon.GetString("LE_IRibbonToolProxyIsNull"));


			// JM BR28473 11-20-07
			if (proxy.CanAddToQat == false)
				throw new NotSupportedException(XamRibbon.GetString("LE_CannotAddToolToQat", fromTool.GetType().FullName));


			FrameworkElement clonedTool = proxy.Clone(fromTool);
			this.SetValue(QatPlaceholderTool.TargetPropertyKey, clonedTool);
			if (this.Target == null)
		        return;


			// Bind the cloned instance's properties.
			IRibbonTool irtAssociatedTool = this.Target as IRibbonTool;
			if (irtAssociatedTool == null)
				return;

			RibbonToolProxy cloneProxy = irtAssociatedTool.ToolProxy;
			if (null == cloneProxy)
				return;

			cloneProxy.Bind(fromTool, this.Target);

			// Set some properties on the cloned tool.
			// AS 10/11/07
			// This should already have been set in the Clone.
			//
			//this.Target.SetValue(RibbonToolHelper.IdProperty, this.TargetId);

			// AS 10/11/07
			// This is set in the proxy clone method now.
			//
			//this.Target.SetValue(XamRibbon.ClonedFromToolProperty, fromTool as FrameworkElement);
			this.Target.SetValue(XamRibbon.LocationPropertyKey, RibbonKnownBoxes.ToolLocationQuickAccessToolbarBox);

			
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			this.Target.SetValue(RibbonToolHelper.SizingModePropertyKey, RibbonKnownBoxes.RibbonToolSizingModeImageOnlyBox);

			// Add the cloned tool as a visual child.
			this.AddLogicalChild(this.Target);
			this.AddVisualChild(this.Target);
		}

				#endregion //VerifyAssociatedTool

			#endregion //Private Methods

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