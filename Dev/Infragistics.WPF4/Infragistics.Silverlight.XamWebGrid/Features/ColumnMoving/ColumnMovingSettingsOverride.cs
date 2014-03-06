using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// An object that contains settings for using ColumnMovingSettings on a particular <see cref="ColumnLayout"/>
	/// </summary>
	public class ColumnMovingSettingsOverride : SettingsOverrideBase
	{
		#region Members

		MovingIndicator _indicator;
		Popup _indicatorContainer, _contentContainer;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ColumnMovingSettingsOverride"/> class.
		/// </summary>		
		public ColumnMovingSettingsOverride()
		{
			this._indicator = new MovingIndicator();
			this._indicatorContainer = new Popup();
			this._indicatorContainer.Child = this._indicator;
			this._contentContainer = new Popup();

            this._contentContainer.Placement = PlacementMode.Relative;
            this._contentContainer.AllowsTransparency = true;

            this._indicatorContainer.Placement = PlacementMode.Relative;
            this._indicatorContainer.AllowsTransparency = true;

		}

		#endregion // Constructor

		#region Properties

		#region Public

		#region IndicatorStyle

		/// <summary>
		/// Identifies the <see cref="IndicatorStyle"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty IndicatorStyleProperty = DependencyProperty.Register("IndicatorStyle", typeof(Style), typeof(ColumnMovingSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(IndicatorStyleChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="Style"/> that should be applied to the visual object.
		/// </summary>
		public Style IndicatorStyle
		{
			get { return (Style)this.GetValue(IndicatorStyleProperty); }
			set { this.SetValue(IndicatorStyleProperty, value); }
		}

		private static void IndicatorStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnMovingSettingsOverride settings = (ColumnMovingSettingsOverride)obj;
			settings.OnPropertyChanged("IndicatorStyle");
		}

		#endregion // IndicatorStyle

		#region IndicatorStyleResolved

		/// <summary>
		/// Gets the actual <see cref="Style"/> of the visual object.
		/// </summary>
		public Style IndicatorStyleResolved
		{
			get
			{
				if (this.IndicatorStyle == null && this.SettingsObject != null)
					return ((ColumnMovingSettings)this.SettingsObject).IndicatorStyle;
				else
					return this.IndicatorStyle;
			}
		}

		#endregion // IndicatorStyleResolved

		#region AllowColumnMoving

		/// <summary>
		/// Identifies the <see cref="AllowColumnMoving"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AllowColumnMovingTypeProperty = DependencyProperty.Register("AllowColumnMoving", typeof(ColumnMovingType?), typeof(ColumnMovingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(AllowColumnMovingTypeChanged)));

		/// <summary>
		/// Gets/sets how column moving will work for a particular <see cref="ColumnLayout"/> object in the <see cref="XamGrid"/>
		/// </summary>
		[TypeConverter(typeof(NullableEnumTypeConverter<ColumnMovingType>))]
		public ColumnMovingType? AllowColumnMoving
		{
			get { return (ColumnMovingType?)this.GetValue(AllowColumnMovingTypeProperty); }
			set { this.SetValue(AllowColumnMovingTypeProperty, value); }
		}

		private static void AllowColumnMovingTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnMovingSettingsOverride settings = (ColumnMovingSettingsOverride)obj;
			settings.OnPropertyChanged("AllowColumnMoving");
		}

		#endregion // AllowColumnMoving

		#region AllowColumnMovingResolved

		/// <summary>
		/// Resolves the type of Column moving supported for this <see cref="ColumnLayout"/>
		/// </summary>
		public ColumnMovingType AllowColumnMovingResolved
		{
            get
            {
                if (this.AllowColumnMoving == null)
                {
                    if (this.SettingsObject != null)
                        return ((ColumnMovingSettings)this.SettingsObject).AllowColumnMoving;
                }
                else
                    return (ColumnMovingType)this.AllowColumnMoving;

                return (ColumnMovingType)ColumnMovingSettings.AllowColumnMovingTypeProperty.GetMetadata(typeof(ColumnMovingSettings)).DefaultValue;
            }
		}

		#endregion //AllowColumnMovingResolved

		#region EasingFunction

		/// <summary>
		/// Identifies the <see cref="EasingFunction"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register("EasingFunction", typeof(EasingFunctionBase), typeof(ColumnMovingSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(EasingFunctionChanged)));

		/// <summary>
		/// Gets/Sets the <see cref="EasingFunctionBase"/> that will be used to animate the immediate moving of columns.
		/// </summary>
		public EasingFunctionBase EasingFunction
		{
			get { return (EasingFunctionBase)this.GetValue(EasingFunctionProperty); }
			set { this.SetValue(EasingFunctionProperty, value); }
		}

		private static void EasingFunctionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnMovingSettingsOverride settings = (ColumnMovingSettingsOverride)obj;
			settings.OnPropertyChanged("EasingFunction");
		}

		#endregion // EasingFunction

		#region EasingFunctionResolved

		/// <summary>
		/// Resolves the type of EasingFunctionBase supported for this <see cref="ColumnLayout"/>
		/// </summary>
		public EasingFunctionBase EasingFunctionResolved
		{
            get
            {
                if (this.EasingFunction == null)
                {
                    if (this.SettingsObject != null)
                        return ((ColumnMovingSettings)this.SettingsObject).EasingFunction;
                }
                else
                    return this.EasingFunction;

                return (EasingFunctionBase)ColumnMovingSettings.EasingFunctionProperty.GetMetadata(typeof(ColumnMovingSettings)).DefaultValue;
            }
		}

		#endregion //EasingFunctionResolved

		#region AnimationDuration

		/// <summary>
		/// Identifies the <see cref="AnimationDuration"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register("AnimationDuration", typeof(int?), typeof(ColumnMovingSettingsOverride), new PropertyMetadata(new PropertyChangedCallback(AnimationDurationChanged)));

		/// <summary>
		/// Gets/sets the duration of the animation of moving a column immediately. 
		/// </summary>
		[TypeConverter(typeof(NullableIntConverter))]
		public int? AnimationDuration
		{
			get { return (int?)this.GetValue(AnimationDurationProperty); }
			set { this.SetValue(AnimationDurationProperty, value); }
		}

		private static void AnimationDurationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			ColumnMovingSettingsOverride settings = (ColumnMovingSettingsOverride)obj;
			settings.OnPropertyChanged("AnimationDuration");
		}

		#endregion // AnimationDuration

		#region AnimationDurationResolved

		/// <summary>
		/// Resolves the type of duration of the animation supported for this <see cref="ColumnLayout"/>
		/// </summary>
		public int AnimationDurationResolved
		{
            get
            {
                if (this.AnimationDuration == null)
                {
                    if (this.SettingsObject != null)
                        return ((ColumnMovingSettings)this.SettingsObject).AnimationDuration;
                }
                else
                    return (int)this.AnimationDuration;

                return (int)ColumnMovingSettings.AnimationDurationProperty.GetMetadata(typeof(ColumnMovingSettings)).DefaultValue;
            }
		}

		#endregion //AnimationDurationResolved

		#endregion // Public

		#region Internal

		#region Indicator

		internal MovingIndicator Indicator
		{
			get
			{
				Style resolved = this.IndicatorStyleResolved;
				if (this._indicator.Style != resolved && resolved != null)
					this._indicator.Style = resolved;

				return this._indicator;
			}
		}

		internal Popup IndicatorContainer
		{
			get { return this._indicatorContainer; }
		}

		internal Popup ContentContainer
		{
			get { return this._contentContainer; }
		}

		#endregion // Indicator

		#endregion // Internal

		#endregion // Properties

		#region Overrides

		#region SettingsObject

		/// <summary>
		/// Gets the <see cref="SettingsBase"/> that is the counterpart to this <see cref="SettingsOverrideBase"/>
		/// </summary>
		protected override SettingsBase SettingsObject
		{
			get
			{
				SettingsBase settings = null;
				if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
					settings = this.ColumnLayout.Grid.ColumnMovingSettings;
				return settings;
			}
		}

		#endregion // SettingsObject

		#endregion // Overrides
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