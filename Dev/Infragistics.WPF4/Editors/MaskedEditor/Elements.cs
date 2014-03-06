using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using Infragistics.Shared;
using Infragistics.Windows;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Automation.Peers.Editors;

namespace Infragistics.Windows.Editors
{
	#region SectionPresenter

	/// <summary>
	/// Represents each section in the masked editor when it's in edit mode.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// This element is used only when the masked editor is in edit mode. When not in edit mode,
	/// the XamMaskedEditor uses a TextBlock to display the value. <b>Note:</b> Whether 
	/// XamMaskedEditor remains in edit mode at all times or not can be controlled using its
	/// <see cref="ValueEditor.IsAlwaysInEditMode"/> property.
	/// </para>
	/// <para class="body">
	/// Each section in the parsed mask has a SectionPresenter in the visual tree of the
	/// XamMaskedEditor when it's in edit mode. <see cref="XamMaskedEditor.Sections"/>
	/// property returns all sections of a XamMaskedEditor.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need to deal with the elements in the visual tree
	/// directly as they are managed by the XamMaskedEditor itself.
	/// </para>
	/// <seealso cref="SectionBase"/>
	/// <seealso cref="XamMaskedEditor.Sections"/>
	/// </remarks>
	//[Description( "Element for displaying a mask section. Used by the XamMaskedEditor." )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SectionPresenter : ContentControl
	{
		#region Constructor

		/// <summary>
		/// Static constructor
		/// </summary>
		static SectionPresenter( )
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( SectionPresenter ), new FrameworkPropertyMetadata( typeof( SectionPresenter ) ) );

			// Make the element non-focusable.
			UIElement.FocusableProperty.OverrideMetadata( typeof( SectionPresenter ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SectionPresenter"/> class
		/// </summary>
		public SectionPresenter( )
		{
		}

		#endregion // Constructor

		#region Initialize

		/// <summary>
		/// Initializes this SectionPresenter with the specified section object.
		/// </summary>
		/// <param name="section"></param>
		public void Initialize( SectionBase section )
		{
			this.SetValue( SectionPropertyKey, section );
		}

		#endregion // Initialize

		#region Section

		private static readonly DependencyPropertyKey SectionPropertyKey = DependencyProperty.RegisterReadOnly(
			"Section",
			typeof( SectionBase ),
			typeof( SectionPresenter ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None)
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="Section"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SectionProperty = SectionPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the section object associated with this section presenter.
		/// </summary>
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public SectionBase Section
		{
			get
			{
				return (SectionBase)this.GetValue( SectionProperty );
			}
		}

		#endregion // Section
	}

	#endregion // SectionPresenter

	#region DisplayCharacterPresenter

	/// <summary>
	/// Represents individual character of MaksedEditor when it's in edit mode.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// This element is used only when the masked editor is in edit mode. When not in edit mode,
	/// the XamMaskedEditor uses a TextBlock to display the value. <b>Note:</b> Whether 
	/// XamMaskedEditor remains in edit mode at all times or not can be controlled using its
	/// <see cref="ValueEditor.IsAlwaysInEditMode"/> property.
	/// </para>
	/// <para class="body">
	/// Each display character in the parsed mask has a DisplayCharacterPresenter in
	/// the visual tree of the XamMaskedEditor when it's in edit mode. 
	/// <see cref="XamMaskedEditor.DisplayChars"/> returns all display characters of
	/// a XamMaskedEditor. Also each Section has a collection of display characters
	/// (<see cref="SectionBase.DisplayChars"/>).
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need to deal with the elements in the visual tree
	/// directly as they are managed by the XamMaskedEditor itself.
	/// </para>
	/// <seealso cref="XamMaskedEditor.Sections"/>
	/// <seealso cref="XamMaskedEditor.DisplayChars"/>
	/// <seealso cref="SectionBase.DisplayChars"/>
	/// </remarks>
	//[Description( "Element for displaying a mask display character. Used by the XamMaskedEditor." )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class DisplayCharacterPresenter : ContentControl
	{
		#region Constructor

		/// <summary>
		/// Static constructor
		/// </summary>
		static DisplayCharacterPresenter( )
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( DisplayCharacterPresenter ), new FrameworkPropertyMetadata( typeof( DisplayCharacterPresenter ) ) );

			// Make the element non-focusable.
			UIElement.FocusableProperty.OverrideMetadata( typeof( DisplayCharacterPresenter ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DisplayCharacterPresenter"/> class
		/// </summary>
		public DisplayCharacterPresenter( )
		{
		}

		#endregion // Constructor

		#region Initialize

		/// <summary>
		/// Initializes this DisplayCharacterPresenter with the specified display character object.
		/// </summary>
		/// <param name="displayCharacter">The display character associated with the element</param>
		public void Initialize( DisplayCharBase displayCharacter )
		{
			this.SetValue( DisplayCharacterPropertyKey, displayCharacter );

		}

		#endregion // Initialize

		#region DisplayCharacter

		private static readonly DependencyPropertyKey DisplayCharacterPropertyKey = DependencyProperty.RegisterReadOnly(
			"DisplayCharacter",
			typeof( DisplayCharBase ),
			typeof( DisplayCharacterPresenter ),
			new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.None)
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="DisplayCharacter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DisplayCharacterProperty = DisplayCharacterPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the display character object associated with this display character presenter.
		/// </summary>
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public DisplayCharBase DisplayCharacter
		{
			get
			{
				return (DisplayCharBase)this.GetValue( DisplayCharacterProperty );
			}
		}

		#endregion // DisplayCharacter

		#region DrawAsSelected

		private static readonly DependencyPropertyKey DrawAsSelectedPropertyKey = DependencyProperty.RegisterReadOnly(
			"DrawAsSelected",
			typeof( bool ),
			typeof( DisplayCharacterPresenter ),
			new FrameworkPropertyMetadata( KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsArrange,
				null )
			);

		/// <summary>
		/// Identifies the Read-Only <see cref="DrawAsSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DrawAsSelectedProperty = DrawAsSelectedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a value indicating whether the display character should be drawn as selected.
		/// This property takes into consideration whether the editor has keyboard focus or not.
		/// </summary>
		[Bindable( true )]
		[Browsable( false )]
		[ReadOnly( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool DrawAsSelected
		{
			get
			{
				return (bool)this.GetValue( DrawAsSelectedProperty );
			}
		}

		internal void SyncDrawAsSelected( )
		{
			this.SetValue( DrawAsSelectedPropertyKey, KnownBoxes.FromValue(this.DisplayCharacter.DrawSelected) );
		}

		#endregion // DrawAsSelected
	}

	#endregion // DisplayCharacterPresenter

	#region SectionsList 

	/// <summary>
	/// UI element that displays sections of masked editor.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// This element is used only when the masked editor is in edit mode. When not in edit mode,
	/// the XamMaskedEditor uses a TextBlock to display the value. <b>Note:</b> Whether 
	/// XamMaskedEditor remains in edit mode at all times or not can be controlled using its
	/// <see cref="ValueEditor.IsAlwaysInEditMode"/> property.
	/// </para>
	/// <para class="body">
	/// SectionsList element is an ItemsControl in the visual tree of the XamMaskedEditor that 
	/// is responsible for displaying all the sections of the parsed mask. XamMaskedEditor exposes 
	/// section collection via its <see cref="XamMaskedEditor.Sections"/> property.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need to deal with the elements in the visual tree
	/// directly as they are managed by the XamMaskedEditor itself.
	/// </para>
	/// <seealso cref="XamMaskedEditor.Sections"/>
	/// </remarks>
	//[Description( "Element for displaying mask sections. Used by the XamMaskedEditor." )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class SectionsList : ItemsControl
	{
		#region Constructor

		/// <summary>
		/// Static constructor
		/// </summary>
		static SectionsList( )
		{
			// Override the default value for the ItemTemplateTemplate so the default orientation of
			// the stack panel is horizontal.
			FrameworkElementFactory factory = new FrameworkElementFactory( typeof( StackPanel ) );
			factory.SetValue( StackPanel.OrientationProperty, KnownBoxes.OrientationHorizontalBox );
			ItemsPanelTemplate template = new ItemsPanelTemplate( factory );
			template.Seal( );

			ItemsControl.ItemsPanelProperty.OverrideMetadata( typeof( SectionsList ), new FrameworkPropertyMetadata( template ) );

			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( SectionsList ), new FrameworkPropertyMetadata( typeof( SectionsList ) ) );

			// Masked editor should not draw any focus rect when it's focused.
			FrameworkElement.FocusVisualStyleProperty.OverrideMetadata( typeof( SectionsList ), new FrameworkPropertyMetadata( new Style( ) ) );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SectionsList"/> class
		/// </summary>
		public SectionsList( )
		{
		}

		#endregion // Constructor

		#region Base class overrides

		#region GetContainerForItemOverride

		/// <summary>
		/// Creates the container to wrap an item.
		/// </summary>
		/// <returns>The newly created container</returns>
		protected override System.Windows.DependencyObject GetContainerForItemOverride()
		{
			return new SectionPresenter();
		}

		#endregion // GetContainerForItemOverride

		// JJD 08/07/12 - TFS118374 - added measure override
		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			Size size = base.MeasureOverride(constraint);

			FontStyle fstyle = this.FontStyle;

			// JJD 08/07/12 - TFS118374 
			// If the font style is Italic or Oblique
			// add a couple of extra pixels to the returned width
			// based on the font size and weight
			if (fstyle == FontStyles.Italic ||
				 fstyle == FontStyles.Oblique)
			{
				double fontSize = this.FontSize;

				FontWeight fweight = this.FontWeight;

				double factor = 8;

				if (fweight == FontWeights.Bold ||
					fweight == FontWeights.Black ||
					fweight == FontWeights.Heavy ||
					fweight == FontWeights.UltraBold ||
					fweight == FontWeights.ExtraBold)
					factor = 6;

				size.Width += fontSize / factor;
			}

			return size;
		}

		#endregion //MeasureOverride	
    
		// JJD 08/07/21 - TFS118020] - added
		#region OnCreateAutomationPeer

		/// <summary>
		/// Returns an automation peer that exposes the <see cref="SectionsList"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.Editors.SectionsListAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new SectionsListAutomationPeer(this);
		}

		#endregion //OnCreateAutomationPeer

		#region PrepareContainerForItemOverride

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <param name="element">The container that wraps the item.</param>
		/// <param name="item">The data item that is wrapped.</param>
		protected override void PrepareContainerForItemOverride(System.Windows.DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);

			SectionPresenter sectionPresenter = element as SectionPresenter;
			SectionBase section = item as SectionBase;

			// JJD 07/12/12 - TFS113981
			// Instead of asserting just do a null check so we don't throw up an assert in Blend
			//Debug.Assert(null != sectionPresenter && null != section);
			//sectionPresenter.Initialize( section );
			if (null != sectionPresenter && null != section)
				sectionPresenter.Initialize(section);
		}

		#endregion //PrepareContainerForItemOverride

		#endregion //Base class overrides	
    
		#region Sections

		/// <summary>
		/// Returns the associated sections.
		/// </summary>
		internal SectionsCollection Sections
		{
			get
			{
				return this.ItemsSource as SectionsCollection;
			}
		}

		#endregion // Sections
	}

	#endregion // SectionsList

	#region DisplayCharactersList

	/// <summary>
	/// UI element that displays display characters of a section in masked editor.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// This element is used only when the masked editor is in edit mode. When not in edit mode,
	/// the XamMaskedEditor uses a TextBlock to display the value. <b>Note:</b> Whether 
	/// XamMaskedEditor remains in edit mode at all times or not can be controlled using its
	/// <see cref="ValueEditor.IsAlwaysInEditMode"/> property.
	/// </para>
	/// <para class="body">
	/// DisplayCharactersList element displays display characters of a Section. This element
	/// is a child of <see cref="SectionPresenter"/>. Section exposes its display characters
	/// via its <see cref="SectionBase.DisplayChars"/> property.
	/// </para>
	/// <para class="body">
	/// <b>Note:</b> Typically there is no need to deal with the elements in the visual tree
	/// directly as they are managed by the XamMaskedEditor itself.
	/// </para>
	/// <seealso cref="SectionBase.DisplayChars"/>
	/// <seealso cref="XamMaskedEditor.Sections"/>
	/// <seealso cref="XamMaskedEditor.DisplayChars"/>
	/// </remarks>
	//[Description( "Element for displaying mask display characters. Used by the XamMaskedEditor." )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class DisplayCharactersList : ItemsControl
	{
		#region Constructor

		/// <summary>
		/// Static constructor
		/// </summary>
		static DisplayCharactersList( )
		{
			// Override the default value for the ItemTemplateTemplate so the default orientation of
			// the stack panel is horizontal.
			FrameworkElementFactory factory = new FrameworkElementFactory( typeof( StackPanel ) );
			factory.SetValue(StackPanel.OrientationProperty, KnownBoxes.OrientationHorizontalBox);
			ItemsPanelTemplate template = new ItemsPanelTemplate( factory );
			template.Seal( );

			ItemsControl.ItemsPanelProperty.OverrideMetadata( typeof( DisplayCharactersList ), new FrameworkPropertyMetadata( template ) );

			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( DisplayCharactersList ), new FrameworkPropertyMetadata( typeof( DisplayCharactersList ) ) );

			// Make the element non-focusable.
			UIElement.FocusableProperty.OverrideMetadata( typeof( DisplayCharactersList ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DisplayCharactersList"/> class
		/// </summary>
		public DisplayCharactersList( )
		{
		}

		#endregion // Constructor

		#region GetContainerForItemOverride

		/// <summary>
		/// Creates the container to wrap an item.
		/// </summary>
		/// <returns>The newly created container</returns>
		protected override System.Windows.DependencyObject GetContainerForItemOverride( )
		{
			return new DisplayCharacterPresenter( );
		}

		#endregion // GetContainerForItemOverride

		#region PrepareContainerForItemOverride

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <param name="element">The container that wraps the item.</param>
		/// <param name="item">The data item that is wrapped.</param>
		protected override void PrepareContainerForItemOverride( System.Windows.DependencyObject element, object item )
		{
			base.PrepareContainerForItemOverride( element, item );

			DisplayCharacterPresenter dcPresenter = element as DisplayCharacterPresenter;
			DisplayCharBase dc = item as DisplayCharBase;

			// JJD 07/12/12 - TFS113981
			// Instead of asserting just do a null check so we don't throw up an assert in Blend
			//Debug.Assert( null != dcPresenter && null != dc );
			//dcPresenter.Initialize( dc );
			if ( dcPresenter != null && dc != null)
				dcPresenter.Initialize( dc );
		}

		#endregion //PrepareContainerForItemOverride

		#region MeasureOverride

		
		
		
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size size = base.MeasureOverride( availableSize );

			if ( size.Height <= 4 )
			{
				DisplayCharsCollection dcCollection = this.ItemsSource as DisplayCharsCollection;
				SectionBase section = null != dcCollection ? dcCollection.Section : null;
				XamMaskedEditor maskedEditor = null != section ? section.MaskedEdit : null;

				double lineHeight = Utils.GetLineHeight( this );
				if ( size.Height < lineHeight )
					size.Height = lineHeight;
			}

			return size;
		}

		#endregion // MeasureOverride
	}

	#endregion // DisplayCharactersList

	#region CaretElement

	/// <summary>
	/// Represents caret element inside a masked editor.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// XamMaskedEditor control uses this element to display caret when it's in edit mode.
	/// </para>
	/// </remarks>
	//[Description( "Element for displaying caret in the XamMaskedEditor." )]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CaretElement : Control
	{
		#region Member Vars

		// SSP 10/23/09 TFS22642
		// 
		// SSP 1/19/10 TFS27963
		// Apparently Loaded event in some cases is raised asynchronously by the framework which
		// may lead to the IsLoaded property being set to true before Loaded event is delivered.
		// 
		//private bool _hasBeenLoadedAndArranged;
		private bool _hasBeenArranged;
		private bool _hasBeenLoaded;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Static constructor
		/// </summary>
		static CaretElement( )
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata( typeof( CaretElement ), new FrameworkPropertyMetadata( typeof( CaretElement ) ) );

			// Make the element non-focusable.
			UIElement.FocusableProperty.OverrideMetadata( typeof( CaretElement ), new FrameworkPropertyMetadata( KnownBoxes.FalseBox ) );
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CaretElement"/> class
		/// </summary>
		public CaretElement( )
		{
			// SSP 1/19/10 TFS27963
			// 
			this.Loaded += new RoutedEventHandler( OnLoadedHandler );
		}

		#endregion // Constructor

		#region Base Overrides

		#region ArrangeOverride

		// SSP 10/23/09 TFS22642
		// Overrode ArrangeOverride so we can set the new _hasBeenLoadedAndArranged flag.
		// 

		/// <summary>
		/// Called to arrange the child elements.
		/// </summary>
		/// <param name="arrangeBounds"></param>
		/// <returns></returns>
		protected override Size ArrangeOverride( Size arrangeBounds )
		{
			Size ret = base.ArrangeOverride( arrangeBounds );

			// SSP 1/19/10 TFS27963
			// 
			//if ( !_hasBeenLoadedAndArranged && this.IsLoaded )
			//	_hasBeenLoadedAndArranged = true;
			_hasBeenArranged = true;

			return ret;
		}

		#endregion // ArrangeOverride		

		#endregion // Base Overrides

		#region Properties

		#region BlinkVisibility

		/// <summary>
		/// Identifies the <see cref="BlinkVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BlinkVisibilityProperty = DependencyProperty.Register(
			"BlinkVisibility",
			typeof( bool ),
			typeof( CaretElement ),
			new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.None,
				new PropertyChangedCallback( OnBlinkVisibilityChanged ),
				null )
			);

		/// <summary>
		/// Specifies the current blinking state - whether it's visible or blinked out.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property is animated in the style of the CaretElement. This property
		/// in turn sets the Visibility property of the element.
		/// </para>
		/// </remarks>
		//[Description( "Current blinking state - whether it's visible or blinked out" )]
		//[Category( "Appearance" )]
		[Bindable( true )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public bool BlinkVisibility
		{
			get
			{
				return (bool)this.GetValue( BlinkVisibilityProperty );
			}
			set
			{
				this.SetValue( BlinkVisibilityProperty, value );
			}
		}

		private static void OnBlinkVisibilityChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			CaretElement elem = (CaretElement)dependencyObject;
			bool newVal = (bool)e.NewValue;

			elem.Visibility = newVal ? Visibility.Visible : Visibility.Hidden;
		}

		#endregion // BlinkVisibility

		#endregion // Properties

		#region Methods

		#region OnLoadedHandler

		// SSP 1/19/10 TFS27963
		// 
		private void OnLoadedHandler( object sender, RoutedEventArgs e )
		{
			_hasBeenLoaded = true;

			// SSP 1/19/10 TFS27963
			// 
			this.Loaded -= new RoutedEventHandler( OnLoadedHandler );
		}

		#endregion // OnLoadedHandler

		#region ResetCaretBlinking

		internal void ResetCaretBlinking( )
		{
			// SSP 10/23/09 TFS22642
			// Don't send the ResetBlinking notification until the storyboard in the template
			// that animates the blinking has started.
			// 
			// ------------------------------------------------------------------------------
			//this.RaiseResetBlinking( );
			// SSP 1/19/10 TFS27963
			// 
			//if ( _hasBeenLoadedAndArranged )
			if ( _hasBeenLoaded && _hasBeenArranged )
				this.RaiseResetBlinking( );
			// ------------------------------------------------------------------------------
		}

		#endregion // ResetCaretBlinking

		#region ResetBlinking

		/// <summary>
		/// This event is raised to indicate that the caret is to reset 
		/// its blinking timer and start with the blinking state of on.
		/// </summary>
		public static readonly RoutedEvent ResetBlinkingEvent =
			EventManager.RegisterRoutedEvent( "ResetBlinking", RoutingStrategy.Direct, typeof( RoutedEventHandler ), typeof( CaretElement ) );

		private void RaiseResetBlinking( )
		{
			RoutedEventArgs args = new RoutedEventArgs( );
			args.RoutedEvent = CaretElement.ResetBlinkingEvent;
			args.Source = this;
			this.RaiseEvent( args );
		}

		#endregion //ResetBlinking

		#endregion // Methods

		#region CaretBlinkDuration

		/// <summary>
		/// Returns the caret blink duration.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// System settings are used to determine the caret blink duration. <b>Note:</b>
		/// This property returns a value that's double the caret blink time as returned
		/// by the OS. This is because this property indicates the time duration between
		/// successive caret blinks.
		/// </para>
		/// </remarks>
		public static Duration CaretBlinkDuration
		{
			get
			{
				int val = Utilities.CaretBlinkTime;
				if ( val <= 0 )
					val = 0;

				return new Duration( TimeSpan.FromMilliseconds( 2 * val ) );
			}
		}

		#endregion // CaretBlinkDuration
	}

	#endregion // CaretElement

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