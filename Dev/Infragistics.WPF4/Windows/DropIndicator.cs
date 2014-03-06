using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Markup;
using Infragistics.Windows.Controls;
using Infragistics.Windows;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Controls
{
	#region DropLocation Enum

	/// <summary>
	/// Used for specifying the drop location of an item being dropped at the current mouse location.
	/// </summary>
	public enum DropLocation
	{
		/// <summary>
		/// The item is over an invalid drop location. The drag indicator is typically hidden when this is the case.
		/// </summary>
		None = 0,

		/// <summary>
		/// The item is being dropped above the drop target.
		/// </summary>
		AboveTarget = 1,

		/// <summary>
		/// The item is being dropped below the drop target.
		/// </summary>
		BelowTarget = 2,

		/// <summary>
		/// The item is being dropped left of the drop target.
		/// </summary>
		LeftOfTarget = 3,

		/// <summary>
		/// The item is being dropped right of the drop target.
		/// </summary>
		RightOfTarget = 4,

		/// <summary>
		/// The item is being dropped over an area where once the drop is processed, it will
		/// occupy the drop area. The drag indicator will indicate where the item will be positioned
		/// once its dropped.
		/// </summary>
		OverTarget = 5
	}

	#endregion // DropLocation Enum

	#region DropIndicator Class

	/// <summary>
	/// Used for displaying drop indicator during a drag-and-drop operation.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>DropIndicator</b> control is used to display drop indicator during a drag-and-drop operation.
	/// For example, when a field in DataGrid is dragged and dropped to rearrange fields, this drop indicator
	/// will be displayed to indicate where the item will be dropped.
	/// </para>
	/// </remarks>
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class DropIndicator : Control
	{
		#region Member Vars

        // JJD 9/18/09 
        // Added events to get around rooting issue in the framework when using animations
        // started via property triggers
        private bool _wasInitializeEventRaised;

		#endregion // Member Vars

		#region Constructor

		static DropIndicator( )
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( DropIndicator ), new FrameworkPropertyMetadata( typeof( DropIndicator ) ) );
			FrameworkElement.FocusableProperty.OverrideMetadata( typeof( DropIndicator ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public DropIndicator( )
		{
		}

		#endregion // Constructor
		
        #region Events

			// JJD 9/18/09 
            // Added events to get around rooting issue in the framework when using animations
            // started via property triggers
			#region DropTargetInitializeEvent

		/// <summary>
		/// Event ID for the <see cref="DropTargetInitialize"/> routed event
		/// </summary>
		public static readonly RoutedEvent DropTargetInitializeEvent =
			EventManager.RegisterRoutedEvent("DropTargetInitialize", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(DropIndicator));

		/// <summary>
        /// Occurs when the target is initialized.
        /// </summary>
		protected virtual void OnDropTargetInitialize(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseDropTargetInitialize(RoutedEventArgs args)
		{
			args.RoutedEvent	= DropIndicator.DropTargetInitializeEvent;
			args.Source			= this;
			this.OnDropTargetInitialize(args);

            this._wasInitializeEventRaised = true;
		}

		/// <summary>
        /// Occurs when the target is initialized.
        /// </summary>
		public event EventHandler<RoutedEventArgs> DropTargetInitialize
		{
			add
			{
				base.AddHandler(DropIndicator.DropTargetInitializeEvent, value);
			}
			remove
			{
				base.RemoveHandler(DropIndicator.DropTargetInitializeEvent, value);
			}
		}

			#endregion //DropTargetInitializeEvent

			// JJD 9/18/09
            // Added events to get around rooting issue in the framework when using animations
            // started via property triggers
			#region DropTargetEnterEvent

		/// <summary>
		/// Event ID for the <see cref="DropTargetEnter"/> routed event
		/// </summary>
		public static readonly RoutedEvent DropTargetEnterEvent =
			EventManager.RegisterRoutedEvent("DropTargetEnter", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(DropIndicator));

		/// <summary>
        /// Occurs when the <see cref="Infragistics.Windows.Controls.DropIndicator.DropLocation"/> is changed to a value other than 'None'.
        /// </summary>
		protected virtual void OnDropTargetEnter(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseDropTargetEnter(RoutedEventArgs args)
		{
			args.RoutedEvent	= DropIndicator.DropTargetEnterEvent;
			args.Source			= this;
			this.OnDropTargetEnter(args);
		}

		/// <summary>
        /// Occurs when the <see cref="Infragistics.Windows.Controls.DropIndicator.DropLocation"/> is changed to a value other than 'None'.
        /// </summary>
		public event EventHandler<RoutedEventArgs> DropTargetEnter
		{
			add
			{
				base.AddHandler(DropIndicator.DropTargetEnterEvent, value);
			}
			remove
			{
				base.RemoveHandler(DropIndicator.DropTargetEnterEvent, value);
			}
		}

			#endregion //DropTargetEnterEvent

			// JJD 9/18/09
            // Added events to get around rooting issue in the framework when using animations
            // started via property triggers
			#region DropTargetLeaveEvent

		/// <summary>
		/// Event ID for the <see cref="DropTargetLeave"/> routed event
		/// </summary>
		public static readonly RoutedEvent DropTargetLeaveEvent =
			EventManager.RegisterRoutedEvent("DropTargetLeave", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(DropIndicator));

		/// <summary>
        /// Occurs when the <see cref="Infragistics.Windows.Controls.DropIndicator.DropLocation"/> is changed to 'None'.
        /// </summary>
		protected virtual void OnDropTargetLeave(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseDropTargetLeave(RoutedEventArgs args)
		{
			args.RoutedEvent	= DropIndicator.DropTargetLeaveEvent;
			args.Source			= this;
			this.OnDropTargetLeave(args);
		}

		/// <summary>
        /// Occurs when the <see cref="Infragistics.Windows.Controls.DropIndicator.DropLocation"/> is changed to 'None'.
        /// </summary>
		public event EventHandler<RoutedEventArgs> DropTargetLeave
		{
			add
			{
				base.AddHandler(DropIndicator.DropTargetLeaveEvent, value);
			}
			remove
			{
				base.RemoveHandler(DropIndicator.DropTargetLeaveEvent, value);
			}
		}

			#endregion //DropTargetLeaveEvent

		#endregion //Events

		#region Properties

		#region Public Properties

		#region DropLocation

		/// <summary>
		/// Identifies the <see cref="DropLocation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DropLocationProperty = DependencyProperty.Register(
				"DropLocation",
				typeof( DropLocation? ),
				typeof( DropIndicator ),
				new FrameworkPropertyMetadata( null,
					new PropertyChangedCallback( OnDropLocationChanged ) )
			);

		/// <summary>
		/// Specifies the the drop location that this drop indicator will convey.
		/// </summary>
		//[Description( "Specifies the drop location this drop indicator will convey." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public DropLocation? DropLocation
		{
			get
			{
				return (DropLocation?)this.GetValue( DropLocationProperty );
			}
			set
			{
				this.SetValue( DropLocationProperty, value );
			}
		}

		private static void OnDropLocationChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			DropLocation? newVal = (DropLocation?)e.NewValue;

			Orientation? orientation = null;
			if ( newVal.HasValue )
			{
				DropLocation loc = newVal.Value;

				if ( Infragistics.Windows.Controls.DropLocation.AboveTarget == loc
					|| Infragistics.Windows.Controls.DropLocation.BelowTarget == loc )
					orientation = System.Windows.Controls.Orientation.Horizontal;
				else if ( Infragistics.Windows.Controls.DropLocation.LeftOfTarget == loc
					|| Infragistics.Windows.Controls.DropLocation.RightOfTarget == loc )
					orientation = System.Windows.Controls.Orientation.Vertical;
			}

			dependencyObject.SetValue( OrientationPropertyKey, orientation );

            DropIndicator di = dependencyObject as DropIndicator;

            // JJD 9/18/09 
            // Added events to get around rooting issue in the framework when using animations
            // started via property triggers
            if (di != null && di.IsLoaded)
            {
                // if we haven't yet raised the initialize event then do it now
                if (!di._wasInitializeEventRaised)
                {
                    di.RaiseDropTargetInitialize(new RoutedEventArgs());

                    // if the newvalue is null then return
                    if ( !newVal.HasValue )
                        return;
                }

                // either raise the Enter or Leave event based on the new setting
                if (newVal.HasValue == false ||
                    newVal.Value == Infragistics.Windows.Controls.DropLocation.None)
                    di.RaiseDropTargetLeave(new RoutedEventArgs());
                else
                    di.RaiseDropTargetEnter(new RoutedEventArgs());
            }
		}

		#endregion // DropLocation

		#region DropTargetHeight

		/// <summary>
		/// Identifies the <see cref="DropTargetHeight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DropTargetHeightProperty = DependencyProperty.Register(
				"DropTargetHeight",
				typeof( double ),
				typeof( DropIndicator ),
				new FrameworkPropertyMetadata( double.NaN )
			);

		/// <summary>
		/// The height of the ui element or area over which the item is being dropped.
		/// </summary>
		//[Description( "Height of the drop target." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public double DropTargetHeight
		{
			get
			{
				return (double)this.GetValue( DropTargetHeightProperty );
			}
			set
			{
				this.SetValue( DropTargetHeightProperty, value );
			}
		}

		#endregion // DropTargetHeight

		#region DropTargetWidth

		/// <summary>
		/// Identifies the <see cref="DropTargetWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DropTargetWidthProperty = DependencyProperty.Register(
				"DropTargetWidth",
				typeof( double ),
				typeof( DropIndicator ),
				new FrameworkPropertyMetadata( double.NaN )
			);


		/// <summary>
		/// The width of the ui element or area over which the item is being dropped.
		/// </summary>
		//[Description( "Width of the drop target." )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public double DropTargetWidth
		{
			get
			{
				return (double)this.GetValue( DropTargetWidthProperty );
			}
			set
			{
				this.SetValue( DropTargetWidthProperty, value );
			}
		}

		#endregion // DropTargetWidth

		#region Orientation

		private static readonly DependencyPropertyKey OrientationPropertyKey = DependencyProperty.RegisterReadOnly(
				"Orientation",
				typeof( Orientation? ),
				typeof( DropIndicator ),
				new FrameworkPropertyMetadata( null )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = OrientationPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns 'Horizontal' if <see cref="DropLocation"/> is <i>AboveTarget</i> or <i>BelowTarget</i>
		/// and 'Vertical' if <i>LeftOfTarget</i> or <i>RightOfTarget</i>. If <i>DropLocation</i> is <i>OverTarget</i>
		/// then returns 'null'.
		/// </summary>
		//[Description( "Drop indicator orientation." )]
		//[Category( "Behavior" )]
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[EditorBrowsable( EditorBrowsableState.Never )]
		public Orientation? Orientation
		{
			get
			{
				return (Orientation?)this.GetValue( OrientationProperty );
			}
		}

		#endregion // Orientation

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#endregion // Methods
	}

	#endregion // DropIndicator Class
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