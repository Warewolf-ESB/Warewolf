using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

using Infragistics.Windows.Helpers;
using Infragistics.Windows.Controls;
using System.Windows.Media.Animation;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
    /// An object that exposes properties for controlling the features supported by the <see cref="CardView"/>. The CardView object is used by <see cref="XamDataCards"/> control and <see cref="XamDataPresenter"/>
	/// </summary>
    /// <remarks>
    /// <p class="body">By manipulating properties on the CardViewSettings object you can control how the <see cref="CardViewPanel"/> arranges items.</p>
    /// <p class="body">The properties exposed by the CardViewSettings object are:
    ///		<ul>
	///			<li><see cref="CardViewSettings.AllowCardWidthResizing"/> - Returns/sets whether the end user can resize Card widths by selecting the right edge of card and dragging.</li>
	///			<li><see cref="CardViewSettings.AllowCardHeightResizing"/> - Returns/sets whether the end user can resize Card heights by selecting the bottom edge of card and dragging.</li>
	///			<li><see cref="CardViewSettings.AutoFitCards"/> - Returns/sets whether Card widths/heights are automatically increased to use up all available horizontal/vertical space.</li>
	///			<li><see cref="CardViewSettings.CardHeight"/> - Returns/sets the Height of each Card.</li>
	///			<li><see cref="CardViewSettings.CardWidth"/> - Returns/sets the Width of each Card.</li>
	///			<li><see cref="CardViewSettings.CollapseCardButtonVisibility"/> - Returns/sets whether a button is displayed in the Card header which can be clicked by the end user to collapse a Card so it shows its header only.</li>
	///			<li><see cref="CardViewSettings.CollapseEmptyCellsButtonVisibility"/> - Returns/sets whether a button is displayed in the Card header which can be clicked by the end user to collapse cells that contain empty values (e.g. empty string/null for string, null for nullable types, and 0 for numeric types).</li>
	///			<li><see cref="CardViewSettings.HeaderPath"/> - Returns/sets a string that represents the Path to a value on the source object that should be displayed as a Card header.</li>
	///			<li><see cref="CardViewSettings.HeaderVisibility"/> - Returns/sets the Visibility of the Card header.</li>
	///			<li><see cref="CardViewSettings.InterCardSpacingX"/> - Returns/sets the horizontal spacing between each Card.</li>
	///			<li><see cref="CardViewSettings.InterCardSpacingY"/> - Returns/sets the vertical spacing between each Card.</li>
	///			<li><see cref="CardViewSettings.MaxCardCols"/> - Returns/sets the maximum number of columns of Cards that should be displayed.  A value of zero forces CardView to display as many columns as space will allow.</li>
	///			<li><see cref="CardViewSettings.MaxCardRows"/> - Returns/sets the maximum number of rows of Cards that should be displayed.  A value of zero forces CardView to display as many rows as space will allow.</li>
	///			<li><see cref="CardViewSettings.Orientation"/> - Returns/sets a value that specifies the dimension in which child content is arranged.</li>
	///			<li><see cref="CardViewSettings.Padding"/> - Returns/sets the padding between the outermost cards and the bounds of the control.</li>
	///			<li><see cref="CardViewSettings.RepositionAnimation"/> - Returns/sets a DoubleAnimationBase derived instance that should be used when animating Cards to new layout positions.</li>
	///			<li><see cref="CardViewSettings.ShouldAnimateCardPositioning"/> - Returns/sets whether Cards should be animated into their new positions when the CardView is scrolled.</li>
	///			<li><see cref="CardViewSettings.ShouldCollapseCards"/> - Returns/sets whether Cards should be collapsed.</li>
	///			<li><see cref="CardViewSettings.ShouldCollapseEmptyCells"/> - Returns/sets whether cells whose value is determined to be empty (e.g. empty string/null for string, null for nullable types, and 0 for numeric types) will be collapsed and not displayed within the record.</li>
	///		</ul>
    /// Refer to the documentation contained within for a complete list of the properties supported by this class and the functionality enabled by each property.
    /// </p>
    /// </remarks>
	/// <seealso cref="XamDataCards"/>
	/// <seealso cref="CardView"/>
	/// <seealso cref="CardView.ViewSettings"/>
	/// <seealso cref="CardViewPanel"/>
	[TypeConverter(typeof(ExpandableObjectConverter))]
	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_CardView, Version = FeatureInfo.Version_10_1)]
	public class CardViewSettings : ViewSettingsBase
	{
		#region Constructor

		/// <summary>
		/// Creates an instance of CardViewSettings.
		/// </summary>
		/// <seealso cref="CardView"/>
        /// <seealso cref="CardView.ViewSettings"/>
        /// <seealso cref="CardViewPanel"/>
		/// <seealso cref="XamDataCards"/>
		/// <seealso cref="XamDataPresenter.View"/>
		/// <remarks>
        /// <p class="body">The <see cref="CardView"/> will automatically create an instance of this class when its <see cref="CardView.ViewSettings"/> property is accessed.
        /// You can also create one manually and assign it to the <see cref="CardView"/>'s <see cref="CardView.ViewSettings"/> property of one or more instances of <see cref="CardView"/> if needed.  </p>
		/// </remarks>
		public CardViewSettings()
		{
		}

		static CardViewSettings()
		{
            DoubleAnimation repositionAnimation		= new DoubleAnimation(0, 1.0, TimeSpan.FromMilliseconds(750), FillBehavior.Stop);
            repositionAnimation.BeginTime			= new Nullable<TimeSpan>();
            repositionAnimation.AutoReverse			= false;
            repositionAnimation.AccelerationRatio	= .6;
            repositionAnimation.DecelerationRatio	= .4;
            repositionAnimation.Freeze();
            s_RepositionAnimation = DependencyProperty.Register("RepositionAnimation",
                    typeof(DoubleAnimationBase), typeof(CardViewSettings), new FrameworkPropertyMetadata(repositionAnimation));

		}

		#endregion //Constructor

		#region Constants

		private		const double DEFAULT_ITEM_WIDTH					= 200;
		private		const double DEFAULT_ITEM_HEIGHT				= 200;

		internal	const double ITEM_ANIMATION_DAMPENING			= .6;//.8;
		internal	const double ITEM_ANIMATION_ATTRACTION			= 2.5;//2;
		private		const double ITEM_ANIMATION_DISTANCE_THRESHOLD	= .1;

		#endregion //Constants

		#region Base class overrides

			#region OnControlInitialized

		/// <summary>
        /// Called when the control that owns this <see cref="ViewSettingsBase"/> derived object has its OnInitialized method called.
		/// </summary>
		internal protected override void OnControlInitialized()
		{
		}

			#endregion //OnControlInitialized	

			#region Reset

		/// <summary>
		/// Resets all properties to their default values.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Reset()
		{
			this.ResetAllowCardWidthResizing();
			this.ResetAllowCardHeightResizing();
			this.ResetAutoFitCards();
			this.ResetCardHeight();
			this.ResetCardWidth();
			this.ResetCollapseCardButtonVisibility();
			this.ResetCollapseEmptyCellsButtonVisibility();
			this.ResetHeaderPath();
			this.ResetHeaderVisibility();
			this.ResetInterCardSpacingX();
			this.ResetInterCardSpacingY();
			this.ResetMaxCardCols();
			this.ResetMaxCardRows();
			this.ResetOrientation();
			this.ResetPadding();
			this.ResetRepositionAnimation();
			this.ResetShouldAnimateCardPositioning();
			this.ResetShouldCollapseCards();
			this.ResetShouldCollapseEmptyCells();
		}

			#endregion //Reset

			#region ShouldSerialize

		/// <summary>
		/// Determines if any property value is set to a non-default value.
		/// </summary>
		/// <returns>Returns true if any property value is set to a non-default value.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool ShouldSerialize()
		{
			return	this.ShouldSerializeAllowCardWidthResizing()				||
					this.ShouldSerializeAllowCardHeightResizing()				||
					this.ShouldSerializeAutoFitCards()							||
					this.ShouldSerializeCardHeight()							||
					this.ShouldSerializeCardWidth()								||
					this.ShouldSerializeCollapseCardButtonVisibility()			||
					this.ShouldSerializeCollapseEmptyCellsButtonVisibility()	||
					this.ShouldSerializeHeaderPath()							||
					this.ShouldSerializeHeaderVisibility()						||
					this.ShouldSerializeInterCardSpacingX()						||
					this.ShouldSerializeInterCardSpacingY()						||
					this.ShouldSerializeMaxCardCols()							||
					this.ShouldSerializeMaxCardRows()							||
					this.ShouldSerializeOrientation()							||
					this.ShouldSerializePadding()								||
					this.ShouldSerializeRepositionAnimation()					||
					this.ShouldSerializeShouldAnimateCardPositioning()			||
					this.ShouldSerializeShouldCollapseCards()					||
					this.ShouldSerializeShouldCollapseEmptyCells();
		}

			#endregion //ShouldSerialize

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region AllowCardWidthResizing

		// JM 02-23-10 TFS28229 - Change default to false.
		/// <summary>
		/// Identifies the <see cref="AllowCardWidthResizing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowCardWidthResizingProperty = DependencyProperty.Register("AllowCardWidthResizing",
			typeof(bool), typeof(CardViewSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns/sets whether the end user can resize Card widths by selecting the right edge of card and dragging.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Card Width resizing is synchronized across all Cards - i.e., if the user resizes the width
		/// of one card, all Cards are resized to the same width.  The resizing operation essentially sets the <see cref="CardViewSettings.CardWidth"/>
		/// property.</para>
		/// </remarks>
		/// <seealso cref="AllowCardWidthResizingProperty"/>
		/// <seealso cref="AllowCardHeightResizingProperty"/>
		/// <seealso cref="CardWidth"/>
		/// <seealso cref="CardHeight"/>
		/// <seealso cref="AutoFitCards"/>
		//[Description("Returns/sets whether the end user can resize Card widths by selecting the right edge of card and dragging.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool AllowCardWidthResizing
		{
			get
			{
				return (bool)this.GetValue(CardViewSettings.AllowCardWidthResizingProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.AllowCardWidthResizingProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="AllowCardWidthResizing"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAllowCardWidthResizing()
		{
			return this.AllowCardWidthResizing != (bool)CardViewSettings.AllowCardWidthResizingProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="AllowCardWidthResizing"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAllowCardWidthResizing()
		{
			this.ClearValue(AllowCardWidthResizingProperty);
		}

				#endregion //AllowCardWidthResizing

				#region AllowCardHeightResizing

		// JM 02-23-10 TFS28229 - Change default to false.
		/// <summary>
		/// Identifies the <see cref="AllowCardHeightResizing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowCardHeightResizingProperty = DependencyProperty.Register("AllowCardHeightResizing",
			typeof(bool), typeof(CardViewSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns/sets whether the end user can resize Card Heights by selecting the bottom edge of card and dragging.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Card Height resizing is synchronized across all Cards - i.e., if the user resizes the height
		/// of one card, all Cards are resized to the same height.  The resizing operation essentially sets the <see cref="CardViewSettings.CardHeight"/>
		/// property.</para>
		/// </remarks>
		/// <seealso cref="AllowCardHeightResizingProperty"/>
		/// <seealso cref="AllowCardWidthResizing"/>
		/// <seealso cref="CardHeight"/>
		/// <seealso cref="CardWidth"/>
		/// <seealso cref="AutoFitCards"/>
		//[Description("Returns/sets whether the end user can resize Card Heights by selecting the bottom edge of card and dragging.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool AllowCardHeightResizing
		{
			get
			{
				return (bool)this.GetValue(CardViewSettings.AllowCardHeightResizingProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.AllowCardHeightResizingProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="AllowCardHeightResizing"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAllowCardHeightResizing()
		{
			return this.AllowCardHeightResizing != (bool)CardViewSettings.AllowCardHeightResizingProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="AllowCardHeightResizing"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAllowCardHeightResizing()
		{
			this.ClearValue(AllowCardHeightResizingProperty);
		}

				#endregion //AllowCardHeightResizing

				#region AutoFitCards

		/// <summary>
		/// Identifies the <see cref="AutoFitCards"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutoFitCardsProperty = DependencyProperty.Register("AutoFitCards",
			typeof(AutoFitCards), typeof(CardViewSettings), new FrameworkPropertyMetadata(AutoFitCards.None, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns/sets whether Card widths/heights are automatically increased to use up all available horizontal/vertical space.  
		/// </summary>
		/// <seealso cref="AutoFitCardsProperty"/>
		/// <seealso cref="CardWidth"/>
		/// <seealso cref="AllowCardWidthResizing"/>
		//[Description("Returns/sets whether Card widths/heights are automatically increased to use up all available horizontal/vertical space.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public AutoFitCards AutoFitCards
		{
			get
			{
				return (AutoFitCards)this.GetValue(CardViewSettings.AutoFitCardsProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.AutoFitCardsProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="AutoFitCards"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeAutoFitCards()
		{
			return this.AutoFitCards != (AutoFitCards)CardViewSettings.AutoFitCardsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="AutoFitCards"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetAutoFitCards()
		{
			this.ClearValue(AutoFitCardsProperty);
		}

				#endregion //AutoFitCards

				#region CardHeight

		/// <summary>
		/// Identifies the <see cref="CardHeight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CardHeightProperty = DependencyProperty.Register("CardHeight",
			typeof(double), typeof(CardViewSettings), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ValidatePositiveDouble));

		/// <summary>
		/// Returns/sets the Height of each Card.
		/// </summary>
		/// <remarks>
		/// If left set to NaN, which is the default, the height of the card is based on the height required to show the contents of the record. 
		/// When using a DataRecordSizingMode that sizes based on content, this could mean that the cards could be variable height. 
		/// Also, if ShouldCollapseEmptyCells is set to true then this could also result in variable height cards. 
		/// When set to a fixed value, the record will be positioned to fill the card. 
		/// If the size required for the records height is larger, then a scrollbar will appear within the card by default to allow the end user to get to the remaining cells.
		/// <para class="note"><b>Note: </b>If <see cref="AutoFitCards"/> is set to AutoFitCards.AutoFitCardsVertically or AutoFitCards.AutoFitCardsBothDimensions, then additional height may be added to the Card if vertical space is available.</para>
		/// </remarks>
		/// <seealso cref="CardHeightProperty"/>
		/// <seealso cref="CardWidth"/>
		/// <seealso cref="ShouldCollapseEmptyCells"/>
		//[Description("Returns/sets the Height of each Card.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public double CardHeight
		{
			get
			{
				return (double)this.GetValue(CardViewSettings.CardHeightProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.CardHeightProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="CardHeight"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeCardHeight()
		{
			return this.CardHeight != (double)CardViewSettings.CardHeightProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="CardHeight"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetCardHeight()
		{
			this.ClearValue(CardHeightProperty);
		}

				#endregion //CardHeight

				#region CardWidth

		/// <summary>
		/// Identifies the <see cref="CardWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CardWidthProperty = DependencyProperty.Register("CardWidth",
			typeof(double), typeof(CardViewSettings), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ValidatePositiveDouble));

		/// <summary>
		/// Returns/sets the Width of each Card.
		/// </summary>
		/// <remarks>
		/// If left unset (double.NaN) Card widths will be determined based on the data contained in the card.
		/// <para class="note"><b>Note: </b>If <see cref="AutoFitCards"/> is set to AutoFitCards.AutoFitCardsHorizontally or AutoFitCards.AutoFitCardsBothDimensions, then additional width may be added to the Card if horizontal space is available.</para>
		/// </remarks>
		/// <seealso cref="CardWidthProperty"/>
		/// <seealso cref="CardHeight"/>
		/// <seealso cref="AutoFitCards"/>
		/// <seealso cref="AllowCardWidthResizing"/>
		//[Description("Returns/sets the Width of each Card.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public double CardWidth
		{
			get
			{
				return (double)this.GetValue(CardViewSettings.CardWidthProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.CardWidthProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="CardWidth"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeCardWidth()
		{
			return this.CardWidth != (double)CardViewSettings.CardWidthProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="CardWidth"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetCardWidth()
		{
			this.ClearValue(CardWidthProperty);
		}

				#endregion //CardWidth

				#region CollapseCardButtonVisibility

		/// <summary>
		/// Identifies the <see cref="CollapseCardButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CollapseCardButtonVisibilityProperty = DependencyProperty.Register("CollapseCardButtonVisibility",
			typeof(Visibility), typeof(CardViewSettings), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns/sets whether a button is displayed in the Card header which can be clicked by the end user to collapse a Card so it shows its header only.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>If the glyph is clicked when the Card is collapsed, the Card will expand to display its content.</para>
		/// </remarks>
		/// <seealso cref="CollapseCardButtonVisibilityProperty"/>
		/// <seealso cref="AutoFitCards"/>
		//[Description("Returns/sets whether a button is displayed in the Card header which can be clicked by the end user to collapse a Card so it shows its header only.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Visibility CollapseCardButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CardViewSettings.CollapseCardButtonVisibilityProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.CollapseCardButtonVisibilityProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="CollapseCardButtonVisibility"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeCollapseCardButtonVisibility()
		{
			return this.CollapseCardButtonVisibility != (Visibility)CardViewSettings.CollapseCardButtonVisibilityProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="CollapseCardButtonVisibility"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetCollapseCardButtonVisibility()
		{
			this.ClearValue(CollapseCardButtonVisibilityProperty);
		}

				#endregion //CollapseCardButtonVisibility

				#region CollapseEmptyCellsButtonVisibility

		/// <summary>
		/// Identifies the <see cref="CollapseEmptyCellsButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CollapseEmptyCellsButtonVisibilityProperty = DependencyProperty.Register("CollapseEmptyCellsButtonVisibility",
			typeof(Visibility), typeof(CardViewSettings), new FrameworkPropertyMetadata(KnownBoxes.VisibilityCollapsedBox, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns/sets whether a button is displayed in the Card header which can be clicked by the end user to collapse cells that contain empty values (e.g. empty string/null for string, null for nullable types, and 0 for numeric types).
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>If the glyph is clicked when empty Cells are collapsed, the empty Cells will be un-collapsed.</para>
		/// </remarks>
		/// <seealso cref="CollapseEmptyCellsButtonVisibilityProperty"/>
		/// <seealso cref="AutoFitCards"/>
		//[Description("Returns/sets whether a button is displayed in the Card header which can be clicked by the end user to collapse cells that contain empty values (e.g. empty string/null for string, null for nullable types, and 0 for numeric types).")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Visibility CollapseEmptyCellsButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CardViewSettings.CollapseEmptyCellsButtonVisibilityProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.CollapseEmptyCellsButtonVisibilityProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="CollapseEmptyCellsButtonVisibility"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeCollapseEmptyCellsButtonVisibility()
		{
			return this.CollapseEmptyCellsButtonVisibility != (Visibility)CardViewSettings.CollapseEmptyCellsButtonVisibilityProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="CollapseEmptyCellsButtonVisibility"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetCollapseEmptyCellsButtonVisibility()
		{
			this.ClearValue(CollapseEmptyCellsButtonVisibilityProperty);
		}

				#endregion //CollapseEmptyCellsButtonVisibility

				#region HeaderPath

		/// <summary>
		/// Identifies the <see cref="HeaderPath"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderPathProperty = DependencyProperty.Register("HeaderPath",
			typeof(string), typeof(CardViewSettings), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));
			
		/// <summary>
		/// Returns/sets a string that represents the Path to a value on the source object that should be displayed as a Card header.  
		/// </summary>
		/// <remarks>
		/// If left unset, the PrimaryField will be used to get the value on the source object.
		/// </remarks>
		/// <seealso cref="HeaderPathProperty"/>
		//[Description("Returns/sets a string that represents the Path to a value on the source object that should be displayed as a Card header.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public string HeaderPath
		{
			get
			{
				return (string)this.GetValue(CardViewSettings.HeaderPathProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.HeaderPathProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="HeaderPath"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeaderPath()
		{
			return this.HeaderPath != (string)CardViewSettings.HeaderPathProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="HeaderPath"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeaderPath()
		{
			this.ClearValue(HeaderPathProperty);
		}

				#endregion //HeaderPath

				#region HeaderVisibility

		/// <summary>
		/// Identifies the <see cref="HeaderVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderVisibilityProperty = DependencyProperty.Register("HeaderVisibility",
			typeof(Visibility), typeof(CardViewSettings), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox, FrameworkPropertyMetadataOptions.AffectsMeasure));
			
		/// <summary>
		/// Returns/sets the Visibility of the Card header.
		/// </summary>
		/// <seealso cref="HeaderVisibilityProperty"/>
		/// <seealso cref="HeaderPath"/>
		//[Description("Returns/sets the Visibility of the Card header.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public Visibility HeaderVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CardViewSettings.HeaderVisibilityProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.HeaderVisibilityProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="HeaderVisibility"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeaderVisibility()
		{
			return this.HeaderVisibility != (Visibility)CardViewSettings.HeaderVisibilityProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="HeaderVisibility"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeaderVisibility()
		{
			this.ClearValue(HeaderVisibilityProperty);
		}

				#endregion //HeaderVisibility

				#region InterCardSpacingX

		/// <summary>
		/// Identifies the <see cref="InterCardSpacingX"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterCardSpacingXProperty = DependencyProperty.Register("InterCardSpacingX",
			typeof(double), typeof(CardViewSettings), new FrameworkPropertyMetadata((double)4.0, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ValidatePositiveDouble));

		/// <summary>
		/// Returns/sets the horizontal spacing between each Card.
		/// </summary>
		/// <seealso cref="InterCardSpacingXProperty"/>
		/// <seealso cref="InterCardSpacingY"/>
		/// <seealso cref="CardHeight"/>
		/// <seealso cref="CardWidth"/>
		/// <seealso cref="AutoFitCards"/>
		//[Description("Returns/sets the horizontal spacing between each Card.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public double InterCardSpacingX
		{
			get
			{
				return (double)this.GetValue(CardViewSettings.InterCardSpacingXProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.InterCardSpacingXProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="InterCardSpacingX"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeInterCardSpacingX()
		{
			return this.InterCardSpacingX != (double)CardViewSettings.InterCardSpacingXProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="InterCardSpacingX"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetInterCardSpacingX()
		{
			this.ClearValue(InterCardSpacingXProperty);
		}

				#endregion //InterCardSpacingX

				#region InterCardSpacingY

		/// <summary>
		/// Identifies the <see cref="InterCardSpacingY"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterCardSpacingYProperty = DependencyProperty.Register("InterCardSpacingY",
			typeof(double), typeof(CardViewSettings), new FrameworkPropertyMetadata((double)4.0, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ValidatePositiveDouble));

		/// <summary>
		/// Returns/sets the vertical spacing between each Card.
		/// </summary>
		/// <seealso cref="InterCardSpacingYProperty"/>
		/// <seealso cref="InterCardSpacingX"/>
		/// <seealso cref="CardHeight"/>
		/// <seealso cref="CardWidth"/>
		/// <seealso cref="AutoFitCards"/>
		//[Description("Returns/sets the vertical spacing between each Card.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public double InterCardSpacingY
		{
			get
			{
				return (double)this.GetValue(CardViewSettings.InterCardSpacingYProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.InterCardSpacingYProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="InterCardSpacingY"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeInterCardSpacingY()
		{
			return this.InterCardSpacingY != (double)CardViewSettings.InterCardSpacingYProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="InterCardSpacingY"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetInterCardSpacingY()
		{
			this.ClearValue(InterCardSpacingYProperty);
		}

				#endregion //InterCardSpacingY

				#region MaxCardCols

		/// <summary>
		/// Identifies the <see cref="MaxCardCols"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxCardColsProperty = DependencyProperty.Register("MaxCardCols",
			typeof(int), typeof(CardViewSettings), new FrameworkPropertyMetadata((int)0, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ValidateNonNegativeInt));

		/// <summary>
		/// Returns/sets the maximum number of columns of Cards that should be displayed.  A value of zero forces CardView to display as many columns as space will allow.
		/// </summary>
		/// <seealso cref="MaxCardColsProperty"/>
		/// <seealso cref="MaxCardCols"/>
		/// <seealso cref="CardWidth"/>
		//[Description("Returns/sets the maximum number of columns of Cards that should be displayed.  A value of zero forces CardView to display as many columns as space will allow.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public int MaxCardCols
		{
			get
			{
				return (int)this.GetValue(CardViewSettings.MaxCardColsProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.MaxCardColsProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="MaxCardCols"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMaxCardCols()
		{
			return this.MaxCardCols != (int)CardViewSettings.MaxCardColsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="MaxCardCols"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMaxCardCols()
		{
			this.ClearValue(MaxCardColsProperty);
		}

				#endregion //MaxCardCols

				#region MaxCardRows

		/// <summary>
		/// Identifies the <see cref="MaxCardRows"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxCardRowsProperty = DependencyProperty.Register("MaxCardRows",
			typeof(int), typeof(CardViewSettings), new FrameworkPropertyMetadata((int)0, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ValidateNonNegativeInt));

		/// <summary>
		/// Returns/sets the maximum number of rows of Cards that should be displayed.  A value of zero forces CardView to display as many rows as space will allow.
		/// </summary>
		/// <seealso cref="MaxCardRowsProperty"/>
		/// <seealso cref="CardHeight"/>
		/// <seealso cref="ShouldCollapseEmptyCells"/>
		//[Description("Returns/sets the maximum number of rows of Cards that should be displayed.  A value of zero forces CardView to display as many rows as space will allow.")]
		//[Category("Appearance")]
		[Bindable(true)]
		public int MaxCardRows
		{
			get
			{
				return (int)this.GetValue(CardViewSettings.MaxCardRowsProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.MaxCardRowsProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="MaxCardRows"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeMaxCardRows()
		{
			return this.MaxCardRows != (int)CardViewSettings.MaxCardRowsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="MaxCardRows"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetMaxCardRows()
		{
			this.ClearValue(MaxCardRowsProperty);
		}

				#endregion //MaxCardRows

				#region Orientation

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
			typeof(Orientation), typeof(CardViewSettings), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns/sets whether Cards are arranged horizontally (left-to-right top-to-bottom) or vertically (top-to-bottom left-to-right).
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		//[Description("Returns/sets whether Cards are arranged horizontally (left-to-right top-to-bottom) or vertically (top-to-bottom left-to-right).")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Orientation Orientation
		{
			get
			{
				return (Orientation)this.GetValue(CardViewSettings.OrientationProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.OrientationProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="Orientation"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeOrientation()
		{
			return this.Orientation != (Orientation)CardViewSettings.OrientationProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="Orientation"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetOrientation()
		{
			this.ClearValue(OrientationProperty);
		}

				#endregion //Orientation

				#region Padding

		/// <summary>
		/// Identifies the <see cref="Padding"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register("Padding",
			typeof(Thickness), typeof(CardViewSettings), new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns/sets the padding between the outermost cards and the bounds of the control.
		/// </summary>
		/// <seealso cref="PaddingProperty"/>
		//[Description("Returns/sets the padding between the outermost cards and the bounds of the control.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public Thickness Padding
		{
			get
			{
				return (Thickness)this.GetValue(CardViewSettings.PaddingProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.PaddingProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="Padding"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializePadding()
		{
			return this.Padding != (Thickness)CardViewSettings.PaddingProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="Padding"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetPadding()
		{
			this.ClearValue(PaddingProperty);
		}

				#endregion //Padding

				#region RepositionAnimation

		private static DependencyProperty s_RepositionAnimation;

		/// <summary>
		/// Identifies the <see cref="RepositionAnimation"/> dependency property
		/// </summary>
		public static DependencyProperty RepositionAnimationProperty { get { return s_RepositionAnimation; } }

		/// <summary>
		/// Returns/sets a DoubleAnimationBase derived instance that should be used when animating Cards to new layout positions.
		/// </summary>
		/// <remarks>
		/// The supplied instance should animate a double value from 0 to 1 using whatever other animation options are desired.  
		/// CardView will interpolate the 0-1 values to represent the correct position for Cards at each frame of the animation.  
		/// If the supplied animation does not animate from 0 to 1, unintended animation behaviors could result.
		/// 
		/// As an exception to this, note that it is perfectly reasonable to construct a DoubleAnimationUsingKeyFrames animation that
		/// 'temporarily' goes outside the 0.0-1.0 range in some intermediate KeyFrames to simulate easing.
		/// 
		/// <para class="note"><b>Note: </b>If you do specify a 'To' value that is not 1.0 you must specify a FillBehavior of 'Stop' for the animation
		/// to work properly.  </para>		
		/// </remarks>
		/// <seealso cref="RepositionAnimationProperty"/>
		//[Description("Returns/sets a DoubleAnimationBase derived instance that should be used when animating Cards to new layout positions.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public DoubleAnimationBase RepositionAnimation
		{
			get
			{
				return (DoubleAnimationBase)this.GetValue(CardViewSettings.RepositionAnimationProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.RepositionAnimationProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="RepositionAnimation"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeRepositionAnimation()
		{
			return this.RepositionAnimation != (DoubleAnimationBase)CardViewSettings.RepositionAnimationProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="RepositionAnimation"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetRepositionAnimation()
		{
			this.ClearValue(RepositionAnimationProperty);
		}

				#endregion //RepositionAnimation

				#region ShouldAnimateCardPositioning

		/// <summary>
		/// Identifies the <see cref="ShouldAnimateCardPositioning"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShouldAnimateCardPositioningProperty = DependencyProperty.Register("ShouldAnimateCardPositioning",
			typeof(bool), typeof(CardViewSettings), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns/sets whether Cards should be animated into their new positions when the <see cref="CardView"/> is scrolled.
		/// </summary>
		/// <seealso cref="ShouldAnimateCardPositioningProperty"/>
		//[Description("Returns/sets whether Cards should be animated into their new positions when the CardView is scrolled.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool ShouldAnimateCardPositioning
		{
			get
			{
				return (bool)this.GetValue(CardViewSettings.ShouldAnimateCardPositioningProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.ShouldAnimateCardPositioningProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ShouldAnimateCardPositioning"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeShouldAnimateCardPositioning()
		{
			return this.ShouldAnimateCardPositioning != (bool)CardViewSettings.ShouldAnimateCardPositioningProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ShouldAnimateCardPositioning"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetShouldAnimateCardPositioning()
		{
			this.ClearValue(ShouldAnimateCardPositioningProperty);
		}

				#endregion //ShouldAnimateCardPositioning

				#region ShouldCollapseCards

		/// <summary>
		/// Identifies the <see cref="ShouldCollapseCards"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShouldCollapseCardsProperty = DependencyProperty.Register("ShouldCollapseCards",
			typeof(bool), typeof(CardViewSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns/sets whether Cards should be collapsed.
		/// </summary>
		/// <seealso cref="ShouldCollapseCardsProperty"/>
		/// <seealso cref="CollapseCardButtonVisibility"/>
		/// <seealso cref="ShouldCollapseEmptyCells"/>
		//[Description("Returns/sets whether Cards should be collapsed.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool ShouldCollapseCards
		{
			get
			{
				return (bool)this.GetValue(CardViewSettings.ShouldCollapseCardsProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.ShouldCollapseCardsProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ShouldCollapseCards"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeShouldCollapseCards()
		{
			return this.ShouldCollapseCards != (bool)CardViewSettings.ShouldCollapseCardsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ShouldCollapseCards"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetShouldCollapseCards()
		{
			this.ClearValue(ShouldCollapseCardsProperty);
		}

				#endregion //ShouldCollapseCards

				#region ShouldCollapseEmptyCells

		/// <summary>
		/// Identifies the <see cref="ShouldCollapseEmptyCells"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShouldCollapseEmptyCellsProperty = DependencyProperty.Register("ShouldCollapseEmptyCells",
			typeof(bool), typeof(CardViewSettings), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns/sets whether cells whose value is determined to be empty (e.g. empty string/null for string, null for nullable types, and 0 for numeric types) will be collapsed and not displayed within the record.
		/// </summary>
		/// <seealso cref="ShouldCollapseEmptyCellsProperty"/>
		/// <seealso cref="CollapseEmptyCellsButtonVisibility"/>
		/// <seealso cref="ShouldCollapseCards"/>
		//[Description("Returns/sets whether cells whose value is determined to be empty (e.g. empty string/null for string, null for nullable types, and 0 for numeric types) will be collapsed and not displayed within the record.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool ShouldCollapseEmptyCells
		{
			get
			{
				return (bool)this.GetValue(CardViewSettings.ShouldCollapseEmptyCellsProperty);
			}
			set
			{
				this.SetValue(CardViewSettings.ShouldCollapseEmptyCellsProperty, value);
			}
		}

		/// <summary>
		/// Determines if the <see cref="ShouldCollapseEmptyCells"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeShouldCollapseEmptyCells()
		{
			return this.ShouldCollapseEmptyCells != (bool)CardViewSettings.ShouldCollapseEmptyCellsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="ShouldCollapseEmptyCells"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetShouldCollapseEmptyCells()
		{
			this.ClearValue(ShouldCollapseEmptyCellsProperty);
		}

				#endregion //ShouldCollapseEmptyCells

			#endregion //Public Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region EnumeratePropertiesWithNonDefaultValues

		internal void EnumeratePropertiesWithNonDefaultValues( PropertyChangedEventHandler callback )
		{
			LocalValueEnumerator enumerator = this.GetLocalValueEnumerator();

			while (enumerator.MoveNext())
			{
				LocalValueEntry entry = enumerator.Current;

				if (!Object.Equals(entry.Property.DefaultMetadata.DefaultValue, entry.Value))
					callback( this, new PropertyChangedEventArgs(entry.Property.Name));
			}
		}

				#endregion //EnumeratePropertiesWithNonDefaultValues	

			#endregion //Internal Methods

			#region Private methods

				#region ValidatePositiveDouble

		private static bool ValidatePositiveDouble(object value)
		{
			if (!(value is double))
				return false;

			if (double.IsNaN((double)value))
				return true;

			return !((double)value <= 0.0);
		}

				#endregion //ValidatePositiveDouble

				#region ValidateNonNegativeInt

		private static bool ValidateNonNegativeInt(object value)
		{
			if (!(value is int))
				return false;

			return (int)value >= 0.0;
		}

				#endregion //ValidateNonNegativeInt

			#endregion //Private methods

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