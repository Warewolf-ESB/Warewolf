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

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
    /// An object that exposes properties for controlling the features supported by the <see cref="GridView"/>. The GridView object is used by <see cref="XamDataGrid"/> and <see cref="XamDataPresenter"/>
	/// </summary>
    /// <remarks>
    /// <p class="body">By manipulating properties on the GridViewSettings object you can control how the <see cref="GridViewPanelFlat"/> or <see cref="GridViewPanelNested"/> arranges items.</p>
    /// <p class="body">The properties exposed by the GridViewSettings object are:
    ///		<ul>
    ///			<li><see cref="GridViewSettings.HeightInInfiniteContainers"/> - Returns/sets a value that is used as default for the height of the control implementing the view when used in a container with infinite height available.</li>
    ///			<li><see cref="GridViewSettings.Orientation"/> - Determines the orientation of records.</li>
    ///			<li><see cref="GridViewSettings.WidthInInfiniteContainers"/> - Returns/sets a value that is used as default for the width of the control implementing the view when used in a container with infinite width available.</li>
    ///			<li><see cref="GridViewSettings.UseNestedPanels"/> - Determines whether a single <see cref="GridViewPanelFlat"/> is used to arrange records or whether multiple <see cref="GridViewPanelNested"/> panels are used.</li>
    ///		</ul>
    /// Refer to the documentation contained within for a complete list of the properties supported by this class and the functionality enabled by each property.
    /// </p>
    /// </remarks>
    /// <seealso cref="GridView"/>
    /// <seealso cref="GridViewPanelFlat"/>
    /// <seealso cref="GridViewPanelNested"/>
    public class GridViewSettings : ViewSettingsBase
	{
		#region Constructor

		/// <summary>
		/// Creates an instance of GridViewSettings.
		/// </summary>
		/// <seealso cref="GridView"/>
        /// <seealso cref="GridView.ViewSettings"/>
        /// <seealso cref="GridViewPanel"/>
		/// <seealso cref="XamDataGrid"/>
		/// <seealso cref="XamDataPresenter.View"/>
		/// <remarks>
        /// <p class="body">The <see cref="GridView"/> will automatically create an instance of this class when its <see cref="GridView.ViewSettings"/> property is accessed.
        /// You can also create one manually and assign it to the <see cref="GridView"/>'s <see cref="GridView.ViewSettings"/> property of one or more instances of <see cref="GridView"/> if needed.  </p>
		/// </remarks>
		public GridViewSettings()
		{
		}

		#endregion //Constructor

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
			this.ResetOrientation();
            // JJD 7/21/09
            this.ResetHeightInInfiniteContainers();
            this.ResetWidthInInfiniteContainers();
            this.ResetUseNestedPanels();
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
			// AS 6/16/06
			//return this.ShouldSerializeOrientation();
			return this.ShouldSerializeHeightInInfiniteContainers() ||
				this.ShouldSerializeOrientation() ||
				this.ShouldSerializeUseNestedPanels() ||
				this.ShouldSerializeWidthInInfiniteContainers();
		}

			#endregion //ShouldSerialize

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region HeightInInfiniteContainers

		/// <summary>
		/// Identifies the <see cref="HeightInInfiniteContainers"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeightInInfiniteContainersProperty = DependencyProperty.Register("HeightInInfiniteContainers",
			typeof(double), typeof(GridViewSettings), new FrameworkPropertyMetadata(double.NaN), new ValidateValueCallback(ValidatePositiveDouble));

		/// <summary>
		/// Returns/sets a value that is used as default for the height of the control implementing the view when used in a container with infinite height available.
		/// </summary>
		/// <remarks>
        /// <p class="body">Certain controls such as <see cref="System.Windows.Controls.ScrollViewer"/> and <see cref="System.Windows.Controls.StackPanel"/> make an infinite amount of height and width available to the controls they contain.
        /// If you place a control that implements a GridView (e.g., <see cref="XamDataGrid"/> or <see cref="XamDataPresenter"/> with a <see cref="XamDataPresenter.View"/> set to an instance of <see cref="GridView"/>) inside one of these controls you may want to set this property to constrain the
        /// height of the control to a convenient value.  If you don't set this property a default height of <see cref="System.Windows.SystemParameters.VirtualScreenHeight"/> will be used.</p>
        /// </remarks>
		/// <seealso cref="WidthInInfiniteContainers"/>
		//[Description("Returns/sets a value that is used as default for the height of the control implementing the view when used in a container with infinite height available.")]
		//[Category("Appearance")]
		public double HeightInInfiniteContainers
		{
			get { return (double)this.GetValue(GridViewSettings.HeightInInfiniteContainersProperty); }
			set { this.SetValue(GridViewSettings.HeightInInfiniteContainersProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="HeightInInfiniteContainers"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeHeightInInfiniteContainers()
		{
			return !double.IsNaN(this.HeightInInfiniteContainers);
		}

		/// <summary>
		/// Resets the <see cref="HeightInInfiniteContainers"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetHeightInInfiniteContainers()
		{
			this.ClearValue(HeightInInfiniteContainersProperty);
		}

				#endregion //HeightInInfiniteContainers

				#region Orientation

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
			typeof(Orientation), typeof(GridViewSettings), 
			new FrameworkPropertyMetadata(Orientation.Vertical,
				
				
				
				
				
				
				
				
				new PropertyChangedCallback( OnOrientationChanged )
			)
			, new ValidateValueCallback(OnValidateOrientation)
		);

		private static bool OnValidateOrientation(object value)
		{
			if (!Enum.IsDefined(typeof(Orientation), value))
				throw new ArgumentException();

			return true;
		}

		
		
		
		
		
		private static void OnOrientationChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			GridViewSettings gridViewSettings = (GridViewSettings)dependencyObject;
			Orientation newVal = (Orientation)e.NewValue;

			gridViewSettings._cachedOrientation = newVal;
		}

		private Orientation _cachedOrientation = Orientation.Vertical;

		
		
		
		
		
		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Determines the orientation of records.
		/// </summary>
		//[Description("Determines the orientation of records.")]
		//[Category("Behavior")]
		public Orientation Orientation
		{
			get 
			{
				// JJD 4/26/07
				// Optimization - use the locally cached property 
				//return (Orientation)this.GetValue(GridViewSettings.OrientationProperty); 
				return this._cachedOrientation;
			}
			set { this.SetValue(GridViewSettings.OrientationProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="Orientation"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeOrientation()
		{
			return this.Orientation != (Orientation)GridViewSettings.OrientationProperty.DefaultMetadata.DefaultValue;
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

				#region WidthInInfiniteContainers

		/// <summary>
		/// Identifies the <see cref="WidthInInfiniteContainers"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WidthInInfiniteContainersProperty = DependencyProperty.Register("WidthInInfiniteContainers",
			typeof(double), typeof(GridViewSettings), new FrameworkPropertyMetadata(double.NaN), new ValidateValueCallback(ValidatePositiveDouble));

		/// <summary>
		/// Returns/sets a value that is used as default for the width of the control implementing the view when used in a container with infinite width available.
		/// </summary>
		/// <remarks>
        /// <p class="body">Certain controls such as <see cref="System.Windows.Controls.ScrollViewer"/> and <see cref="System.Windows.Controls.StackPanel"/> make an infinite amount of height and width available to the controls they contain.
        /// If you place a control that implements a GridView (e.g., <see cref="XamDataGrid"/> or <see cref="XamDataPresenter"/> with a <see cref="XamDataPresenter.View"/> set to an instance of <see cref="GridView"/>) inside one of these controls you may want to set this property to constrain the
        /// width of the control to a convenient value.  If you don't set this property a default width of <see cref="System.Windows.SystemParameters.VirtualScreenWidth"/> will be used.</p>
        /// </remarks>
		/// <seealso cref="WidthInInfiniteContainers"/>
		//[Description("Returns/sets a value that is used as default for the width of the control implementing the view when used in a container with infinite width available.")]
		//[Category("Appearance")]
		public double WidthInInfiniteContainers
		{
			get { return (double)this.GetValue(GridViewSettings.WidthInInfiniteContainersProperty); }
			set { this.SetValue(GridViewSettings.WidthInInfiniteContainersProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="WidthInInfiniteContainers"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWidthInInfiniteContainers()
		{
			return !double.IsNaN(this.WidthInInfiniteContainers);
		}

		/// <summary>
		/// Resets the <see cref="WidthInInfiniteContainers"/> property to its default value.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWidthInInfiniteContainers()
		{
			this.ClearValue(GridViewSettings.WidthInInfiniteContainersProperty);
		}

				#endregion //WidthInInfiniteContainers

                // JJD 7/20/09 - NA 2009 vol 2 - Enhanced grid view
				#region UseNestedPanels

		/// <summary>
		/// Identifies the <see cref="UseNestedPanels"/> dependency property
		/// </summary>
        /// <seealso cref="UseNestedPanels"/>
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_EnhancedGridView, Version = FeatureInfo.Version_9_2)]
        public static readonly DependencyProperty UseNestedPanelsProperty = DependencyProperty.Register("UseNestedPanels",
			typeof(bool), typeof(GridViewSettings), 
			new FrameworkPropertyMetadata(KnownBoxes.FalseBox,
				new PropertyChangedCallback( OnUseNestedPanelsChanged )
			)
		);

		private static void OnUseNestedPanelsChanged( DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e )
		{
			GridViewSettings gridViewSettings = (GridViewSettings)dependencyObject;
			bool newVal = (bool)e.NewValue;

			gridViewSettings._cachedUseNestedPanels = newVal;
		}

        private bool _cachedUseNestedPanels = (bool)GridViewSettings.UseNestedPanelsProperty.DefaultMetadata.DefaultValue;

		/// <summary>
        /// Determines how items panels are used to display hierarchical data.
		/// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> setting this property to 'False' is more performant with deep nested structures 
        /// (e.g. with hierarchical data and/or multiple groupby fields) since it uses a single <see cref="GridViewPanelFlat"/>
        /// to arrange <see cref="RecordPresenter"/>s. However, it can not make use of each <see cref="RecordPresenter"/>'s <see cref="RecordPresenter.NestedContent"/> and therefore will 
        /// not pick up any chrome or animations relating to that nested content.</para>
        /// </remarks>
        /// <seealso cref="GridViewPanelFlat"/>
        /// <seealso cref="GridViewPanelNested"/>
        //[Description("Determines how items panels are used to display hierarchical data.")]
		//[Category("Behavior")]
        [InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_EnhancedGridView, Version = FeatureInfo.Version_9_2)]
        public bool UseNestedPanels
		{
			get 
			{
				return this._cachedUseNestedPanels;
			}
			set { this.SetValue(GridViewSettings.UseNestedPanelsProperty, value); }
		}

		/// <summary>
		/// Determines if the <see cref="UseNestedPanels"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeUseNestedPanels()
		{
			return this.UseNestedPanels != (bool)GridViewSettings.UseNestedPanelsProperty.DefaultMetadata.DefaultValue;
		}

		/// <summary>
		/// Resets the <see cref="UseNestedPanels"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetUseNestedPanels()
		{
			this.ClearValue(UseNestedPanelsProperty);
		}

				#endregion //UseNestedPanels	

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