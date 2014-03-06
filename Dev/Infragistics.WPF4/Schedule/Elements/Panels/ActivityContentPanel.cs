using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using Infragistics.AutomationPeers;
using System.Globalization;
using System.ComponentModel;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// A custom panel for arranging the content of an <see cref="ActivityPresenter"/>
	/// </summary>
	[DesignTimeVisible(false)]
	public class ActivityContentPanel : Panel
	{
		#region Member Variables

		private UIElement _prefix = null;
		private UIElement _suffix = null;
		private UIElement _indicators = null;
		private UIElement _content = null;
		private ActivityPresenter _activityPresenter;
		private bool _indicatorsConstrainWidth;
		private bool _indicatorsConstrainHeight;

		private double _lastMeasureWidth;

		#endregion // Member Variables

		#region Constructor
		static ActivityContentPanel()
		{
		}

		/// <summary>
		/// Initializes a new <see cref="ActivityContentPanel"/>
		/// </summary>
		public ActivityContentPanel()
		{
		} 
		#endregion // Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			Size sizeUsed = new Size();

			double prefixWidth = _prefix != null ? _prefix.DesiredSize.Width : 0;
			double suffixWidth = _suffix != null ? _suffix.DesiredSize.Width : 0;
			double indicatorWidth = _indicators != null ? _indicators.DesiredSize.Width : 0;
			double indicatorHeight = _indicators != null ? _indicators.DesiredSize.Height : 0;
			double contentWidth = _content != null ? _content.DesiredSize.Width : 0;
			double maxArrangeWidth = Math.Min(_lastMeasureWidth, finalSize.Width);
			double interAreaSpacing = this.InterAreaSpacing;

			bool isSingleLine = this._activityPresenter != null ? this._activityPresenter.IsSingleLineDisplay : true;

			HorizontalAlignment contentAlignment = this.ContentAreaAlignment;

			if (prefixWidth < 5 && _prefix != null)
			{
				prefixWidth = 0;

				if (suffixWidth < 5 ||
					_activityPresenter != null && _activityPresenter.PrefixFormatType != DateRangeFormatType.None)
					suffixWidth = 0;
			}

			if (suffixWidth < 5 &&
				_suffix != null &&
				_activityPresenter != null && _activityPresenter.SuffixFormatType != DateRangeFormatType.None)
			{
				suffixWidth = 0;
				prefixWidth = 0;
			}

			double spacingRequired = prefixWidth > 0 ? interAreaSpacing : 0;

			if (suffixWidth > 0)
				spacingRequired += interAreaSpacing;

			if (_indicatorsConstrainWidth)
				spacingRequired += interAreaSpacing;
			
			double nonContentWidthRequired = prefixWidth + suffixWidth;

			if (_indicatorsConstrainWidth)
				nonContentWidthRequired += indicatorWidth;

			if (nonContentWidthRequired + contentWidth + spacingRequired < maxArrangeWidth)
			{
				contentWidth = maxArrangeWidth - (nonContentWidthRequired + spacingRequired);
			}
			else if ((prefixWidth > 0 || suffixWidth > 0) &&
				nonContentWidthRequired + contentWidth + spacingRequired > maxArrangeWidth)
			{
				double minContentAndIndicatorsWidth = this.ContentAreaMinWidth;

				if (_indicatorsConstrainWidth)
					minContentAndIndicatorsWidth += indicatorWidth;

				if (minContentAndIndicatorsWidth + prefixWidth + suffixWidth + spacingRequired > maxArrangeWidth)
				{
					suffixWidth = 0;
					prefixWidth = 0;
				}
				else
				{
					if (_indicatorsConstrainWidth)
						contentWidth = Math.Max(maxArrangeWidth - (prefixWidth + suffixWidth + indicatorWidth + spacingRequired), 0);
					else
						contentWidth = Math.Max(maxArrangeWidth - (prefixWidth + suffixWidth + spacingRequired), 0);
				}

			}

			double offset = 0;
			double remainingWidth = maxArrangeWidth;
			double heightForContent = finalSize.Height;

			if (_indicatorsConstrainHeight)
				heightForContent = Math.Max(heightForContent - indicatorHeight, 0);

			// arrange the prefix
			if (_prefix != null)
			{
				if (prefixWidth > 0)
				{
					_prefix.Arrange(new Rect(new Point(0, 0), new Size(prefixWidth, heightForContent)));

					offset = Math.Min( prefixWidth,_prefix.DesiredSize.Width) + interAreaSpacing;

					remainingWidth = Math.Max(remainingWidth - offset, 0);

					sizeUsed.Height = Math.Max(sizeUsed.Height, _prefix.DesiredSize.Height);
				}
				else
				{
					// AS 1/10/12 TFS98969
					//_prefix.Arrange(new Rect(new Point(-10000, -10000), finalSize));
					_prefix.Arrange(new Rect(-10000, -10000, 0, 0));
				}
			}

			double suffixWidthUsed = 0;

			// arrange the suffix
			if (_suffix!= null)
			{
				if (suffixWidth > 0)
				{
					Rect arrangeRect = new Rect(new Point(offset + contentWidth + interAreaSpacing, 0), new Size(suffixWidth, heightForContent));

					if ( _indicatorsConstrainWidth )
						arrangeRect.X += indicatorWidth + interAreaSpacing;

					_suffix.Arrange(arrangeRect);
				
					suffixWidthUsed = Math.Min(suffixWidth, _suffix.DesiredSize.Width);

					remainingWidth = Math.Max(remainingWidth - (suffixWidthUsed + interAreaSpacing), 0);

					sizeUsed.Height = Math.Max(sizeUsed.Height, _suffix.DesiredSize.Height);
				}
				else
				{
					// AS 1/10/12 TFS98969
					//_suffix.Arrange(new Rect(new Point(-10000, -10000), finalSize));
					_suffix.Arrange(new Rect(-10000, -10000, 0, 0));
				}
			}

			if (_indicatorsConstrainWidth)
				remainingWidth = Math.Max(remainingWidth - (indicatorWidth + interAreaSpacing), 0);

			double indicatorOffset = 0;

			// arrange the content
			if (_content != null)
			{
				double contentArrangeWidth;
				double contentOffset;

				switch (contentAlignment)
				{
					default:
					case System.Windows.HorizontalAlignment.Stretch:
					case System.Windows.HorizontalAlignment.Left:
						contentOffset = offset;
						contentArrangeWidth = Math.Max(Math.Min(remainingWidth, contentWidth), 0);
						break;
					case System.Windows.HorizontalAlignment.Right:
						contentOffset = offset + Math.Max(remainingWidth - _content.DesiredSize.Width, 0);
						contentArrangeWidth = contentWidth;
						break;
					case System.Windows.HorizontalAlignment.Center:
						contentOffset = offset + Math.Max((remainingWidth - _content.DesiredSize.Width) / 2, 0);
						contentArrangeWidth = Math.Max(Math.Min(remainingWidth, _content.DesiredSize.Width), 0);
						break;
				}

				Rect arrangeRect = new Rect(new Point(contentOffset, 0), new Size(contentArrangeWidth, heightForContent));
				
				_content.Arrange(arrangeRect);

				offset += remainingWidth;
				
				sizeUsed.Height = Math.Max(sizeUsed.Height, _content.DesiredSize.Height);

				switch (contentAlignment)
				{
					default:
					case System.Windows.HorizontalAlignment.Stretch:
					case System.Windows.HorizontalAlignment.Left:
					case System.Windows.HorizontalAlignment.Right:
						indicatorOffset = offset;
						break;
					case System.Windows.HorizontalAlignment.Center:
						indicatorOffset = contentOffset + contentArrangeWidth;
						break;
				}
			}

			// arrange the indicators
			if (_indicators != null)
			{
				Rect arrangeRect = new Rect(new Point(indicatorOffset + interAreaSpacing, 0), new Size(indicatorWidth, indicatorHeight));

				if (isSingleLine == false)
				{
					arrangeRect.X = Math.Max(finalSize.Width - (indicatorWidth + 1), 0);
					arrangeRect.Y = Math.Max(finalSize.Height - (indicatorHeight + 1), 0);
				}
				else
				{
					// AS 4/1/11 TFS64812
					// Give the element the full height so it has the option of how to handle its vertical alignment.
					//
					arrangeRect.Height = finalSize.Height;
				}
				
				_indicators.Arrange(arrangeRect);

				if (isSingleLine)
					sizeUsed.Height = Math.Max(sizeUsed.Height, indicatorHeight);
				else
				{
					sizeUsed.Height = Math.Max(sizeUsed.Height, arrangeRect.Y + indicatorHeight);

					maxArrangeWidth = finalSize.Width;
				}
			}

			sizeUsed.Width = Math.Max(maxArrangeWidth, offset + suffixWidthUsed);

			return sizeUsed;
		}

		#endregion //ArrangeOverride

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			if ( _activityPresenter == null )
				_activityPresenter = PresentationUtilities.GetVisualAncestor<ActivityPresenter>(this, null);

			Debug.Assert(_activityPresenter != null, "ActivityPresenter not found");

			_lastMeasureWidth = availableSize.Width;

			Size desiredSize = new Size();

			_prefix = null;
			_suffix = null;
			_indicators = null;
			_content = null;
			_indicatorsConstrainWidth = false;
			_indicatorsConstrainHeight = false;

			UIElementCollection children = this.Children;
			int count = children.Count;

			double interAreaSpacing = this.InterAreaSpacing;
			double indicatorsHeight = 0;
			double indicatorsWidth = 0;

			bool isSingleLine = this._activityPresenter != null ? this._activityPresenter.IsSingleLineDisplay : true;

			for (int i = 0; i < count; i++)
			{
				UIElement child = children[i];

				if (child.Visibility == System.Windows.Visibility.Collapsed)
					continue;

				ActivityContentArea area = GetArea(child);

				switch (area)
				{
					case ActivityContentArea.Prefix:
						if (_prefix == null)
							_prefix = child;
						else
							continue;
						break;
					case ActivityContentArea.Suffix:
						if (_suffix == null)
							_suffix = child;
						else
							continue;
						break;
					case ActivityContentArea.Indicators:
						if (_indicators == null)
							_indicators = child;
						else
							continue;
						break;
					default:
					case ActivityContentArea.Content:
						if (_content == null)
							_content = child;
						else
							continue;
						break;
				}

				child.Measure(availableSize);

				double childWidth =  child.DesiredSize.Width;

				if (child == _indicators)
				{
					indicatorsHeight = child.DesiredSize.Height;
					indicatorsWidth = childWidth;

					if (isSingleLine)
						_indicatorsConstrainHeight = false;
					else
					{
						ScheduleControlBase control = _activityPresenter != null ? _activityPresenter.Control : null;

						double singleLineHeight = control != null ? control.SingleLineActivityHeight : 25;

						double aspectRatio;

						if (!double.IsPositiveInfinity(availableSize.Width) &&
							!double.IsPositiveInfinity(availableSize.Height) &&
							availableSize.Height > 0)
							aspectRatio = Math.Max(availableSize.Width / availableSize.Height, 1);
						else
							aspectRatio = 1;


						// if the aspect ratio (width/height) is less than 3 and the height minus the indicator
						// is greater than the single line height times the aspect ration then
						// we want to constrain the height instead of the width
						// JJD 4/5/11 - TFS65511
						// Allow for a wider aspect ratio when determining if we should constrain the height
						//_indicatorsConstrainHeight = aspectRatio < 3 && availableSize.Height - indicatorsHeight > singleLineHeight * aspectRatio;
						_indicatorsConstrainHeight = aspectRatio < 6 && availableSize.Height - indicatorsHeight > (singleLineHeight * aspectRatio / 2);
					}

					_indicatorsConstrainWidth = !this._indicatorsConstrainHeight;

					if (_indicatorsConstrainWidth)
						childWidth += interAreaSpacing;
				}
				else
				{
					if (child != _content &&
						childWidth > 0)
						childWidth += interAreaSpacing;
				}

				// adjust the desired size but ignore the indicators if we aren't
				// in single line mode. We will adjust to that below
				if (isSingleLine || child != _indicators)
				{
					desiredSize.Height = Math.Max(desiredSize.Height, child.DesiredSize.Height);
					desiredSize.Width += childWidth;
				}
			}

			// adjust the desired size if we aren't in single line display mode.
			// because in this mode the indicators are below other content
			if (isSingleLine == false)
			{
				if (indicatorsHeight > 0)
				{
					desiredSize.Height = interAreaSpacing + indicatorsHeight;
					
					if ( !double.IsPositiveInfinity(availableSize.Height ) )
						desiredSize.Height = Math.Max(desiredSize.Height, availableSize.Height);
				}

				desiredSize.Width = Math.Max(desiredSize.Width, indicatorsWidth);
			}

			if (desiredSize.Width <= availableSize.Width ||
				(_prefix == null && _suffix == null ))
				return desiredSize;

			double minContentWidth = this.ContentAreaMinWidth;
			double indicatorWidth = _indicators != null ? _indicators.DesiredSize.Width : 0;
			double minContentOverallWidth = minContentWidth;
			
			if ( _indicatorsConstrainWidth && indicatorWidth > 0)
				minContentOverallWidth += interAreaSpacing + indicatorWidth;

			double avalableWidthAfterContent = Math.Max(availableSize.Width - (minContentOverallWidth + interAreaSpacing), 0);

			if (_suffix != null)
			{
				double oldDesiredWidth = _suffix.DesiredSize.Width;

				Size szToMeasure = new Size(avalableWidthAfterContent, desiredSize.Height);
				if (_prefix != null)
				{
					szToMeasure.Width = Math.Max(avalableWidthAfterContent - (interAreaSpacing + _prefix.DesiredSize.Width), 0);
				}

				_suffix.Measure(szToMeasure);

				desiredSize.Width = Math.Max(desiredSize.Width + _suffix.DesiredSize.Width - oldDesiredWidth, 0);
				
				if (desiredSize.Width <= availableSize.Width ||
					_prefix == null)
					return desiredSize;

				avalableWidthAfterContent = Math.Max(avalableWidthAfterContent - interAreaSpacing, 0);
			}

			if (_prefix != null)
			{
				double oldDesiredWidth = _prefix.DesiredSize.Width;

				Size szToMeasure = new Size(Math.Max(avalableWidthAfterContent - interAreaSpacing, 0), desiredSize.Height);

				_prefix.Measure(szToMeasure);

				desiredSize.Width = Math.Max(desiredSize.Width + _prefix.DesiredSize.Width - oldDesiredWidth, 0);
			}

			return desiredSize;
		}

		#endregion //MeasureOverride

		#endregion // Base class overrides

		#region Properties

		#region PublicProperties

		#region Area

		/// <summary>
		/// Identifies the Area attached dependency property
		/// </summary>
		/// <seealso cref="GetArea"/>
		/// <seealso cref="SetArea"/>
		public static readonly DependencyProperty AreaProperty = DependencyPropertyUtilities.RegisterAttached("Area",
			typeof(ActivityContentArea), typeof(ActivityContentPanel),
			DependencyPropertyUtilities.CreateMetadata(ActivityContentArea.Content)
			);

		/// <summary>
		/// Gets the value of the attached Area DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="AreaProperty"/>
		/// <seealso cref="SetArea"/>
		public static ActivityContentArea GetArea(DependencyObject d)
		{
			return (ActivityContentArea)d.GetValue(ActivityContentPanel.AreaProperty);
		}

		/// <summary>
		/// Sets the value of the attached Area DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="AreaProperty"/>
		/// <seealso cref="GetArea"/>
		public static void SetArea(DependencyObject d, ActivityContentArea value)
		{
			d.SetValue(ActivityContentPanel.AreaProperty, value);
		}

		#endregion //Area

		#region ContentAreaAlignment

		/// <summary>
		/// Identifies the <see cref="ContentAreaAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContentAreaAlignmentProperty = DependencyPropertyUtilities.Register("ContentAreaAlignment",
			typeof(HorizontalAlignment), typeof(ActivityContentPanel),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.HorizontalAlignmentLeftBox, new PropertyChangedCallback(OnMeasurePropertyChanged))
			);

		private static void OnMeasurePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityContentPanel instance = (ActivityContentPanel)d;

			instance.InvalidateMeasure();
		}

		/// <summary>
		/// Returns or sets the horizontal alignment of the content area
		/// </summary>
		/// <seealso cref="ContentAreaAlignmentProperty"/>
		public HorizontalAlignment ContentAreaAlignment
		{
			get
			{
				return (HorizontalAlignment)this.GetValue(ActivityContentPanel.ContentAreaAlignmentProperty);
			}
			set
			{
				this.SetValue(ActivityContentPanel.ContentAreaAlignmentProperty, value);
			}
		}

		#endregion //ContentAreaAlignment

		#region ContentAreaMinWidth

		/// <summary>
		/// Identifies the <see cref="ContentAreaMinWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ContentAreaMinWidthProperty = DependencyPropertyUtilities.Register("ContentAreaMinWidth",
			typeof(double), typeof(ActivityContentPanel),
			DependencyPropertyUtilities.CreateMetadata(60d, new PropertyChangedCallback(OnMeasurePropertyChanged))
			);

		/// <summary>
		/// Returns or sets the min width for the content area before the prefix and suffix areas will be reduced.
		/// </summary>
		/// <seealso cref="ContentAreaMinWidthProperty"/>
		public double ContentAreaMinWidth
		{
			get
			{
				return (double)this.GetValue(ActivityContentPanel.ContentAreaMinWidthProperty);
			}
			set
			{
				this.SetValue(ActivityContentPanel.ContentAreaMinWidthProperty, value);
			}
		}

		#endregion //ContentAreaMinWidth

		#region InterAreaSpacing

		/// <summary>
		/// Identifies the <see cref="InterAreaSpacing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty InterAreaSpacingProperty = DependencyPropertyUtilities.Register("InterAreaSpacing",
			typeof(double), typeof(ActivityContentPanel),
			DependencyPropertyUtilities.CreateMetadata(0.0d, new PropertyChangedCallback(OnMeasurePropertyChanged))
			);

		/// <summary>
		/// Returns or sets the spacing between the prefix, suffix and content
		/// </summary>
		/// <seealso cref="InterAreaSpacingProperty"/>
		public double InterAreaSpacing
		{
			get
			{
				return (double)this.GetValue(ActivityContentPanel.InterAreaSpacingProperty);
			}
			set
			{
				this.SetValue(ActivityContentPanel.InterAreaSpacingProperty, value);
			}
		}

		#endregion //InterAreaSpacing

		#endregion //PublicProperties	
    
		#endregion //Properties	
    
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