using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using Infragistics.Windows.Ribbon.Events;
using Infragistics.Windows.Ribbon.Internal;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Represents a tool that displays an indicator (usually a vertical or horizontal line) that is used to visually separate and organize other tools.  
	/// Since the tool is derived from the WPF Separator element, the usage semantics of the tool are the same as a WPF Separator.
	/// </summary>

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateMenu,                GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateRibbon,              GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateQAT,                 GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAppMenu,             GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAppMenuFooterToolbar,GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAppMenuRecentItems,  GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAppMenuSubMenu,      GroupName = VisualStateUtilities.GroupLocation)]

    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class SeparatorTool : Separator,
								 IRibbonTool
	{
        #region Private Members


        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


        #endregion //Private Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of a <see cref="SeparatorTool"/> class.
		/// </summary>
		public SeparatorTool()
		{
		}

		static SeparatorTool()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SeparatorTool), new FrameworkPropertyMetadata(typeof(SeparatorTool)));
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(SeparatorTool), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            XamRibbon.LocationProperty.OverrideMetadata(typeof(SeparatorTool), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), XamRibbon.LocationPropertyKey);

        }

		#endregion //Constructor	

		#region Base Class Overrides

            #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// OnApplyTemplate is a .NET framework method exposed by the FrameworkElement. This class overrides
        /// it to get the focus site from the control template whenever template gets applied to the control.
        /// </p>
        /// </remarks>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

            #endregion //OnApplyTemplate	

		#endregion //Base Class Overrides
    
		#region Properties

			#region Common Tool Properties

				#region Location

		/// <summary>
		/// Identifies the Location dependency property.
		/// </summary>
		/// <seealso cref="Location"/>
		public static readonly DependencyProperty LocationProperty = XamRibbon.LocationProperty.AddOwner(typeof(SeparatorTool));

		/// <summary>
		/// Returns an enumeration that indicates the location of the tool. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">Possible tool locations include: Ribbon, Menu, QuickAccessToolbar, ApplicationMenu, ApplicationMenuFooterToolbar, ApplicationMenuRecentItems.</p>
		/// </remarks>
		/// <seealso cref="LocationProperty"/>
		/// <seealso cref="ToolLocation"/>
		//[Description("Returns an enumeration that indicates the location of the tool. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ToolLocation Location
		{
			get
			{
				return (ToolLocation)this.GetValue(SeparatorTool.LocationProperty);
			}
		}

				#endregion //Location

			#endregion //Common Tool Properties

		#endregion //Properties

		#region Events

		#region Common Tool Events

		#region Cloned

		/// <summary>
		/// Event ID for the <see cref="Cloned"/> routed event
		/// </summary>
		/// <seealso cref="Cloned"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		public static readonly RoutedEvent ClonedEvent = MenuTool.ClonedEvent.AddOwner(typeof(SeparatorTool));

		/// <summary>
		/// Occurs after a tool has been cloned.
		/// </summary>
		/// <remarks>
		/// <para class="body">Fired when a tool is cloned to enable its placement in an additional location in the XamRibbon.  For example, when a tool is added to the QuickAccessToolbar, the XamRibbon clones the instance of the tool that appears on the Ribbon and places the cloned instance on the QAT.  This event is fired after the cloning takes place and is a convenient point hook up event listeners for events on the cloned tool.</para>
		/// </remarks>
		/// <seealso cref="OnRaiseToolEvent"/>
		/// <seealso cref="ClonedEvent"/>
		/// <seealso cref="ToolClonedEventArgs"/>
		//[Description("Occurs after a tool has been cloned")]
		//[Category("Ribbon Events")]
		public event EventHandler<ToolClonedEventArgs> Cloned
		{
			add
			{
				base.AddHandler(MenuTool.ClonedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuTool.ClonedEvent, value);
			}
		}

				#endregion //Cloned

				#region CloneDiscarded

		/// <summary>
		/// Event ID for the <see cref="CloneDiscarded"/> routed event
		/// </summary>
		/// <seealso cref="CloneDiscarded"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		public static readonly RoutedEvent CloneDiscardedEvent = MenuTool.CloneDiscardedEvent.AddOwner(typeof(SeparatorTool));

		/// <summary>
		/// Occurs when a clone of a tool is being discarded.
		/// </summary>
		/// <seealso cref="OnRaiseToolEvent"/>
		/// <seealso cref="CloneDiscardedEvent"/>
		/// <seealso cref="ToolCloneDiscardedEventArgs"/>
		//[Description("Occurs when a clone of a tool is being discarded.")]
		//[Category("Ribbon Events")]
		public event EventHandler<ToolCloneDiscardedEventArgs> CloneDiscarded
		{
			add
			{
				base.AddHandler(MenuTool.CloneDiscardedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuTool.CloneDiscardedEvent, value);
			}
		}

				#endregion //CloneDiscarded

			#endregion //Common Tool Events

		#endregion //Events

		#region Methods

			#region Protected methods (common tool methods)

				#region OnRaiseToolEvent

		/// <summary>
		/// Occurs when the tool event is about to be raised
		/// </summary>
		/// <remarks><para class="body">The base class implemenation simply calls the RaiseEvent method.</para></remarks>
		protected virtual void OnRaiseToolEvent(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

				#endregion //OnRaiseToolEvent

			#endregion //Protected methods (common tool methods)

			#region Protected methods

                #region VisualState... Methods


        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            RibbonToolHelper.SetToolVisualState(this, useTransitions, null);

        }

        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            SeparatorTool tool = target as SeparatorTool;

            if ( tool != null )
                tool.UpdateVisualStates();
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



                #endregion //VisualState... Methods	

			#endregion //Protected methods

		#endregion //Methods

		#region IRibbonTool Members

		RibbonToolProxy IRibbonTool.ToolProxy
		{
			get { return SeparatorToolProxy.Instance; }
		}

		#endregion

		#region SeparatorToolProxy
		/// <summary>
		/// Derived <see cref="RibbonToolProxy"/> for <see cref="SeparatorTool"/> instances
		/// </summary>
		protected class SeparatorToolProxy : RibbonToolProxy<SeparatorTool>
		{
			// AS 5/16/08 BR32980 - See the ToolProxyTests.NoInstanceVariablesOnProxies proxy for details.
			//[ThreadStatic()]
			internal static readonly SeparatorToolProxy Instance = new SeparatorToolProxy();

			/// <summary>
			/// Called by the <b>Ribbon</b> to raise one of the common tool events. 
			/// </summary>
			/// <remarks>
			/// <para class="body">This method will be called to raise a commmon tool event, e.g. <see cref="ButtonTool.Cloned"/>, <see cref="ButtonTool.CloneDiscarded"/>.</para>
			/// <para class="note"><b>Note:</b> the implementation of this method calls a protected virtual method named <see cref="SeparatorTool.OnRaiseToolEvent"/> that simply calls the RaiseEvent method. This allows derived classes the opportunity of adding custom logic.</para>
			/// </remarks>
			/// <param name="sourceTool">The tool for which the event should be raised.</param>
			/// <param name="args">The event arguments</param>
			/// <seealso cref="XamRibbon"/>
			/// <seealso cref="ToolClonedEventArgs"/>
			/// <seealso cref="ToolCloneDiscardedEventArgs"/>
			protected override void RaiseToolEvent(SeparatorTool sourceTool, RoutedEventArgs args)
			{
				sourceTool.OnRaiseToolEvent(args);
			}

			/// <summary>
			/// Returns display mode for this tool when it is inside a menu.
			/// </summary>
			/// <returns>The default implementation returns 'Standard'.</returns>
			/// <param name="tool">The tool instance whose display mode is being queried.</param>
			protected override ToolMenuItemDisplayMode GetMenuItemDisplayMode(SeparatorTool tool)
			{
                // JJD 12/4/07 - BR28873
                // Embed the tool in the Caption Area
                //return ToolMenuItemDisplayMode.UseToolForEntireArea;
				return ToolMenuItemDisplayMode.EmbedToolInCaptionArea;
			}

			/// <summary>
			/// Returns true if the tool supports activation or false if the tool cannot be activated.
			/// </summary>
			/// <param name="tool">The tool instance whose activable state is being queried.</param>
			protected override bool IsActivatable(SeparatorTool tool)
			{
				return false;
			}

			#region CanAddToQat
			/// <summary>
			/// Returns a boolean indicating whether the tool type can be added to the <see cref="QuickAccessToolbar"/>.
			/// </summary>
			public override bool CanAddToQat
			{
				get { return false; }
			}
			#endregion //CanAddToQat

			#region GetKeyTipProviders
			internal override IKeyTipProvider[] GetKeyTipProviders(SeparatorTool tool, ToolMenuItem menuItem)
			{
				// the separator doesn't show a keytip
				return new IKeyTipProvider[0];
			}
			#endregion //GetKeyTipProviders
		}
		#endregion //SeparatorToolProxy

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